# Final-Project

The Final Project of "Back-End Development with .NET" course on Coursera

## User API

This ASP.NET Core minimal API provides CRUD endpoints for users:

- `GET /users` lists all users.
- `GET /users/{name}` retrieves one user.
- `POST /users` creates a user.
- `PUT /users/{name}` updates a user's age.
- `DELETE /users/{name}` removes a user.

`POST` and `PUT` validate user data before changing the in-memory collection. Names must be between 2 and 100 characters, and ages must be valid adult ages. Invalid requests return `400 Bad Request` with validation details.

The request logging middleware records the HTTP method, path, response status code, and elapsed time for every request.
