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
using System.Collections.Generic;
using LabelChecker.IO;
using Num = System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace LabelChecker.Controls
{
    public class LabelsMenu(Application app) : Control(app)
    {
        public override string Name => "LabelsMenu";
        string labelName = "";
        private string labelNameNew = "";
        string labelCode = "";
        public override void Update(GameTime time)
        {

        }

        public override void Draw(ImGuiIOPtr iOPtr)
        {
            #region MainMenu -> Labels
            var labelFile = App.MainMenuControl.EventTracker.LabelFileMenu;

            // open file browser
            if (App.MainMenuControl.EventTracker.LabelFileMenu.OpenLabelListFile)
            {
                labelFile.OpenLabelListFile = !App.MainMenuControl.EventTracker.LabelFileMenu.OpenLabelListFile;
                ImGui.OpenPopup("please select file.");
            }

            // save as
            if (App.MainMenuControl.EventTracker.LabelFileMenu.SaveLabelListFile)
            {

                labelFile.SaveLabelListFile = !App.MainMenuControl.EventTracker.LabelFileMenu.SaveLabelListFile;
                ImGui.OpenPopup("select folder to save file.##");
            }

            // add label
            if (App.MainMenuControl.EventTracker.LabelFileMenu.AddLabelToLabelList)
            {
                labelFile.AddLabelToLabelList = !App.MainMenuControl.EventTracker.LabelFileMenu.AddLabelToLabelList;
                ImGui.OpenPopup("Add label");
            }

            ImGui.SetNextWindowSize(new Num.Vector2(600, 300), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowPos(new Num.Vector2(500, 500), ImGuiCond.FirstUseEver);
            if (ImGui.BeginPopupModal("Add label"))
            {
                // disable hotkeys
                App.HotkeyManager.DisableHotkeys = true;

                // get enumerable list of label names and label codes
                IEnumerable<CSVLabelFile.LabelCode> LabelsCodes = App.LabelManager.GetLabelsAndCodes();

                // Type new label name
                ImGui.Text("New label name:");
                ImGui.SameLine();
                Util.InputTextWithHint("##labelName", "Type here...", ref labelName, 200);


                // Type new code for the new label name
                ImGui.Text("New label code:");
                ImGui.SameLine();
                Util.InputTextWithHint("##labelCode", "Type here...", ref labelCode, 4);

                // check if label name and label code aren't empty
                if (labelName != "" & labelCode != "")
                {
                    // check if label name and label code don't already exists
                    if (!AlreadyExists(labelCode, LabelsCodes, true) & !AlreadyExists(labelName, LabelsCodes, true))
                    {
                        if (ImGui.Button("add label") || Keyboard.GetState().IsKeyDown(Keys.Enter))
                        {
                            App.LabelManager.Create(labelName, labelCode);
                            ImGui.CloseCurrentPopup();

                            // enable hotkeys again
                            App.HotkeyManager.DisableHotkeys = false;
                        }
                    }
                }
                else
                {
                    // generate greyed-out buttons
                    ImGui.PushStyleColor(ImGuiCol.Text, Color.Gray.PackedValue);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Color.Transparent.PackedValue);
                    ImGui.Button("add label###");
                    ImGui.PopStyleColor(2);
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
            }

            // rename label in label file
            if (App.MainMenuControl.EventTracker.LabelFileMenu.RenameLabelInLabelList)
            {
                labelFile.RenameLabelInLabelList = !App.MainMenuControl.EventTracker.LabelFileMenu.RenameLabelInLabelList;
                ImGui.OpenPopup("Rename File label");
            }

            ImGui.SetNextWindowSize(new Num.Vector2(300, 300), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowPos(new Num.Vector2(500, 500), ImGuiCond.FirstUseEver);
            if (ImGui.BeginPopupModal("Rename File label"))
            {

                // disable hotkeys
                App.HotkeyManager.DisableHotkeys = true;

                // Type old label name
                ImGui.Text("Type current label name:");
                ImGui.SameLine();
                Util.InputTextWithHint("##labelName", "Type here...", ref labelName, 200, ImGuiInputTextFlags.AutoSelectAll);
                ImGui.NewLine();
                ImGui.Text("Or select current label name:");

                // generate a list of possible option buttons
                // add one columns
                ImGui.Columns(1, "##LabelGrid");
                List<CSVLabelFile.LabelCode> labelCodes = App.LabelManager.GetLabelsAndCodes();
                foreach (CSVLabelFile.LabelCode labelCode in labelCodes)
                {
                    // update as the user types the code
                    if (labelCode.Label.ToString().Contains(labelName))
                    {
                        ImGui.PushID(labelCode.Label.ToString());
                        if (ImGui.Button(labelCode.Label.ToString()))
                        {
                            // set selected label name when clicked on
                            labelName = labelCode.Label.ToString();
                        }
                        ImGui.PopID();
                        // step to next column
                        ImGui.NextColumn();
                    }
                }
                ImGui.NewLine();

                // check whether the _currentLabelName is equal to what is in the list
                if (labelCodes.Select(l => l.Label).ToList().Contains(labelName))
                {
                    ImGui.Text("Type new label name:");
                    ImGui.SameLine();
                    Util.InputTextWithHint("##labelNameNew", "Type here...", ref labelNameNew, 200);
                }
                ImGui.NewLine();

                // rename current label into the new label
                if (labelCodes.Any(labelCode => labelCode.Label.Contains(labelName)) & labelNameNew != "")
                {
                    if (ImGui.Button("Rename") | Keyboard.GetState().IsKeyDown(Keys.Enter))
                    {
                        // rename label
                        App.LabelManager.Update(labelName, labelNameNew);

                        // reset variables
                        labelName = "";
                        labelNameNew = "";

                        // close popup and allow shortcut keys to be used again.
                        ImGui.CloseCurrentPopup();
                        App.HotkeyManager.DisableHotkeys = false;
                    }

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
            }

            // remove label from label file
            if (App.MainMenuControl.EventTracker.LabelFileMenu.RemoveLabelFromLabelList)
            {
                labelFile.RemoveLabelFromLabelList = !App.MainMenuControl.EventTracker.LabelFileMenu.RemoveLabelFromLabelList;
                ImGui.OpenPopup("Remove label");
            }
            ImGui.SetNextWindowSize(new Num.Vector2(400, 300), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowPos(new Num.Vector2(500, 500), ImGuiCond.FirstUseEver);
            if (ImGui.BeginPopupModal("Remove label"))
            {

                // disable hotkeys
                App.HotkeyManager.DisableHotkeys = true;

                // get enumerable list of label names and label codes
                IEnumerable<CSVLabelFile.LabelCode> LabelsCodes = App.LabelManager.GetLabelsAndCodes();

                // Type label name or code
                ImGui.Text("Label name, or code:");
                ImGui.SameLine();
                Util.InputTextWithHint("##labelName", "Type here...", ref labelName, 200);

                // check for similar names already in the list
                App.LabelManager.GetSelectedLabel(labelName, out string label);

                // check if equal to remove existing label and codes
                if (AlreadyExists(labelName, LabelsCodes, false))
                {
                    if (ImGui.Button("remove label") | Keyboard.GetState().IsKeyDown(Keys.Enter))
                    {
                        App.LabelManager.Delete(labelName);
                        ImGui.CloseCurrentPopup();

                        // flag a popup to be closed
                        App.HotkeyManager.DisableHotkeys = false;
                    }
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, Color.Gray.PackedValue);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Color.Transparent.PackedValue);
                    ImGui.Button("remove label###");
                    ImGui.PopStyleColor(2);
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    App.ImageGridControl.SelectedImageGridIndices.Clear();
                    ImGui.CloseCurrentPopup();

                    // flag a popup to be closed
                    App.HotkeyManager.DisableHotkeys = false;
                }
                ImGui.EndPopup();
            }


            // update main menu event tracker
            App.MainMenuControl.EventTracker.LabelFileMenu = labelFile;
            #endregion  MainMenu -> Labels -> File
        }

        private static bool AlreadyExists(
            string item,
            IEnumerable<CSVLabelFile.LabelCode> list,
            bool output
        )
        {

            bool result = false;
            // add four columns
            if (item != "")
            {
                // test whether a code or label is given
                int _testInt;
                bool intCheck = int.TryParse(item, out _testInt);
                int start = 0;
                // if it is a code
                if (intCheck)
                {
                    foreach (var lc in list)
                    {
                        // if the code is already being used
                        if (lc.Code.ToString().Equals(item))
                        {
                            if (output)
                            {
                                ImGui.Text("Code is already used");
                            }
                            result = true;
                        }
                    }
                }
                // if it is a label
                else
                {
                    foreach (var labelCode in list)
                    {
                        // if the label is equal to an existing label
                        if (labelCode.Label.ToString().ToLower().Equals(item.ToLower()))
                        {
                            if (output)
                            {
                                ImGui.Text("Label already exists:");
                            }
                            result = true;
                        }
                        else
                        {
                            // to show similar labels to avoid adding double labels
                            if (labelCode.Label.ToString().ToLower().Contains(item.ToLower()))
                            {
                                if (start == 0)
                                {
                                    start++;
                                    ImGui.Text("Similar labels are: ");
                                }
                                ImGui.SameLine();
                                ImGui.Text(labelCode.Label.ToString() + ";");

                            }
                        }
                    }
                }
                ImGui.NewLine();
            }
            else
            {
                ImGui.NewLine();
            }

            return result;
        }
    }
}
