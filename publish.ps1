param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",

    [ValidateSet("win-x64", "win-x86", "win-arm64", "linux-x64", "linux-arm64", "osx-x64", "osx-arm64")]
    [string]$Runtime = "win-x64",

    [string]$TargetFramework = "net10.0",

    [string]$OutputDir = ""
)

$ErrorActionPreference = "Stop"

$slnDir = $PSScriptRoot
$projectPath = Join-Path $slnDir "SpaceKatMotionMapper\SpaceKatMotionMapper.csproj"

$publishArgs = @(
    "publish", $projectPath,
    "-c", $Configuration,
    "-f", $TargetFramework,
    "-r", $Runtime,
    "-p:SelfContained=true",
    "-p:PublishSingleFile=true",
    "-p:EnableCompressionInSingleFile=true",
    "-p:IncludeNativeLibrariesForSelfExtract=true",
    "-p:PublishTrimmed=true",
    "-p:TrimMode=full"
)

if ($OutputDir) {
    $publishArgs += "-o", $OutputDir
}

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  SpaceKat Motion Mapper - Publish" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Configuration  : $Configuration"
Write-Host "Runtime        : $Runtime"
Write-Host "Framework      : $TargetFramework"
Write-Host "SelfContained  : true"
Write-Host "SingleFile     : true"
Write-Host "Compression    : true"
Write-Host "TrimMode       : full"
if ($OutputDir) {
    Write-Host "Output         : $OutputDir"
}
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/2] Cleaning..." -ForegroundColor Yellow
dotnet clean $projectPath -c $Configuration -f $TargetFramework -r $Runtime 2>&1 | Out-Null

Write-Host "[2/2] Publishing..." -ForegroundColor Yellow
dotnet @publishArgs

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Publish succeeded!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "Publish failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}
