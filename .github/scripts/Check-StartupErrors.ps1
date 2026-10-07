param(
    [Parameter(Mandatory=$true)]
    [string]$LogPath
)

Write-Host "Checking startup logs at: $LogPath"

# The app logs via Serilog to its own JSON trace log file, not bare console/stdout. Filename
# includes the runner's hostname, so glob.
$traceLogFiles = Get-ChildItem -Path $LogPath -Filter "UmbracoTraceLog*.json" -ErrorAction SilentlyContinue

if (-not $traceLogFiles) {
    Write-Host "::error::No Umbraco trace log found at $LogPath"
    exit 1
}

$logs = $traceLogFiles | Get-Content -ErrorAction SilentlyContinue

if (-not $logs) {
    Write-Host "::error::Trace log found at $LogPath but could not be read or was empty"
    exit 1
}

$umbracoInstallSuccess = $logs | Select-String -Pattern "Unattended install completed|Unattended upgrade completed successfully" -CaseSensitive:$false

# Only flag errors between install starting and the app shutting down - CI's own "Stop Project"
# step triggers an expected TaskCanceledException (Examine index rebuild cancelled) logged at
# Error level, which isn't a real startup problem.
$startIndex = 0
$endIndex = $logs.Count - 1
for ($i = 0; $i -lt $logs.Count; $i++) {
    if ($logs[$i] -match "Starting unattended install") {
        $startIndex = $i
        break
    }
}
for ($i = $logs.Count - 1; $i -ge 0; $i--) {
    if ($logs[$i] -match "Application is shutting down") {
        $endIndex = $i - 1
        break
    }
}

$startupLogs = if ($endIndex -ge $startIndex) { $logs[$startIndex..$endIndex] } else { @() }
$errors = $startupLogs | Select-String -Pattern '"Log4NetLevel":"(ERROR|FATAL)"' -CaseSensitive:$false

if ($errors) {
    Write-Host "::error::Errors detected during startup:"
    $errors | ForEach-Object { Write-Host $_ }
    exit 1
}

if ($umbracoInstallSuccess) {
    Write-Host "✓ Umbraco install/upgrade completed"
} else {
    Write-Host "::warning::Could not verify unattended install/upgrade completed"
}

exit 0
