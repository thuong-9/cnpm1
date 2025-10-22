$handler = New-Object System.Net.Http.HttpClientHandler
$handler.UseCookies = $true
$handler.CookieContainer = New-Object System.Net.CookieContainer
$client = New-Object System.Net.Http.HttpClient($handler)

$uri = 'http://localhost:5015/Admin/Register'
Write-Host "GET $uri"
$get = $client.GetAsync($uri).Result
$html = $get.Content.ReadAsStringAsync().Result

$matches = [regex]::Matches($html,'name="__RequestVerificationToken"[^>]*value="([^"]+)"')
if ($matches.Count -gt 0) {
    $token = $matches[0].Groups[1].Value
} else {
    Write-Host "No antiforgery token found in page.";
    exit 1
}
Write-Host "Token: $token"

$form = @{
    '__RequestVerificationToken' = $token
    'UserName' = 'testuser'
    'Email' = 'test@example.com'
    'Password' = 'Pass123'
    'IsActive' = 'true'
    'terms' = 'on'
}

$content = New-Object System.Net.Http.FormUrlEncodedContent($form)
$postUri = 'http://localhost:5015/Admin/Register/Index'
Write-Host "POST $postUri"
$post = $client.PostAsync($postUri, $content).Result
Write-Host "Status: $($post.StatusCode)"
$body = $post.Content.ReadAsStringAsync().Result
Write-Host "Response body length: $($body.Length)"
Write-Host $body
