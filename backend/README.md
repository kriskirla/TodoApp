# Backend Component
This component uses C# dotnet9 as the backend.

## Setup Instructions (Only if want to host backend alone, otherwise refer to parent README.md)
1. **Build the Docker Image**
   ```bash
   docker build -t todo-api .
   ```

2. **Run the Application**
   You can run the application using Docker Compose, which will also set up the frontend and database services.
   ```bash
   docker run -p 5286:80 todo-api
   ```

3. **Access the API**
   The API will be available at `http://localhost:5286`.

**Note**: The Migration folder should already be created and within the folder. If not found, run the following to setup DB migration.
```
dotnet ef migrations add TodoApp
```

## SwaggerUI (For testing purposes)
Create a user, then Login with the same email. Using the token returned, Authorize as follows:
```
Bearer <token>
```

## Unit tests
Nagivate to test folder
```
cd tests
dotnet test
```
