param([string]$ScriptPath,
      [string]$Server,
      [string]$Database,
      [string]$User,
      [string]$Password)

function Get-TempDir {
    return $env:LOCALAPPDATA
}

function Install-DevToolsAndGetExePath {
    $workingDir = Join-Path (Get-TempDir) 'DatabaseMigration'
    $dllFilePattern = Join-Path $workingDir 'Fig.Cli.*\lib\net452\dev.exe'

    if (-not (Test-Path $workingDir)) {
        New-Item $workingDir -ItemType Directory -Force | Out-Null
    }

    if (-not (Test-Path $dllFilePattern)) {
        $oldLocation = Get-Location
        try {
            Set-Location $workingDir
            
            # Check if nuget.exe is already in the path and use that
            $nuget = Get-Command ".\nuget.exe" -ErrorAction SilentlyContinue
            if ($nuget -eq $null) {
                # nuget.exe not in path, download it
                Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile nuget.exe -UseBasicParsing
                $nuget = ".\nuget.exe"
            }

            & $nuget install Fig.Cli -source https://srv-source/tfs/UnicusCollection/_packaging/Unicus/nuget/v3/index.json

            # if (Test-Path .\nuget.exe) {
            #     Remove-Item .\nuget.exe
            # }
        }
        finally {
            Set-Location $oldLocation
        }
    }
    return Resolve-Path $dllFilePattern | Select-Object -ExpandProperty Path -First 1
}

Write-Host "Runing scripts..."
Write-Host "Scripts path: $ScriptPath"
Write-Host "Server: $Server"
Write-Host "Database: $Database"

$devToolsPath = Install-DevToolsAndGetExePath
Write-Host "Dev Tools path: $devToolsPath"

& $devToolsPath runscripts --server $Server --user $User --password $Password --db $Database --directory $ScriptPath --supressconfirm --ignoredirectory

if ($LastExitCode -ne 0) {
    throw "Command failed with exit code $LastExitCode."
 }
