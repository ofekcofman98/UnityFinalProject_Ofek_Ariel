from collections import defaultdict
from flask import Flask, request, jsonify
from flask_cors import CORS
from google.cloud import storage
import logging
import random
import json
import os

app = Flask(__name__)
CORS(app)

### LOGGING ###
logging.basicConfig(filename='server.log', level=logging.INFO,
                    format='%(asctime)s - %(levelname)s - %(message)s')


@app.before_request
def log_request():
    logging.info(f"{request.method} request to {request.path} from {request.remote_addr}")


# Port for Cloud Run
PORT = int(os.getenv("PORT", 8080))

### INITIALIZATIONS FOR GOOGLE CLOUD ###
BUCKET_NAME = "sqlgamebucket"
KEYS_OBJECT = "keys/keys.txt"
FILE_NAME = "relay-query.json"
stored_data = []
storage_client = storage.Client()
bucket = storage_client.bucket(BUCKET_NAME)


### HELPERS ###

def _game_blob_name(key: str) -> str:
    return f"saved games/{key}/.json"


def _query_blob_name(key: str) -> str:
    return f"relay-query-{key}.json"


def get_key_or_default():
    sid = None
    if request.method == 'POST':
        try:
            data = request.get_json(silent=True) or {}
            sid = data.get('key')
        except Exception:
            sid = None
    sid = sid or request.args.get('key')
    return sid or "0"


### DICTIONARIES DEFINITIONS TO HOLD DATA SEPARATELY ###

is_Level_done_by_key = defaultdict(lambda: 0)  # sid -> int
current_reset_by_key = defaultdict(lambda: 0)  # sid -> int
current_level_by_key = defaultdict(int)  # sid -> int
seq_by_key = defaultdict(lambda: 1)  # sid -> int
query_ready_by_key = defaultdict(lambda: False)  # sid -> bool
is_mobile_ready_by_key = defaultdict(lambda: False)  # sid -> bool
waiting_keys = []


### BASIC HEALTH CHECKS ENDPOINTS ###

@app.route('/', methods=['GET'])
def home():
    return "✅ Unified SQL Game Server is running!"


@app.route('/store', methods=['POST'])
def store_data():
    global stored_data
    data = request.get_json()
    if not data or 'data' not in data:
        return jsonify({'error': 'Expected JSON with "data" key'}), 400

    word = data['data']
    if word in stored_data:
        return jsonify({'message': f"The data piece {word} is already stored.", 'data': word})
    stored_data.append(word)
    return jsonify({'message': 'Data stored successfully!', 'data': word})


@app.route('/retrieve', methods=['GET'])
def retrieve_data():
    return jsonify({'stored_data': stored_data})


@app.route('/echo', methods=['POST'])
def echo():
    data = request.get_json()
    if not data or 'data' not in data:
        return jsonify({'error': 'Expected JSON with "data" key'}), 400
    return jsonify({'message': 'echoing back :', 'data': data['data']})


### GET AND SEND STATE ENDPOINTS ###

@app.route('/send-state', methods=['POST'])
def send_state():
    key = get_key_or_default()

    data = request.get_json() or {}
    if 'seqNumber' not in data or not isinstance(data['seqNumber'], int):
        return jsonify({'error': 'Expected JSON with integer "seqNumber" key'}), 400
    if 'currentLevelIndex' not in data or not isinstance(data['currentLevelIndex'], int):
        return jsonify({'error': 'Expected JSON with integer "currentLevelIndex" key'}), 400

    is_Level_done_by_key[key] = 1
    seq_by_key[key] = data['seqNumber']
    current_level_by_key[key] = data['currentLevelIndex']

    app.logger.info(
        f"[{key}] isLevelDone=1, level={current_level_by_key[key]}, seq={seq_by_key[key]}")
    return jsonify({
        'message': 'State updated successfully',
        'key': key,
        'currentLevelIndex': current_level_by_key[key],
        'seqNumber': seq_by_key[key]
    }), 200


@app.route('/get-state', methods=['GET'])
def get_state():
    key = get_key_or_default()

    isDone = is_Level_done_by_key[key]
    currentLevel = current_level_by_key[key]
    seq_number = seq_by_key[key]

    if isDone == 1:
        is_Level_done_by_key[key] = 0
        app.logger.info(f"[{key}] isLevelDone was 1 -> return 200 and reset to 0")
        return jsonify({'key': key, 'isLevelDone': 1, 'currentLevelIndex': currentLevel, 'seqNumber': seq_number}), 200

    return jsonify({'key': key, 'isLevelDone': 0, 'currentLevelIndex': currentLevel, 'seqNumber': seq_number}), 200


### GET AND SEND RESET ENDPOINTS ###

@app.route('/send-reset', methods=['POST'])
def send_reset():
    key = get_key_or_default()
    data = request.get_json() or {}

    if 'seqNumber' not in data or not isinstance(data['seqNumber'], int):
        return jsonify({'error': 'Expected JSON with int "seqNumber" key'}), 400

    current_reset_by_key[key] = 1
    seq_by_key[key] = data['seqNumber']

    return jsonify({
        'message': 'reset sent successfully',
        'key': key,
        'seqNumber': seq_by_key[key]
    }), 200


@app.route('/get-reset', methods=['GET'])
def get_reset():
    key = get_key_or_default()

    cr = current_reset_by_key[key]
    if cr == 1:
        is_Level_done_by_key[key] = 0
        current_reset_by_key[key] = 0
        app.logger.info(f"[{key}] reset was 1 -> return 200 and reset to 0")
        return jsonify({
            'key': key,
            'reset': 0,
            'seqNumber': seq_by_key[key]
        }), 200

    return jsonify({
        'key': key,
        'reset': 0,
        'seqNumber': seq_by_key[key]
    }), 204


### GET AND SEND QUERY ENDPOINTS ###

@app.route('/send-query', methods=['POST'])
def send_query():
    key = get_key_or_default()
    query_object = request.get_json()

    if not query_object or not isinstance(query_object, dict):
        return jsonify({'error': "Expected a full query object in JSON format."}), 400

    try:
        blob = bucket.blob(_query_blob_name(key))
        blob.upload_from_string(json.dumps(query_object), content_type='application/json')
        query_ready_by_key[key] = True
        app.logger.info(f"[{key}] Query saved to GCS.")
        return jsonify({'status': "Query stored successfully!", 'key': key}), 200
    except Exception as e:
        app.logger.exception("Failed to store query")
        return jsonify({'error': str(e)}), 500


@app.route('/get-query', methods=['GET'])
def get_query():
    key = get_key_or_default()

    if not query_ready_by_key[key]:
        return '', 204

    try:
        blob = bucket.blob(_query_blob_name(key))
        if not blob.exists():
            return '', 204

        contents = blob.download_as_text().strip()
        data = json.loads(contents)

        if not data or not isinstance(data, dict):
            return '', 204

        query_ready_by_key[key] = False
        return jsonify(data), 200
    except Exception as e:
        app.logger.exception("Failed in get-query")
        return jsonify({'error': str(e)}), 500


## FULL AND NORMAL SERVER RESET ENDPOINTS ###

@app.route('/server-reset', methods=['GET'])
def server_reset_per_session():
    key = get_key_or_default()

    is_Level_done_by_key[key] = 0
    current_reset_by_key[key] = 0
    current_level_by_key[key] = 0
    seq_by_key[key] = 1
    query_ready_by_key[key] = False
    is_mobile_ready_by_key[key] = False

    try:
        blob = bucket.blob(_query_blob_name(key))
        if blob.exists():
            blob.delete()
    except Exception:
        pass

    app.logger.info(f"[{key}] key reset to defaults")
    return jsonify({'message': f'key {key} reset to defaults', 'key': key}), 200


@app.route('/full-server-reset', methods=['GET'])
def server_reset_all():
    global stored_data, waiting_keys
    is_Level_done_by_key.clear()
    current_reset_by_key.clear()
    current_level_by_key.clear()
    seq_by_key.clear()
    query_ready_by_key.clear()
    is_mobile_ready_by_key.clear()
    waiting_keys = []
    stored_data = []
    app.logger.info("Reset ALL keys to defaults")
    return jsonify({'message': 'all keys reset to defaults'}), 200


### SEND AND GET GAME-PROGRESS ENDPOINTS ###

@app.route('/send-gameprogress', methods=['POST'])
def send_gameprogress():
    key = get_key_or_default()
    data = request.get_json() or {}

    game = data.get('game')  # your full progress object (dict/JSON)

    if game is None:
        return jsonify({'error': 'Expected JSON "game" object'}), 400

    try:
        blob = bucket.blob(_game_blob_name(key))
        payload = json.dumps(game) if not isinstance(game, str) else game
        append_key_to_store(key)
        blob.upload_from_string(payload, content_type='application/json')

        return jsonify({'message': 'Game stored', 'key': key}), 200
    except Exception as e:
        app.logger.exception("send-gameprogress failed")
        return jsonify({'error': str(e)}), 500


@app.route('/get-gameprogress', methods=['POST'])
def get_gameprogress():
    key = get_key_or_default()

    try:
        blob = bucket.blob(_game_blob_name(key))
        if not blob.exists():
            return jsonify({'error': f'No saved game for key {key}'}), 404

        content = blob.download_as_text()
        # If the saved payload is a JSON string, parse it; otherwise return as-is.
        try:
            obj = json.loads(content)
            waiting_keys.append(key)
        except Exception:
            obj = content
        return jsonify(obj), 200
    except Exception as e:
        app.logger.exception("get-gameprogress failed")
        return jsonify({'error': str(e)}), 500


### CONNECT SENDER AND LISTENER ENDPOINTS ###
@app.route('/send-connect', methods=['POST'])
def send_connect():
    try:
        data = request.get_json() or {}
        given_key = data['key']
        if given_key in is_mobile_ready_by_key.keys():
            is_mobile_ready_by_key[given_key] = True
            return jsonify({'message': 'key was in list, now removed ', 'key': given_key}), 200
        else:
            return jsonify({'message': 'key was not in list', 'key': given_key}), 404
    except Exception as e:
        app.logger.exception("send-connect failed")
        return jsonify({'error': str(e)}), 500


@app.route('/get-connect', methods=['GET'])
def get_connect():
    try:
        key = get_key_or_default()
        if key in is_mobile_ready_by_key.keys() and is_mobile_ready_by_key[key]:
            return jsonify({'message': 'mobile is ready, updating PC', 'key': key}), 200
        else:
            return jsonify({'message': 'mobile is not ready', 'key': key}), 204
    except Exception as e:
        app.logger.exception("get-connect failed")
        return jsonify({'error': str(e)}), 500


### KEYS RELATED ENDPOINTS ###

def load_saved_keys():
    blob = bucket.blob(KEYS_OBJECT)
    if not blob.exists():
        return set()
    return set(line for line in blob.download_as_text().splitlines() if line)


def append_key_to_store(key):
    blob = bucket.blob(KEYS_OBJECT)
    existing = ""
    if blob.exists():
        existing = blob.download_as_text()
    blob.upload_from_string(existing + f"{key}\n", content_type="text/plain")


@app.route('/delete-all-keys-and-games', methods=['POST'])
def delete_all_keys_and_games():
    try:
        # 1. Delete keys file
        blob = bucket.blob(KEYS_OBJECT)
        if blob.exists():
            blob.delete()

        # 2. Reset in-memory tracking
        global waiting_keys
        waiting_keys = []
        is_mobile_ready_by_key.clear()

        # 3. Delete all saved games
        blobs = bucket.list_blobs(prefix="saved games/")
        deleted_count = 0
        for b in blobs:
            b.delete()
            deleted_count += 1

        app.logger.info(f"🗑️ Deleted all keys and {deleted_count} saved games.")
        return jsonify({
            'message': f'All keys and saved games deleted successfully',
            'deleted_games': deleted_count
        }), 200
    except Exception as e:
        app.logger.exception("delete-all-keys-and-games failed")
        return jsonify({'error': str(e)}), 500



@app.route('/validate-key', methods=['GET'])
def validate_key():
    key = get_key_or_default()
    try:
        blob = bucket.blob(_game_blob_name(key))
        if not blob.exists():
            return jsonify({'error': f'No saved game for key {key}'}), 204
        else:
            waiting_keys.append(key)
            is_mobile_ready_by_key[key] = False
            return jsonify({'message': 'the key exists in the server'}), 200
    except Exception as e:
        app.logger.exception("send-connect failed")
        return jsonify({'error': str(e)}), 500


@app.route('/generate-key', methods=['GET'])
def generate_unique_key():
    keys = load_saved_keys()
    for _ in range(10000):
        k = str(random.randint(100000, 999999))
        if k not in keys:
            ##append_key_to_store(k)
            waiting_keys.append(k)
            is_mobile_ready_by_key[k] = False
            return jsonify({'key': k}), 200
    return jsonify({'error': 'Unable to generate a unique key'}), 500


@app.route('/all-keys', methods=['GET'])
def view_keys():
    keys = sorted(load_saved_keys())
    return jsonify({'keys': keys, 'count': len(keys)})


@app.route('/compare-keys', methods=['POST'])
def compare_keys():
    try:
        data = request.get_json() or {}
        given_key = data['key']
        if given_key in waiting_keys:
            waiting_keys.remove(given_key)
            return jsonify({'message': 'key was in list, now removed ', 'key': given_key}), 200
        else:
            return jsonify({'message': 'key was not in list', 'key': given_key}), 204
    except Exception as e:
        app.logger.exception("compare-keys failed")
        return jsonify({'error': str(e)}), 500


# ===== Launch =====
if __name__ == '__main__':
    app.run(host='0.0.0.0', port=PORT)
