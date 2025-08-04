from flask import Flask, request, jsonify
from flask_cors import CORS
from google.cloud import storage
import logging
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
FILE_NAME = "relay-query.json"
storage_client = storage.Client()
bucket = storage_client.bucket(BUCKET_NAME)

# In-memory state
stored_data = []
current_state = {'isLevelDone': False}
current_reset = {'reset': False}
current_SQLmode = {'sqlmode': False}
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
    data = request.get_json()
    if not data or 'isLevelDone' not in data or not isinstance(data['isLevelDone'], bool):
        return jsonify({'error': 'Expected JSON with boolean "isLevelDone" key'}), 400

    current_state['isLevelDone'] = data['isLevelDone']
    logging.info(f"isLevelDone updated to: {current_state['isLevelDone']}")
    return jsonify({'message': 'State updated successfully', 'isLevelDone': current_state['isLevelDone']}), 200


@app.route('/get-state', methods=['GET'])
def get_state():
    if current_state['isLevelDone']:
        current_state['isLevelDone'] = False
        logging.info("isLevelDone was True, returning 200 and resetting to False")
        return jsonify({'isLevelDone': True}), 200
    return '', 204


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
    data = request.get_json()
    if not data or 'reset' not in data or not isinstance(data['reset'], bool):
        return jsonify({'error': 'Expected JSON with boolean "reset" key'}), 400

    current_reset['reset'] = data['reset']
    logging.info(f"reset updated to: {current_reset['reset']}")
    return jsonify({'message': 'reset sent successfully', 'reset': current_reset['reset']}), 200


@app.route('/get-reset', methods=['GET'])
def get_reset():
    if current_reset['reset']:
        current_state['isLevelDone'] = False
        current_reset['reset'] = False
        current_SQLmode['sqlmode'] = False
        current_reset['reset'] = False
        logging.info("reset was True, returning 200 and resetting to False")

        return jsonify({'reset': True}), 200
    return '', 204


# ===== SQLMODE SEND AND RETRIEVE ENDPOINTS =====
@app.route('/send-sqlmode', methods=['POST'])
def send_sqlmode():
    data = request.get_json()
    if not data or 'sqlmode' not in data or not isinstance(data['sqlmode'], bool):
        return jsonify({'error': 'Expected JSON with boolean "sqlmode" key'}), 400

    current_SQLmode['sqlmode'] = data['sqlmode']
    logging.info(f"Setting the sqlmode value in server to {current_SQLmode['sqlmode']}")
    return jsonify(current_SQLmode), 200


@app.route('/get-sqlmode', methods=['GET'])
def get_sqlmode():
    if current_SQLmode['sqlmode']:
        logging.info(f"sqlmode is True.")
        return jsonify({'message': 'sqlmode is TRUE', 'sqlmode': True}), 200
    else:
        logging.info(f"sqlmode changes to False.")
        return jsonify({'message': 'changing SQLmode to False', 'sqlmode': False}), 201


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

# ==== SERVER RESET ====
@app.route('/server-reset', methods=['POST'])
def server_reset():
    data = request.get_json()
    if not data or 'password' not in data:
        return jsonify({'error': 'Expected JSON with a string value for password key'}), 400

    password = data['password']
    if password == 'nuw39miNC83MF94989D3nmomcl9j4mfnnxE83NTS12fvded':
        logging.info(f"reseting server values and information stored to default")
        stored_data = []
        current_state = {'isLevelDone': False}
        current_reset = {'reset': False}
        current_SQLmode = {'sqlmode': False}
        query_ready = False
        return jsonify({'message': 'server contents have been reset'}), 200
    return jsonify({'message': 'incorrect password'}), 403


# ===== Launch =====
if __name__ == '__main__':
    app.run(host='0.0.0.0', port=PORT)
