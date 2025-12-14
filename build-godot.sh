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
    scons platform=linuxbsd target=template_release library_type=shared_library
    
    mkdir -p "$BUILD_DIR/linux"
    cp bin/libgodot.linux.*.so "$BUILD_DIR/linux/" || echo "Warning: Could not find built library"
    
# For macOS
elif [[ "$OSTYPE" == "darwin"* ]]; then
    scons platform=macos target=template_release library_type=shared_library
    
    mkdir -p "$BUILD_DIR/macos"
    cp bin/libgodot.macos.*.dylib "$BUILD_DIR/macos/" || echo "Warning: Could not find built library"
    
# For Windows (Git Bash/MSYS)
elif [[ "$OSTYPE" == "msys"* ]] || [[ "$OSTYPE" == "cygwin"* ]]; then
    scons platform=windows target=template_release library_type=shared_library
    
    mkdir -p "$BUILD_DIR/windows"
    cp bin/libgodot.windows.*.dll "$BUILD_DIR/windows/" || echo "Warning: Could not find built library"
else
    echo "Unsupported platform: $OSTYPE"
    exit 1
fi

echo "Godot library build complete!"
echo "Library files are in: $BUILD_DIR"

# Build GodotSharp bindings
echo "Building GodotSharp bindings..."
cd "$GODOT_DIR/modules/mono"

# Generate C# glue
cd "$GODOT_DIR"
# Note: This requires the editor build to generate bindings
# scons platform=linuxbsd target=editor module_mono_enabled=yes

echo ""
echo "Build complete!"
echo "Note: You may need to manually generate GodotSharp bindings by building the editor"
