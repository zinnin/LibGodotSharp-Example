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
    private static string Platform = string.Empty;
    private static string LibExt = string.Empty;
    private static string PlatformDir = string.Empty;

    static int Main(string[] args)
    {
        try
        {
            // Determine script directory (use current directory if running from source)
            ScriptDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
            if (!File.Exists(Path.Combine(ScriptDir, "build-godot.sh")))
            {
                // Running from published build, use working directory
                ScriptDir = Directory.GetCurrentDirectory();
            }

            GodotDir = Path.Combine(ScriptDir, "godot");
            BuildDir = Path.Combine(ScriptDir, "build");

            Console.WriteLine("Building Godot as a library...");
            Console.WriteLine($"Script directory: {ScriptDir}");
            Console.WriteLine($"Godot directory: {GodotDir}");
            Console.WriteLine($"Build directory: {BuildDir}");
            Console.WriteLine();

            // Detect platform
            DetectPlatform();
            Console.WriteLine($"Detected platform: {Platform} (lib ext: {LibExt}, dir: {PlatformDir})");
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

            // Build Godot library
            Console.WriteLine($"Building Godot library for {Platform}...");
            RunCommand("scons", $"platform={Platform} target=template_release library_type=shared_library", GodotDir);
            Console.WriteLine();

            // Copy library files
            Directory.CreateDirectory(Path.Combine(BuildDir, PlatformDir));
            CopyLibraryFiles();
            Console.WriteLine();

            Console.WriteLine("Godot library build complete!");
            Console.WriteLine($"Library files are in: {BuildDir}");
            Console.WriteLine();

            // Build GodotSharp bindings
            Console.WriteLine("Building GodotSharp bindings...");
            Console.WriteLine("Building Godot editor with Mono support...");
            RunCommand("scons", $"platform={Platform} target=editor module_mono_enabled=yes", GodotDir);
            Console.WriteLine();

            // Generate C# glue
            Console.WriteLine("Generating C# glue code...");
            GenerateCSharpGlue();
            Console.WriteLine();

            // Build C# assemblies
            Console.WriteLine("Building C# assemblies...");
            BuildCSharpAssemblies();
            Console.WriteLine();

            // Copy GodotSharp assemblies
            Console.WriteLine("Copying GodotSharp assemblies to build directory...");
            CopyGodotSharpAssemblies();
            Console.WriteLine();

            Console.WriteLine("Build complete!");
            Console.WriteLine($"Library files are in: {Path.Combine(BuildDir, PlatformDir)}");
            Console.WriteLine($"GodotSharp assemblies are in: {Path.Combine(BuildDir, "GodotSharp")}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
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

    static void CopyLibraryFiles()
    {
        var binDir = Path.Combine(GodotDir, "bin");
        string? libFile = null;

        // Try godot.* pattern first
        var pattern1 = $"godot.{Platform}.*.{LibExt}";
        var files = Directory.GetFiles(binDir, pattern1, SearchOption.TopDirectoryOnly);
        if (files.Length > 0)
        {
            libFile = files[0];
        }
        else
        {
            // Try libgodot.* pattern as fallback
            var pattern2 = $"libgodot.{Platform}.*.{LibExt}";
            files = Directory.GetFiles(binDir, pattern2, SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                libFile = files[0];
            }
        }

        if (libFile != null)
        {
            var destFile = Path.Combine(BuildDir, PlatformDir, Path.GetFileName(libFile));
            File.Copy(libFile, destFile, overwrite: true);
            Console.WriteLine($"Copied {Path.GetFileName(libFile)} to {Path.Combine(BuildDir, PlatformDir)}");
        }
        else
        {
            Console.WriteLine($"Warning: Could not find built library (tried godot.{Platform}.*.{LibExt} and libgodot.{Platform}.*.{LibExt})");
        }
    }

    static void GenerateCSharpGlue()
    {
        var binDir = Path.Combine(GodotDir, "bin");
        // Pattern needs to match files like godot.windows.editor.x86_64.mono.exe
        var pattern = $"godot.{Platform}.editor.*.mono*";
        var files = Directory.GetFiles(binDir, pattern, SearchOption.TopDirectoryOnly);

        if (files.Length == 0)
        {
            Console.WriteLine($"Warning: Could not find editor binary (godot.{Platform}.editor.*.mono*)");
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
        var destDir = Path.Combine(BuildDir, "GodotSharp");

        if (!Directory.Exists(sourceDir))
        {
            Console.WriteLine($"Warning: GodotSharp directory not found at {sourceDir}");
            return;
        }

        try
        {
            Directory.CreateDirectory(destDir);
            CopyDirectory(sourceDir, destDir);
            Console.WriteLine($"Copied GodotSharp assemblies to {destDir}");
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
