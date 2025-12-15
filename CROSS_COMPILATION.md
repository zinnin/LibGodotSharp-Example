# Cross-Compilation Guide

This document explains how to build Godot libraries for multiple platforms.

## Quick Answer

**Can I build libraries for all platforms from Windows?**

**Short answer:** Not easily. Each platform is best built natively.

**Practical answer:** 
- ✅ Windows → Windows: Yes (native)
- ⚠️ Windows → Linux: Possible with WSL2 but complex
- ❌ Windows → macOS: No (requires OSXCross, extremely difficult)

## The `--all` Flag

When you run:
```bash
dotnet run --project GodotBuilder/GodotBuilder.csproj -- --all
```

The builder will:
1. Check for required cross-compilation tools for each platform
2. Skip platforms where tools are missing (with clear explanation)
3. Build for platforms where tools are available
4. Provide a summary of successful/failed/skipped builds

### Example Output on Windows (without cross-compilation tools):

```
Building for all platforms...
NOTE: Cross-platform builds require appropriate toolchains installed.
      Builds will be skipped for platforms without required tools.

============================================================
Building for windows...
============================================================

✓ windows build successful

============================================================
Building for linuxbsd...
============================================================

⊘ linuxbsd build skipped: Cross-compiling Linux from Windows requires WSL2 or Linux cross-compiler

Required tools for linuxbsd:
  - WSL2 (Windows Subsystem for Linux)
  - Linux cross-compilation toolchain
  - GCC/Clang for Linux target

============================================================
Building for macos...
============================================================

⊘ macos build skipped: Cross-compiling macOS from non-macOS systems requires OSXCross (advanced setup)

Required tools for macos:
  - OSXCross toolchain
  - Xcode SDK
  - Note: macOS cross-compilation is complex and rarely used

============================================================
Build Summary
============================================================
Successful: 1/3
Failed: 0/3
Skipped: 2/3

To build for all platforms, you need:
  - Native build on each OS, OR
  - Cross-compilation toolchains installed

Recommendation: Run the builder on each target platform natively
                for the most reliable results.
```

## Platform-Specific Requirements

### Building on Windows

**Native Windows builds:** ✅ Ready out of the box
- Visual Studio 2019+ with C++ tools, OR
- MinGW-w64

**Cross-compile to Linux:** ⚠️ Use WSL2 (separate environment)
- Install WSL2: `wsl --install` (requires restart)
- The GodotBuilder can detect if WSL2 is installed
- However, you must RUN the builder INSIDE WSL2, not from Windows
- Steps:
  1. Open WSL2 terminal: `wsl`
  2. Navigate to project: `cd /mnt/c/path/to/project`
  3. Run builder: `dotnet run --project GodotBuilder/GodotBuilder.csproj`
- Note: WSL2 is essentially a Linux environment, so this is "native" Linux build

**Cross-compile to macOS:** ❌ Not practical
- Would require OSXCross (extremely complex setup)
- Not recommended

### Building on Linux

**Native Linux builds:** ✅ Ready out of the box
- `sudo apt install build-essential scons`

**Cross-compile to Windows:** ✅ Relatively easy
- Install MinGW-w64: `sudo apt install mingw-w64`
- The builder will detect and use it automatically

**Cross-compile to macOS:** ❌ Not practical
- Would require OSXCross
- Not commonly done

### Building on macOS

**Native macOS builds:** ✅ Ready out of the box
- Install Xcode command line tools: `xcode-select --install`

**Cross-compile to Windows:** ⚠️ Possible but may have issues
- Install MinGW-w64: `brew install mingw-w64`
- May require additional configuration

**Cross-compile to Linux:** ❌ Not commonly supported

## Recommended Approach

For reliable multi-platform builds:

1. **Use native builds on each platform:**
   - Build Windows binaries on Windows
   - Build Linux binaries on Linux
   - Build macOS binaries on macOS

2. **Use CI/CD:**
   - GitHub Actions can build on all three platforms
   - Each platform builds natively in its own runner
   - Example workflow:
     ```yaml
     jobs:
       build-windows:
         runs-on: windows-latest
       build-linux:
         runs-on: ubuntu-latest
       build-macos:
         runs-on: macos-latest
     ```

3. **Store built libraries in the repository:**
   - Check in the built `.dll`, `.so`, `.dylib` files
   - Developers can use them without building
   - Rebuild only when updating Godot version

## What Tools Do I Need?

### On Windows (to build for all platforms):
```bash
# Windows native: Already have (Visual Studio or MinGW)

# Linux: Requires WSL2
# 1. Install WSL2:
wsl --install

# 2. Restart your computer if prompted

# 3. Open WSL2 terminal:
wsl

# 4. Inside WSL2, install build tools:
sudo apt update
sudo apt install build-essential scons pkg-config libx11-dev libxcursor-dev \
  libxinerama-dev libgl1-mesa-dev libglu-dev libasound2-dev libpulse-dev \
  libudev-dev libxi-dev libxrandr-dev

# 5. Inside WSL2, navigate to project and build:
cd /mnt/c/path/to/LibGodotSharp-Example
dotnet run --project GodotBuilder/GodotBuilder.csproj

# IMPORTANT: The builder must run INSIDE WSL2, not from Windows command prompt
# The --all flag from Windows will detect WSL2 but skip Linux build with instructions

# macOS: Not practical
```

### On Linux (to build for all platforms):
```bash
# Linux native: Install build tools
sudo apt install build-essential scons pkg-config libx11-dev libxcursor-dev \
  libxinerama-dev libgl1-mesa-dev libglu-dev libasound2-dev libpulse-dev \
  libudev-dev libxi-dev libxrandr-dev

# Windows: Install cross-compiler
sudo apt install mingw-w64

# macOS: Not practical
```

### On macOS (to build for all platforms):
```bash
# macOS native: Install Xcode tools
xcode-select --install

# Windows: Try MinGW (may have issues)
brew install mingw-w64

# Linux: Not practical
```

## Summary

- **`--all` flag is smart:** It will tell you exactly what's missing
- **Native builds are best:** Most reliable and easiest
- **Some cross-compilation works:** Linux → Windows is relatively easy
- **Some doesn't work:** macOS cross-compilation is impractical
- **Builder provides clear feedback:** You'll know what tools are needed

If you see platforms being skipped, the builder will tell you:
1. Why it was skipped
2. What tools are required
3. How to install them (or that it's not practical)
