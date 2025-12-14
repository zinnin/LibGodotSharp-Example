using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace GodotBuilder;

class Program
{
    private static string ScriptDir = AppContext.BaseDirectory;
    private static string GodotDir = string.Empty;
    private static string BuildDir = string.Empty;
    private static string GodotAssembliesDir = string.Empty;
    private static string Platform = string.Empty;
    private static string LibExt = string.Empty;
    private static string PlatformDir = string.Empty;

    static int Main(string[] args)
    {
        try
        {
            // Determine script directory (use current directory if running from source)
            // From bin/Debug/net10.0 we need to go up 4 levels to reach repository root
            ScriptDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
            if (!File.Exists(Path.Combine(ScriptDir, "LibGodotSharp.sln")))
            {
                // Running from published build, use working directory
                ScriptDir = Directory.GetCurrentDirectory();
            }

            GodotDir = Path.Combine(ScriptDir, "godot");
            BuildDir = Path.Combine(ScriptDir, "lib");
            GodotAssembliesDir = Path.Combine(ScriptDir, "src", "GodotAssemblies");

            // Parse command line arguments
            bool buildAll = args.Length > 0 && (args[0] == "--all" || args[0] == "-a");
            bool showHelp = args.Length > 0 && (args[0] == "--help" || args[0] == "-h");

            if (showHelp)
            {
                ShowHelp();
                return 0;
            }

            Console.WriteLine("GodotBuilder - Build Godot as a library with C# bindings");
            Console.WriteLine("==========================================================");
            Console.WriteLine($"Script directory: {ScriptDir}");
            Console.WriteLine($"Godot directory: {GodotDir}");
            Console.WriteLine($"Native libs output: {BuildDir}");
            Console.WriteLine($"GodotSharp output: {GodotAssembliesDir}");
            Console.WriteLine();

            // Detect current platform
            DetectPlatform();
            Console.WriteLine($"Current platform: {Platform}");
            
            if (buildAll)
            {
                Console.WriteLine("Build mode: ALL PLATFORMS (Windows, Linux, macOS)");
            }
            else
            {
                Console.WriteLine($"Build mode: CURRENT PLATFORM ONLY ({Platform})");
                Console.WriteLine("  (Use --all to build for all platforms)");
            }
            Console.WriteLine();

            // Clone Godot if needed
            if (!Directory.Exists(GodotDir))
            {
                Console.WriteLine("Cloning Godot repository...");
                RunCommand("git", $"clone --depth 1 --branch master https://github.com/godotengine/godot.git \"{GodotDir}\"", ScriptDir);
            }

            // Update Godot
            Console.WriteLine("Checking Godot version...");
            RunCommand("git", "fetch --depth 1 origin master", GodotDir);
            RunCommand("git", "checkout master", GodotDir);
            Console.WriteLine();

            if (buildAll)
            {
                // Build for all platforms
                BuildAllPlatforms();
            }
            else
            {
                // Build for current platform only
                BuildForPlatform(Platform, LibExt, PlatformDir);
            }

            Console.WriteLine("\n========================================");
            Console.WriteLine("Build complete!");
            Console.WriteLine("========================================");
            Console.WriteLine($"Library files are in: {BuildDir}");
            Console.WriteLine($"GodotSharp assemblies are in: {GodotAssembliesDir}");
            Console.WriteLine();

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("GodotBuilder - Build Godot as a library with C# bindings");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run [options]");
        Console.WriteLine("  GodotBuilder [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  (none)        Build for current platform only");
        Console.WriteLine("  --all, -a     Build for all platforms (Windows, Linux, macOS)");
        Console.WriteLine("  --help, -h    Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run --project GodotBuilder/GodotBuilder.csproj");
        Console.WriteLine("  dotnet run --project GodotBuilder/GodotBuilder.csproj -- --all");
        Console.WriteLine();
        Console.WriteLine("Cross-Platform Building:");
        Console.WriteLine("  The --all flag attempts to build for all platforms. However,");
        Console.WriteLine("  cross-compilation requires specific toolchains:");
        Console.WriteLine();
        Console.WriteLine("  From Windows:");
        Console.WriteLine("    - Windows: Native (Visual Studio or MinGW-w64)");
        Console.WriteLine("    - Linux: Requires WSL2 or Linux cross-compiler (complex)");
        Console.WriteLine("    - macOS: Not supported (requires OSXCross - very complex)");
        Console.WriteLine();
        Console.WriteLine("  From Linux:");
        Console.WriteLine("    - Linux: Native (GCC or Clang)");
        Console.WriteLine("    - Windows: Requires MinGW-w64 (apt install mingw-w64)");
        Console.WriteLine("    - macOS: Not commonly supported");
        Console.WriteLine();
        Console.WriteLine("  From macOS:");
        Console.WriteLine("    - macOS: Native (Xcode command line tools)");
        Console.WriteLine("    - Windows: Requires MinGW-w64 (brew install mingw-w64)");
        Console.WriteLine("    - Linux: Not commonly supported");
        Console.WriteLine();
        Console.WriteLine("  RECOMMENDATION: Run the builder natively on each target OS");
        Console.WriteLine("                  for the most reliable builds.");
        Console.WriteLine();
        Console.WriteLine("Build time: 30-60 minutes per platform");
    }

    static void DetectPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Platform = "linuxbsd";
            LibExt = "so";
            PlatformDir = "linux";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Platform = "macos";
            LibExt = "dylib";
            PlatformDir = "macos";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Platform = "windows";
            LibExt = "dll";
            PlatformDir = "windows";
        }
        else
        {
            throw new PlatformNotSupportedException($"Unsupported platform: {RuntimeInformation.OSDescription}");
        }
    }

    static void BuildAllPlatforms()
    {
        Console.WriteLine("Building for all platforms...");
        Console.WriteLine("This will take significant time (30-60 minutes per platform)");
        Console.WriteLine();
        Console.WriteLine("NOTE: Cross-platform builds require appropriate toolchains installed.");
        Console.WriteLine("      Builds will be skipped for platforms without required tools.");
        Console.WriteLine();

        var platforms = new[]
        {
            ("windows", "dll", "windows"),
            ("linuxbsd", "so", "linux"),
            ("macos", "dylib", "macos")
        };

        int successCount = 0;
        int failCount = 0;
        int skippedCount = 0;

        foreach (var (platform, libExt, platformDir) in platforms)
        {
            Console.WriteLine($"\n{new string('=', 60)}");
            Console.WriteLine($"Building for {platform}...");
            Console.WriteLine($"{new string('=', 60)}\n");

            // Check if we can build for this platform
            var canBuild = CheckPlatformRequirements(platform);
            if (!canBuild.CanBuild)
            {
                skippedCount++;
                Console.WriteLine($"⊘ {platform} build skipped: {canBuild.Reason}");
                Console.WriteLine($"\nRequired tools for {platform}:");
                foreach (var tool in canBuild.RequiredTools)
                {
                    Console.WriteLine($"  - {tool}");
                }
                Console.WriteLine();
                continue;
            }

            try
            {
                BuildForPlatform(platform, libExt, platformDir);
                successCount++;
                Console.WriteLine($"✓ {platform} build successful");
            }
            catch (Exception ex)
            {
                failCount++;
                Console.WriteLine($"✗ {platform} build failed: {ex.Message}");
                Console.WriteLine("\nPossible causes:");
                Console.WriteLine("  - Missing compiler or build tools");
                Console.WriteLine("  - Incompatible SCons version");
                Console.WriteLine("  - Missing platform-specific dependencies");
                Console.WriteLine($"\nFor {platform} build requirements, see:");
                Console.WriteLine("  https://docs.godotengine.org/en/stable/contributing/development/compiling/");
                Console.WriteLine("\nContinuing with next platform...");
            }
        }

        Console.WriteLine($"\n{new string('=', 60)}");
        Console.WriteLine("Build Summary");
        Console.WriteLine($"{new string('=', 60)}");
        Console.WriteLine($"Successful: {successCount}/{platforms.Length}");
        Console.WriteLine($"Failed: {failCount}/{platforms.Length}");
        Console.WriteLine($"Skipped: {skippedCount}/{platforms.Length}");
        
        if (skippedCount > 0 || failCount > 0)
        {
            Console.WriteLine("\nTo build for all platforms, you need:");
            Console.WriteLine("  - Native build on each OS, OR");
            Console.WriteLine("  - Cross-compilation toolchains installed");
            Console.WriteLine("\nRecommendation: Run the builder on each target platform natively");
            Console.WriteLine("                for the most reliable results.");
        }
    }

    static (bool CanBuild, string Reason, List<string> RequiredTools) CheckPlatformRequirements(string platform)
    {
        var requiredTools = new List<string>();
        
        // Determine current host platform
        string hostPlatform = Platform;
        
        // Same platform - always can build
        if (platform == hostPlatform)
        {
            return (true, "", new List<string>());
        }

        // Cross-compilation requirements
        switch (platform)
        {
            case "windows":
                if (hostPlatform == "linuxbsd")
                {
                    requiredTools.Add("MinGW-w64 (x86_64-w64-mingw32-gcc)");
                    requiredTools.Add("mingw-w64 package installed");
                    
                    // Check if MinGW is available
                    if (!IsCommandAvailable("x86_64-w64-mingw32-gcc"))
                    {
                        return (false, "MinGW-w64 cross-compiler not found", requiredTools);
                    }
                }
                else if (hostPlatform == "macos")
                {
                    requiredTools.Add("MinGW-w64 via Homebrew (brew install mingw-w64)");
                    return (false, "Cross-compiling Windows from macOS requires MinGW-w64", requiredTools);
                }
                break;

            case "linuxbsd":
                if (hostPlatform == "windows")
                {
                    requiredTools.Add("WSL2 (Windows Subsystem for Linux)");
                    requiredTools.Add("Linux cross-compilation toolchain");
                    requiredTools.Add("GCC/Clang for Linux target");
                    return (false, "Cross-compiling Linux from Windows requires WSL2 or Linux cross-compiler", requiredTools);
                }
                else if (hostPlatform == "macos")
                {
                    requiredTools.Add("Linux cross-compilation toolchain");
                    return (false, "Cross-compiling Linux from macOS is not commonly supported", requiredTools);
                }
                break;

            case "macos":
                if (hostPlatform != "macos")
                {
                    requiredTools.Add("OSXCross toolchain");
                    requiredTools.Add("Xcode SDK");
                    requiredTools.Add("Note: macOS cross-compilation is complex and rarely used");
                    return (false, "Cross-compiling macOS from non-macOS systems requires OSXCross (advanced setup)", requiredTools);
                }
                break;
        }

        return (true, "", requiredTools);
    }

    static bool IsCommandAvailable(string command)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "which",
                Arguments = command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
            
            if (process != null)
            {
                process.WaitForExit();
                return process.ExitCode == 0;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    static void BuildForPlatform(string platform, string libExt, string platformDir)
    {
        // Build Godot library
        Console.WriteLine($"Building Godot library for {platform}...");
        RunCommand("scons", $"platform={platform} target=template_release library_type=shared_library", GodotDir);
        Console.WriteLine();

        // Copy library files
        Directory.CreateDirectory(Path.Combine(BuildDir, platformDir));
        CopyLibraryFiles(platform, libExt, platformDir);
        Console.WriteLine();

        // Only build GodotSharp assemblies once (they're platform-independent)
        // Build them on the first platform or current platform
        if (platform == Platform)
        {
            Console.WriteLine("Building GodotSharp bindings...");
            Console.WriteLine("Building Godot editor with Mono support...");
            RunCommand("scons", $"platform={platform} target=editor module_mono_enabled=yes", GodotDir);
            Console.WriteLine();

            // Generate C# glue
            Console.WriteLine("Generating C# glue code...");
            GenerateCSharpGlue(platform);
            Console.WriteLine();

            // Build C# assemblies
            Console.WriteLine("Building C# assemblies...");
            BuildCSharpAssemblies();
            Console.WriteLine();

            // Copy GodotSharp assemblies
            Console.WriteLine("Copying GodotSharp assemblies to output directory...");
            CopyGodotSharpAssemblies();
            Console.WriteLine();
        }
    }

    static void RunCommand(string command, string arguments, string workingDirectory)
    {
        Console.WriteLine($"Running: {command} {arguments}");
        Console.WriteLine($"Working directory: {workingDirectory}");
        Console.WriteLine();

        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException($"Failed to start process: {command}");
        }

        // Read output in real-time
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.WriteLine(e.Data);
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                Console.Error.WriteLine(e.Data);
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Command failed with exit code {process.ExitCode}: {command} {arguments}");
        }
    }

    static void CopyLibraryFiles(string platform, string libExt, string platformDir)
    {
        var binDir = Path.Combine(GodotDir, "bin");
        string? libFile = null;

        // Try godot.* pattern first
        var pattern1 = $"godot.{platform}.*.{libExt}";
        var files = Directory.GetFiles(binDir, pattern1, SearchOption.TopDirectoryOnly);
        if (files.Length > 0)
        {
            libFile = files[0];
        }
        else
        {
            // Try libgodot.* pattern as fallback
            var pattern2 = $"libgodot.{platform}.*.{libExt}";
            files = Directory.GetFiles(binDir, pattern2, SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                libFile = files[0];
            }
        }

        if (libFile != null)
        {
            var destFile = Path.Combine(BuildDir, platformDir, Path.GetFileName(libFile));
            File.Copy(libFile, destFile, overwrite: true);
            Console.WriteLine($"Copied {Path.GetFileName(libFile)} to {Path.Combine(BuildDir, platformDir)}");
        }
        else
        {
            Console.WriteLine($"Warning: Could not find built library (tried godot.{platform}.*.{libExt} and libgodot.{platform}.*.{libExt})");
        }
    }

    static void GenerateCSharpGlue(string platform)
    {
        var binDir = Path.Combine(GodotDir, "bin");
        // Pattern needs to match files like godot.windows.editor.x86_64.mono.exe
        var pattern = $"godot.{platform}.editor.*.mono*";
        var files = Directory.GetFiles(binDir, pattern, SearchOption.TopDirectoryOnly);

        if (files.Length == 0)
        {
            Console.WriteLine($"Warning: Could not find editor binary (godot.{platform}.editor.*.mono*)");
            Console.WriteLine($"Searched in: {binDir}");
            Console.WriteLine("Available files:");
            foreach (var file in Directory.GetFiles(binDir))
            {
                Console.WriteLine($"  {Path.GetFileName(file)}");
            }
            return;
        }

        // Prefer executables: .mono.exe over .mono.console.exe over other files
        var editorBin = files.FirstOrDefault(f => f.EndsWith(".mono.exe") && !f.Contains(".console."))
                     ?? files.FirstOrDefault(f => f.EndsWith(".exe"))
                     ?? files.FirstOrDefault(f => f.EndsWith(".mono"))
                     ?? files[0];
        Console.WriteLine($"Found editor binary: {editorBin}");

        var glueDir = Path.Combine(GodotDir, "modules", "mono", "glue");
        try
        {
            RunCommand(editorBin, $"--headless --generate-mono-glue \"{glueDir}\"", GodotDir);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Glue generation failed: {ex.Message}");
        }
    }

    static void BuildCSharpAssemblies()
    {
        var buildScript = Path.Combine(GodotDir, "modules", "mono", "build_scripts", "build_assemblies.py");
        if (!File.Exists(buildScript))
        {
            Console.WriteLine($"Warning: build_assemblies.py not found at {buildScript}");
            return;
        }

        // Try python3 first, then python
        string pythonCommand = "python3";
        try
        {
            var testProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "python3",
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
            
            if (testProcess != null)
            {
                testProcess.WaitForExit();
                if (testProcess.ExitCode != 0)
                {
                    pythonCommand = "python";
                }
            }
            else
            {
                pythonCommand = "python";
            }
        }
        catch
        {
            pythonCommand = "python";
        }

        var binDir = Path.Combine(GodotDir, "bin");
        var nupkgDir = Path.Combine(binDir, "GodotSharp", "NuPkgs");
        try
        {
            RunCommand(pythonCommand, $"\"{buildScript}\" --godot-output-dir=\"{binDir}\" --push-nupkgs-local \"{nupkgDir}\"", GodotDir);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Assembly build failed: {ex.Message}");
        }
    }

    static void CopyGodotSharpAssemblies()
    {
        var sourceDir = Path.Combine(GodotDir, "bin", "GodotSharp");

        if (!Directory.Exists(sourceDir))
        {
            Console.WriteLine($"Warning: GodotSharp directory not found at {sourceDir}");
            Console.WriteLine("This might be because:");
            Console.WriteLine("  - The editor build with Mono support failed");
            Console.WriteLine("  - The C# glue generation failed");
            Console.WriteLine("  - The assembly build script failed");
            return;
        }

        try
        {
            // Copy to src/GodotAssemblies for the example project to use
            Directory.CreateDirectory(GodotAssembliesDir);
            CopyDirectory(sourceDir, GodotAssembliesDir);
            Console.WriteLine($"Copied GodotSharp assemblies to {GodotAssembliesDir}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not copy GodotSharp assemblies: {ex.Message}");
        }
    }

    static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, destSubDir);
        }
    }
}
