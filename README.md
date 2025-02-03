# LabelChecker Application

## Download
You can find the compiled version of the program for Windows, Linux and MasOS in the [releases](https://github.com/TimWalles/LabelChecker/releases/laterst).

## Overview
LabelChecker is a desktop application built with MonoGame and ImGui.NET for processing and managing label data files. It works in conjunction with the [LabelChecker Data Pipeline](https://github.com/TimWalles/LabelChecker_Pipeline) to provide a user-friendly interface for:
- Managing and editing label files
- Processing image data with associated labels
- Validating and correcting pipeline outputs

## Features
- Label management (add, rename, remove labels)
- Image grid visualization
- Parameter value display
- Data sorting and filtering
- Support for checked/unchecked data states
- Customizable UI with multiple themes
- Magnification options (5x, 10x, 20x)
- Undo/Redo functionality

## Prerequisites

### Windows
- .NET 8.0 SDK
- Visual Studio 2022 or later with:
  - .NET desktop development workload
  - MSBuild support
- Git

### Linux (Ubuntu/Debian)
- .NET 8.0 SDK
- Development libraries:
  ```bash
  sudo apt-get install -y libopenal-dev
  sudo apt-get install -y libsdl2-dev
  ```
- Git

### macOS
- .NET 8.0 SDK
- Xcode Command Line Tools
- Homebrew for dependencies:
  ```bash
  brew install mono-libgdiplus
  ```
- Git

## Building from Source

### Using Build Scripts

The repository includes build scripts that handle compilation for all platforms:

#### Windows (PowerShell)
```powershell
./build.ps1 -Platform <platform>
```
Platform options: 'all', 'windows', 'linux', 'macos'

#### Linux/macOS (Bash)
```bash
./build.sh [platform]
```
Platform options: all, windows, linux, macos

The build scripts will:
1. Create platform-specific distributions in `dist/{platform}`
2. Package the application with required dependencies
3. Create distributable archives in `releases/`

### Manual Build

1. Clone the repository:
```bash
git clone https://gitlab.igb-berlin.de/data_pipelines/labelchecker.git
cd labelchecker
```

2. Build the project:
```bash
dotnet publish -c Release -p:PublishReadyToRun=false -p:TieredCompilation=false -p:PublishAot=true --self-contained true
```

The compiled application will be available in `bin/Release/net8.0/{runtime}/publish/`

## Installation

### Linux
After building, install the application system-wide:

```bash
# Create required directories
sudo mkdir -p /usr/games/labelchecker
sudo mkdir -p /usr/share/applications

# Copy application files
sudo cp -r dist/linux/usr/games/labelchecker/* /usr/games/labelchecker/
sudo cp dist/linux/usr/share/applications/labelchecker.desktop /usr/share/applications/

# Set correct permissions
sudo chmod +x /usr/games/labelchecker/LabelChecker
```

The application can then be launched either from the command line with `labelchecker` or from the system's application menu.

### macOS
After building, install the application:

```bash
\ Copy the app bundle to Applications folder
sudo cp -r dist/macos/LabelChecker.app /Applications/

\ Set correct permissions
sudo chmod -R +x /Applications/LabelChecker.app/Contents/MacOS/

\ Update system cache (if needed)
/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Support/lsregister -f /Applications/LabelChecker.app
```

The application can then be launched either from:
- The Applications folder
- Spotlight search
- Terminal with `open /Applications/LabelChecker.app`

## Usage

### Main Features
1. **File Management**
   - Open single or multiple LabelChecker files
   - Save and manage label files
   - Recent files history

2. **Label Management**
   - Create new labels with codes
   - Rename existing labels
   - Remove labels
   - Import/Export label lists

3. **Data Visualization**
   - Image grid view with customizable magnification
   - Parameter value display
   - Sort data by various properties
   - Filter by checked/unchecked status

4. **Settings**
   - Multiple UI themes (Default, Light, Dark, Classic, Monokai)
   - Adjustable font sizes
   - Full screen mode
   - Customizable image scaling
   - Optional numpad display

### Keyboard Shortcuts
The application supports various keyboard shortcuts for efficient workflow (detailed shortcuts can be found in the application's help menu).

## File Locations

Application data is stored in:
- Windows: `%LOCALAPPDATA%\LabelChecker\`
- Linux: `~/.local/share/LabelChecker/`
- macOS: `~/Library/Application Support/LabelChecker/`

## Version
Current version: 1.0.1

## Support
For issues and feature requests, please use the GitLab issue tracker.
