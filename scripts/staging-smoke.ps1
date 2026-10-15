# M3-STG-06 (Issue #80): interim staging deploy + smoke test.
#
# Deploys the current tree as the staging stack (dotnet run + PostgreSQL,
# per the M3-STG-01 interim: compose lands in M3-STG-03) and proves it:
#   1. /api/version commit equals the expected tag/SHA.
#   2. /health is green.
#   3. One authenticated read endpoint works (dashboard as MANAGER).
#
# Read-only against the database (no writes, no migrations, no seeds).
# DemoAuth is opted in for this run only (DemoAuth__Enabled=true) so the smoke
# can mint a token; staging config default stays off.
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

$expectedSha = (git rev-parse --short HEAD).Trim()
if (-not $expectedSha) { Fail "could not determine HEAD SHA" }
Write-Host "Expected commit: $expectedSha"

$env:ASPNETCORE_ENVIRONMENT = "Staging"
$env:DemoAuth__Enabled = "true"
$env:GIT_SHA = $expectedSha

$proc = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --project src/CivicConnect.Web/CivicConnect.Web.csproj --launch-profile http --no-build" `
    -PassThru -WindowStyle Hidden
Start-Sleep -Seconds 14

try {
    # 1. Version equals the tag under test.
    $version = Invoke-WebRequest -Uri "$BaseUrl/api/version" -UseBasicParsing -TimeoutSec 10
    $body = $version.Content | ConvertFrom-Json
    Write-Host "Version endpoint: $($version.StatusCode) commit=$($body.commit)"
    if ($body.commit -ne $expectedSha) { Fail "commit mismatch (got $($body.commit))" }

    # 2. Health is green.
    $health = Invoke-WebRequest -Uri "$BaseUrl/health" -UseBasicParsing -TimeoutSec 10
    Write-Host "Health endpoint: $($health.StatusCode) $($health.Content)"
    if ($health.StatusCode -ne 200) { Fail "health not green" }

    # 3. Authenticated read works (MANAGER dashboard).
    $token = (Invoke-WebRequest -Uri "$BaseUrl/api/testauth/token/smoke?role=MANAGER" -UseBasicParsing -TimeoutSec 10).Content | ConvertFrom-Json
    $dashboard = Invoke-WebRequest -Uri "$BaseUrl/api/reports/requests-summary" -UseBasicParsing -TimeoutSec 10 -Headers @{ Authorization = "Bearer $($token.token)" }
    Write-Host "Dashboard endpoint: $($dashboard.StatusCode) $($dashboard.Content)"
    if ($dashboard.StatusCode -ne 200) { Fail "dashboard read failed" }
}
catch {
    Fail $_.Exception.Message
}
finally {
    Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
}

Write-Host "SMOKE PASS: version, health and dashboard read all green on $BaseUrl"
exit 0
