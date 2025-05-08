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

using ImGuiNET;
using System;
using System.Linq;
using LabelChecker.IO;
using Microsoft.Xna.Framework;
using LabelChecker.Structs;
using LabelChecker.Enums;
using System.Reflection;
using System.IO;
using LabelChecker.Addons;

namespace LabelChecker.Controls
{
    public class MainMenu : Control
    {

        public override string Name => "MainMenu";
        public MainMenuEventTracker EventTracker = new();
        public float LabelFontSize = 1.25f;
        public PropertyInfo SelectedProperty = null;
        readonly string[] sort_by_column_names = ["Id", "AbdArea", "AbdDiameter", "AbdVolume", "CircleFit", "Length", "Roughness", "Transparency", "Width", "ProbabilityScore", "BiovolumeHSosik", "SurfaceAreaHSosik"];
        readonly string[] to_exclude = ["GroupId", "SrcImage", "SrcX", "SrcY", "ImageX", "ImageY", "ImageId", "ImageW", "ImageH", "IntensityCalimage", "ElapsedTime", "CalConst", "CalImage"];
        public bool AutoFontSize = false;
        public bool CheckedOnly = false;
        public bool NotCheckedOnly = false;
        public bool FullScreenMode = false;

        public MainMenu(Application app) : base(app)
        {
        }

        public override void Update(GameTime time)
        {

        }
        public override void Draw(ImGuiIOPtr iOPtr)
        {
            #region MainMenu
            if (ImGui.BeginMainMenuBar())
            {
                #region MainMenu -> File
                if (ImGui.BeginMenu("File"))
                {
                    // initialize FileMenu event tracker
                    var fileMenuEventTracker = EventTracker.FileMenu;

                    // Open labelChecker file
                    if (ImGui.MenuItem("Open file"))
                    {
                        // set Filetype
                        App.Filetype = Filetype.LabelCheckerFile;

                        // open file browser
                        fileMenuEventTracker.OpenLabelCheckerFile = !EventTracker.FileMenu.OpenLabelCheckerFile;
                    }

                    // Open labelChecker files
                    if (ImGui.MenuItem("Open folder"))
                    {
                        // set Filetype
                        App.Filetype = Filetype.LabelCheckerFile;

                        // open folder browser
                        fileMenuEventTracker.OpenLabelCheckerFiles = !EventTracker.FileMenu.OpenLabelCheckerFiles;
                    }

                    if (ImGui.BeginMenu("Open recent file"))
                    {
                        if (App.imGuiSettings.RecentLabelCheckerFiles.Count != 0)
                        {
                            foreach (string recentFile in App.imGuiSettings.RecentLabelCheckerFiles)
                            {
                                if (Path.Exists(recentFile))
                                {
                                    if (ImGui.MenuItem(Path.GetFileName(recentFile)))
                                    {
                                        // set Filetype
                                        App.Filetype = Filetype.LabelCheckerFile;

                                        // Open file
                                        App.BackgroundReaderControl.ReadFile(recentFile);

                                        // Update FilePicker selected file to update ImageGrid title
                                        if (App.OpenLabelCheckerFilePicker == null)
                                        {
                                            App.OpenLabelCheckerFilePicker = LabelCheckerFilePicker.GetFilePicker(this, recentFile, ".csv");
                                        }
                                        App.OpenLabelCheckerFilePicker.SelectedFile = recentFile;
                                        App.OpenLabelCheckerFilePicker.CurrentFolder = Path.GetDirectoryName(recentFile);

                                        // flag new File
                                        App.SaveWindow.SaveMessage = "";
                                        App.LabelSelectionControl.Reinitialize = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, Color.DimGray.PackedValue);
                            ImGui.Text("...");
                            ImGui.PopStyleColor();
                        }
                        ImGui.EndMenu();

                        // set opened item to top of the recent files list
                        if (App.OpenLabelCheckerFilePicker != null && App.OpenLabelCheckerFilePicker.SelectedFile != null)
                        {
                            App.Utils.UpdateLabelCheckerToRecentFiles(App.OpenLabelCheckerFilePicker.SelectedFile);
                        }
                    }

                    // Save (as) LabelChecker File
                    if (App.ActiveLabelCheckerFile == null)
                    // grey-out save options if no LabelChecker file is active
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, Color.DimGray.PackedValue);
                        ImGui.Text("Save");
                        ImGui.Text("Save as");
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        // TODO add "Save successful pop-up 
                        if (ImGui.MenuItem("Save"))
                        {
                            // save data to the opened file
                            try
                            {
                                App.DataManager.Save(manualSave: true);
                            }
                            catch (Exception e)
                            {
                                App.ErrorWindow.ErrorPopupMessage = e.Message;
                            }
                        }
                        if (ImGui.MenuItem("Save as"))
                        {
                            // set Filetype
                            App.Filetype = Filetype.LabelCheckerFile;

                            // save data at different location  or different name
                            fileMenuEventTracker.SaveLabelCheckerFile = !EventTracker.FileMenu.SaveLabelCheckerFile;
                        }
                    }

                    if (ImGui.BeginMenu("Settings"))
                    {
                        if (ImGui.MenuItem("Full screen", "", FullScreenMode))
                        {
                            FullScreenMode = !FullScreenMode;
                            App.Utils.SaveFullScreenMode(FullScreenMode);
                        }
                        if (ImGui.MenuItem("Show numpad", "", App.imGuiSettings.ShowNumpad))
                        {
                            App.imGuiSettings.ShowNumpad = !App.imGuiSettings.ShowNumpad;
                            App.Utils.SaveShowNumpad(App.imGuiSettings.ShowNumpad);
                        }
                        if (ImGui.BeginMenu("Theme"))
                        {
                            if (ImGui.MenuItem("Default"))
                            {
                                App.ThemeManager.SetTheme(ImGuiTheme.Default);
                            }
                            if (ImGui.MenuItem("Light"))
                            {
                                App.ThemeManager.SetTheme(ImGuiTheme.White);
                            }
                            if (ImGui.MenuItem("Dark"))
                            {
                                App.ThemeManager.SetTheme(ImGuiTheme.Dark);
                            }
                            if (ImGui.MenuItem("Classic"))
                            {
                                App.ThemeManager.SetTheme(ImGuiTheme.Classic);
                            }
                            if (ImGui.MenuItem("Monokai"))
                            {
                                App.ThemeManager.SetTheme(ImGuiTheme.Monokai);
                            }
                            ImGui.EndMenu();
                        }
                        if (ImGui.MenuItem("Auto size object labels", "", AutoFontSize))
                        {
                            AutoFontSize = !AutoFontSize;
                            App.Utils.SaveAutoFontSize(AutoFontSize);
                        }
                        if (ImGui.BeginMenu("Font size"))
                        {
                            if (ImGui.MenuItem("Reset"))
                            {
                                LabelFontSize = 0.9f;
                                App.Utils.SaveFontSize(LabelFontSize);
                            }
                            if (ImGui.MenuItem("50%"))
                            {
                                LabelFontSize = .5f;
                                App.Utils.SaveFontSize(LabelFontSize);
                            }
                            if (ImGui.MenuItem("75%"))
                            {
                                LabelFontSize = .75f;
                                App.Utils.SaveFontSize(LabelFontSize);
                            }
                            if (ImGui.MenuItem("100%"))
                            {
                                LabelFontSize = 1.0f;
                                App.Utils.SaveFontSize(LabelFontSize);
                            }
                            if (ImGui.MenuItem("125%"))
                            {
                                LabelFontSize = 1.25f;
                                App.Utils.SaveFontSize(LabelFontSize);
                            }
                            if (ImGui.MenuItem("150%"))
                            {
                                LabelFontSize = 1.5f;
                                App.Utils.SaveFontSize(LabelFontSize);
                            }
                            if (ImGui.MenuItem("200%"))
                            {
                                LabelFontSize = 2.0f;
                                App.Utils.SaveFontSize(LabelFontSize);
                            }
                            ImGui.EndMenu();
                        }
                        if (ImGui.MenuItem("Show file info", "", App.ImageGridControl.ShowFileInfo))
                        {
                            App.ImageGridControl.ShowFileInfo = !App.ImageGridControl.ShowFileInfo;
                            App.Utils.SaveShowFileInfo(App.ImageGridControl.ShowFileInfo);
                        }
                        if (ImGui.MenuItem("Scale Images to fit", "", App.ImageGridControl.ScaleImage))
                        {
                            App.ImageGridControl.ScaleImage = !App.ImageGridControl.ScaleImage;
                            App.Utils.SaveImageScale(App.ImageGridControl.ScaleImage);
                        }
                        if (ImGui.BeginMenu("Magnification"))
                        {
                            if (ImGui.MenuItem("5x", "", App.ImageGridControl.MagnificationFactor == 5 & App.ImageGridControl.Magnification))
                            {
                                App.ImageGridControl.Magnification = App.ImageGridControl.MagnificationFactor != 5 | App.ImageGridControl.Magnification != true;
                                App.ImageGridControl.MagnificationFactor = 5;
                                App.Utils.SaveMagnification(App.ImageGridControl.Magnification, App.ImageGridControl.MagnificationFactor);
                            }
                            if (ImGui.MenuItem("10x", "", App.ImageGridControl.MagnificationFactor == 10 & App.ImageGridControl.Magnification))
                            {
                                App.ImageGridControl.Magnification = App.ImageGridControl.MagnificationFactor != 10 | App.ImageGridControl.Magnification != true;
                                App.ImageGridControl.MagnificationFactor = 10;
                                App.Utils.SaveMagnification(App.ImageGridControl.Magnification, App.ImageGridControl.MagnificationFactor);
                            }
                            if (ImGui.MenuItem("20x", "", App.ImageGridControl.MagnificationFactor == 20 & App.ImageGridControl.Magnification))
                            {
                                App.ImageGridControl.Magnification = App.ImageGridControl.MagnificationFactor != 20 | App.ImageGridControl.Magnification != true;
                                App.ImageGridControl.MagnificationFactor = 20;
                                App.Utils.SaveMagnification(App.ImageGridControl.Magnification, App.ImageGridControl.MagnificationFactor);
                            }
                            ImGui.EndMenu();
                        }

                        ImGui.EndMenu();
                    }

                    // Close application
                    if (ImGui.MenuItem("Exit"))
                    {
                        fileMenuEventTracker.CloseApplication = !EventTracker.FileMenu.CloseApplication;

                    }
                    //TODO OPEN RECENT FILE
                    ImGui.EndMenu();

                    // update FileMenu event tracker
                    EventTracker.FileMenu = fileMenuEventTracker;
                }
                #endregion MainMenu -> File

                #region MainMenu -> Edit
                // edit menu
                if (ImGui.BeginMenu("Edit"))
                {
                    if (App.ActiveLabelCheckerFile == null)
                    {
                        // grey out menu
                        ImGui.PushStyleColor(ImGuiCol.Text, Color.DimGray.PackedValue);
                        ImGui.Text("Undo");
                        ImGui.Text("Redo");
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        // initialize EditMenu event tracker
                        var editMenuEventTracker = EventTracker.EditMenu;
                        // Undo action
                        if (ImGui.MenuItem("Undo"))
                        {
                            App.DataManager.UndoChanges();
                        }
                        // Redo action
                        if (ImGui.MenuItem("Redo"))
                        {
                            App.DataManager.ReDoChanges();
                        }
                        // update EditMenu event tracker
                        EventTracker.EditMenu = editMenuEventTracker;
                    }
                    ImGui.EndMenu();
                }
                #endregion MainMenu -> Edit

                #region MainMenu -> Data
                if (ImGui.BeginMenu("Data"))
                {
                    if (App.ActiveLabelCheckerFile == null)
                    {
                        // grey out menu
                        ImGui.PushStyleColor(ImGuiCol.Text, Color.DimGray.PackedValue);

                        ImGui.Text("Correct Label");
                        if (ImGui.BeginMenu("Show data"))
                        {
                            ImGui.Text("Not checked only");
                            ImGui.Text("Checked only");
                            ImGui.EndMenu();
                        }
                        if (ImGui.BeginMenu("Show parameter value"))
                        {
                            foreach (var property in typeof(CSVLabelCheckerFile.DataRow).GetProperties())
                            {
                                // only add option if exists in the active LabelChecker file
                                if (property.PropertyType.IsValueType && (property.PropertyType == typeof(int) || property.PropertyType == typeof(float) || property.PropertyType == typeof(double)) && !to_exclude.Contains(property.Name.ToString()))
                                {
                                    ImGui.Text(property.Name);
                                }
                            }
                            ImGui.EndMenu();
                        }
                        if (ImGui.BeginMenu("Order"))
                        {
                            ImGui.Text("Ascending");
                            ImGui.Text("Descending");
                            ImGui.EndMenu();
                        }
                        if (ImGui.BeginMenu("Sort by"))
                        {
                            foreach (var property in typeof(CSVLabelCheckerFile.DataRow).GetProperties())
                            {
                                // only add option if exists in the active LabelChecker file
                                if (sort_by_column_names.Contains(property.Name.ToString()))
                                {
                                    ImGui.Text(property.Name);
                                }
                            }
                            ImGui.EndMenu();
                        }

                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        // initialize dataMenu event tracker
                        var dataMenuEventTracker = EventTracker.DataMenu;
                        // rename label in data
                        if (App.DataProcessStep == DataProcessingStep.Preprocessing)
                        {
                            // grey out menu
                            ImGui.PushStyleColor(ImGuiCol.Text, Color.DimGray.PackedValue);
                            ImGui.Text("Correct label");
                            ImGui.PopStyleColor();
                        }
                        else
                        {
                            if (ImGui.MenuItem("Correct label"))
                            {
                                dataMenuEventTracker.CorrectLabel = !EventTracker.DataMenu.CorrectLabel;
                            }
                        }
                        // show data
                        if (ImGui.BeginMenu("Show data"))
                        {
                            if (ImGui.MenuItem("Checked only", "", CheckedOnly))
                            {
                                CheckedOnly = !CheckedOnly;
                                if (CheckedOnly)
                                {
                                    NotCheckedOnly = false;
                                }
                            }
                            if (ImGui.MenuItem("Not checked only", "", NotCheckedOnly))
                            {
                                NotCheckedOnly = !NotCheckedOnly;
                                if (NotCheckedOnly)
                                {
                                    CheckedOnly = false;
                                }
                            }
                            ImGui.EndMenu();
                        }

                        if (ImGui.BeginMenu("Show parameter value"))
                        {
                            foreach (var property in typeof(CSVLabelCheckerFile.DataRow).GetProperties())
                            {
                                // only add option if exists in the active LabelChecker file
                                if (property.PropertyType.IsValueType && (property.PropertyType == typeof(int) || property.PropertyType == typeof(float) || property.PropertyType == typeof(double)) && !to_exclude.Contains(property.Name.ToString()))
                                {
                                    if (ImGui.MenuItem(property.Name, "", App.ImageGridControl.ShowParameterValue & App.ImageGridControl.ParameterValueName == property.Name))
                                    {
                                        App.ImageGridControl.ParameterValueName = property.Name == App.ImageGridControl.ParameterValueName
                                            ? ""
                                            : property.Name;
                                        if (App.ImageGridControl.ParameterValueName == "")
                                        {
                                            App.ImageGridControl.ShowParameterValue = false;
                                        }
                                        else
                                        {
                                            App.ImageGridControl.ShowParameterValue = true;
                                        }
                                        App.Utils.SaveShowParameterValue(App.ImageGridControl.ShowParameterValue, App.ImageGridControl.ParameterValueName);
                                    }
                                }
                            }
                            ImGui.EndMenu();
                        }
                        if (ImGui.BeginMenu("Order"))
                        {
                            if (ImGui.MenuItem("Ascending"))
                            {
                                dataMenuEventTracker.IsDescendingOrder = false;
                            }
                            if (ImGui.MenuItem("Descending"))
                            {
                                dataMenuEventTracker.IsDescendingOrder = true;
                            }
                            ImGui.EndMenu();
                        }
                        // set parameter used for sorting the data
                        if (ImGui.BeginMenu("Sort by"))
                        {
                            foreach (var property in typeof(CSVLabelCheckerFile.DataRow).GetProperties())
                            {
                                // only add option if exists in the active LabelChecker file
                                if (sort_by_column_names.Contains(property.Name.ToString()))
                                {
                                    if (ImGui.MenuItem(property.Name))
                                    {
                                        SelectedProperty = property;
                                    }
                                }
                            }
                            ImGui.EndMenu();
                        }

                        // data stats menu
                        if (ImGui.BeginMenu("Stats"))
                        {
                            if (ImGui.MenuItem("Labels confirmed"))
                            {
                                dataMenuEventTracker.ShowStats = true;
                            }
                            ImGui.EndMenu();
                        }
                        // Update dataMenu event tracker
                        EventTracker.DataMenu = dataMenuEventTracker;
                    }
                    ImGui.EndMenu();
                }
                #endregion MainMenu -> Data


                #region MainMenu -> Labels
                if (ImGui.BeginMenu("Labels"))
                {
                    // initialize LabelFileMenu event tracker
                    var labelFileMenuEventTracker = EventTracker.LabelFileMenu;
                    // New Label file
                    if (ImGui.MenuItem("New file"))
                    {
                        // create new active file
                        App.ActiveLabelsFile = CSVLabelFile.Create(this.App);
                    }

                    // Open label file
                    if (ImGui.MenuItem("Open file"))
                    {
                        // set Filetype 
                        App.Filetype = Filetype.LabelsFile;

                        // open file browser
                        labelFileMenuEventTracker.OpenLabelListFile = !EventTracker.LabelFileMenu.OpenLabelListFile;
                    }
                    if (ImGui.BeginMenu("Open recent file"))
                    {
                        if (App.imGuiSettings.RecentLabelFiles.Count != 0)
                        {
                            foreach (string recentFile in App.imGuiSettings.RecentLabelFiles)
                            {
                                if (Path.Exists(recentFile))
                                {
                                    if (ImGui.MenuItem(Path.GetFileName(recentFile)))
                                    {
                                        // set Filetype
                                        App.Filetype = Filetype.LabelsFile;

                                        // Open file
                                        App.ActiveLabelsFile = CSVLabelFile.Read(recentFile, App);

                                        // Update FilePicker selected file to update ImageGrid title
                                        if (App.OpenLabelPicker == null)
                                        {
                                            App.OpenLabelPicker = LabelFilePicker.GetFilePicker(this, recentFile, ".csv");
                                        }
                                        App.OpenLabelPicker.SelectedFile = recentFile;
                                        App.OpenLabelPicker.CurrentFolder = Path.GetDirectoryName(recentFile);

                                        // flag new File
                                        App.LabelSelectionControl.Reinitialize = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, Color.DimGray.PackedValue);
                            ImGui.Text("...");
                            ImGui.PopStyleColor();
                        }
                        ImGui.EndMenu();

                        // set opened item to top of the recent files list
                        if (App.OpenLabelPicker != null && App.OpenLabelPicker.SelectedFile != null)
                        {
                            App.Utils.UpdateLabelToRecentFiles(App.OpenLabelPicker.SelectedFile);
                        }
                    }

                    // grey-out save, add, rename and remove options if no Labels is active
                    if (App.ActiveLabelsFile == null)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, Color.DimGray.PackedValue);
                        ImGui.Text("Save");
                        ImGui.Text("Save as");
                        ImGui.Text("Add label");
                        ImGui.Text("Rename label");
                        ImGui.Text("Remove label");
                        ImGui.PopStyleColor();
                    }
                    else
                    {
                        // Save open label file
                        if (ImGui.MenuItem("Save"))
                        {
                            // set Filetype 
                            App.Filetype = Filetype.LabelsFile;

                            // check if OpenLabelPicker is already initialized
                            if (App.OpenLabelPicker == null)
                            {
                                labelFileMenuEventTracker.SaveLabelListFile = !EventTracker.LabelFileMenu.SaveLabelListFile;
                            }
                            else
                            {
                                App.LabelManager.Save();
                            }
                        }
                        if (ImGui.MenuItem("Save as"))
                        {
                            // set Filetype 
                            App.Filetype = Filetype.LabelsFile;

                            // check if OpenLabelPicker is already initialized
                            labelFileMenuEventTracker.SaveLabelListFile = !EventTracker.LabelFileMenu.SaveLabelListFile;
                        }
                        // Add label to Label file
                        if (ImGui.MenuItem("Add label"))
                        {
                            labelFileMenuEventTracker.AddLabelToLabelList = !EventTracker.LabelFileMenu.AddLabelToLabelList;
                        }
                        // Rename label from Label file
                        if (ImGui.MenuItem("Rename label"))
                        {
                            labelFileMenuEventTracker.RenameLabelInLabelList = !EventTracker.LabelFileMenu.RenameLabelInLabelList;
                        }
                        // Remove label from Label file
                        if (ImGui.MenuItem("Remove label"))
                        {
                            labelFileMenuEventTracker.RemoveLabelFromLabelList = !EventTracker.LabelFileMenu.RemoveLabelFromLabelList;
                        }
                    }
                    ImGui.EndMenu();

                    // update FileMenu event tracker
                    EventTracker.LabelFileMenu = labelFileMenuEventTracker;
                }
            }
            #endregion MainMenu -> Labels

            #region MainMenu -> Help

            if (ImGui.BeginMenu("Help"))
            {
                if (ImGui.MenuItem("About"))
                {
                    App.AboutWindow.AboutPopup = !App.AboutWindow.AboutPopup;
                }
                ImGui.EndMenu();
            }
            #endregion MainMenu -> Help





            ImGui.EndMainMenuBar();
        }
        #endregion MainMenu
    }
}
