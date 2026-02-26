param(
    [string]$OwnerRepo = "NeilKetting/OrangeCircleConstruction",
    [int]$KeepCount = 5
)

$token = Read-Host "Enter your GitHub Personal Access Token (PAT) with 'repo' scope" -AsSecureString
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($token)
$plainToken = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

$headers = @{
    "Authorization" = "token $plainToken"
    "Accept"        = "application/vnd.github.v3+json"
}

Write-Host "Fetching releases for $OwnerRepo..." -ForegroundColor Cyan

$releases = @()
$page = 1
do {
    $url = "https://api.github.com/repos/$OwnerRepo/releases?per_page=100&page=$page"
    try {
        $response = Invoke-RestMethod -Uri $url -Headers $headers -Method Get -ErrorAction Stop
        $releases += $response
        $page++
    }
    catch {
        Write-Host "Error fetching releases: $_" -ForegroundColor Red
        exit
    }
} while ($response.Count -eq 100)

Write-Host "Found $($releases.Count) total releases." -ForegroundColor Green

if ($releases.Count -le $KeepCount) {
    Write-Host "Keeping $KeepCount releases. Nothing to delete." -ForegroundColor Yellow
    exit
}

# Sort by created_at descending (newest first)
$releasesToKeep = $releases | Sort-Object created_at -Descending | Select-Object -First $KeepCount
$releasesToDelete = $releases | Sort-Object created_at -Descending | Select-Object -Skip $KeepCount

Write-Host "`nKeeping the $KeepCount most recent releases." -ForegroundColor Cyan
Write-Host "The following $($releasesToDelete.Count) releases will be DELETED:" -ForegroundColor Red
foreach ($r in $releasesToDelete) {
    Write-Host " - $($r.name) ($($r.tag_name)) created at $($r.created_at)"
}

$confirm = Read-Host "`nAre you sure you want to safely delete these $($releasesToDelete.Count) older releases AND their tags? (Y/N)"
if ($confirm -ne 'Y' -and $confirm -ne 'y') {
    Write-Host "Cleanup aborted by user." -ForegroundColor Yellow
    exit
}

foreach ($r in $releasesToDelete) {
    Write-Host "Deleting release $($r.tag_name)..." -NoNewline
    $delUrl = "https://api.github.com/repos/$OwnerRepo/releases/$($r.id)"
    try {
        Invoke-RestMethod -Uri $delUrl -Headers $headers -Method Delete
        Write-Host " [DONE]" -ForegroundColor Green
    }
    catch {
        Write-Host " [FAILED]" -ForegroundColor Red
    }
    
    Write-Host "Deleting tag $($r.tag_name)..." -NoNewline
    $tagUrl = "https://api.github.com/repos/$OwnerRepo/git/refs/tags/$($r.tag_name)"
    try {
        Invoke-RestMethod -Uri $tagUrl -Headers $headers -Method Delete
        Write-Host " [DONE]" -ForegroundColor Green
    }
    catch {
        Write-Host " [FAILED/SKIP]" -ForegroundColor Yellow
    }
}

Write-Host "`nCleanup complete!" -ForegroundColor Cyan
