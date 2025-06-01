# Collaborative TODO Application

## Overview
The Collaborative TODO Application is a real-time collaborative platform that allows users to create, update, and manage TODO lists. Users can share their lists with others, allowing for collaborative editing and viewing. The application supports media content such as images and videos, enhancing the functionality of TODO items.

## Video Demo
[Demo Video 1](https://youtu.be/mh1iMUtocj0?si=bmEyb83SKPgpZytG): Showcasing basic register/login/token usage, CRUD Todo List and items, Real-time sharing and collaboration
[Demo Video 2](https://youtu.be/iWkVmqnb11Y): Showcasing decoupled login/registration, Filter/Sorting functionality and combined.

## Features
- Create, update, and delete TODO lists.
- Add media content (images and videos) to TODO items.
- Share TODO lists with other users with edit or view-only permissions.
- Real-time updates for collaborative editing.
- Scalable architecture to support up to 100 million daily active users.

## Architecture
The application is structured into three main components:
1. **Frontend**: Built with React, providing a user-friendly interface for managing TODO lists.
2. **Backend**: Developed using C# and .NET 9, handling business logic and data management.
3. **Database**: PostgreSQL is used for data storage, ensuring reliable and efficient data access.

## Technologies Used
- **Frontend**: React, TypeScript
- **Backend**: C#, .NET 9
- **Database**: PostgreSQL
- **Containerization**: Docker for all components

## Setup Instructions
### Prerequisites
- Docker and Docker Compose installed on your machine.

### Running the Application
1. Clone the repository:
   ```
   git clone https://github.com/kriskirla/TodoApp.git
   cd TodoApp
   ```

2. Build and run the application using Docker Compose:
   ```
   docker-compose up --build
   ```

   **Note**: You may encounter issue during dotnet restore when running on coperate machine like below
   ```
   The remote certificate is invalid because of errors in the certificate chain: PartialChain
   ```
   In that case, you can add this above dotnet restore in the backend Dockerfile
   ```
   RUN apt-get update && apt-get install -y ca-certificates && update-ca-certificates
   ```
   If it still doesn't work, then you can manually add your corporate root CA certificate in the container.
   Otherwise, use personal machine.

3. Access the application:
   - Frontend: `http://localhost:3000`
   - Backend API (Swagger UI): `http://localhost:5286`

### Killing the Application
1. Remove all running containers and images
   ```
   docker-compose down --rmi all --volumes
   ```
