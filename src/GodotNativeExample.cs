using System;
using System.Runtime.InteropServices;
using Godot;

namespace LibGodotSharpExample;

/// <summary>
/// Complete working example that creates a Godot window with a visible cube.
/// 
/// This integrates native libgodot initialization with GodotSharp API.
/// Based on patterns from: https://github.com/migeran/libgodot
/// 
/// The example:
/// 1. Initializes the native Godot engine
/// 2. Creates a window
/// 3. Sets up a 3D scene with a rotating red cube
/// 4. Runs until the window is closed
/// </summary>
public class GodotNativeExample
{
    // Platform-specific library name
#if WINDOWS
    private const string LibGodot = "godot.windows.template_release.x86_64.dll";
#elif OSX
    private const string LibGodot = "libgodot.macos.template_release.universal.dylib";
#else
    private const string LibGodot = "libgodot.linuxbsd.template_release.x86_64.so";
#endif

    private static Node3D? mainScene;
    private static MeshInstance3D? cube;

    /// <summary>
    /// Native function to initialize Godot engine
    /// Based on libgodot examples pattern
    /// </summary>
    [DllImport(LibGodot, CallingConvention = CallingConvention.Cdecl, EntryPoint = "godot_initialize")]
    private static extern int GodotInitialize(int argc, IntPtr argv);

    /// <summary>
    /// Native function to process one frame
    /// Returns true to continue, false to quit
    /// </summary>
    [DllImport(LibGodot, CallingConvention = CallingConvention.Cdecl, EntryPoint = "godot_iterate")]
    private static extern bool GodotIterate();

    /// <summary>
    /// Native function to finalize and cleanup
    /// </summary>
    [DllImport(LibGodot, CallingConvention = CallingConvention.Cdecl, EntryPoint = "godot_finalize")]
    private static extern void GodotFinalize();

    /// <summary>
    /// Main entry point - creates a Godot window with a rotating cube
    /// </summary>
    public static void Run()
    {
        Console.WriteLine("=== Godot Window with 3D Cube Example ===\n");
        Console.WriteLine("This example creates a Godot window displaying a rotating red cube.");
        Console.WriteLine("Based on patterns from: https://github.com/migeran/libgodot\n");

        try
        {
            // Step 1: Initialize native Godot engine
            Console.WriteLine("Initializing Godot engine...");
            if (!InitializeNativeEngine())
            {
                Console.WriteLine("⚠ Native Godot library not available.");
                Console.WriteLine("  To run this example, you need the native libgodot library:");
                Console.WriteLine("  - Windows: godot.windows.template_release.x86_64.dll");
                Console.WriteLine("  - Linux: libgodot.linuxbsd.template_release.x86_64.so");
                Console.WriteLine("  - macOS: libgodot.macos.template_release.universal.dylib\n");
                ShowIntegrationPattern();
                return;
            }
            Console.WriteLine("✓ Godot engine initialized\n");

            // Step 2: Create window and set up scene
            Console.WriteLine("Creating window and setting up 3D scene...");
            if (!SetupSceneAndWindow())
            {
                Console.WriteLine("⚠ Scene setup failed - GodotSharp bridge not fully initialized");
                Console.WriteLine("  This requires proper integration between native engine and C# runtime.\n");
                Cleanup();
                ShowIntegrationPattern();
                return;
            }
            Console.WriteLine("✓ Window created with 3D scene\n");

            // Step 3: Run main loop until window is closed
            Console.WriteLine("Running application...");
            Console.WriteLine("A window should appear with a rotating red cube.");
            Console.WriteLine("Close the window to exit.\n");
            RunMainLoop();

            // Step 4: Cleanup
            Console.WriteLine("\nShutting down...");
            Cleanup();
            Console.WriteLine("✓ Application closed\n");
        }
        catch (DllNotFoundException ex)
        {
            Console.WriteLine($"\n⚠ Native library not found: {ex.Message}");
            Console.WriteLine($"   Looking for: {LibGodot}\n");
            Console.WriteLine("To build the native library, run:");
            Console.WriteLine("  dotnet run --project GodotBuilder/GodotBuilder.csproj\n");
            ShowIntegrationPattern();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"\nError: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            
            try
            {
                Cleanup();
            }
            catch { }
            
            throw;
        }
    }

    /// <summary>
    /// Initialize the native Godot engine
    /// Following the pattern from libgodot C++/Swift examples
    /// </summary>
    private static bool InitializeNativeEngine()
    {
        try
        {
            // Prepare arguments (application name)
            IntPtr argv = Marshal.StringToHGlobalAnsi("LibGodotSharpExample");
            
            try
            {
                int result = GodotInitialize(1, argv);
                
                if (result != 0)
                {
                    Console.WriteLine($"   ⚠ Initialization returned code: {result}");
                    return false;
                }
                
                return true;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }
        catch (DllNotFoundException)
        {
            return false;
        }
    }

    /// <summary>
    /// Create window and set up the 3D scene using GodotSharp API
    /// </summary>
    private static bool SetupSceneAndWindow()
    {
        try
        {
            // Get the main SceneTree from the engine
            var sceneTree = Engine.GetMainLoop() as SceneTree;
            if (sceneTree == null)
            {
                Console.WriteLine("  ⚠ Could not get SceneTree from engine");
                return false;
            }

            // Create the main 3D scene container
            mainScene = new Node3D();
            mainScene.Name = "MainScene";

            // Add a directional light so we can see the cube
            var light = new DirectionalLight3D();
            light.Name = "Sun";
            light.Position = new Vector3(5, 5, 5);
            light.LookAt(Vector3.Zero, Vector3.Up);
            light.LightEnergy = 1.0f;
            mainScene.AddChild(light);

            // Add an environment light for better visibility
            var worldEnvironment = new WorldEnvironment();
            var environment = new Godot.Environment();
            environment.BackgroundMode = Godot.Environment.BGMode.Color;
            environment.BackgroundColor = new Color(0.1f, 0.1f, 0.15f);
            environment.AmbientLightSource = Godot.Environment.AmbientSource.Color;
            environment.AmbientLightColor = new Color(0.3f, 0.3f, 0.3f);
            worldEnvironment.Environment = environment;
            mainScene.AddChild(worldEnvironment);

            // Create and configure the camera
            var camera = new Camera3D();
            camera.Name = "MainCamera";
            camera.Position = new Vector3(0, 1, 4);
            camera.LookAt(Vector3.Zero, Vector3.Up);
            mainScene.AddChild(camera);

            // Create the red cube
            cube = new MeshInstance3D();
            cube.Name = "RedCube";
            
            var boxMesh = new BoxMesh();
            boxMesh.Size = new Vector3(1, 1, 1);
            cube.Mesh = boxMesh;

            // Create and apply a red material
            var material = new StandardMaterial3D();
            material.AlbedoColor = new Color(0.8f, 0.2f, 0.2f);
            material.Metallic = 0.5f;
            material.Roughness = 0.4f;
            cube.SetSurfaceOverrideMaterial(0, material);

            cube.Position = Vector3.Zero;
            mainScene.AddChild(cube);

            // Add the scene to the scene tree
            sceneTree.Root.AddChild(mainScene);

            // Configure the window
            var window = sceneTree.Root;
            window.Title = "LibGodotSharp Example - Rotating Cube";
            window.Size = new Vector2I(800, 600);

            Console.WriteLine("  Scene hierarchy created:");
            Console.WriteLine("    MainScene (Node3D)");
            Console.WriteLine("    ├── Sun (DirectionalLight3D)");
            Console.WriteLine("    ├── WorldEnvironment");
            Console.WriteLine("    ├── MainCamera (Camera3D)");
            Console.WriteLine("    └── RedCube (MeshInstance3D)");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠ Error setting up scene: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Main loop - runs until window is closed
    /// </summary>
    private static void RunMainLoop()
    {
        int frameCount = 0;
        float rotationSpeed = 1.0f;
        
        try
        {
            // Main game loop - continues until user closes the window
            while (true)
            {
                // Process one frame (events, updates, rendering)
                bool shouldContinue = GodotIterate();
                
                if (!shouldContinue)
                {
                    // User closed the window or requested quit
                    Console.WriteLine($"Window closed after {frameCount} frames");
                    break;
                }
                
                // Rotate the cube for visual effect
                if (cube != null)
                {
                    float delta = 0.016f; // Approximate frame time (60 FPS)
                    cube.RotateY(delta * rotationSpeed);
                    cube.RotateX(delta * rotationSpeed * 0.5f);
                }
                
                frameCount++;
                
                // Print status every 5 seconds (approx 300 frames at 60 FPS)
                if (frameCount % 300 == 0)
                {
                    Console.WriteLine($"Running... {frameCount} frames processed");
                }
            }
        }
        catch (DllNotFoundException)
        {
            Console.WriteLine("⚠ Native iterate function not available");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Error in main loop: {ex.Message}");
        }
    }

    /// <summary>
    /// Cleanup and finalize
    /// </summary>
    private static void Cleanup()
    {
        try
        {
            GodotFinalize();
        }
        catch (DllNotFoundException)
        {
            // Expected if library not loaded
        }
    }

    /// <summary>
    /// Shows the integration pattern when native library is not available
    /// </summary>
    private static void ShowIntegrationPattern()
    {
        Console.WriteLine("\n=== Integration Pattern ===\n");
        Console.WriteLine("The complete integration follows this pattern:");
        Console.WriteLine();
        Console.WriteLine("1. Native Initialization (C API):");
        Console.WriteLine("   IntPtr argv = Marshal.StringToHGlobalAnsi(\"AppName\");");
        Console.WriteLine("   int result = GodotInitialize(1, argv);");
        Console.WriteLine("   Marshal.FreeHGlobal(argv);");
        Console.WriteLine();
        Console.WriteLine("2. Scene Setup (GodotSharp API):");
        Console.WriteLine("   var sceneTree = Engine.GetMainLoop() as SceneTree;");
        Console.WriteLine("   var mainScene = new Node3D();");
        Console.WriteLine("   var camera = new Camera3D { Position = new Vector3(0, 0, 5) };");
        Console.WriteLine("   mainScene.AddChild(camera);");
        Console.WriteLine("   // ... add more nodes");
        Console.WriteLine("   sceneTree.Root.AddChild(mainScene);");
        Console.WriteLine();
        Console.WriteLine("3. Main Loop (Native + GodotSharp):");
        Console.WriteLine("   while (true)");
        Console.WriteLine("   {");
        Console.WriteLine("       if (!GodotIterate()) break;");
        Console.WriteLine("       // Optional: Custom game logic here");
        Console.WriteLine("   }");
        Console.WriteLine();
        Console.WriteLine("4. Cleanup (Native):");
        Console.WriteLine("   GodotFinalize();");
        Console.WriteLine();
        Console.WriteLine("This approach combines:");
        Console.WriteLine("  • Low-level native control (initialization, main loop)");
        Console.WriteLine("  • High-level C# API (scene creation, game logic)");
        Console.WriteLine();
        Console.WriteLine("See libgodot examples for working C++ and Swift implementations:");
        Console.WriteLine("https://github.com/migeran/libgodot");
    }
}
