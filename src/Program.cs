using System;
using System.Runtime.InteropServices;

namespace LibGodotSharpExample;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("LibGodotSharp Example - Creating Godot Scene with Camera and Cube");
        Console.WriteLine("==================================================================");
        Console.WriteLine();
        Console.WriteLine("This example demonstrates how to use GodotSharp bindings from C#.");
        Console.WriteLine("Reference: https://github.com/godotengine/godot/pull/110863");
        Console.WriteLine();
        
        try
        {
            // Run the GodotSharp example
            GodotSharpExample.Run();
            
            Console.WriteLine("\n✓ Example completed successfully!");
            Console.WriteLine("\nThe example demonstrated:");
            Console.WriteLine("  - Creating a SceneTree");
            Console.WriteLine("  - Setting up a 3D scene with Node3D");
            Console.WriteLine("  - Adding a DirectionalLight3D for illumination");
            Console.WriteLine("  - Adding a Camera3D positioned at (0, 0, 5)");
            Console.WriteLine("  - Creating a MeshInstance3D with a red BoxMesh");
            Console.WriteLine("  - Applying a StandardMaterial3D with color and properties");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"\nError: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }
}
