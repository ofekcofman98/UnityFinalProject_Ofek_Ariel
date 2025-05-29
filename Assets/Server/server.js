const express = require('express');
const cors = require('cors');
const { Storage } = require('@google-cloud/storage');
const bodyParser = require('body-parser');

const app = express();
app.use(cors());
app.use(bodyParser.json());

const PORT = process.env.PORT || 8080;

const BUCKET_NAME = "sqlgamebucket";
const FILE_NAME = "relay-query.json";

const storage = new Storage();
const bucket = storage.bucket(BUCKET_NAME);

let queryReady = false;  // 🧠 This must be respected BEFORE file reads

// ✅ POST: Save query and mark ready
app.post('/send-query', async (req, res) => {
  const queryObject = req.body;

  if (!queryObject || typeof queryObject !== 'object') {
    return res.status(400).json({ error: "Expected a full query object in JSON format." });
  }

  try {
    const file = bucket.file(FILE_NAME);
    await file.save(JSON.stringify(queryObject), {
      contentType: 'application/json',
    });

    queryReady = true;
    console.log("📤 Full Query object saved to GCS.");
    res.json({ status: "Query stored successfully!" });
  } catch (error) {
    console.error("❌ Failed to store query:", error.message);
    res.status(500).json({ error: error.message });
  }
});


// ✅ GET: Only return query once, then 204s
app.get('/get-query', async (req, res) => {
  if (!queryReady) {
    console.log("⏳ No new query. Returning 204.");
    return res.status(204).send();
  }

  try {
    const file = bucket.file(FILE_NAME);
    const [exists] = await file.exists();
    if (!exists) {
      console.log("⚠️ Query file doesn't exist.");
      return res.status(204).send();
    }

    const [contents] = await file.download();
    const raw = contents.toString().trim();

    let json;
    try {
      json = JSON.parse(raw);
    } catch {
      console.log("⚠️ Invalid JSON. Returning 204.");
      return res.status(204).send();
    }

    // ✅ Minimal sanity check
    if (!json || typeof json !== 'object' || Object.keys(json).length === 0) {
      console.log("⚠️ JSON is empty.");
      return res.status(204).send();
    }

    // ✅ Serve and clear
    queryReady = false;
    console.log("📥 Full query object served:", json.QueryString || "<no query string>");
    return res.status(200).json(json);

  } catch (error) {
    console.error("❌ Failed in get-query:", error.message);
    return res.status(500).json({ error: error.message });
  }
});


app.listen(PORT, () => {
  console.log(`🚀 Server running on port ${PORT}`);
});
