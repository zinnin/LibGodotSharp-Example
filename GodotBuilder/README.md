# GodotBuilder

A C# command-line tool to build Godot as a library with C# bindings support.

## Purpose

This tool replaces the `build-godot.sh` bash script with a cross-platform C# implementation that:
- Clones the Godot repository
- Builds Godot as a shared library
- Builds the Godot editor with Mono support
- Generates C# glue code
- Builds GodotSharp assemblies
- Copies all output files to the build directory

## Benefits over Bash Script

- **Better debugging**: Can use C# debuggers (VS Code, Visual Studio, Rider) with breakpoints
- **Cross-platform**: Runs natively on Windows, Linux, and macOS without bash environment
- **Better error handling**: Exceptions with full stack traces
- **Real-time output**: See build progress as it happens
- **IDE integration**: Full IntelliSense and refactoring support

## Requirements

- .NET 10.0 SDK or later
- SCons (for building Godot)
- Python 3 (for building C# assemblies)
- Git
- C++ compiler (GCC/Clang/MSVC)

## Usage

### Running from source:

```bash
cd GodotBuilder
dotnet run
```

### Running from build:

```bash
dotnet build -c Release
cd bin/Release/net10.0
./GodotBuilder  # or GodotBuilder.exe on Windows
```

### From solution:

```bash
dotnet run --project GodotBuilder/GodotBuilder.csproj
```

## How It Works

1. **Platform Detection**: Automatically detects the current OS and sets appropriate build parameters
2. **Godot Clone**: Clones the Godot repository if not already present
3. **Library Build**: Builds Godot as a shared library (`.so`, `.dll`, or `.dylib`)
4. **Editor Build**: Builds the Godot editor with Mono/C# support
5. **Glue Generation**: Runs the editor to generate C# glue code
6. **Assembly Build**: Builds the GodotSharp assemblies using Python script
7. **File Copy**: Copies all output files to the `build/` directory

## Output Structure

```
build/
├── linux/          # Linux library files (.so)
│   └── godot.linuxbsd.template_release.x86_64.so
├── macos/          # macOS library files (.dylib)
│   └── godot.macos.template_release.universal.dylib
├── windows/        # Windows library files (.dll)
│   └── godot.windows.template_release.x86_64.dll
└── GodotSharp/     # C# assemblies
    ├── Api/
    ├── Tools/
    └── NuPkgs/
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
