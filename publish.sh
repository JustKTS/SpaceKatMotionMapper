#!/usr/bin/env bash
set -euo pipefail

# ── Defaults ────────────────────────────────────────────────────────────────
CONFIGURATION="Release"
RUNTIME=""
FRAMEWORK="net10.0"
OUTPUT_DIR=""
AOT=false
DRY_RUN=false
VERBOSE=false

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="${SCRIPT_DIR}/SpaceKatMotionMapper/SpaceKatMotionMapper.csproj"

# ── Colors ─────────────────────────────────────────────────────────────────
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
CYAN='\033[0;36m'
BOLD='\033[1m'
NC='\033[0m'
DIM='\033[2m'

info()    { echo -e "  ${GREEN}→${NC} $*"; }
warn()    { echo -e "  ${YELLOW}⚠${NC} $*" >&2; }
error()   { echo -e "${RED}[ERROR]${NC} $*" >&2; }
success() { echo -e "  ${GREEN}✓${NC} $*"; }

banner() {
    local mode="$1"
    echo -e "${CYAN}════════════════════════════════════════════${NC}"
    echo -e "${CYAN}  SpaceKat Motion Mapper${mode}${NC}"
    echo -e "${CYAN}════════════════════════════════════════════${NC}"
}

# ── Platform detection ─────────────────────────────────────────────────────
detect_rid() {
    local os arch
    case "$(uname -s)" in
        Linux)  os="linux" ;;
        Darwin) os="osx" ;;
        MINGW*|MSYS*|CYGWIN*) os="win" ;;
        *) error "Unsupported OS: $(uname -s)"; exit 1 ;;
    esac
    case "$(uname -m)" in
        x86_64|amd64) arch="x64" ;;
        aarch64|arm64) arch="arm64" ;;
        i686)          arch="x86" ;;
        *) error "Unsupported architecture: $(uname -m)"; exit 1 ;;
    esac
    echo "${os}-${arch}"
}

valid_rids() {
    # Same RIDs for both standard and AOT mode (AOT warns on linux/osx)
    echo "win-x64 win-x86 win-arm64 linux-x64 linux-arm64 osx-x64 osx-arm64"
}

# ── Pre-flight checks ──────────────────────────────────────────────────────
preflight() {
    if ! command -v dotnet &>/dev/null; then
        error "dotnet SDK not found (install from https://dotnet.microsoft.com)"
        exit 1
    fi
    if [[ ! -f "$PROJECT" ]]; then
        error "Project not found: $PROJECT"
        exit 1
    fi
    if [[ "$DRY_RUN" != "true" ]]; then
        return
    fi
    if [[ "$VERBOSE" == "true" ]]; then
        warn "Dry-run mode; commands will be printed but not executed"
    fi
}

# ── Signal handler ─────────────────────────────────────────────────────────
interrupted() {
    echo ""
    warn "Build interrupted"
    exit 130
}
trap interrupted INT TERM

# ── Usage ──────────────────────────────────────────────────────────────────
usage() {
    cat <<'EOF'
Usage: publish.sh [OPTIONS]

Build and publish a self-contained single-file binary.

Options:
  -c, --config <cfg>     Build configuration: Release | Debug (default: Release)
  -r, --runtime <rid>    Target runtime identifier (default: auto-detect)
  -f, --framework <tfm>  Target framework moniker (default: net10.0)
  -o, --output <dir>     Publish output directory (overrides default)

      --aot              Enable NativeAOT compilation
      --dry-run          Print commands without executing
  -v, --verbose          Verbose output
  -h, --help             Show this help

Valid RIDs: win-x64 win-x86 win-arm64 linux-x64 linux-arm64 osx-x64 osx-arm64

Note: --aot on linux/osx is experimental and untested in this project.
      Static SkiaSharp/HarfBuzz libraries are NOT bundled for these platforms.
      The build relies on dynamic P/Invoke resolution at runtime.
EOF
    exit 0
}

# ── Args ───────────────────────────────────────────────────────────────────
OPTS=$(getopt -o c:r:f:o:vh --long config:,runtime:,framework:,output:,aot,dry-run,verbose,help -n 'publish.sh' -- "$@") || exit 1
eval set -- "$OPTS"

while true; do
    case "$1" in
        -c|--config)    CONFIGURATION="$2"; shift 2 ;;
        -r|--runtime)   RUNTIME="$2";      shift 2 ;;
        -f|--framework) FRAMEWORK="$2";    shift 2 ;;
        -o|--output)    OUTPUT_DIR="$2";   shift 2 ;;
        --aot)          AOT=true;          shift   ;;
        --dry-run)      DRY_RUN=true;      shift   ;;
        -v|--verbose)   VERBOSE=true;      shift   ;;
        -h|--help)      usage              ;;
        --) shift; break ;;
        *) error "Unknown option: $1"; usage ;;
    esac
done

# ── Defaults & validation ──────────────────────────────────────────────────
: "${RUNTIME:=$(detect_rid)}"

validate_rid() {
    local candidate="$1" valid
    for valid in $(valid_rids); do
        [[ "$candidate" == "$valid" ]] && return 0
    done
    return 1
}

if [[ "$CONFIGURATION" != "Release" && "$CONFIGURATION" != "Debug" ]]; then
    error "Invalid configuration: $CONFIGURATION (must be Release or Debug)"
    exit 1
fi

if ! validate_rid "$RUNTIME"; then
    error "Invalid runtime: $RUNTIME"
    echo -e "  Valid RIDs: $(valid_rids)" >&2
    exit 1
fi

# ── Output directory ───────────────────────────────────────────────────────
SUFFIX="publish"
if [[ "$AOT" == "true" ]]; then
    SUFFIX="publish_aot"
fi
: "${OUTPUT_DIR:=${PROJECT%/*}/bin/${CONFIGURATION}/${FRAMEWORK}/${RUNTIME}/${SUFFIX}}"

# ── Executable name ────────────────────────────────────────────────────────
if [[ "$RUNTIME" == win-* ]]; then
    EXE_NAME="SpaceKatMotionMapper.exe"
else
    EXE_NAME="SpaceKatMotionMapper"
fi
EXE_PATH="${OUTPUT_DIR}/${EXE_NAME}"

# ── AOT warnings ───────────────────────────────────────────────────────────
if [[ "$AOT" == "true" && "$RUNTIME" != win-* ]]; then
    warn "NativeAOT on $RUNTIME is experimental in this project"
    warn "No static SkiaSharp/HarfBuzz/ANGLE libraries are bundled"
    warn "Runtime relies on dynamic P/Invoke — may not work with Avalonia"
fi

# ── Pre-flight ─────────────────────────────────────────────────────────────
preflight

# ── Configuration summary ──────────────────────────────────────────────────
if [[ "$VERBOSE" == "true" || "$DRY_RUN" == "true" ]]; then
    echo ""
    echo -e "${DIM}Configuration:${NC}"
    echo -e "${DIM}  Project:      ${PROJECT}${NC}"
    echo -e "${DIM}  Config:       ${CONFIGURATION}${NC}"
    echo -e "${DIM}  Runtime:      ${RUNTIME}${NC}"
    echo -e "${DIM}  Framework:    ${FRAMEWORK}${NC}"
    echo -e "${DIM}  Output:       ${OUTPUT_DIR}${NC}"
    echo -e "${DIM}  AOT:          ${AOT}${NC}"
    echo -e "${DIM}  SingleFile:   true${NC}"
    echo -e "${DIM}  Trimmed:      true (full)${NC}"
    echo -e "${DIM}  Compression:  true${NC}"
    echo -e "${DIM}  SelfContained:true${NC}"
    echo ""
fi

# ── Build publish arguments ────────────────────────────────────────────────
PUBLISH_ARGS=(
    publish "$PROJECT"
    -c "$CONFIGURATION"
    -f "$FRAMEWORK"
    -r "$RUNTIME"
    -p:SelfContained=true
    -p:PublishSingleFile=true
    -p:PublishTrimmed=true
    -p:TrimMode=full
    -p:EnableCompressionInSingleFile=true
    -p:IncludeNativeLibrariesForSelfExtract=true
)

if [[ "$AOT" == "true" ]]; then
    PUBLISH_ARGS+=(
        -p:PublishAot=true
        -p:OptimizationPreference=Size
        -p:IlcFoldIdenticalMethodBodies=true
        -p:StackTraceSupport=false
        -p:IlcGenerateMapFile=true
        -p:IlcGenerateMstatFile=true
    )
fi

PUBLISH_ARGS+=(-o "$OUTPUT_DIR")

# ── Restore args ────────────────────────────────────────────────────────────
RESTORE_ARGS=(
    restore "$PROJECT"
    -r "$RUNTIME"
)

# ── Clean args ─────────────────────────────────────────────────────────────
CLEAN_ARGS=(
    clean "$PROJECT"
    -c "$CONFIGURATION"
    -f "$FRAMEWORK"
    -r "$RUNTIME"
)

# ── Dry-run ────────────────────────────────────────────────────────────────
if [[ "$DRY_RUN" == "true" ]]; then
    banner " - Dry Run"
    echo ""
    echo -e "  ${BOLD}\$ dotnet ${RESTORE_ARGS[*]}${NC}"
    echo ""
    echo -e "  ${BOLD}\$ dotnet ${CLEAN_ARGS[*]}${NC}"
    echo ""
    echo -e "  ${BOLD}\$ dotnet ${PUBLISH_ARGS[*]}${NC}"
    echo ""
    exit 0
fi

# ── Main ───────────────────────────────────────────────────────────────────
if [[ "$AOT" == "true" ]]; then
    banner " - AOT Publish"
else
    banner " - Publish"
fi
echo ""
info "Restoring packages..."
if ! dotnet "${RESTORE_ARGS[@]}" 2>&1 | while IFS= read -r line; do
    [[ "$VERBOSE" == "true" ]] && echo "    $line"
    :
done; then
    error "Restore failed"
    exit 1
fi

info "Cleaning..."
if ! dotnet "${CLEAN_ARGS[@]}" 2>&1 | while IFS= read -r line; do
    [[ "$VERBOSE" == "true" ]] && echo "    $line"
    :
done; then
    error "Clean failed"
    exit 1
fi

info "Publishing..."$([[ "$AOT" == "true" ]] && echo " (NativeAOT, this may take several minutes)")

SECONDS=0
if ! dotnet "${PUBLISH_ARGS[@]}" 2>&1 | while IFS= read -r line; do
    [[ "$VERBOSE" == "true" ]] && echo "    $line"
    :
done; then
    echo ""
    error "Publish failed"
    exit 1
fi
ELAPSED=$SECONDS

# ── Result ─────────────────────────────────────────────────────────────────
echo ""
success "Publish succeeded (${ELAPSED}s elapsed)"
echo -e "  Output: ${OUTPUT_DIR}"

if [[ -f "$EXE_PATH" ]]; then
    SIZE=$(stat -c%s "$EXE_PATH" 2>/dev/null || echo 0)
    SIZE_MB=$(awk "BEGIN {printf \"%.2f\", $SIZE/1048576}")
    echo -e "  Binary: ${EXE_NAME} (${SIZE_MB} MB)"
else
    warn "Binary not found at expected path: $EXE_PATH"
fi
