# LibGodotSharp-Example

A minimal example project demonstrating how to use Godot as a library from C# code.

## Overview

This project demonstrates Godot's new library build feature (introduced in [PR #110863](https://github.com/godotengine/godot/pull/110863)), which allows Godot to be compiled as a shared library and used from other applications.

This C# example shows how to:
- Build Godot as a shared library
- Interface with the Godot library from C# using P/Invoke
- Create a Godot window programmatically
- Set up a 3D scene with a Camera3D
- Add and render a cube (MeshInstance3D with BoxMesh)

## Quick Start

```bash
# Run the example (no Godot build required - uses pre-built assemblies)
cd src
dotnet build
dotnet run

# Optional: Build Godot native libraries
# For current platform only (30-60 minutes):
dotnet run --project GodotBuilder/GodotBuilder.csproj

# For all platforms (Windows, Linux, macOS) - requires cross-compilation tools:
dotnet run --project GodotBuilder/GodotBuilder.csproj -- --all
```

See [BUILD.md](BUILD.md) for detailed build instructions.

## Features

- ✅ **C# Console Application**: Standard .NET console app that uses GodotSharp API
- ✅ **GodotSharp Integration**: Full implementation using official GodotSharp bindings
- ✅ **Built Assemblies**: Pre-built GodotSharp assemblies included (v4.6.0)
- ✅ **3D Scene Setup**: Programmatically create Camera3D, MeshInstance3D, and lighting
- ✅ **Material System**: StandardMaterial3D with color, metallic, and roughness properties
- ✅ **Cross-Platform**: Supports Linux, macOS, and Windows
- ✅ **Build Scripts**: Automated scripts to build Godot as a library
- ✅ **C# Builder Tool**: Cross-platform C# builder with better debugging support
- ✅ **Solution Structure**: Organized with proper solution and project structure

## Project Structure

```
├── LibGodotSharp.sln         # Solution file
├── GodotBuilder/             # C# builder tool
│   ├── Program.cs            # Builder implementation
│   └── README.md             # Builder documentation
├── src/                      # Example application
│   ├── Program.cs            # Main entry point
│   ├── GodotSharpExample.cs  # GodotSharp implementation
│   ├── GodotApplication.cs   # Alternative simulation approach
│   ├── GodotNativeInterop.cs # Native library interface
│   └── GodotAssemblies/      # Pre-built GodotSharp assemblies
│       ├── Api/              # GodotSharp.dll, GodotSharpEditor.dll
│       └── Tools/            # Build tools
├── lib/                      # Built libraries (committed)
│   └── windows/              # godot.windows.template_release.x86_64.dll
├── BUILD.md                  # Detailed build instructions
└── README.md                 # This file
```

## Requirements

- .NET SDK 10.0+ (for building the solution)
- SCons (for building Godot)
- C++ compiler (GCC/Clang/MSVC)
- Python 3.6+
- Git

## Implementation Details

This example demonstrates using the **GodotSharp Integration** approach with official C# bindings:

- **GodotSharpExample.cs**: Fully implemented using actual GodotSharp assemblies
  - Validates assembly loading and type availability
  - Demonstrates complete scene setup with Node3D, Camera3D, MeshInstance3D
  - Shows material configuration with StandardMaterial3D
  - Includes ready-to-use SceneSetup class for native integration
  
- **GodotApplication.cs**: Alternative simulation approach showing structure
- **GodotNativeInterop.cs**: P/Invoke declarations for native library (when needed)

The code uses pre-built GodotSharp assemblies (v4.6.0) from `src/GodotAssemblies/`.

## References

- [Godot Library Build PR](https://github.com/godotengine/godot/pull/110863)
- [libgodot Examples (C++/Swift)](https://github.com/migeran/libgodot)
- [Godot Documentation](https://docs.godotengine.org/)

## License

This is an example project for demonstration purposes.

## Contributing

This project serves as a reference implementation. Feel free to submit issues or improvements.
