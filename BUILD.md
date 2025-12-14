# Building LibGodotSharp Example

This document explains how to build and run the LibGodotSharp example application.

## Overview

This project demonstrates how to use Godot as a library from C# code. It shows:

1. Building Godot as a shared library
2. Interfacing with the Godot library from C#
3. Creating a 3D scene with camera and cube programmatically

## Prerequisites

### For Building Godot Library

- **SCons**: Build system used by Godot
  ```bash
  # Ubuntu/Debian
  sudo apt-get install scons
  
  # macOS
  brew install scons
  
  # Windows
  pip install scons
  ```

- **Python 3.6+**: Required by SCons

- **C++ Compiler**:
  - Linux: GCC 9+ or Clang 6+
  - macOS: Xcode command line tools
  - Windows: Visual Studio 2019+ or MinGW-w64

- **Git**: For cloning Godot repository

### For Building C# Application

- **.NET SDK 10.0+**: The C# application requires .NET 10
  ```bash
  # Download from https://dotnet.microsoft.com/download
  dotnet --version
  ```

## Build Steps

### Step 1: Build Godot as a Library

Use the C# builder to compile Godot as a shared library:

```bash
# Build from solution
dotnet run --project GodotBuilder/GodotBuilder.csproj

# Or build and run separately
dotnet build GodotBuilder/GodotBuilder.csproj -c Release
./GodotBuilder/bin/Release/net10.0/GodotBuilder
```

Benefits:
- Better error messages and debugging
- Real-time build progress output
- Native .NET experience
- Can set breakpoints in your IDE
- Cross-platform support

See [GodotBuilder/README.md](GodotBuilder/README.md) for detailed documentation.

The builder will:
1. Clone the Godot repository (if not already present)
2. Build Godot as a shared library for your platform
3. Build Godot editor with Mono support
4. Generate C# glue code
5. Build GodotSharp assemblies
6. Copy all files to the `lib/` directory

**Note**: Building Godot from source can take 30-60 minutes depending on your system.

#### Manual Build (Alternative)

If you prefer to build manually:

```bash
# Clone Godot
git clone --depth 1 https://github.com/godotengine/godot.git

cd godot

# Build as library (Linux)
scons platform=linuxbsd target=template_release library_type=shared_library

# Build as library (macOS)
scons platform=macos target=template_release library_type=shared_library

# Build as library (Windows)
scons platform=windows target=template_release library_type=shared_library
```

### Step 2: Build GodotSharp Bindings (Optional)

For full GodotSharp integration, you need to generate the C# bindings:

```bash
cd godot

# Build editor with Mono support
scons platform=linuxbsd target=editor module_mono_enabled=yes

# Generate C# glue code
./bin/godot.linuxbsd.editor.x86_64.mono --generate-mono-glue modules/mono/glue

# Build C# bindings
./modules/mono/build_scripts/build_assemblies.py --godot-output-dir=./bin
```

### Step 3: Build the C# Application

```bash
# Navigate to src directory
cd src

# Restore dependencies
dotnet restore

# Build the application
dotnet build

# Run the application
dotnet run
```

## Project Structure

```
LibGodotSharp-Example/
├── LibGodotSharp.sln         # Solution file
├── README.md                  # Project overview
├── BUILD.md                   # This file
├── .gitignore                # Git ignore file
├── GodotBuilder/             # C# builder tool
│   └── Program.cs            # Builder implementation
├── src/                      # C# source code
│   ├── LibGodotSharpExample.csproj
│   ├── Program.cs            # Main entry point
│   ├── GodotApplication.cs   # Main application logic
│   └── GodotNativeInterop.cs # Native library interface
├── godot/                    # Godot source (created by builder)
└── lib/                      # Built libraries (created by builder, committed)
    ├── linux/
    ├── macos/
    └── windows/
```

## Usage

Once built, run the application:

```bash
cd src
dotnet run
```

The application will:
1. Initialize the Godot engine
2. Create a 3D scene
3. Add a Camera3D positioned at (0, 0, 5)
4. Add a MeshInstance3D with a red cube at the origin
5. Run the main loop to display the window

## Implementation Details

### Native Interop

The `GodotNativeInterop.cs` file demonstrates how to interface with the Godot library using P/Invoke:

```csharp
[DllImport(LibGodot, CallingConvention = CallingConvention.Cdecl)]
public static extern int godot_initialize(int argc, IntPtr argv);

[DllImport(LibGodot, CallingConvention = CallingConvention.Cdecl)]
public static extern bool godot_iterate();
```

### Scene Setup

The `GodotApplication.cs` shows the typical structure:

```csharp
// Create scene tree
var sceneTree = new SceneTree();

// Create 3D scene
var mainScene = new Node3D();

// Add camera
var camera = new Camera3D();
camera.Position = new Vector3(0, 0, 5);
mainScene.AddChild(camera);

// Add cube
var cube = new MeshInstance3D();
cube.Mesh = new BoxMesh();
mainScene.AddChild(cube);
```

## References

- [Godot Library PR #110863](https://github.com/godotengine/godot/pull/110863) - Original PR adding library support
- [libgodot Example](https://github.com/migeran/libgodot) - C++ and Swift examples
- [Godot Documentation](https://docs.godotengine.org/)
- [GodotSharp Documentation](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/)

## Troubleshooting

### Library Not Found

If you get a "DLL not found" error:

1. Ensure the Godot library is built in the `lib/` directory
2. Copy the library to the same directory as the executable
3. Or add the library path to your system's library path:
   - Linux: `LD_LIBRARY_PATH`
   - macOS: `DYLD_LIBRARY_PATH`
   - Windows: `PATH`

### Build Errors

If Godot fails to build:

1. Check that all dependencies are installed
2. Verify your compiler version meets requirements
3. Check the [Godot build documentation](https://docs.godotengine.org/en/stable/contributing/development/compiling/)

### GodotSharp Not Found

The current implementation shows the structure without requiring the full GodotSharp bindings. To use actual GodotSharp:

1. Build Godot with Mono support
2. Generate the C# glue code
3. Add reference to GodotSharp assemblies in the .csproj file

## Current Implementation Status

**Note**: The current implementation is a demonstration/template that shows:

- ✅ Project structure
- ✅ Build scripts
- ✅ Native interop interfaces
- ✅ Scene setup logic
- ⚠️ Simulated mode (actual Godot library calls are commented)

To make it fully functional:

1. Build the Godot library using the GodotBuilder tool
2. Uncomment the native interop calls in the code
3. Ensure the library is in the correct path
4. Run the application

The simulated mode demonstrates what the application would do without requiring the full Godot build.
