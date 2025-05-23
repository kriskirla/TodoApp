# Collaborative TODO Application - Frontend

## Overview
This is the frontend part of the Collaborative TODO Application, built using React and TypeScript. The application allows users to create, update, delete, and share TODO lists in real-time.

## Features
- Create, update, and delete TODO lists.
- Add media content (images and videos) to TODO items.
- Share TODO lists with other users with edit or view-only permissions.
- Real-time collaboration on TODO lists, with immediate updates visible to all users.

## Getting Started

### Prerequisites
- Node.js (version 14 or higher)
- npm (Node Package Manager)

### Installation
1. Clone the repository:
   ```
   git clone <repository-url>
   ```
2. Navigate to the frontend directory:
   ```
   cd collaborative-todo-app/frontend
   ```
3. Install the dependencies:
   ```
   npm install
   ```

### Running the Application
To start the development server, run:
```
npm start
```
This will start the application on `http://localhost:3000`.

### Building for Production
To create a production build, run:
```
npm run build
```
The build artifacts will be stored in the `build` directory.

## Docker
To build and run the frontend application using Docker, follow these steps:

1. Build the Docker image:
   ```
   docker build -t collaborative-todo-frontend .
   ```
2. Run the Docker container:
   ```
   docker run -p 3000:3000 collaborative-todo-frontend
   ```

## Contributing
Contributions are welcome! Please open an issue or submit a pull request for any enhancements or bug fixes.

## License
This project is licensed under the MIT License. See the LICENSE file for details.