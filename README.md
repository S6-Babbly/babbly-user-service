# Babbly User Service

The user management microservice for the Babbly platform, handling user profiles, registration, and consuming authentication events from Kafka.

## Tech Stack

- **Backend**: ASP.NET Core 9.0
- **ORM**: Entity Framework Core
- **Database**: PostgreSQL
- **Message Broker**: Kafka (Confluent.Kafka client)
- **Authentication**: Auth0 integration

## Features

- User profile management (CRUD operations)
- User search functionality
- Kafka event consumption from Auth Service
- Service-to-service communication
- User data persistence in PostgreSQL

## Local Development Setup

### Prerequisites

- .NET SDK 9.0 or later
- PostgreSQL 14+
- Apache Kafka (or Docker Compose)

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

3. Configure the database connection and Kafka:
   ```bash
   # Using user secrets for development
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=babbly-users;Username=postgres;Password=your_password"
   dotnet user-secrets set "Kafka:BootstrapServers" "localhost:9092"
   dotnet user-secrets set "Kafka:UserTopic" "user-events"
   ```

4. Run database migrations:
   ```bash
   dotnet ef database update --project babbly-user-service
   ```

5. Run the service:
   ```bash
   dotnet run --project babbly-user-service/babbly-user-service.csproj
   ```

The API will be available at `http://localhost:8081`.

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | - |
| `KAFKA_BOOTSTRAP_SERVERS` | Kafka broker addresses | `localhost:9092` |
| `KAFKA_USER_TOPIC` | Kafka topic for user events | `user-events` |

## API Endpoints

### User Management
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/auth0/{auth0Id}` - Get user by Auth0 ID
- `GET /api/users/search?term={searchTerm}` - Search users
- `POST /api/users` - Create a new user
- `PUT /api/users/{id}` - Update a user
- `DELETE /api/users/{id}` - Delete a user
- `GET /api/users/me` - Get current authenticated user

### Profile Management
- `POST /api/users/profile` - Create or update user profile from Auth0 data

### Health Check
- `GET /api/health` - Service health check

## Database Schema

### Users Table
- `id` (PK) - Integer primary key
- `auth0_id` - Unique Auth0 identifier
- `username` - Unique username
- `email` - Unique email address
- `role` - User role (user, admin, etc.)
- `first_name` - User's first name
- `last_name` - User's last name
- `created_at` - Account creation timestamp
- `updated_at` - Last update timestamp

### User Extra Data Table
- `id` (PK) - Integer primary key
- `user_id` (FK) - Foreign key to Users table
- `display_name` - Display name for UI
- `profile_picture` - URL to profile picture
- `bio` - User biography
- `address` - Physical address
- `phone_number` - Contact phone number
- `created_at` - Record creation timestamp
- `updated_at` - Last update timestamp

## Docker Support

Run the service with Docker Compose:

```bash
# From the root of the Babbly organization
docker-compose up -d user-service
```

Or run with its own Docker Compose (includes PostgreSQL and Kafka):

```bash
# From the babbly-user-service directory
docker-compose up -d
```

The service will be available at `http://localhost:8081`.

## Architecture Notes

### Kafka Integration

The User Service consumes events from the Auth Service via Kafka:

**Event Types:**
- `UserCreated` - When a new user registers through Auth0
- `UserUpdated` - When a user's profile information is updated

**Topic:** `user-events`

The service automatically creates or updates user records based on these events, ensuring user data is synchronized across the system.

### Integration with Babbly Ecosystem

- **Auth Service**: Publishes user authentication events that this service consumes
- **API Gateway**: Routes user-related requests to this service
- **Other Services**: Query this service for user information via the API Gateway

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
