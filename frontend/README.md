# Frontend Component
This component uses react as the frontend

## Prerequisites
- Node.js (version 14 or higher)
- npm (Node Package Manager)
- Docker

## Setup Instructions (Only if want to host backend alone, otherwise refer to parent README.md)
1. Install the dependencies:
   ```
   npm install
   ```

2. Start the server
   ```
   npm start
   ```
3. 
This will start the application on `http://localhost:3000`.

### Docker
To build and run the frontend application using Docker, follow these steps:

1. Build the Docker image:
   ```
   docker build -t todo-frontend .
   ```
2. Run the Docker container:
   ```
   docker run -p 3000:3000 todo-frontend
   ```
