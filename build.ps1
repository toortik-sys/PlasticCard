param(
    [Parameter(Mandatory = $true)][string]$UnityEditor,
    [string]$SptRuntimePath = 'C:\SPT\SPT_Runtime'
)
$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$unityProject = Join-Path $projectRoot 'UnityAssetBundleProject'
$log = Join-Path $projectRoot 'unity-build.log'
$process = Start-Process -FilePath $UnityEditor -ArgumentList @('-batchmode', '-quit', '-projectPath', ('"' + $unityProject + '"'), '-executeMethod', 'PlasticCardEuroSetup.PrepareAndBuild', '-logFile', ('"' + $log + '"')) -WindowStyle Hidden -Wait -PassThru
if ($process.ExitCode -ne 0) { throw "Unity build failed. See $log" }
foreach ($pair in @(@('roubles','rouble'), @('dollars','dollar'), @('euros','euro'))) {
    $currency = $pair[0]
    $name = $pair[1] + '_magnet_card.bundle'
    $destination = Join-Path $projectRoot "bundles/Mods/$currency"
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $unityProject "BuildOutput/Mods/$currency/$name") -Destination $destination -Force
}
& dotnet build (Join-Path $projectRoot 'PlasticCard.csproj') -c Release "-p:SptRuntimePath=$SptRuntimePath"
if ($LASTEXITCODE -ne 0) { throw 'Server plugin build failed.' }
$release = Join-Path $projectRoot 'dist/PlasticCard'
New-Item -ItemType Directory -Path $release -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $projectRoot 'bin/Release/PlasticCard/PlasticCard.dll'), (Join-Path $projectRoot 'bundles.json') -Destination $release -Force
Copy-Item -LiteralPath (Join-Path $projectRoot 'bundles') -Destination $release -Recurse -Force
Write-Host "Release files: $release"
