#!/bin/bash

echo "Setting up initial migration..."

# Navigate to the project directory
cd "$(dirname "$0")/../babbly-user-service"

# Run the migration command
dotnet ef migrations add "InitialCreate" --output-dir "Migrations"

if [ $? -eq 0 ]; then
    echo "Initial migration created successfully!"
    echo "To apply this migration, start the application or run: dotnet ef database update"
else
    echo "Error creating migration. See above for details."
fi 