$api = "backend\src\TaskManagement.Api\TaskManagement.Api.csproj"
$infra = "backend\src\TaskManagement.Infrastructure\TaskManagement.Infrastructure.csproj"

echo "Setup and run application..."

# 1 - Store a random signing key in .NET user-secrets (not in source control).
$secrets = dotnet user-secrets list --project $api
if ($secrets -notmatch '(?m)^Jwt:Key\s*=') {
    $bytes = New-Object byte[] 32
    [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    $key = [Convert]::ToBase64String($bytes)
    dotnet user-secrets set "Jwt:Key" $key --project $api
}
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
echo "Step 1 from 4: Jwt key stored..."

# 2 - Create database
docker compose -f infrastructure\docker\docker-compose.yml up -d
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
echo "Step 2 from 4: Database created..."

# Check if Entity Framework CLI 9.0.4 is installed
$efTool = dotnet tool list -g | Select-String "dotnet-ef"
if (-not $efTool) {
    echo "Installing Entity Framework Core CLI tool..."
    dotnet tool install --global dotnet-ef --version 9.0.4
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
} elseif ($efTool -notmatch "\s9\.0\.4\s") {
    echo "Updating Entity Framework Core CLI tool to 9.0.4..."
    dotnet tool update --global dotnet-ef --version 9.0.4
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

# Clean intermediate build artifacts
Get-ChildItem -Path "backend\src" -Recurse -Directory -Include "bin", "obj" | Remove-Item -Recurse -Force
echo "Removed intermediate build artifacts..."

# 3 - Create/update the PostgreSQL schema using the checked-in EF Core migration.
dotnet ef database update `
    --project $infra `
    --startup-project $api `
    --context TaskManagementDbContext
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
echo "Step 3 from 4: PostgreSQL schema updated..."

# 4 - Run backend app
echo "Step 4 from 4: Run Backend application..."
dotnet run --project $api
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
