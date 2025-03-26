param (
    [Parameter(Mandatory = $true)]
    [string]$MigrationName
)

Write-Host "Creating migration: $MigrationName"

# Navigate to the project directory
cd "$PSScriptRoot/../babbly-user-service"

# Run the migration command
dotnet ef migrations add $MigrationName --output-dir "Migrations"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migration '$MigrationName' created successfully!"
}
else {
    Write-Host "Error creating migration. See above for details."
} 