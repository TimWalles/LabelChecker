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

using System.IO;
using Num = System.Numerics;
using ImGuiNET;
using LabelChecker.Addons;
using LabelChecker.Enums;
using System;

namespace LabelChecker.Controls
{
    public class SaveFile(Application app) : Control(app)
    {
        public override string Name => "SaveFile";

        bool openFileNamePopUp = false;

        public override void Draw(ImGuiIOPtr iOPtr)
        {
            App.Utils.CenterWindow();
            if (ImGui.BeginPopupModal("select folder to save file.##"))
            {
                switch (App.Filetype)
                {
                    case Filetype.LabelCheckerFile:
                        // open folder picker
                        App.OpenLabelCheckerFilePicker = LabelCheckerFilePicker.GetFolderPicker(this, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                        if (App.OpenLabelCheckerFilePicker.Draw())
                        {
                            // open popup to set folder, name and save the file.
                            openFileNamePopUp = !openFileNamePopUp;
                        }
                        break;

                    case Filetype.LabelsFile:
                        // open folder picker
                        App.OpenLabelPicker = LabelFilePicker.GetFolderPicker(this, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

                        if (App.OpenLabelPicker.Draw())
                        {
                            // open popup to set folder, name and save the file.
                            openFileNamePopUp = !openFileNamePopUp;
                        }
                        break;

                    default:
                        break;
                }
                // close the popup
                ImGui.EndPopup();

                // enable hotkeys again
                App.HotkeyManager.DisableHotkeys = false;
            }

            // open box to set filename
            if (openFileNamePopUp)
            {
                ImGui.OpenPopup("set filename##");
                openFileNamePopUp = !openFileNamePopUp;
            }

            ImGui.SetNextWindowSize(new Num.Vector2(300, 100), ImGuiCond.FirstUseEver);
            App.Utils.CenterWindow();
            if (ImGui.BeginPopupModal("set filename##"))
            {
                // flag a popup to be open
                App.HotkeyManager.DisableHotkeys = true;

                // initialize empty string
                string _fileName = "";

                // Type new label name
                if (Util.InputTextWithHint("##filename", "Type filename here without extension and hit enter.", ref _fileName, 200, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    switch (App.Filetype)
                    {
                        case Filetype.LabelCheckerFile:
                            // set filepath with filename
                            App.OpenLabelCheckerFilePicker.SelectedFile = Path.Combine(App.OpenLabelCheckerFilePicker.CurrentFolder, _fileName + ".csv");

                            // save label file
                            try
                            {
                                App.DataManager.Save(manualSave: true);
                            }
                            catch (Exception e)
                            {
                                App.ErrorWindow.ErrorPopupMessage = e.Message;
                            }

                            // Add opened file to recent files list and update user settings
                            App.Utils.SaveLabelCheckerToRecentFiles(App.OpenLabelCheckerFilePicker.SelectedFile);
                            // close popup
                            ImGui.CloseCurrentPopup();

                            // flag a popup to be closed
                            App.HotkeyManager.DisableHotkeys = false;
                            break;

                        case Filetype.LabelsFile:
                            // set filepath with filename
                            App.OpenLabelPicker.SelectedFile = Path.Combine(App.OpenLabelPicker.CurrentFolder, _fileName + ".csv");

                            // save label file
                            App.LabelManager.Save();

                            // Add opened file to recent files list and update user settings
                            App.Utils.SaveLabelToRecentFiles(App.OpenLabelPicker.SelectedFile);

                            // close popup
                            ImGui.CloseCurrentPopup();

                            // flag a popup to be closed
                            App.HotkeyManager.DisableHotkeys = false;
                            break;

                        default:
                            break;
                    }
                }
                // close the popup
                ImGui.EndPopup();
            }
        }
    }
}