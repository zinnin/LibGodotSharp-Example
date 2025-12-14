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
# Build Godot as a library (takes 30-60 minutes)
./build-godot.sh

# Build and run the C# application
cd src
dotnet build
dotnet run
```

See [BUILD.md](BUILD.md) for detailed build instructions.

## Features

- ✅ **C# Console Application**: Standard .NET console app that creates Godot UI
- ✅ **Native Interop**: P/Invoke interfaces to Godot library functions
- ✅ **3D Scene Setup**: Programmatically create Camera3D and MeshInstance3D
- ✅ **Cross-Platform**: Supports Linux, macOS, and Windows
- ✅ **Build Scripts**: Automated scripts to build Godot as a library

## Project Structure

```
├── src/                       # C# source code
│   ├── Program.cs            # Main entry point
│   ├── GodotApplication.cs   # Application logic
│   └── GodotNativeInterop.cs # Native library interface
├── build-godot.sh            # Build Godot as library
├── BUILD.md                  # Detailed build instructions
└── README.md                 # This file
```

## Requirements

- .NET SDK 6.0+
- SCons (for building Godot)
- C++ compiler (GCC/Clang/MSVC)
- Python 3.6+

## Implementation Details

This example demonstrates two approaches:

1. **Direct Native Interop**: Using P/Invoke to call Godot C API functions
2. **GodotSharp Integration**: Using the official C# bindings (when available)

The code includes both approaches with documentation showing how each works.

## References

- [Godot Library Build PR](https://github.com/godotengine/godot/pull/110863)
- [libgodot Examples (C++/Swift)](https://github.com/migeran/libgodot)
- [Godot Documentation](https://docs.godotengine.org/)

## License

This is an example project for demonstration purposes.

## Contributing

This project serves as a reference implementation. Feel free to submit issues or improvements.
