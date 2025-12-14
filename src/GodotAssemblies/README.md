# GodotSharp Assemblies

This directory contains the GodotSharp C# assemblies required by LibGodotSharpExample.

## Contents

After running the GodotBuilder tool, this directory will contain:

- **Api/** - GodotSharp API assemblies (GodotSharp.dll, etc.)
  - Contains the C# bindings for Godot classes and APIs
  - These are the assemblies you reference in your .csproj

- **Tools/** - Build tools and utilities

- **NuPkgs/** - NuGet packages for distribution

## Usage in LibGodotSharpExample

The LibGodotSharpExample project references the assemblies from this directory:

```xml
<ItemGroup>
  <Reference Include="GodotSharp">
    <HintPath>GodotAssemblies/Api/Release/GodotSharp.dll</HintPath>
  </Reference>
</ItemGroup>
```

## Building

To generate these assemblies, run the GodotBuilder tool:

```bash
cd ..
dotnet run --project GodotBuilder/GodotBuilder.csproj
```

The builder will:
1. Build Godot editor with Mono support
2. Generate C# glue code
3. Build the GodotSharp assemblies
4. Copy them to this directory

## Why Committed?

These assemblies are committed to the repository so that:
- Developers can work on the example without building Godot (30-60 minutes)
- CI/CD pipelines have the required dependencies
- Quick setup for contributors

## Size Consideration

The GodotSharp assemblies can be several MB in size. If repository size becomes an issue, consider:
- Using Git LFS for these files
- Downloading pre-built assemblies as part of a setup script
- Excluding from the repository and documenting the build process
