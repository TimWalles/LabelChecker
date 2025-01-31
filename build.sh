#!/bin/bash

PROJ_NAME="LabelChecker"
CONFIGURATION="Release"

# Function to validate platform argument
valid_platform() {
    case "$1" in
        all|windows|linux|macos) return 0 ;;
        *) return 1 ;;
    esac
}

# Check platform argument
PLATFORM=${1:-all}
if ! valid_platform "$PLATFORM"; then
    echo "Invalid platform. Use: all, windows, linux, or macos"
    exit 1
fi

# Function to build for specific platform
build_platform() {
    local runtime=$1
    local platform=$2
    
    echo "Building for $platform..."
    
    # Create platform-specific output directory
    DIST_DIR="dist/$platform"
    rm -rf "$DIST_DIR"
    mkdir -p "$DIST_DIR"

    # Build the application
    dotnet publish -c $CONFIGURATION \
        -p:PublishReadyToRun=false \
        -p:TieredCompilation=false \
        -p:PublishAot=true \
        --self-contained true \
        -r $runtime

    # Platform-specific post-processing
    case "$platform" in
        "windows")
            cp -r "bin/$CONFIGURATION/net8.0/$runtime/publish/"* "$DIST_DIR/"
            # Copy icon if exists
            if [ -f "Content/icon.png" ]; then
                cp "Content/icon.png" "$DIST_DIR/"
            fi
            ;;
            
        "linux")
            GAMES_DIR="$DIST_DIR/usr/games/$PROJ_NAME"
            mkdir -p "$GAMES_DIR"
            cp -r "bin/$CONFIGURATION/net8.0/$runtime/publish/"* "$GAMES_DIR/"
            
            # Copy icon if exists
            if [ -f "Content/icon.png" ]; then
                cp "Content/icon.png" "$GAMES_DIR/"
            fi
            
            # Create .desktop file
            APPS_DIR="$DIST_DIR/usr/share/applications"
            mkdir -p "$APPS_DIR"
            cat > "$APPS_DIR/$PROJ_NAME.desktop" << EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=LabelChecker
Comment=Label Checking Game
Exec=/usr/games/$PROJ_NAME/$PROJ_NAME
Icon=/usr/games/$PROJ_NAME/Content/Icon.png
Categories=Game;
EOF
            ;;
            
        "macos")
            APP_NAME="LabelChecker.app"
            APP_CONTENTS="$DIST_DIR/$APP_NAME/Contents"
            MACOS_DIR="$APP_CONTENTS/MacOS"
            RESOURCES_DIR="$APP_CONTENTS/Resources"
            
            mkdir -p "$MACOS_DIR"
            mkdir -p "$RESOURCES_DIR"
            
            cp -r "bin/$CONFIGURATION/net8.0/$runtime/publish/"* "$MACOS_DIR/"
            
            # Convert and copy icon if exists
            if [ -f "Content/icon.png" ]; then
                echo "Converting icon.png to icns..."
                mkdir -p icon.iconset
                sips -z 16 16   Content/icon.png --out icon.iconset/icon_16x16.png
                sips -z 32 32   Content/icon.png --out icon.iconset/icon_16x16@2x.png
                sips -z 32 32   Content/icon.png --out icon.iconset/icon_32x32.png
                sips -z 64 64   Content/icon.png --out icon.iconset/icon_32x32@2x.png
                sips -z 128 128 Content/icon.png --out icon.iconset/icon_128x128.png
                sips -z 256 256 Content/icon.png --out icon.iconset/icon_128x128@2x.png
                sips -z 256 256 Content/icon.png --out icon.iconset/icon_256x256.png
                sips -z 512 512 Content/icon.png --out icon.iconset/icon_256x256@2x.png
                sips -z 512 512 Content/icon.png --out icon.iconset/icon_512x512.png
                sips -z 1024 1024 Content/icon.png --out icon.iconset/icon_512x512@2x.png
                iconutil -c icns icon.iconset
                mv icon.icns "$RESOURCES_DIR/"
                rm -rf icon.iconset
            fi
            
            # Create Info.plist
            cat > "$APP_CONTENTS/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>$PROJ_NAME</string>
    <key>CFBundleIconFile</key>
    <string>icon</string>
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
EOF
            # Make the executable actually executable
            chmod +x "$MACOS_DIR/$PROJ_NAME"
            ;;
    esac
    
    echo "$platform build completed in $DIST_DIR"

    # Create release archive
    mkdir -p releases
    cd dist
    tar -czf "../releases/$PROJ_NAME-$platform.tar.gz" "$platform"
    cd ..
    echo "Created archive releases/$PROJ_NAME-$platform.tar.gz"
}

# Build requested platform(s)
case "$PLATFORM" in
    "all")
        build_platform "win-x64" "windows"
        build_platform "linux-x64" "linux"
        build_platform "osx-x64" "macos"
        ;;
    *)
        case "$PLATFORM" in
            "windows") build_platform "win-x64" "windows" ;;
            "linux") build_platform "linux-x64" "linux" ;;
            "macos") build_platform "osx-x64" "macos" ;;
        esac
        ;;
esac
