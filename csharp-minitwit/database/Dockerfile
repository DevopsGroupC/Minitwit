# Use a base image with Ubuntu
FROM ubuntu:latest

# Install SQLite
RUN apt-get update && \
    apt-get install -y sqlite3 && \
    rm -rf /var/lib/apt/lists/*

# Set the working directory in the container
WORKDIR /app

# Copy the schema and initial content files into the container
COPY database/schema.sql /app/schema.sql
COPY database/minitwit.db /app/minitwit.db

# Run SQLite commands to create the schema and import initial data
RUN sqlite3 minitwit.db < schema.sql

# Expose any necessary ports (if your application needs to listen on a specific port)

# Define the command to run your application (if needed)
CMD ["sqlite3", "minitwit.db"]