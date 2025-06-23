FROM python:3.10-slim

# Set working directory inside the container
WORKDIR /app

# Copy all files including requirements.txt and server.py
COPY . .

# Install required Python packages
RUN pip install --no-cache-dir -r requirements.txt

# Set the expected port for Google Cloud Run
ENV PORT=8080
EXPOSE 8080

# Command to start the Flask server
CMD ["python", "server.py"]
