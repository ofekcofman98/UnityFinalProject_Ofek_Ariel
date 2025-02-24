const express = require('express');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(express.json()); // Ensures JSON body parsing

let lastQuery = ""; // Stores the latest SQL query

// 1️ HTTP Endpoint (For Mobile to Send SQL Query)
app.post('/send-query', (req, res) => {
    if (!req.body || !req.body.query) {
        console.log("❌ No query received in the request body.");
        return res.status(400).json({ error: "Bad Request: No query found." });
    }

    lastQuery = req.body.query;
    console.log("📤 Query Received: ", lastQuery);
    res.json({ status: "Query stored successfully!" });
});

// 2️ HTTP Endpoint (For PC/WebGL to Fetch Query)
app.get('/get-query', (req, res) => {
    if (!lastQuery) {
        console.log("⚠️ No query found, returning empty response.");
        return res.json({ query: "" }); // ✅ Return an empty query instead of 404
    }

    res.json({ query: lastQuery }); 
    lastQuery = ""; // Optional: Clear after sending
});

// Start the HTTP Server
app.listen(8080, () => {
    console.log("🚀 HTTP Server running on http://localhost:8080");
});
