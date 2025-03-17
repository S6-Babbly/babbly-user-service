# Babbly User Service

## Overview

This is the user service for the Babbly social media platform. It's built with ASP.NET Core, providing RESTful API endpoints for user management.

## Features

- RESTful API endpoints for user management
- JWT validation
- Business logic implementation
- Service-to-service communication

## Getting Started

### Prerequisites

- .NET SDK 7.0 or later
- PostgreSQL

### Installation

1. Clone the repository:

```bash
git clone https://github.com/yourusername/babbly-user-service.git
cd babbly-user-service
```

2. Restore dependencies:

```bash
dotnet restore
```

3. Set up the database connection string in your environment variables or user secrets:

```bash
# For development, you can use user secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=babbly-users;Username=your_username;Password=your_password;"
```

4. Run the application:

```bash
dotnet run --project babbly-user-service/babbly-user-service.csproj
```

5. The API will be available at [http://localhost:5001](http://localhost:5001).

## API Endpoints

- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get a specific user
- `POST /api/users` - Create a new user
- `PUT /api/users/{id}` - Update a user
- `DELETE /api/users/{id}` - Delete a user
- `GET /api/health` - Health check endpoint

## Testing

```bash
# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Docker

You can also run the application using Docker:

```bash
# Build the Docker image
docker build -t babbly-user-service .

# Run the container
docker run -p 5001:80 -e "ConnectionStrings__DefaultConnection=Host=your_db_host;Database=babbly-users;Username=your_username;Password=your_password;" babbly-user-service
```

## CI/CD Pipeline

This repository uses GitHub Actions for continuous integration and deployment:

- **Code Quality**: SonarCloud analysis
- **Tests**: Unit and integration tests
- **Docker Build**: Builds and validates Docker image
- **Deployment**: Automated deployment to staging/production environments

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
