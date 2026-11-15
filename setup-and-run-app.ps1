$root = $PSScriptRoot
$api = Join-Path $root "backend\src\TaskManagement.Api\TaskManagement.Api.csproj"
$infra = Join-Path $root "backend\src\TaskManagement.Infrastructure\TaskManagement.Infrastructure.csproj"
$frontend = Join-Path $root "frontend\task-management-ui"
Set-Location $root

echo "Setup and run application..."

# 1 - Store a random signing key in .NET user-secrets
$secrets = dotnet user-secrets list --project $api
if ($secrets -notmatch '(?m)^Jwt:Key\s*=') {
    $bytes = New-Object byte[] 32
    [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    $key = [Convert]::ToBase64String($bytes)
    dotnet user-secrets set "Jwt:Key" $key --project $api
}
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
echo "Step 1 from 5: Jwt key stored..."

# 2 - Create database
docker compose -f infrastructure\docker\docker-compose.yml up -d
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
echo "Step 2 from 5: Database created..."

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


# 3 - Create/update the PostgreSQL schema
dotnet ef database update `
    --project $infra `
    --startup-project $api `
    --context TaskManagementDbContext
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
echo "Step 3 from 5: PostgreSQL schema updated..."

# 4 - Install frontend dependencies
npm install --prefix $frontend
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
echo "Step 4 from 5: Frontend dependencies installed..."

# Start both applications in separate terminal windows
echo "Step 5 from 5: Starting backend and frontend applications..."
Start-Process -FilePath "dotnet" -ArgumentList "run --project `"$api`"" -WorkingDirectory $root
Start-Process -FilePath "cmd.exe" -ArgumentList "/k npm start" -WorkingDirectory $frontend
