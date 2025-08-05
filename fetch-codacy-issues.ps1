# Define variables
$apiToken = "pW2zW55yl6tj1tzNLAjQ"
$organization = "julerobb1"
$repository = "Processor-Emulator"
$baseUrl = "https://api.codacy.com/2.0"

# Fetch issues for the repository
$response = Invoke-RestMethod -Uri "$baseUrl/organizations/$organization/repositories/$repository/issues" `
    -Headers @{ "Authorization" = "Bearer $apiToken" }

# Check if there are issues
if ($response.issues.Count -gt 0) {
    Write-Output "Issues detected:"
    $response.issues | ForEach-Object {
        Write-Output "Issue ID: $_.id"
        Write-Output "Description: $_.description"
        Write-Output "Severity: $_.severity"
        Write-Output "-----------------------------"
    }
} else {
    Write-Output "No issues detected."
}
