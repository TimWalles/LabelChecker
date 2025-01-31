/*
LabelChecker - A tool for checking and correcting image labels
Copyright (C) 2025 Tim Johannes Wim Walles

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Num = System.Numerics;
using ImGuiNET;
using LabelChecker.Addons;
using LabelChecker.Controls;
using LabelChecker.IO;
using LabelChecker.Enums;
using LabelChecker.Utilities;
using LabelChecker.Manager;
using LabelChecker.Settings;
using System.Linq;

namespace LabelChecker
{
    public class Application : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private ImGuiRenderer _imGuiRenderer;
        private Texture2D _xnaTexture;
        private IntPtr _imGuiTexture;
        public AboutWindow AboutWindow { get; private set; }  // Add this line

        #region IO 
        CSVLabelCheckerFile newCSVFlowCamFile = null;
        CSVLabelFile newCSVLabelFile = null;
        #endregion IO 

        #region Managers
        LabelCheckerFilePicker openLabelCheckerFilePicker = null;
        LabelFilePicker openLabelListFilePicker = null;
        TextureManager textureManager;
        DataManager dataManager;
        LabelManager labelManager;
        ThemeManager themeManager;
        HotkeyManager hotkeyManager;
        UndoRedoManager undoRedoManager;
        #endregion Managers

        #region Controls
        #region UI controls
        List<Control> controls;
        // MainMenuBar
        MainMenu mainMenu;
        FileMenu fileMenu;
        DataMenu dataMenu;
        LabelsMenu labelsMenu;
        OpenFile openFile;
        OpenFolder openFolder;
        BackgroundReader backgroundReader;
        SaveFile saveFile;
        // UI windows
        LabelList labelList;
        LabelSelection labelSelection;
        LabelCorrection labelCorrection;
        SetRowsColumns setRowsColumns;
        //  PageNavigation
        PageNrSelection pageNrSelection;
        PageNextPreviousSelection pageNextPreviousSelection;
        //  ImageGrid
        ImageGrid imageGrid;
        #endregion UI controls
        #endregion Controls
        Utils utilities;
        ErrorWindow errorWindow;
        SaveWindow saveWindow;
        // Installer _installer;
        // bool isDownloading = false;v
        // bool NewVersion = false;
        // readonly Version _currentVersion = null;

        #region Initialize Application parameters
        public ImGuiViewportPtr imGuiViewportPtr;
        public ImGuiSettings imGuiSettings;  // Changed from UserSettings.ImGuiSettings
        public UserSettings userSettings;
        public DataProcessingStep DataProcessStep { get; set; } = DataProcessingStep.None;
        public Filetype Filetype { get; set; }

        #endregion Initialize Application parameters

        #region Initialize IO
        public CSVLabelCheckerFile ActiveLabelCheckerFile
        {
            get { return newCSVFlowCamFile; }
            set { newCSVFlowCamFile = value; }
        }
        public CSVLabelFile ActiveLabelsFile
        {
            get { return newCSVLabelFile; }
            set { newCSVLabelFile = value; }
        }
        #endregion Initialize IO
        #region Initialize Application Controllers
        public MainMenu MainMenuControl
        {
            get { return mainMenu; }
            set { mainMenu = value; }
        }
        public BackgroundReader BackgroundReaderControl
        {
            get { return backgroundReader; }
            set { backgroundReader = value; }
        }
        public LabelSelection LabelSelectionControl
        {
            get { return labelSelection; }
            set { labelSelection = value; }
        }
        public SetRowsColumns SetRowColumnControl
        {
            get { return setRowsColumns; }
            set { setRowsColumns = value; }
        }
        public PageNrSelection PageNrSelectionControl
        {
            set { pageNrSelection = value; }
            get { return pageNrSelection; }
        }
        public PageNextPreviousSelection PageNextPreviousSelectionControl
        {
            get { return pageNextPreviousSelection; }
        }
        public ImageGrid ImageGridControl
        {
            get { return imageGrid; }
            set { imageGrid = value; }
        }
        #endregion Initialize Application Controllers
        #region Initialize Application Managers
        public DataManager DataManager
        {
            get { return dataManager; }
        }
        public TextureManager TextureManager
        {
            get { return textureManager; }
        }
        public LabelManager LabelManager
        {
            get { return labelManager; }
        }
        public HotkeyManager HotkeyManager
        {
            get { return hotkeyManager; }
            set { hotkeyManager = value; }
        }
        public UndoRedoManager UndoRedoManager
        {
            get { return undoRedoManager; }
            set { undoRedoManager = value; }
        }
        public ThemeManager ThemeManager
        {
            get { return themeManager; }
            set { themeManager = value; }
        }
        public LabelCheckerFilePicker OpenLabelCheckerFilePicker
        {
            get { return openLabelCheckerFilePicker; }
            set { openLabelCheckerFilePicker = value; }
        }
        public LabelFilePicker OpenLabelPicker
        {
            get { return openLabelListFilePicker; }
            set { openLabelListFilePicker = value; }
        }
        #endregion Initialize Application Managers

        public UserSettings UserSettings
        {
            get { return userSettings; }
            set { userSettings = value; }
        }
        public Utils Utils
        {
            get { return utilities; }
        }
        public ErrorWindow ErrorWindow
        {
            get { return errorWindow; }
            set { errorWindow = value; }
        }
        public SaveWindow SaveWindow
        {
            get { return saveWindow; }
            set { saveWindow = value; }
        }
        public ImGuiRenderer Renderer
        {
            get { return _imGuiRenderer; }
        }
        public Application()
        {
            graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
        }


        protected override void Initialize()
        {
            if (GraphicsDevice == null)
            {
                graphics.ApplyChanges();
            }

            controls = [];
            _imGuiRenderer = new ImGuiRenderer(this);
            _imGuiRenderer.RebuildFontAtlas();

            // Initialize with default settings first
            imGuiSettings = new ImGuiSettings();
            try
            {
                userSettings = new UserSettings();
                var loadedSettings = userSettings.LoadSettings(this);
                if (loadedSettings != null)
                {
                    imGuiSettings = loadedSettings;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings, using defaults: {ex.Message}");
                // Continue with default settings already created
            }

            // Apply settings
            try
            {
                graphics.IsFullScreen = imGuiSettings.FullScreenMode;
                graphics.PreferredBackBufferWidth = (int)(GraphicsDevice.Adapter.CurrentDisplayMode.Width * 0.9f);
                graphics.PreferredBackBufferHeight = (int)(GraphicsDevice.Adapter.CurrentDisplayMode.Height * 0.9f);
                graphics.PreferMultiSampling = true;
                graphics.ApplyChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying graphics settings: {ex.Message}");
                // Use default window size if graphics settings fail
                graphics.PreferredBackBufferWidth = 800;
                graphics.PreferredBackBufferHeight = 600;
                graphics.ApplyChanges();
            }

            textureManager = new TextureManager(this);
            dataManager = new DataManager(this);
            labelManager = new LabelManager(this);
            undoRedoManager = new UndoRedoManager(this);
            hotkeyManager = new HotkeyManager(this);
            themeManager = new ThemeManager(this);
            utilities = new Utils(this);
            mainMenu = new MainMenu(this);
            fileMenu = new FileMenu(this);
            dataMenu = new DataMenu(this);
            labelsMenu = new LabelsMenu(this);
            openFile = new OpenFile(this);
            openFolder = new OpenFolder(this);
            backgroundReader = new BackgroundReader(this);
            saveFile = new SaveFile(this);
            pageNrSelection = new PageNrSelection(this);
            setRowsColumns = new SetRowsColumns(this);
            labelSelection = new LabelSelection(this);
            pageNextPreviousSelection = new PageNextPreviousSelection(this);
            errorWindow = new ErrorWindow(this);
            saveWindow = new SaveWindow(this);
            imageGrid = new ImageGrid(this);
            labelCorrection = new LabelCorrection(this);
            // _installer = new Installer(this);
            labelList = new LabelList(this);
            AboutWindow = new AboutWindow(this);  // Add this line

            AddControl(mainMenu);
            AddControl(fileMenu);
            AddControl(dataMenu);
            AddControl(labelsMenu);
            AddControl(openFile);
            AddControl(openFolder);
            AddControl(backgroundReader);
            AddControl(saveFile);
            AddControl(pageNrSelection);
            AddControl(setRowsColumns);
            AddControl(labelSelection);
            AddControl(pageNextPreviousSelection);
            AddControl(errorWindow);
            AddControl(saveWindow);
            AddControl(imageGrid);
            AddControl(labelCorrection);
            AddControl(labelList);
            AddControl(hotkeyManager);
            AddControl(AboutWindow);  // Add this line

            // set user settings
            themeManager.SetTheme(imGuiSettings.Theme);
            imageGrid.ScaleImage = imGuiSettings.ScaleImage;
            imageGrid.Magnification = imGuiSettings.Magnification;
            imageGrid.ShowFileInfo = imGuiSettings.ShowFileInfo;
            imageGrid.ShowParameterValue = imGuiSettings.ShowParameterValue;
            imageGrid.ParameterValueName = imGuiSettings.ParameterValue;
            imageGrid.MagnificationFactor = imGuiSettings.MagnificationFactor;
            imageGrid.WindowSize = imGuiSettings.WindowSize != Num.Vector2.Zero ? imGuiSettings.WindowSize : new Num.Vector2(800, 600);
            imageGrid.WindowPosition = imGuiSettings.WindowPosition != Num.Vector2.Zero ? imGuiSettings.WindowPosition : new Num.Vector2(300, 50);
            mainMenu.LabelFontSize = imGuiSettings.FontSize != 0.0f ? imGuiSettings.FontSize : MainMenuControl.LabelFontSize;
            mainMenu.FullScreenMode = imGuiSettings.FullScreenMode;


            // Window.Title = "LabelChecker " + _currentVersion.ToString();//Installer.CURRENT_VERSION;
            Window.Title = "LabelChecker ";
            Window.AllowUserResizing = true;

            // System.Threading.Tasks.Task.Run(() =>
            //                         {
            //                             NewVersion = _installer.CheckForNewVersion();
            //                         });
            base.Initialize();

        }

        protected override void LoadContent()
        {
            // Texture loading example

            // First, load the texture as a Texture2D (can also be done using the XNA/FNA content pipeline)
            _xnaTexture = CreateTexture(GraphicsDevice, 300, 150, pixel =>
            {
                var red = pixel % 300 / 2;
                return new Color(red, 1, 1);
            });
            // Then, bind it to an ImGui-friendly pointer, that we can use during regular ImGui.** calls (see below)
            _imGuiTexture = _imGuiRenderer.BindTexture(_xnaTexture);

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            // Properly cleanup resources
            _xnaTexture?.Dispose();

            // Save window state before cleanup
            if (imageGrid != null)
            {
                imGuiSettings.WindowPosition = imageGrid.WindowPosition;
                imGuiSettings.WindowSize = imageGrid.WindowSize;
                userSettings.SaveSettings(imGuiSettings);  // Changed from UserSettings.SaveSettings to userSettings.SaveSettings
            }

            // Clean up controls
            foreach (var control in controls)
            {
                control?.Cleanup();
            }
            controls.Clear();

            // Cleanup ImGui
            if (_imGuiRenderer != null)
            {
                ImGui.DestroyContext();
                _imGuiRenderer = null;
            }
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);

            // Ensure proper cleanup
            UnloadContent();
        }

        // Add this new method
        protected override void OnActivated(object sender, EventArgs args)
        {
            base.OnActivated(sender, args);

            if (!IsActive)
            {
                // Reinitialize renderer if needed
                _imGuiRenderer?.RebuildFontAtlas();
            }
        }

        public void AddControl(Control control)
        {
            if (control != null)
                controls.Add(control);
        }


        public void RemoveControl(Control control)
        {
            if (control != null)
                controls.Remove(control);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(clear_color.X, clear_color.Y, clear_color.Z));
            // Call BeforeLayout first to set things up
            _imGuiRenderer.BeforeLayout(gameTime);

            var controls = this.controls.ToList();
            var io = ImGui.GetIO();

            // set global font size taken either from user settings or MainMenu control
            io.FontGlobalScale = imGuiSettings.FontSize != 0.0f
                ? imGuiSettings.FontSize
                : MainMenuControl.LabelFontSize;

            if (imGuiSettings.FullScreenMode != graphics.IsFullScreen)
                graphics.ToggleFullScreen();

            foreach (var control in controls)
            {
                control.Update(gameTime);
            }
            // Draw our UI
            ImGuiLayout();


            // Call AfterLayout now to finish up and draw all the things
            _imGuiRenderer.AfterLayout();

            base.Draw(gameTime);
        }


        public Num.Vector3 clear_color = new(114f / 255f, 144f / 255f, 154f / 255f);
        private readonly byte[] _textBuffer = new byte[100];


        public void Draw()
        {
            // if (NewVersion)
            // {
            //     if (ImGui.Begin("New Version!"))
            //     {
            //         if (ImGui.BeginChild("info", new Num.Vector2(500, 300), true))
            //         {
            //             if (!isDownloading)
            //             {
            //                 ImGui.Text($"There is a new Version: {_installer.NewVersion}");
            //                 ImGui.Text("Whats new?");
            //                 ImGui.TextWrapped(_installer.WhatsNew);
            //                 ImGui.Text("You want to update?");
            //                 ImGui.Separator();
            //                 if (ImGui.Button("Yes"))
            //                 {
            //                     isDownloading = true;
            //                     System.Threading.Tasks.Task.Run(() =>
            //                     {
            //                         _installer.Reinstall();
            //                     });

            //                 }
            //                 ImGui.SameLine();
            //                 if (ImGui.Button("No"))
            //                 {
            //                     NewVersion = false;
            //                 }
            //             }
            //             else
            //             {
            //                 ImGui.Text($"{_installer.ProgessBytes}");
            //                 ImGui.ProgressBar(_installer.Progress / 100.0f);
            //             }
            //             ImGui.EndChild();
            //         }
            //         ImGui.End();
            //     }
            //     return;
            // }

            var io = ImGui.GetIO();
            var controls = this.controls.ToList();

            // Draw all UI controllers inside the control list
            foreach (var control in controls)
            {
                control.Draw(io);
            }
        }
        protected virtual void ImGuiLayout()
        {
            Draw();
        }

        public static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Func<int, Microsoft.Xna.Framework.Color> paint)
        {
            //initialize a texture
            var texture = new Texture2D(device, width, height);

            //the array holds the color for each pixel in the texture
            Microsoft.Xna.Framework.Color[] data = new Microsoft.Xna.Framework.Color[width * height];
            for (var pixel = 0; pixel < data.Length; pixel++)
            {
                //the function applies the color according to the specified pixel
                data[pixel] = paint(pixel);
            }

            //set the color
            texture.SetData(data);

            return texture;
        }
    }
}