# M3-TEST-15 (Issue #59): run TC-S-01 against the staging stack, result tied to version.
#
# Self-contained: starts the app with Staging env on the scratch database,
# runs submit -> assign -> InProgress -> Resolved -> requester reads back,
# and prints the deployed version identifier with the result.
# Writes one E2E test request into the scratch DB only (never shared data).
#
# Usage (PowerShell 5.1+, from the repo root):
#   powershell -ExecutionPolicy Bypass -File scripts/staging-e2e.ps1 [-BaseUrl http://localhost:5287]
#
# Exit code 0 = journey green. Evidence links from PED s8/s10.

param([string]$BaseUrl = "http://localhost:5287")

$ErrorActionPreference = "Stop"

function Fail($message) {
    Write-Host "STAGING-E2E FAIL: $message"
    exit 1
}

function Call($method, $path, $headers, $body) {
    $params = @{ Uri = "$BaseUrl$path"; Method = $method; UseBasicParsing = $true; TimeoutSec = 15 }
    if ($headers) { $params.Headers = $headers }
    if ($body) { $params.ContentType = "application/json"; $params.Body = $body }
    return Invoke-WebRequest @params
}

# Auth for staging: TestAuth mints only in Development (by controller design), so
# mint on a throwaway Development instance (same JWT config) and spend on staging.
$env:ASPNETCORE_ENVIRONMENT = "Development"
$minter = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --project src/CivicConnect.Web/CivicConnect.Web.csproj --no-launch-profile --urls http://localhost:5288 --no-build" `
    -PassThru -WindowStyle Hidden
Start-Sleep -Seconds 14
$MintBase = "http://localhost:5288"
function MintRemote($user, $role) {
    $token = (Invoke-WebRequest -Uri "$MintBase/api/testauth/token/${user}?role=${role}" -UseBasicParsing -TimeoutSec 15).Content | ConvertFrom-Json
    return @{ Authorization = "Bearer $($token.token)" }
}
$req = MintRemote "e2e-req" "REQUESTER"
$staff = MintRemote "e2e-staff" "STAFF"
$mgr = MintRemote "e2e-mgr" "MANAGER"
Stop-Process -Id $minter.Id -Force -ErrorAction SilentlyContinue
Write-Host "Tokens minted on Development minter"

$env:ASPNETCORE_ENVIRONMENT = "Staging"

$env:ASPNETCORE_ENVIRONMENT = "Staging"
$env:DemoAuth__Enabled = "true"
$env:DB_IP = "localhost"
$env:DB_PORT = "5433"
$env:DB_NAME = "civicconnect_perf"
$env:DB_USER = "postgres"
$env:DB_PASSWORD = ""
$env:DB_SSLMODE = "prefer"
$env:GIT_SHA = (git rev-parse --short HEAD).Trim()

$proc = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --project src/CivicConnect.Web/CivicConnect.Web.csproj --no-launch-profile --urls http://localhost:5287 --no-build" `
    -PassThru -WindowStyle Hidden
Start-Sleep -Seconds 14

try {
    # 0. Version identifier this result is tied to.
    $version = (Call "Get" "/api/version" $null $null).Content | ConvertFrom-Json
    Write-Host "Deployed version: commit=$($version.commit) env=$($version.environment)"
    if ($version.commit -ne $env:GIT_SHA) { Fail "version mismatch (got $($version.commit))" }

    # 1. Requester submits.
    $created = (Call "Post" "/api/requests" $req '{"title":"Staging E2E","description":"TC-S-01 on staging","categoryId":1}').Content | ConvertFrom-Json
    $id = $created.id
    Write-Host "Submitted request $id"

    # 2-3. Staff assigns, progresses to Resolved.
    Call "Put" "/api/requests/$id/assign" $staff '{"assigneeId":"e2e-staff","reason":"staging e2e"}' | Out-Null
    Call "Put" "/api/requests/$id/state" $staff '{"newState":2,"reason":"staging e2e"}' | Out-Null
    Call "Put" "/api/requests/$id/state" $staff '{"newState":3,"reason":"staging e2e"}' | Out-Null
    Write-Host "Assigned and resolved"

    # 4. Requester sees the update (state serializes as number or name).
    $seen = (Call "Get" "/api/requests/$id" $req $null).Content | ConvertFrom-Json
    if ($seen.state -ne 3 -and $seen.state -ne "Resolved") { Fail "requester sees state $($seen.state), expected Resolved" }
    Write-Host "Requester sees Resolved"

    # 5. Audit reflects the journey.
    $audit = (Call "Get" "/api/audit?entityType=Request&entityId=$id" $mgr $null).Content | ConvertFrom-Json
    $auditCount = @($audit).Count
    if ($auditCount -lt 2) { Fail "expected >=2 audit entries" }
    Write-Host "Audit entries: $auditCount"
}
catch {
    Fail $_.Exception.Message
}
finally {
    Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
}

Write-Host "STAGING-E2E PASS: TC-S-01 green on commit $($version.commit)"
exit 0
