# M3-STG-06 (Issue #80): interim staging deploy + smoke test.
#
# Deploys the current tree as the staging stack (dotnet run + PostgreSQL,
# per the M3-STG-01 interim: compose lands in M3-STG-03) and proves it:
#   1. /api/version commit equals the expected tag/SHA.
#   2. /health is green.
#   3. One authenticated read endpoint works (dashboard as MANAGER).
#
# Read-only against the database (no writes, no migrations, no seeds).
# TestAuth mints only in Development (controller design), so tokens come from a
# throwaway Development minter (same JWT config); staging config default stays off.
#
# Usage (PowerShell 5.1+, from the repo root):
#   powershell -ExecutionPolicy Bypass -File scripts/staging-smoke.ps1 [-BaseUrl http://localhost:5287]
#
# Exit code 0 = all checks pass. Evidence (this log) links from PED s8/s10.

param([string]$BaseUrl = "http://localhost:5287")

$ErrorActionPreference = "Stop"

function Fail($message) {
    Write-Host "SMOKE FAIL: $message"
    exit 1
}

function FailFinish($message, $ids) {
    foreach ($id in $ids) { Stop-Process -Id $id -Force -ErrorAction SilentlyContinue }
    Fail $message
}

$expectedSha = (git rev-parse --short HEAD).Trim()
if (-not $expectedSha) { Fail "could not determine HEAD SHA" }
Write-Host "Expected commit: $expectedSha"

$env:ASPNETCORE_ENVIRONMENT = "Development"
$minter = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --project src/CivicConnect.Web/CivicConnect.Web.csproj --no-launch-profile --urls http://localhost:5288 --no-build" `
    -PassThru -WindowStyle Hidden
Start-Sleep -Seconds 14
try {
    $token = (Invoke-WebRequest -Uri "http://localhost:5288/api/testauth/token/smoke?role=MANAGER" -UseBasicParsing -TimeoutSec 15).Content | ConvertFrom-Json
    $auth = @{ Authorization = "Bearer $($token.token)" }
    Write-Host "Token minted on Development minter"
}
catch {
    FailFinish "token mint failed: $($_.Exception.Message)" @($minter.Id)
}

$env:ASPNETCORE_ENVIRONMENT = "Staging"
$env:GIT_SHA = $expectedSha

$proc = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --project src/CivicConnect.Web/CivicConnect.Web.csproj --no-launch-profile --urls http://localhost:5287 --no-build" `
    -PassThru -WindowStyle Hidden
Start-Sleep -Seconds 14

try {
    # 1. Version equals the tag under test.
    $version = Invoke-WebRequest -Uri "$BaseUrl/api/version" -UseBasicParsing -TimeoutSec 10
    $body = $version.Content | ConvertFrom-Json
    Write-Host "Version endpoint: $($version.StatusCode) commit=$($body.commit)"
    if ($body.commit -ne $expectedSha) { FailFinish "commit mismatch (got $($body.commit))" @($proc.Id, $minter.Id) }

    # 2. Health is green.
    $health = Invoke-WebRequest -Uri "$BaseUrl/health" -UseBasicParsing -TimeoutSec 10
    Write-Host "Health endpoint: $($health.StatusCode) $($health.Content)"
    if ($health.StatusCode -ne 200) { FailFinish "health not green" @($proc.Id, $minter.Id) }

    # 3. Authenticated read works (MANAGER dashboard).
    $dashboard = Invoke-WebRequest -Uri "$BaseUrl/api/reports/requests-summary" -UseBasicParsing -TimeoutSec 10 -Headers $auth
    Write-Host "Dashboard endpoint: $($dashboard.StatusCode) $($dashboard.Content)"
    if ($dashboard.StatusCode -ne 200) { FailFinish "dashboard read failed" @($proc.Id, $minter.Id) }
}
catch {
    FailFinish $_.Exception.Message @($proc.Id, $minter.Id)
}
finally {
    Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    Stop-Process -Id $minter.Id -Force -ErrorAction SilentlyContinue
}

Write-Host "SMOKE PASS: version, health and dashboard read all green on $BaseUrl"
exit 0
