# Babbly User Service

## Overview

Babbly User Service is a microservice within the Babbly application ecosystem responsible for handling user management, authentication, and profile data. It's built with .NET 9 API and uses Entity Framework Core as the ORM (Object-Relational Mapper) with a PostgreSQL database backend.

## Features

- RESTful API endpoints for user management
- User registration and profile management
- Authentication and authorization
- Service-to-service communication

## Technology Stack

- **.NET 9 API**: Latest version of the .NET platform
- **Entity Framework Core**: ORM for database access
- **PostgreSQL**: Relational database for data storage
- **Docker**: Containerization for easy deployment

## Getting Started

### Prerequisites

- .NET SDK 9.0 or later
- PostgreSQL
- Docker and Docker Compose (for containerized deployment)

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
- `GET /api/users/auth0/{auth0Id}` - Get a user by Auth0 ID
- `POST /api/users` - Create a new user
- `PUT /api/users/{id}` - Update a user
- `DELETE /api/users/{id}` - Delete a user
- `GET /api/health` - Health check endpoint

## Database Schema

### Users Table

```
[TODO: Add schema details here]
- id (PK)
- auth0_id
- username
- email
- role
- created_at
- updated_at
```

### User Extra Data Table

```
[TODO: Add schema details here]
- id (PK)
- user_id (FK)
- display_name
- profile_picture
- bio
- address
- phone_number
- created_at
- updated_at
```

## Running with Docker

The Babbly User Service can be easily run using Docker and Docker Compose:

### Using Docker Compose

1. Ensure Docker and Docker Compose are installed on your system.

2. Clone the repository and navigate to the project directory.

3. Run the application using Docker Compose:

```bash
docker-compose up
```

This will start both the user service and a PostgreSQL database container. The user service will be available at http://localhost:5001.

### Configuration

The Docker Compose configuration includes:

- A PostgreSQL database with persistence
- Automatic database migrations
- Environment variable configuration for development

### Environment Variables

You can customize the deployment by setting environment variables:

```bash
# Set database credentials
POSTGRES_USER=custom_user POSTGRES_PASSWORD=custom_password docker-compose up
```

## Testing

```bash
# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## CI/CD Pipeline

This repository uses GitHub Actions for continuous integration and deployment:

- **Code Quality**: SonarCloud analysis
- **Tests**: Unit and integration tests
- **Docker Build**: Builds and validates Docker image
- **Deployment**: Automated deployment to staging/production environments

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
