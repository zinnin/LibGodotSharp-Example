# Building Linux Libraries on Windows

This guide explains how to build Godot Linux libraries from Windows using the automated WSL2 build scripts.

## Prerequisites

### 1. Install WSL2

If you don't have WSL2 installed yet:

```powershell
# Run in PowerShell as Administrator
wsl --install
```

Then restart your computer.

### 2. Verify WSL2 Installation

```powershell
wsl --list --verbose
```

You should see at least one distribution (e.g., Ubuntu) with version 2.

### 3. Install .NET SDK in WSL2

Open a WSL2 terminal and install .NET:

```bash
# In WSL2 terminal
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Add to PATH
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc

# Verify installation
dotnet --version
```

### 4. Install Build Dependencies in WSL2

```bash
# In WSL2 terminal
sudo apt update
sudo apt install -y build-essential scons pkg-config libx11-dev libxcursor-dev \
    libxinerama-dev libgl1-mesa-dev libglu-dev libasound2-dev libpulse-dev \
    libudev-dev libxi-dev libxrandr-dev python3 git
```

## Building Linux Libraries - Three Methods

### Method 1: Visual Studio (Single Click) ⭐ **RECOMMENDED**

This is the easiest method:

1. Open `LibGodotSharp.sln` in Visual Studio
2. Set **GodotBuilder** as the startup project (right-click → Set as Startup Project)
3. In the toolbar, select the launch profile: **"Build Linux via WSL2 (Windows)"**
4. Click **Start** (▶️) or press **F5**

The build will run automatically in WSL2. Progress will be shown in the output window.

### Method 2: PowerShell Script

Run from the repository root:

```powershell
.\Build-Linux.ps1
```

The script will:
- ✅ Check for WSL2 installation
- ✅ Verify .NET SDK in WSL2
- ✅ Convert Windows paths to WSL2 paths
- ✅ Launch the build inside WSL2
- ✅ Show detailed progress

### Method 3: Batch File

For Command Prompt users:

```cmd
Build-Linux.bat
```

This calls the PowerShell script with proper execution policy.

## What the Scripts Do

The automated build scripts (`Build-Linux.ps1` and `Build-Linux.bat`):

1. **Check Prerequisites**:
   - Verify WSL2 is installed
   - Check for .NET SDK in WSL2
   - List available WSL2 distributions

2. **Path Conversion**:
   - Automatically convert Windows paths (e.g., `C:\Projects\LibGodotSharp-Example`)
   - To WSL2 paths (e.g., `/mnt/c/Projects/LibGodotSharp-Example`)

3. **Build Execution**:
   - Run `dotnet run --project GodotBuilder/GodotBuilder.csproj -- linuxbsd` inside WSL2
   - Stream output in real-time
   - Handle errors gracefully

4. **Result**:
   - On success: Linux library files in `lib/linux/`
   - On failure: Detailed error messages and troubleshooting tips

## Build Time

- **First build**: 45-60 minutes (downloads Godot source, compiles everything)
- **Subsequent builds**: 30-45 minutes (incremental compilation)

## Output Files

After successful build, you'll find:

```
lib/linux/
├── libgodot.linuxbsd.template_release.x86_64.so
└── (other Linux library files)
```

## Troubleshooting

### "WSL2 is not installed"

**Solution**: Install WSL2
```powershell
# As Administrator
wsl --install
```
Then restart your computer.

### ".NET SDK is not installed in WSL2"

**Solution**: Follow the installation steps in Prerequisites section above.

### "No WSL2 distributions found"

**Solution**: Install a Linux distribution
```powershell
wsl --install -d Ubuntu
```

### Build fails with "command not found: dotnet"

**Solution**: Make sure .NET is in your PATH in WSL2
```bash
# In WSL2
export PATH=$PATH:$HOME/.dotnet
dotnet --version
```

If it works, add to `.bashrc`:
```bash
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
```

### Build fails with missing dependencies

**Solution**: Install build dependencies in WSL2
```bash
# In WSL2
sudo apt update
sudo apt install -y build-essential scons pkg-config libx11-dev libxcursor-dev \
    libxinerama-dev libgl1-mesa-dev libglu-dev libasound2-dev libpulse-dev \
    libudev-dev libxi-dev libxrandr-dev python3 git
```

### "Cannot find the path" errors

This usually means the path conversion failed. Try:
1. Make sure you're in the repository root when running the script
2. Check that your path doesn't have special characters
3. Try the manual method instead (see below)

## Manual Method (Advanced)

If the automated scripts don't work, you can build manually:

1. Open WSL2:
   ```cmd
   wsl
   ```

2. Navigate to your project (adjust path as needed):
   ```bash
   cd /mnt/c/Users/YourUsername/Projects/LibGodotSharp-Example
   ```

3. Run the builder:
   ```bash
   dotnet run --project GodotBuilder/GodotBuilder.csproj -- linuxbsd
   ```

## Additional Resources

- [WSL2 Installation Guide](https://learn.microsoft.com/en-us/windows/wsl/install)
- [.NET SDK Installation](https://dotnet.microsoft.com/download)
- [Godot Compilation Guide](https://docs.godotengine.org/en/stable/contributing/development/compiling/)
- [Cross-Compilation Documentation](CROSS_COMPILATION.md)

## Tips

1. **Use an SSD**: Building Godot is I/O intensive. WSL2 on SSD is significantly faster.

2. **Allocate Resources**: By default, WSL2 uses up to 50% of your RAM. For faster builds, you can increase this by creating `.wslconfig`:
   ```ini
   # C:\Users\YourUsername\.wslconfig
   [wsl2]
   memory=8GB
   processors=4
   ```

3. **Keep WSL2 Updated**:
   ```powershell
   wsl --update
   ```

4. **Clean Builds**: If you encounter issues, try cleaning:
   ```bash
   # In WSL2, in project directory
   rm -rf godot/
   ```
   Then run the build again.
