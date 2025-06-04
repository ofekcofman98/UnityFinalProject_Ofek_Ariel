from flask import Flask, request, jsonify
from google.cloud import storage
from flask_cors import CORS
import json
import os

app = Flask(__name__)
CORS(app)

PORT = int(os.getenv("PORT", 8080))

BUCKET_NAME = "sqlgamebucket"
FILE_NAME = "relay-query.json"

storage_client = storage.Client()
bucket = storage_client.bucket(BUCKET_NAME)

query_ready = False  # 🧠 Only serve the query once until replaced


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
        print("⏳ No new query. Returning 204.")
        return '', 204

    try:
        blob = bucket.blob(FILE_NAME)
        if not blob.exists():
            print("⚠️ Query file doesn't exist.")
            return '', 204

        contents = blob.download_as_text().strip()

        try:
            data = json.loads(contents)
        except json.JSONDecodeError:
            print("⚠️ Invalid JSON. Returning 204.")
            return '', 204

        if not data or not isinstance(data, dict) or len(data) == 0:
            print("⚠️ JSON is empty.")
            return '', 204

        query_ready = False
        print("📥 Full query object served:", data.get("QueryString", "<no query string>"))
        return jsonify(data)
    except Exception as e:
        print(f"❌ Failed in get-query: {e}")
        return jsonify({'error': str(e)}), 500


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=PORT)
