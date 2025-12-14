using System;
using System.Runtime.InteropServices;

namespace LibGodotSharpExample;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("LibGodotSharp Example - Creating Godot Window with Camera and Cube");
        Console.WriteLine("====================================================================");
        Console.WriteLine();
        Console.WriteLine("This example demonstrates how to use Godot as a library from C#.");
        Console.WriteLine("Reference: https://github.com/godotengine/godot/pull/110863");
        Console.WriteLine();
        
        try
        {
            // Show what the GodotSharp implementation structure looks like
            GodotSharpExample.ShowStructure();
            
            // Run the demonstration
            Console.WriteLine("Running demonstration...\n");
            
            // Initialize the Godot engine
            var app = new GodotApplication();
            
            // Setup the scene
            app.SetupScene();
            
            // Run the application
            app.Run();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            Environment.Exit(1);
        }
    }
}
