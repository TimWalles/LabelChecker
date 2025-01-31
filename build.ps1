param(
    [Parameter()]
    [ValidateSet('all', 'windows', 'linux', 'macos')]
    [string]$Platform = 'all'
)

$PROJ_NAME = "LabelChecker"
$CONFIGURATION = "Release"

function Build-Platform {
    param (
        [string]$Runtime,
        [string]$Platform
    )
    
    Write-Host "Building for $Platform..." -ForegroundColor Green
    
    # Create platform-specific output directory
    $DIST_DIR = "dist/$Platform"
    if (Test-Path $DIST_DIR) {
        Remove-Item -Path $DIST_DIR -Recurse -Force
    }
    New-Item -ItemType Directory -Path $DIST_DIR -Force | Out-Null

    # Run build in Docker
    dotnet publish -c $CONFIGURATION -p:PublishReadyToRun=false -p:TieredCompilation=false -p:PublishAot=true --self-contained true

    # Platform-specific post-processing
    switch ($Platform) {
        'windows' {
            Copy-Item -Path "bin/$CONFIGURATION/net8.0/$Runtime/publish/*" -Destination $DIST_DIR -Recurse
            # Copy icon if exists
            if (Test-Path "Content/icon.png") {
                Copy-Item -Path "Content/icon.png" -Destination $DIST_DIR
            }
        }
        'linux' {
            $GAMES_DIR = "$DIST_DIR/usr/games/$PROJ_NAME"
            New-Item -ItemType Directory -Path $GAMES_DIR -Force | Out-Null
            Copy-Item -Path "bin/$CONFIGURATION/net8.0/$Runtime/publish/*" -Destination $GAMES_DIR -Recurse
            
            # Create .desktop file
            $APPS_DIR = "$DIST_DIR/usr/share/applications"
            New-Item -ItemType Directory -Path $APPS_DIR -Force | Out-Null
            Set-Content -Path "$APPS_DIR/$PROJ_NAME.desktop" -Value @"
[Desktop Entry]
Version=1.0
Type=Application
Name=Label Checker
Comment=Label Checking Game
Exec=/usr/games/$PROJ_NAME/$PROJ_NAME
Icon=/usr/games/$PROJ_NAME/icon.png
Categories=Game;
"@
            if (Test-Path "Content/icon.png") {
                Copy-Item -Path "Content/icon.png" -Destination $GAMES_DIR
            }
        }
        'macos' {
            $APP_NAME = "LabelChecker.app"
            $APP_CONTENTS = "$DIST_DIR/$APP_NAME/Contents"
            $MACOS_DIR = "$APP_CONTENTS/MacOS"
            $RESOURCES_DIR = "$APP_CONTENTS/Resources"
            
            New-Item -ItemType Directory -Path $MACOS_DIR -Force | Out-Null
            New-Item -ItemType Directory -Path $RESOURCES_DIR -Force | Out-Null
            
            Copy-Item -Path "bin/$CONFIGURATION/net8.0/$Runtime/publish/*" -Destination $MACOS_DIR -Recurse
            
            # Create Info.plist
            Set-Content -Path "$APP_CONTENTS/Info.plist" -Value @"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>$PROJ_NAME</string>
    <key>CFBundleIconFile</key>
    <string>icon.icns</string>
    <key>CFBundleIdentifier</key>
    <string>com.yourcompany.$PROJ_NAME</string>
    <key>CFBundleName</key>
    <string>$PROJ_NAME</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.12</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
"@
            if (Test-Path "Content/icon.png") {
                # Note: Icon conversion will need to be done on macOS
                Copy-Item -Path "Content/icon.png" -Destination $RESOURCES_DIR
            }
        }
    }
    
    Write-Host "$Platform build completed in $DIST_DIR" -ForegroundColor Green

    # Create release archive
    New-Item -ItemType Directory -Path "releases" -Force | Out-Null
    Push-Location dist
    tar -czf "../releases/$PROJ_NAME-$Platform.tar.gz" $Platform
    Pop-Location
    Write-Host "Created archive releases/$PROJ_NAME-$Platform.tar.gz" -ForegroundColor Green
}

# Build requested platform(s)
switch ($Platform) {
    'all' {
        Build-Platform -Runtime "win-x64" -Platform "windows"
        Build-Platform -Runtime "linux-x64" -Platform "linux"
        Build-Platform -Runtime "osx-x64" -Platform "macos"
        break
    }
    'windows' { Build-Platform -Runtime "win-x64" -Platform "windows"; break }
    'linux' { Build-Platform -Runtime "linux-x64"-Platform "linux"; break }
    'macos' { Build-Platform -Runtime "osx-x64" -Platform "macos"; break }
}
