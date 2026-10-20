# Smoke check: health -> markets -> orderbook. Exit code 0 means the project is up and working.
$ErrorActionPreference = 'Stop'
$port = if ($env:API_PORT) { $env:API_PORT } else { '8080' }
$base = if ($env:BASE_URL) { $env:BASE_URL } else { "http://localhost:$port" }

function Fail($msg) { Write-Host "FAIL: $msg" -ForegroundColor Red; exit 1 }

Write-Host "[1/3] GET $base/health"
$health = $null
foreach ($i in 1..30) {
    try { $health = Invoke-RestMethod "$base/health"; break } catch { Start-Sleep -Seconds 2 }
}
if (-not $health) { Fail 'service did not respond within 60 seconds' }
if ($health.status -ne 'ok') { Fail "health: $($health | ConvertTo-Json -Compress)" }
Write-Host "      $($health | ConvertTo-Json -Compress)"

Write-Host "[2/3] GET $base/api/v1/markets"
# Windows PowerShell 5.1 returns a JSON array as one object, so wrap it after the call, not around it.
$markets = Invoke-RestMethod "$base/api/v1/markets"
$markets = @($markets)
if ($markets.Count -ne 2) { Fail "expected 2 markets, got $($markets.Count)" }
Write-Host "      $($markets | ConvertTo-Json -Compress)"

Write-Host "[3/3] GET $base/api/v1/markets/BTC/USDT/orderbook"
$book = Invoke-RestMethod "$base/api/v1/markets/BTC/USDT/orderbook"
if ($book.market -ne 'BTC/USDT' -or $null -eq $book.PSObject.Properties['bids'] -or $null -eq $book.PSObject.Properties['asks']) { Fail 'response has no bids/asks lists' }
Write-Host "      $($book | ConvertTo-Json -Compress)"

Write-Host 'OK: the project is up and responding'
