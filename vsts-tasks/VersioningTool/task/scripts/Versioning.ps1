function GetVersion(){
	$versionFile = GetVersionFile
	$versionFileText = [IO.File]::ReadAllText($versionFile)
	$v = $versionFileText | ConvertFrom-Json
    return $v.Major.ToString() + "." + $v.Minor.ToString() + "." + $v.Patch.ToString()
}

function GetVersionFile(){
	$filesFound = Get-ChildItem -Path "**\.version" -Recurse

	if ($filesFound.Count -eq 0)
	{
		Write-Error ("No files matching pattern found.")
	}

	if ($filesFound.Count -gt 1)
	{
	   Write-Error ("Multiple version files found.")
	}

	return	$filesFound[0];
}

$version = GetVersion;
$url = "$($env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI)$env:SYSTEM_TEAMPROJECTID/_apis/build/builds?definitions=1&buildNumber=$version%" 
Write-Host "URL: $url"
$result = Invoke-RestMethod -Uri $url -Headers @{
    Authorization = "Bearer $env:SYSTEM_ACCESSTOKEN"
}
$builds = $result | ConvertTo-Json 
$revision = $builds.count + 1

