from flask import Flask, request, jsonify
from flask_cors import CORS
from google.cloud import storage
import logging
import random
import json
import os

app = Flask(__name__)
CORS(app)

# Logging
logging.basicConfig(filename='server.log', level=logging.INFO,
                    format='%(asctime)s - %(levelname)s - %(message)s')

# Port for Cloud Run
PORT = int(os.getenv("PORT", 8080))

# Google Cloud Storage setup
BUCKET_NAME = "sqlgamebucket"
KEYS_OBJECT = "keys/keys.txt"
FILE_NAME = "relay-query.json"
storage_client = storage.Client()
bucket = storage_client.bucket(BUCKET_NAME)

# In-memory state
stored_data = []
current_state = {'isLevelDone': 0}
current_reset = {'reset': 1}
current_level = 0
seqNumber = 1
query_ready = False  # Only serve query once


@app.before_request
def log_request():
    logging.info(f"{request.method} request to {request.path} from {request.remote_addr}")


@app.route('/', methods=['GET'])
def home():
    return "✅ Unified SQL Game Server is running!"


# ===== In-Memory Endpoints =====

@app.route('/store', methods=['POST'])
def store_data():
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


# ===== STATE SEND AND RETRIEVE ENDPOINTS =====

@app.route('/send-state', methods=['POST'])
def send_state():
    global current_level

    data = request.get_json()
    if not data or 'isLevelDone' not in data or not isinstance(data['isLevelDone'], int):
        return jsonify({'error': 'Expected JSON with integer "isLevelDone" key'}), 400

    if not data or 'currentLevelIndex' not in data or not isinstance(data['currentLevelIndex'], int):
        return jsonify({'error': 'Expected JSON with integer "currentLevelIndex" key'}), 400

    current_state['isLevelDone'] = data['isLevelDone']
    current_level = data['currentLevelIndex']
    logging.info(f"isLevelDone updated to: {current_state['isLevelDone']}")
    return jsonify({'message': 'State updated successfully', 'isLevelDone': current_state['isLevelDone'],
                    'currentLevelIndex': current_level}), 200


@app.route('/get-state', methods=['GET'])
def get_state():
    if current_state['isLevelDone'] == 1:
        current_state['isLevelDone'] = 0
        logging.info("isLevelDone was 1, returning 200 and resetting to 0")
        return jsonify({'isLevelDone': 1, 'currentLevelIndex': current_level}), 200
    return jsonify({'isLevelDone': 0, 'currentLevelIndex': current_level}), 200



# ===== QUERY SEND AND RETRIEVE ENDPOINTS =====

@app.route('/send-query', methods=['POST'])
def send_query():
    global query_ready
    query_object = request.get_json()

    if not query_object or not isinstance(query_object, dict):
        return jsonify({'error': "Expected a full query object in JSON format."}), 400

    try:
        blob = bucket.blob(FILE_NAME)
        blob.upload_from_string(json.dumps(query_object), content_type='application/json')
        query_ready = True
        print("📤 Full Query object saved to GCS.")
        return jsonify({'status': "Query stored successfully!"})
    except Exception as e:
        print(f"❌ Failed to store query: {e}")
        return jsonify({'error': str(e)}), 500


@app.route('/get-query', methods=['GET'])
def get_query():
    global query_ready
    if not query_ready:
        return '', 204

    try:
        blob = bucket.blob(FILE_NAME)
        if not blob.exists():
            return '', 204

        contents = blob.download_as_text().strip()
        data = json.loads(contents)

        if not data or not isinstance(data, dict):
            return '', 204

        query_ready = False
        return jsonify(data)
    except Exception as e:
        print(f"❌ Failed in get-query: {e}")
        return jsonify({'error': str(e)}), 500


# ===== RESET SEND AND RETRIEVE ENDPOINTS =====

@app.route('/send-reset', methods=['POST'])
def send_reset():
    global seqNumber
    data = request.get_json()
    if not data or 'reset' not in data or not isinstance(data['reset'], int):
        return jsonify({'error': 'Expected JSON with int "reset" key'}), 400

    current_reset['reset'] = data['reset']

    if 'seqNumber' not in data or not isinstance(data['seqNumber'], int):
        return jsonify({'error': 'Expected JSON with int "seqNumber" key'}), 400

    seqNumber = data['seqNumber']  # ✅ Now updates global seqNumber
    return jsonify({
        'message': 'reset sent successfully',
        'reset': current_reset['reset'],
        'seqNumber': seqNumber
    }), 200


@app.route('/get-reset', methods=['GET'])
def get_reset():
    if current_reset['reset'] == 1:
        current_state['isLevelDone'] = False
        current_reset['reset'] = 0
        logging.info("reset was 1, returning 200 and resetting to 0")

        return jsonify({
            'reset': current_reset['reset'],
            'seqNumber': seqNumber
        }), 200

    return jsonify({
        'reset': current_reset['reset'],
        'seqNumber': seqNumber
    }), 204


# ===== SEND AND RETRIEVE GAME PROGRESS ENDPOINTS =====

@app.route('/send-gameprogress', methods=['POST'])
def store_object():
    data = request.get_json()
    if not data or 'key' not in data or 'game' not in data:
        return jsonify({'error': 'Expected JSON with "key" and "game"'}), 400

    key = data['key']
    game = data['game']
    filename = f"saved games/{key}.json"

    try:
        blob = bucket.blob(filename)
        blob.upload_from_string(json.dumps(game), content_type='application/json')
        return jsonify({'message': f"Game stored with key: {key}"}), 200
    except Exception as e:
        return jsonify({'error': str(e)}), 500


@app.route('/get-gameprogress', methods=['POST'])
def get_object_post():
    data = request.get_json()
    if not data or 'key' not in data:
        return jsonify({'error': 'Expected JSON with "key"'}), 400

    key = data['key']
    filename = f"saved games/{key}.json"

    try:
        blob = bucket.blob(filename)
        if not blob.exists():
            return jsonify({'error': f"No saved game object found for key: {key}"}), 404

        content = blob.download_as_text()
        obj = json.loads(content)
        return jsonify(obj), 200
    except Exception as e:
        return jsonify({'error': str(e)}), 500


# ===== UNIQUE KEY GENERATOR LOGIC, METHODS AND ENDPOINTS =====


def load_existing_keys():
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


@app.route('/generate-key', methods=['GET'])
def generate_unique_key():
    keys = load_existing_keys()
    for _ in range(10000):
        k = str(random.randint(100000, 999999))
        if k not in keys:
            append_key_to_store(k)
            return jsonify({'key': k}), 200
    return jsonify({'error': 'Unable to generate a unique key'}), 500


@app.route('/all-keys', methods=['GET'])
def view_keys():
    keys = sorted(load_existing_keys())
    return jsonify({'keys': keys, 'count': len(keys)})


# ==== SERVER RESET ====
@app.route('/server-reset', methods=['GET'])
def server_reset():
    logging.info("resetting server values to defaults")
    global stored_data, current_state, current_reset, query_ready, seqNumber, current_level
    stored_data = []
    current_state = {'isLevelDone': 0}
    current_reset = {'reset': 0}
    query_ready = False
    seqNumber = 1
    current_level = 0
    return jsonify({'message': 'server contents have been reset'}), 200


# ===== Launch =====
if __name__ == '__main__':
    app.run(host='0.0.0.0', port=PORT)
