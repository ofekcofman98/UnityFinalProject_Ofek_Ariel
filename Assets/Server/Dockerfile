# Use a lightweight Python base image
FROM python:3.10-slim

# Set working directory
WORKDIR /app

# Copy files
COPY server.py requirements.txt ./

# Install dependencies
RUN pip install -r requirements.txt

# Set Flask environment
ENV PORT=8080

# Run the server
CMD ["python", "server.py"]
