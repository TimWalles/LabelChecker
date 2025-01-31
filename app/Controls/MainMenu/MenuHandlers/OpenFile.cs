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
using System.IO;
using Num = System.Numerics;
using LabelChecker.Addons;
using LabelChecker.Enums;
using LabelChecker.IO;
using Microsoft.Xna.Framework.Input;

namespace LabelChecker.Controls
{
    public class OpenFile(Application app) : Control(app)
    {
        public override string Name => "OpenFile";
        public override void Draw(ImGuiIOPtr iOPtr)
        {
            ImGui.SetNextWindowSize(new Num.Vector2(1200, 800), ImGuiCond.FirstUseEver);
            App.Utils.CenterWindow();
            if (ImGui.BeginPopupModal("please select file."))
            {
                // TODO Refactor code
                switch (App.Filetype)
                {
                    case Filetype.LabelCheckerFile:
                        // close current CSV active file
                        App.ActiveLabelCheckerFile = null;

                        // open file picker and look for .csv files
                        App.OpenLabelCheckerFilePicker = LabelCheckerFilePicker.GetFilePicker(this, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".csv");
                        if (App.OpenLabelCheckerFilePicker.Draw())
                        {
                            if (!string.IsNullOrEmpty(App.OpenLabelCheckerFilePicker.SelectedFile))
                            {
                                var isReadonly = new FileInfo(App.OpenLabelCheckerFilePicker.SelectedFile).Attributes.HasFlag(System.IO.FileAttributes.ReadOnly);
                                // check writing permissions on the file
                                if (isReadonly)
                                {
                                    App.ErrorWindow.ErrorPopupMessage = "You have no writing permission to this file. Try running the program as administrator.";
                                }
                                else
                                {
                                    // open the selected .csv file
                                    App.BackgroundReaderControl.ReadFile(App.OpenLabelCheckerFilePicker.SelectedFile);
                                    App.SaveWindow.SaveMessage = "";
                                }
                            }
                        }

                        // flag new File
                        App.LabelSelectionControl.Reinitialize = true;
                        break;

                    case Filetype.LabelsFile:
                        // close current CSV active file
                        App.ActiveLabelsFile = null;

                        // open file picker and look for .csv files
                        App.OpenLabelPicker = LabelFilePicker.GetFilePicker(this, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ".csv");
                        if (App.OpenLabelPicker.Draw())
                        {
                            // open the selected .csv file
                            App.ActiveLabelsFile = CSVLabelFile.Read(App.OpenLabelPicker.SelectedFile, this.App);

                            // Add opened file to recent files list and update user settings
                            if (App.ActiveLabelsFile != null)
                            {
                                App.Utils.SaveLabelToRecentFiles(App.OpenLabelPicker.SelectedFile);
                            }
                        }

                        // flag new File
                        App.LabelSelectionControl.Reinitialize = true;
                        break;

                    default:
                        break;

                }
                // in case of unwanted clicking of an image.
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    App.ImageGridControl.SelectedImageGridIndices.Clear();
                    ImGui.CloseCurrentPopup();

                    // flag a popup to be closed
                    App.HotkeyManager.DisableHotkeys = false;
                }
                ImGui.EndPopup();

                // enable hotkeys again
                App.HotkeyManager.DisableHotkeys = false;
            }
        }
    }
}