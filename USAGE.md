# Usage Guide

This guide explains how to use this example and adapt it for your own projects.

## Running the Example

### Quick Demo (No Godot Build Required)

To see the demonstration without building Godot:

```bash
cd src
dotnet run
```

This will show:
- The structure of a GodotSharp implementation
- A simulation of what the application would do
- The API calls that would be made

### Full Implementation (Requires Godot Build)

To run with the actual Godot library:

1. Build Godot as a library:
   ```bash
   ./build-godot.sh
   ```

2. Uncomment the native interop calls in the source code (see below)

3. Run the application:
   ```bash
   cd src
   dotnet run
   ```

## Code Structure

### Program.cs

The main entry point. This file:
- Initializes the application
- Shows the implementation structure
- Handles errors

### GodotApplication.cs

High-level application logic. This demonstrates:
- Initialization flow
- Scene setup process
- Main loop structure

**Current**: Simulation mode with explanatory output
**With Godot**: Uncomment the GodotSharp calls

### GodotNativeInterop.cs

Low-level native interface. This shows:
- P/Invoke declarations
- Platform-specific library loading
- C API wrapper functions

**To enable**: 
1. Build Godot library
2. Ensure library is in correct path
3. Uncomment DllImport calls

### GodotSharpExample.cs

Complete example using GodotSharp API. This demonstrates:
- Full scene setup with GodotSharp
- Camera configuration
- Mesh creation
- Material application
- Main loop implementation

**To enable**:
1. Build Godot with Mono support
2. Generate C# glue code
3. Add GodotSharp references
4. Uncomment the implementation code

## Adapting for Your Project

### Approach 1: Using Native Interop (C API)

If you want to use the C API directly:

1. Build Godot as a library
2. Use `GodotNativeInterop.cs` as a template
3. Add P/Invoke declarations for needed functions
4. Call functions directly from your C# code

**Pros**:
- Full control over Godot initialization
- No dependency on C# bindings
- Smaller footprint

**Cons**:
- More manual work
- Less type-safe
- Need to understand C API

### Approach 2: Using GodotSharp Bindings

If you want to use the official C# API:

1. Build Godot with Mono support:
   ```bash
   scons platform=linuxbsd target=editor module_mono_enabled=yes
   ```

2. Generate C# glue:
   ```bash
   ./bin/godot.*.editor.*.mono --generate-mono-glue modules/mono/glue
   ```

3. Build assemblies:
   ```bash
   ./modules/mono/build_scripts/build_assemblies.py --godot-output-dir=./bin
   ```

4. Reference GodotSharp in your .csproj:
   ```xml
   <ItemGroup>
     <Reference Include="GodotSharp">
       <HintPath>../godot/bin/GodotSharp/Api/Release/GodotSharp.dll</HintPath>
     </Reference>
     <Reference Include="GodotSharpEditor">
       <HintPath>../godot/bin/GodotSharp/Api/Release/GodotSharpEditor.dll</HintPath>
     </Reference>
   </ItemGroup>
   ```

5. Use the code from `GodotSharpExample.cs`

**Pros**:
- Type-safe C# API
- Better IntelliSense support
- Easier to use
- Official support

**Cons**:
- Requires full Godot build with Mono
- Larger dependency
- More complex build process

## Common Scenarios

### Creating a 3D Scene

```csharp
// Create scene
var scene = new Node3D();

// Add camera
var camera = new Camera3D();
camera.Position = new Vector3(0, 0, 5);
scene.AddChild(camera);

// Add mesh
var mesh = new MeshInstance3D();
mesh.Mesh = new BoxMesh();
scene.AddChild(mesh);
```

### Creating a 2D Scene

```csharp
// Create 2D scene
var scene = new Node2D();

// Add sprite
var sprite = new Sprite2D();
sprite.Texture = ResourceLoader.Load<Texture2D>("res://icon.png");
scene.AddChild(sprite);

// Add camera
var camera = new Camera2D();
scene.AddChild(camera);
```

### Loading Resources

```csharp
// Load texture
var texture = ResourceLoader.Load<Texture2D>("res://path/to/texture.png");

// Load scene
var packedScene = ResourceLoader.Load<PackedScene>("res://path/to/scene.tscn");
var instance = packedScene.Instantiate();

// Load mesh
var mesh = ResourceLoader.Load<Mesh>("res://path/to/mesh.obj");
```

### Handling Input

```csharp
public override void _Process(double delta)
{
    if (Input.IsActionPressed("ui_accept"))
    {
        Console.WriteLine("Action pressed!");
    }
    
    var mousePos = GetViewport().GetMousePosition();
    Console.WriteLine($"Mouse: {mousePos}");
}
```

## Platform-Specific Notes

### Linux

- Library: `libgodot.linuxbsd.template_release.x86_64.so`
- Path: Set `LD_LIBRARY_PATH` or copy to executable directory
- Build requirements: GCC 9+ or Clang 6+

### macOS

- Library: `libgodot.macos.template_release.universal.dylib`
- Path: Set `DYLD_LIBRARY_PATH` or copy to executable directory
- Build requirements: Xcode command line tools
- Note: May need to sign the library

### Windows

- Library: `libgodot.windows.template_release.x86_64.dll`
- Path: Add to `PATH` or copy to executable directory
- Build requirements: Visual Studio 2019+ or MinGW-w64

## Troubleshooting

### "Godot library not found"

1. Check that library is built: `ls build/linux/` (or macos/windows)
2. Copy library to output directory: `cp build/linux/*.so src/bin/Debug/net10.0/`
3. Set library path environment variable

### "DllImport failed"

1. Verify library name matches in `GodotNativeInterop.cs`
2. Check platform conditional compilation (`#if WINDOWS`, etc.)
3. Ensure library is for correct architecture (x64 vs x86)

### "Method not found"

1. Verify Godot version supports library mode
2. Check that PR #110863 is merged in your Godot version
3. Update P/Invoke declarations to match actual API

### "GodotSharp assembly not found"

1. Build Godot with `module_mono_enabled=yes`
2. Generate glue code
3. Build assemblies
4. Verify reference path in .csproj

## Next Steps

Once you have this example working, you can:

1. Add more complex scenes
2. Implement input handling
3. Load resources from files
4. Create custom nodes
5. Build a full application

## Additional Resources

- [Godot C# Documentation](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/)
- [Godot API Reference](https://docs.godotengine.org/en/stable/classes/)
- [GodotSharp GitHub](https://github.com/godotengine/godot/tree/master/modules/mono)
- [libgodot Examples](https://github.com/migeran/libgodot)
