#!/bin/bash

if [ -z "$1" ]; then
    echo "Error: Migration name is required"
    echo "Usage: ./add-migration.sh <MigrationName>"
    exit 1
fi

MIGRATION_NAME=$1

echo "Creating migration: $MIGRATION_NAME"

# Navigate to the project directory
cd "$(dirname "$0")/../babbly-user-service"

# Run the migration command
dotnet ef migrations add $MIGRATION_NAME --output-dir "Migrations"

if [ $? -eq 0 ]; then
    echo "Migration '$MIGRATION_NAME' created successfully!"
else
    echo "Error creating migration. See above for details."
fi 