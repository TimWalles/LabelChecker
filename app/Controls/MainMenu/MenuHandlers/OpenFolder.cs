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
using System.Collections.Generic;
using System.Threading.Tasks;
using Num = System.Numerics;
using LabelChecker.Addons;
using Microsoft.Xna.Framework.Input;

namespace LabelChecker.Controls
{
    public class OpenFolder(Application app) : Control(app)
    {
        public override string Name => "OpenFolder";
        private List<string> files;
        public override void Draw(ImGuiIOPtr iOPtr)
        {
            ImGui.SetNextWindowSize(new Num.Vector2(1200, 800), ImGuiCond.FirstUseEver);
            App.Utils.CenterWindow();
            if (ImGui.BeginPopupModal("please select folder.###"))
            {
                // close current CSV active file
                App.ActiveLabelCheckerFile = null;

                // open file picker and look for .csv files
                App.OpenLabelCheckerFilePicker = LabelCheckerFilePicker.GetFolderPicker(this, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                if (App.OpenLabelCheckerFilePicker.Draw())
                {
                    if (App.OpenLabelCheckerFilePicker.SelectedFolder.Count > 0)
                    {
                        // get all .csv files in the selected folder
                        files = GetLabelCheckerFiles(App.OpenLabelCheckerFilePicker.SelectedFolder);
                        if (files.Count == 0)
                        {
                            App.ErrorWindow.ErrorPopupName = "NoFilesFoundException";
                            App.ErrorWindow.ErrorPopupMessage = "No LabelChecker files found in the selected folder and subfolders.";
                        }
                        else
                        {
                            App.BackgroundReaderControl.ReadFiles(files);
                            App.SaveWindow.SaveMessage = "";
                        }
                        // clear selected folders
                        App.OpenLabelCheckerFilePicker.SelectedFolder.Clear();
                        App.OpenLabelCheckerFilePicker.LastSelectedFolderIndices.Clear();
                    }
                }

                // flag new File
                App.LabelSelectionControl.Reinitialize = true;

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

        private static List<string> GetLabelCheckerFiles(List<string> SelectedFolder)
        {
            var files = new List<string>();
            Parallel.ForEach(SelectedFolder, folder =>
                {
                    DirectoryInfo directory = new(folder);
                    foreach (var file in directory.GetFiles("", SearchOption.AllDirectories))
                    {
                        if (file.Extension == ".csv" & file.Name.Contains("LabelChecker_") & new FileInfo(file.FullName).Attributes.HasFlag(FileAttributes.ReadOnly) == false & new FileInfo(file.FullName).Attributes.HasFlag(FileAttributes.Hidden) == false)
                        {
                            files.Add(file.FullName);
                        }
                    }
                }
            );
            return files;
        }
    }
}