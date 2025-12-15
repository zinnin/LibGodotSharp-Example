# GodotBuilder

A C# command-line tool to build Godot as a library with C# bindings support.

## Purpose

This cross-platform C# tool:
- Clones the Godot repository
- Builds Godot as a shared library
- Builds the Godot editor with Mono support
- Generates C# glue code
- Builds GodotSharp assemblies
- Copies all output files to the lib directory

## Benefits

- **Better debugging**: Can use C# debuggers (VS Code, Visual Studio, Rider) with breakpoints
- **Cross-platform**: Runs natively on Windows, Linux, and macOS
- **Better error handling**: Exceptions with full stack traces
- **Real-time output**: See build progress as it happens
- **IDE integration**: Full IntelliSense and refactoring support

## Requirements

### Basic Requirements (all platforms)
- .NET 10.0 SDK or later
- SCons (for building Godot)
- Python 3 (for building C# assemblies)
- Git

### Platform-Specific Compilers

**Linux:**
- GCC 9+ or Clang 6+
- Development libraries: `build-essential`, `pkg-config`, `libx11-dev`, `libxcursor-dev`, `libxinerama-dev`, `libgl1-mesa-dev`, `libglu-dev`, `libasound2-dev`, `libpulse-dev`, `libudev-dev`, `libxi-dev`, `libxrandr-dev`

**Windows:**
- Visual Studio 2019+ with C++ build tools
- OR MinGW-w64

**macOS:**
- Xcode command line tools

### Cross-Compilation (Optional)

The `--all` flag will attempt to build for all platforms, but cross-compilation has specific requirements:

**From Windows:**
- ✅ **Windows**: Native build (no extra tools needed with Visual Studio or MinGW-w64)
- ⚠️ **Linux**: Requires WSL2 (the builder detects WSL2 but you must run it INSIDE WSL2, not from Windows)
  - Install: `wsl --install`
  - Usage: Open WSL2 terminal (`wsl`), navigate to project, and run builder from there
  - Note: WSL2 provides a Linux environment, so it's essentially a native Linux build
- ❌ **macOS**: Not supported (requires OSXCross - extremely complex setup)

**From Linux:**
- ✅ **Linux**: Native build (no extra tools needed with GCC/Clang)
- ✅ **Windows**: Install MinGW-w64: `sudo apt install mingw-w64`
- ❌ **macOS**: Not commonly supported (requires OSXCross)

**From macOS:**
- ✅ **macOS**: Native build (no extra tools needed with Xcode)
- ⚠️ **Windows**: Install MinGW-w64: `brew install mingw-w64` (may have issues)
- ❌ **Linux**: Not commonly supported

**IMPORTANT:** The builder will automatically detect missing toolchains and skip platforms that cannot be built, with clear messaging about what's needed.

**Recommended Approach:** 
For production builds, run the GodotBuilder natively on each target operating system. This ensures:
- Most reliable builds
- No cross-compilation complexity
- Native compiler optimizations
- Easier troubleshooting

## Usage

### Basic usage (current platform only):

```bash
# From solution root
dotnet run --project GodotBuilder/GodotBuilder.csproj

# From GodotBuilder directory
cd GodotBuilder
dotnet run
```

### Build for specific platform:

```bash
# Build for Windows
dotnet run --project GodotBuilder/GodotBuilder.csproj -- windows

# Build for Linux
dotnet run --project GodotBuilder/GodotBuilder.csproj -- linuxbsd
# or
dotnet run --project GodotBuilder/GodotBuilder.csproj -- linux

# Build for macOS
dotnet run --project GodotBuilder/GodotBuilder.csproj -- macos
```

### Build for all platforms (Windows, Linux, macOS):

```bash
dotnet run --project GodotBuilder/GodotBuilder.csproj -- --all
```

**Note:** Cross-compilation requires appropriate toolchains. See CROSS_COMPILATION.md for details.

### Show help:

```bash
dotnet run --project GodotBuilder/GodotBuilder.csproj -- --help
```

### Visual Studio / IDE Usage:

The project includes launch profiles in `Properties/launchSettings.json` for easy Visual Studio integration:

- **GodotBuilder** - Build for current platform
- **Build All Platforms** - Build for all platforms (--all)
- **Build Linux Only** - Build for Linux specifically (direct, requires running in WSL2 if on Windows)
- **Build Linux via WSL2 (Windows)** - ⭐ **Single-click Linux build on Windows via WSL2**
- **Build Windows Only** - Build for Windows specifically
- **Build macOS Only** - Build for macOS specifically
- **Show Help** - Display help information

**To use in Visual Studio:**
1. Open the solution in Visual Studio
2. Set GodotBuilder as the startup project
3. Select the desired launch profile from the dropdown (next to the Start button)
4. Click Start (F5) or Start Without Debugging (Ctrl+F5)

**Recommended for Windows users building Linux:** Use the "Build Linux via WSL2 (Windows)" profile for automatic WSL2 integration.

**To build Linux on Windows (Easy Method - Single Click):**

Use the provided scripts that automatically handle WSL2:

**PowerShell:**
```powershell
.\Build-Linux.ps1
```

**Command Prompt:**
```cmd
Build-Linux.bat
```

**Visual Studio:**
- Select "Build Linux via WSL2 (Windows)" launch profile
- Click Start (F5) or Start Without Debugging (Ctrl+F5)

These scripts automatically:
- Check for WSL2 installation
- Convert Windows paths to WSL2 paths
- Launch the build inside WSL2
- Handle .NET SDK detection

**Manual Method (Advanced):**
1. Install and start WSL2 (`wsl --install`)
2. Open a WSL2 terminal (`wsl`)
3. Navigate to your project: `cd /mnt/c/path/to/LibGodotSharp-Example`
4. Run: `dotnet run --project GodotBuilder/GodotBuilder.csproj linuxbsd`

### Running from build:

```bash
dotnet build -c Release
cd GodotBuilder/bin/Release/net10.0
./GodotBuilder          # Current platform
./GodotBuilder --all    # All platforms
```

## How It Works

1. **Platform Detection**: Automatically detects the current OS and sets appropriate build parameters
2. **Godot Clone**: Clones the Godot repository if not already present
3. **Library Build**: Builds Godot as a shared library for the target platform(s)
   - Current platform only (default)
   - All platforms (with `--all` flag)
4. **Editor Build**: Builds the Godot editor with Mono/C# support (for current platform)
5. **Glue Generation**: Runs the editor to generate C# glue code
6. **Assembly Build**: Builds the GodotSharp assemblies using Python script
7. **File Copy**: Copies native libraries to `lib/<platform>/` and GodotSharp assemblies to `src/GodotAssemblies/`

### Multi-Platform Build

When using the `--all` flag, the builder will:
- Build the native library for Windows, Linux, and macOS
- Build the editor and C# assemblies for the current platform (these are platform-independent)
- Provide a summary of successful and failed builds

This is useful for:
- Creating releases with libraries for all platforms
- CI/CD pipelines that need to generate all platform binaries
- Developers who want to test their application on multiple platforms

## Output Structure

```
lib/
├── linux/          # Linux library files (.so)
│   └── godot.linuxbsd.template_release.x86_64.so
├── macos/          # macOS library files (.dylib)
│   └── godot.macos.template_release.universal.dylib
└── windows/        # Windows library files (.dll)
    └── godot.windows.template_release.x86_64.dll

src/GodotAssemblies/ # GodotSharp C# assemblies for LibGodotSharpExample
├── Api/             # Core API assemblies (GodotSharp.dll)
├── Tools/           # Build tools
└── NuPkgs/          # NuGet packages
```

## Debugging

If GodotSharp library generation fails:

1. Set breakpoints in `GenerateCSharpGlue()` or `BuildCSharpAssemblies()`
2. Run with debugger attached
3. Check the console output for detailed error messages
4. Inspect the `GodotDir` and `BuildDir` variables
5. Verify that the editor binary exists and is executable

## Troubleshooting

### Command not found errors
- Ensure `scons`, `git`, and `python3` are in your PATH
- On Windows, may need to run from Developer Command Prompt

### Permission errors
- Ensure the script directory is writable
- On Linux/macOS, ensure the editor binary has execute permissions

### Build failures
- Check that you have the necessary C++ compiler installed
- Review the full error output in the console
- Check the Godot build documentation for platform-specific requirements
