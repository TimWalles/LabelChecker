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

using Num = System.Numerics;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;

namespace LabelChecker.Controls
{
    public class RenameLabel(Application app) : Control(app)
    {
        public override string Name => "RenameLabel";

        string labelName = "";
        static string newLabelName = "";
        public bool UpdateLabelFile { get; set; } = true;

        public override void Draw(ImGuiIOPtr iOPtr)
        {
            ImGui.SetNextWindowSize(new Num.Vector2(300, 300), ImGuiCond.FirstUseEver);
            App.Utils.CenterWindow();
            if (ImGui.BeginPopupModal("Rename label"))
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
                foreach (var label in App.DataManager.SelectableLabels)
                {
                    // update as the user types the code
                    if (label.ToString().Contains(labelName))
                    {
                        ImGui.PushID(label.ToString());
                        if (ImGui.Button(label.ToString()))
                        {
                            // set selected label name when clicked on
                            labelName = label.ToString();
                        }
                        ImGui.PopID();
                        // step to next column
                        ImGui.NextColumn();
                    }
                }
                ImGui.NewLine();

                // check whether the _currentLabelName is equal to what is in the list
                if (App.DataManager.SelectableLabels.Contains(labelName))
                {
                    ImGui.Text("Type new label name:");
                    ImGui.SameLine();
                    Util.InputTextWithHint("##newLabelName", "Type here...", ref newLabelName, 200);
                }
                ImGui.NewLine();

                // rename current label into the new label
                if (App.DataManager.SelectableLabels.Contains(labelName) && newLabelName != "")
                {
                    if (ImGui.Button("Rename") | Keyboard.GetState().IsKeyDown(Keys.Enter))
                    {
                        // rename label
                        if (UpdateLabelFile)
                        {
                            App.LabelManager.Update(labelName, newLabelName);
                        }
                        else
                        {
                            App.DataManager.RenameLabel(labelName, newLabelName);
                        }

                        // reset variables
                        labelName = "";
                        newLabelName = "";

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
        }
    }
}