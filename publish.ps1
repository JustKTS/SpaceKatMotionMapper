param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",

    [ValidateSet("win-x64", "win-x86", "win-arm64", "linux-x64", "linux-arm64", "osx-x64", "osx-arm64")]
    [string]$Runtime = "win-x64",

    [string]$TargetFramework = "net10.0",

    [string]$OutputDir = "",

    [switch]$Aot,

    [switch]$NoSingleFile
)

$ErrorActionPreference = "Stop"

$slnDir = $PSScriptRoot
$projectPath = Join-Path $slnDir "SpaceKatMotionMapper\SpaceKatMotionMapper.csproj"

$publishArgs = @(
    "publish", $projectPath,
    "-c", $Configuration,
    "-f", $TargetFramework,
    "-r", $Runtime,
    "-p:SelfContained=true"
)

if (-not $NoSingleFile) {
    $publishArgs += @(
        "-p:PublishSingleFile=true",
        "-p:PublishTrimmed=true",
        "-p:TrimMode=full",
        "-p:EnableCompressionInSingleFile=true",
        "-p:IncludeNativeLibrariesForSelfExtract=true"
    )
}

if ($Aot) {
    $publishArgs += @(
        "-p:PublishAot=true",
        "-p:OptimizationPreference=Size",
        "-p:IlcFoldIdenticalMethodBodies=true",
        "-p:StackTraceSupport=false",
        "-p:IlcGenerateMapFile=true",
        "-p:IlcGenerateMstatFile=true"
    )
}

if (-not $OutputDir) {
    $suffix = if ($Aot) { "publish_aot" } else { "publish" }
    $OutputDir = Join-Path $slnDir "SpaceKatMotionMapper\bin\$Configuration\$TargetFramework\$Runtime\$suffix"
}
$publishArgs += "-o", $OutputDir

if ($Aot -and $Runtime -notmatch '^win-') {
    Write-Host "  WARNING: NativeAOT on $Runtime is experimental in this project" -ForegroundColor Yellow
    Write-Host "  No static SkiaSharp/HarfBuzz/ANGLE libraries are bundled" -ForegroundColor Yellow
    Write-Host ""
}

$modeLabel = if ($Aot) { "AOT Publish" } else { "Publish" }
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  SpaceKat Motion Mapper - $modeLabel" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Configuration  : $Configuration"
Write-Host "Runtime        : $Runtime"
Write-Host "Framework      : $TargetFramework"
Write-Host "SelfContained  : true"
if ($NoSingleFile) {
    Write-Host "SingleFile     : false"
} else {
    Write-Host "SingleFile     : true"
    Write-Host "Compression    : true"
    Write-Host "TrimMode       : full"
}
if ($Aot) {
    Write-Host "AOT            : true"
    Write-Host "OptPreference  : Size"
}
Write-Host "Output         : $OutputDir"
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "[1/2] Cleaning..." -ForegroundColor Yellow
dotnet clean $projectPath -c $Configuration -f $TargetFramework -r $Runtime 2>&1 | Out-Null

if ($Aot) {
    Write-Host "[2/2] Publishing (NativeAOT, this may take several minutes)..." -ForegroundColor Yellow
} else {
    Write-Host "[2/2] Publishing..." -ForegroundColor Yellow
}

$sw = [System.Diagnostics.Stopwatch]::StartNew()
dotnet @publishArgs
$sw.Stop()

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "$modeLabel succeeded! ($($sw.Elapsed.ToString('mm\:ss')) elapsed)" -ForegroundColor Green
    Write-Host "Output: $OutputDir" -ForegroundColor Green

    $exe = Join-Path $OutputDir "SpaceKatMotionMapper.exe"
    if (Test-Path $exe) {
        $size = [math]::Round((Get-Item $exe).Length / 1MB, 2)
        Write-Host "Executable: SpaceKatMotionMapper.exe ($size MB)" -ForegroundColor Green
    }
} else {
    Write-Host ""
    Write-Host "$modeLabel failed with exit code: $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}
