#!/bin/bash
set -e

# This script builds Godot as a library
# Reference: https://github.com/godotengine/godot/pull/110863

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GODOT_DIR="$SCRIPT_DIR/godot"
BUILD_DIR="$SCRIPT_DIR/build"

echo "Building Godot as a library..."

# Check if godot directory exists
if [ ! -d "$GODOT_DIR" ]; then
    echo "Cloning Godot repository..."
    git clone --depth 1 --branch master https://github.com/godotengine/godot.git "$GODOT_DIR"
fi

cd "$GODOT_DIR"

# Ensure we're on a version that supports library build
echo "Checking Godot version..."
git fetch --depth 1 origin master
git checkout master

# Build Godot as a shared library
# The library build support was added in PR #110863
echo "Building Godot library..."

# For Linux
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    PLATFORM="linuxbsd"
    LIB_EXT="so"
    PLATFORM_DIR="linux"
# For macOS
elif [[ "$OSTYPE" == "darwin"* ]]; then
    PLATFORM="macos"
    LIB_EXT="dylib"
    PLATFORM_DIR="macos"
# For Windows (Git Bash/MSYS)
elif [[ "$OSTYPE" == "msys"* ]] || [[ "$OSTYPE" == "cygwin"* ]]; then
    PLATFORM="windows"
    LIB_EXT="dll"
    PLATFORM_DIR="windows"
else
    echo "Unsupported platform: $OSTYPE"
    exit 1
fi

echo "Building Godot library for $PLATFORM..."
scons platform=$PLATFORM target=template_release library_type=shared_library

mkdir -p "$BUILD_DIR/$PLATFORM_DIR"
# Copy the library files - Godot outputs as godot.* not libgodot.*
LIB_FILE=$(find bin -maxdepth 1 -name "godot.$PLATFORM.*.$LIB_EXT" -type f 2>/dev/null | head -1)
if [ -z "$LIB_FILE" ]; then
    LIB_FILE=$(find bin -maxdepth 1 -name "libgodot.$PLATFORM.*.$LIB_EXT" -type f 2>/dev/null | head -1)
fi

if [ -n "$LIB_FILE" ]; then
    cp "$LIB_FILE" "$BUILD_DIR/$PLATFORM_DIR/"
    echo "Copied $(basename "$LIB_FILE") to $BUILD_DIR/$PLATFORM_DIR/"
else
    echo "Warning: Could not find built library (tried godot.$PLATFORM.*.$LIB_EXT and libgodot.$PLATFORM.*.$LIB_EXT)"
fi

echo "Godot library build complete!"
echo "Library files are in: $BUILD_DIR"

# Build GodotSharp bindings
echo ""
echo "Building GodotSharp bindings..."
cd "$GODOT_DIR"

# Build the editor with Mono support to generate C# glue
echo "Building Godot editor with Mono support..."
scons platform=$PLATFORM target=editor module_mono_enabled=yes

# Generate C# glue code
echo "Generating C# glue code..."
EDITOR_BIN=$(find bin -maxdepth 1 -name "godot.$PLATFORM.editor.*.mono" -type f 2>/dev/null | head -1)
if [ -z "$EDITOR_BIN" ]; then
    echo "Warning: Could not find editor binary (godot.$PLATFORM.editor.*.mono)"
else
    echo "Found editor binary: $EDITOR_BIN"
    "$EDITOR_BIN" --headless --generate-mono-glue modules/mono/glue \
        || echo "Warning: Glue generation failed"
fi

# Build C# assemblies
echo "Building C# assemblies..."
if [ -f "modules/mono/build_scripts/build_assemblies.py" ]; then
    # Try python3 first, then fall back to python
    PYTHON_CMD="python3"
    if ! command -v python3 &> /dev/null; then
        PYTHON_CMD="python"
    fi
    
    $PYTHON_CMD modules/mono/build_scripts/build_assemblies.py \
        --godot-output-dir="$GODOT_DIR/bin" \
        --push-nupkgs-local "$GODOT_DIR/bin/GodotSharp/NuPkgs" \
        || echo "Warning: Assembly build failed"
else
    echo "Warning: build_assemblies.py not found at modules/mono/build_scripts/build_assemblies.py"
fi

# Copy GodotSharp assemblies to build directory
echo "Copying GodotSharp assemblies to build directory..."
mkdir -p "$BUILD_DIR/GodotSharp"
if [ -d "bin/GodotSharp" ]; then
    cp -r bin/GodotSharp "$BUILD_DIR/" || echo "Warning: Could not copy GodotSharp assemblies"
fi

echo ""
echo "Build complete!"
echo "Library files are in: $BUILD_DIR/$PLATFORM_DIR"
echo "GodotSharp assemblies are in: $BUILD_DIR/GodotSharp"
