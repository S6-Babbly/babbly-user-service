Write-Host "Setting up initial migration..."

# Navigate to the project directory
cd "$PSScriptRoot/../babbly-user-service"

# Run the migration command
dotnet ef migrations add "InitialCreate" --output-dir "Migrations"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Initial migration created successfully!"
    Write-Host "To apply this migration, start the application or run: dotnet ef database update"
}
else {
    Write-Host "Error creating migration. See above for details."
} 