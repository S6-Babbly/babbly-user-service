# Babbly User Service

## Overview

Babbly User Service is a microservice within the Babbly application ecosystem responsible for handling user management, authentication, and profile data. It's built with .NET 9 API and uses Entity Framework Core as the ORM (Object-Relational Mapper) with a PostgreSQL database backend. It integrates with Kafka for event-driven communication with other services.

## Features

- RESTful API endpoints for user management
- User registration and profile management
- Authentication and authorization via Auth0
- Kafka integration for consuming user events from the Auth Service
- Service-to-service communication

## Technology Stack

- **.NET 9 API**: Latest version of the .NET platform
- **Entity Framework Core**: ORM for database access
- **PostgreSQL**: Relational database for data storage
- **Confluent.Kafka**: Kafka client for .NET
- **Docker**: Containerization for easy deployment

## Getting Started

### Prerequisites

- .NET SDK 9.0 or later
- PostgreSQL
- Kafka (for local development, you can use Docker)
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

3. Set up the database connection string and Kafka configuration in your environment variables or user secrets:

```bash
# For development, you can use user secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=babbly-users;Username=your_username;Password=your_password;"
dotnet user-secrets set "Kafka:BootstrapServers" "localhost:9092"
dotnet user-secrets set "Kafka:UserTopic" "user-events"
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
- `GET /api/users/search?term={searchTerm}` - Search users
- `POST /api/users` - Create a new user
- `POST /api/users/profile` - Create or update user from Auth0 profile
- `PUT /api/users/{id}` - Update a user
- `DELETE /api/users/{id}` - Delete a user
- `GET /api/users/me` - Get current user
- `GET /api/health` - Health check endpoint

## Kafka Integration

The User Service consumes events from the Auth Service via Kafka:

### Event Types

- **UserCreated**: When a new user is registered through Auth0
- **UserUpdated**: When a user's profile is updated

### Kafka Topics

- `user-events`: Topic for user-related events

## Database Schema

### Users Table

```
- id (PK)
- auth0_id (unique)
- username (unique)
- email (unique)
- role
- first_name
- last_name
- created_at
- updated_at
```

### User Extra Data Table

```
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

This will start the user service, a PostgreSQL database container, and Kafka. The user service will be available at http://localhost:5001.

### Configuration

The Docker Compose configuration includes:

- A PostgreSQL database with persistence
- Kafka and Zookeeper for event messaging
- Automatic database migrations
- Environment variable configuration for development

### Environment Variables

You can customize the deployment by setting environment variables:

```bash
# Database configuration
ConnectionStrings__DefaultConnection=Host=postgres;Database=babbly_user_service;Username=postgres;Password=postgres

# Kafka configuration
KAFKA_BOOTSTRAP_SERVERS=kafka:9092
KAFKA_USER_TOPIC=user-events
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
