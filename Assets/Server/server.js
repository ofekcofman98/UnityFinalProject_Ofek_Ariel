const express = require('express');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(express.json()); // Ensures JSON body parsing

let lastQuery = ""; // Stores the latest SQL query

// 1️⃣ HTTP Endpoint (For Mobile to Send SQL Query)
app.post('/send-query', (req, res) => {
    if (!req.body || !req.body.query) {
        console.log("❌ No query received in the request body.");
        return res.status(400).send("Bad Request: No query found.");
    }

    lastQuery = req.body.query;
    console.log("📤 Query Received: ", lastQuery);
    res.sendStatus(200);
});

// 2️⃣ HTTP Endpoint (For PC/WebGL to Fetch Query)
app.get('/get-query', (req, res) => {
    res.send(lastQuery);
    lastQuery = ""; // Clear after sending (optional)
});

// Start the HTTP Server
app.listen(8080, () => {
    console.log("🚀 HTTP Server running on http://localhost:8080");
});
