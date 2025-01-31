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

using System.Collections.Generic;
using System.Linq;
using Num = System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Color = Microsoft.Xna.Framework.Color;
using ImGuiNET;
using LabelChecker.IO;
using LabelChecker.Enums;
using System;


namespace LabelChecker.Controls
{
    public class LabelCorrection(Application app) : Control(app)
    {
        public override string Name => "LabelCorrection";
        string InputLabelCode = "";
        bool openLabelCorrectionPopUp = false;
        bool addLabelPopUp = false;
        string addLabel = "";
        bool openCorrectionWindow = true;
        List<string> labels = [];

        public override void Update(GameTime time)
        {
            // handle case where Popup is closed via X button
            if (!openCorrectionWindow)
            {
                if (!addLabelPopUp)
                {
                    App.ImageGridControl.SelectedImageGridIndices.Clear();
                }
                App.HotkeyManager.DisableHotkeys = false;
                openCorrectionWindow = true;
                openLabelCorrectionPopUp = false;
            }
        }
        public override void Draw(ImGuiIOPtr iOPtr)
        {
            if (App.ImageGridControl.LastSelectedIndices.Count == 2 && Manager.HotkeyManager.Select())
            {
                App.Utils.SelectImages();
            }

            if (App.ImageGridControl.SelectedImageGridIndices.Count != 0 && (App.DataManager.SelectableLabels != null) && !Manager.HotkeyManager.IsCtrlKeyDown() && !Manager.HotkeyManager.Select() && !addLabelPopUp)
            {
                openLabelCorrectionPopUp = true;
            }


            // open file picker
            if (openLabelCorrectionPopUp)
            {
                // for taxonomy step we use code inputs to set the label
                if (App.DataProcessStep == DataProcessingStep.Classification)
                {
                    ImGui.OpenPopup("Classification Correction Box");
                    ImGui.SetNextWindowSize(new Num.Vector2(600, 1300), ImGuiCond.FirstUseEver);

                }
                else
                {
                    ImGui.OpenPopup("Preprocessing Correction Box");
                    ImGui.SetNextWindowSize(new Num.Vector2(600, 1300), ImGuiCond.FirstUseEver);
                }
            }

            // add label popup
            if (addLabelPopUp)
            {
                ImGui.OpenPopup("Add Label");
            }

            // label correction box popup
            App.Utils.CenterWindow();
            if (ImGui.BeginPopupModal("Preprocessing Correction Box", ref openCorrectionWindow, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDecoration))
            {
                // flag that there's a popup open
                App.HotkeyManager.DisableHotkeys = true;

                // generate buttons
                MakeLabelButtons(DataProcessingStep.Preprocessing);

                // Add label
                ImGui.NewLine();
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Num.Vector2(2, 2));
                ImGui.PushStyleColor(ImGuiCol.Button, Color.SeaGreen.PackedValue);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Color.GreenYellow.PackedValue);
                ImGui.PushStyleColor(ImGuiCol.Text, Color.White.PackedValue);
                if (ImGui.Button("Add Label", new Num.Vector2(100, 50)))
                {
                    addLabelPopUp = true;
                    ImGui.CloseCurrentPopup();
                    openCorrectionWindow = false;
                }
                ImGui.PopStyleColor(3);

                // cancel.
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Button, Color.Red.PackedValue);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Color.OrangeRed.PackedValue);
                ImGui.PushStyleColor(ImGuiCol.Text, Color.White.PackedValue);
                if (ImGui.Button("Cancel", new Num.Vector2(100, 50)) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    App.ImageGridControl.SelectedImageGridIndices.Clear();
                    App.ImageGridControl.LastSelectedIndices.Clear();
                    ImGui.CloseCurrentPopup();
                    App.HotkeyManager.DisableHotkeys = false;
                    openCorrectionWindow = false;
                }
                ImGui.PopStyleColor(3);
                ImGui.PopStyleVar();
                ImGui.EndPopup();

                // Close label correction box
                openLabelCorrectionPopUp = false;
            }

            // label correction box popup
            App.Utils.CenterWindow();
            if (ImGui.BeginPopupModal("Classification Correction Box", ref openCorrectionWindow, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDecoration))
            {
                // flag that there's a popup open
                App.HotkeyManager.DisableHotkeys = true;


                // To allow directly to type the code
                if (!ImGui.IsAnyItemActive() && !ImGui.IsAnyItemFocused())
                {
                    ImGui.SetKeyboardFocusHere();
                }
                Util.InputTextWithHint("##labelCode", "Type code or name and hit enter.", ref InputLabelCode, 200, ImGuiInputTextFlags.AutoSelectAll);

                // add empty line
                ImGui.NewLine();

                // Create numpad grid
                if (App.imGuiSettings.ShowNumpad)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Num.Vector2(2, 2));
                    for (int i = 1; i <= 9; i++)
                    {
                        if (ImGui.Button($"{i}##num{i}", new Num.Vector2(50, 50)))
                        {
                            InputLabelCode += i.ToString();
                        }
                        if (i % 3 != 0) ImGui.SameLine();
                    }

                    // Add 0 button
                    if (ImGui.Button("0##num0", new Num.Vector2(50, 50)))
                    {
                        InputLabelCode += "0";
                    }
                    ImGui.SameLine();

                    // Add backspace button
                    if (ImGui.Button("C##clear", new Num.Vector2(102, 50)))
                    {
                        InputLabelCode = "";
                    }

                    ImGui.PopStyleVar();
                    ImGui.NewLine();
                }

                // display code and label options
                List<CSVLabelFile.LabelCode> labelCodes = App.LabelManager.GetLabelsAndCodes();
                if (string.IsNullOrEmpty(InputLabelCode))
                {
                    foreach (var labelCode in labelCodes)
                    {
                        ImGui.Text(labelCode.Code.ToString().PadLeft(4));
                        ImGui.SameLine();
                        ImGui.Text(labelCode.Label.ToString());
                    }
                }
                else
                {
                    // always show the matching code on top
                    foreach (var labelCode in labelCodes)
                    {
                        if (labelCode.Code.ToString().Contains(InputLabelCode))
                        {
                            ImGui.Text(labelCode.Code.ToString().PadLeft(4));
                            ImGui.SameLine();
                            ImGui.Text(labelCode.Label.ToString());
                        }
                    }
                    // show any labels containing the value below the code
                    foreach (var labelCode in labelCodes)
                    {
                        if (labelCode.Label.ToString().Contains(InputLabelCode, System.StringComparison.CurrentCultureIgnoreCase))
                        {
                            ImGui.Text(labelCode.Code.ToString().PadLeft(4));
                            ImGui.SameLine();
                            ImGui.Text(labelCode.Label.ToString());
                        }
                    }
                }

                ImGui.NewLine();
                if (string.IsNullOrEmpty(InputLabelCode) || string.IsNullOrEmpty(App.LabelManager.GetSelectedLabel(InputLabelCode, out _)))
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, Color.Gray.PackedValue);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Color.LightGray.PackedValue);
                    ImGui.PushStyleColor(ImGuiCol.Text, Color.White.PackedValue);
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, Color.Green.PackedValue);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Color.GreenYellow.PackedValue);
                    ImGui.PushStyleColor(ImGuiCol.Text, Color.White.PackedValue);
                }

                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Num.Vector2(2, 2));
                if ((ImGui.Button("Confirm", new Num.Vector2(75, 50)) || Keyboard.GetState().IsKeyDown(Keys.Enter)) && !string.IsNullOrEmpty(InputLabelCode) && !string.IsNullOrEmpty(App.LabelManager.GetSelectedLabel(InputLabelCode, out _)))
                {
                    // get label that matches the code
                    string label = App.LabelManager.GetSelectedLabel(InputLabelCode, out _);

                    // assign label to selected images
                    App.DataManager.AssignCorrectedLabels(App.DataProcessStep, label, App.ImageGridControl.SelectedImageGridIndices);

                    // clear list
                    App.ImageGridControl.SelectedImageGridIndices.Clear();
                    App.ImageGridControl.LastSelectedIndices.Clear();

                    // Close current popup
                    ImGui.CloseCurrentPopup();
                    // flag popup to false again
                    App.HotkeyManager.DisableHotkeys = false;
                }
                ImGui.PopStyleColor(3);
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Button, Color.Red.PackedValue);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Color.OrangeRed.PackedValue);
                ImGui.PushStyleColor(ImGuiCol.Text, Color.White.PackedValue);
                if (ImGui.Button("Cancel", new Num.Vector2(75, 50)) || Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    App.ImageGridControl.SelectedImageGridIndices.Clear();
                    App.ImageGridControl.LastSelectedIndices.Clear();
                    ImGui.CloseCurrentPopup();
                    App.HotkeyManager.DisableHotkeys = false;
                    openCorrectionWindow = false;
                }
                ImGui.PopStyleColor(3);
                ImGui.PopStyleVar();

                ImGui.EndPopup();

                // Close label correction box
                openLabelCorrectionPopUp = false;
            }

            // add label popup
            App.Utils.CenterWindow();
            if (ImGui.BeginPopupModal("Add Label", ref addLabelPopUp, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDecoration))
            {
                // flag that there's a popup open
                App.HotkeyManager.DisableHotkeys = true;

                // To allow directly to type the code
                if (!ImGui.IsAnyItemActive() && !ImGui.IsAnyItemFocused())
                {
                    ImGui.SetKeyboardFocusHere();
                }

                ImGui.Text("Add label");
                ImGui.InputTextWithHint("##label", "Type label name here...", ref addLabel, 200, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue);
                ImGui.NewLine();
                ImGui.PushStyleColor(ImGuiCol.Button, Color.SeaGreen.PackedValue);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Color.GreenYellow.PackedValue);
                ImGui.PushStyleColor(ImGuiCol.Text, Color.White.PackedValue);
                if (ImGui.Button("Add Label"))
                {
                    labels.Add(addLabel);
                    addLabelPopUp = false;
                    ImGui.CloseCurrentPopup();
                    openLabelCorrectionPopUp = true;
                    addLabel = "";
                }
                ImGui.PopStyleColor(3);
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Button, Color.Red.PackedValue);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Color.OrangeRed.PackedValue);
                ImGui.PushStyleColor(ImGuiCol.Text, Color.White.PackedValue);
                if (ImGui.Button("Cancel"))
                {
                    addLabelPopUp = false;
                    ImGui.CloseCurrentPopup();
                    openLabelCorrectionPopUp = true;
                    addLabel = "";
                }
                ImGui.PopStyleColor(3);
                ImGui.EndPopup();
            }
        }

        private void MakeLabelButtons(DataProcessingStep step)
        {
            // count buttons that are generated
            int _buttonCounter = 1;
            foreach (string l in App.DataManager.SelectableLabels.Cast<string>())
            {
                if (!labels.OfType<string>().Any(label => label.Equals(l, StringComparison.OrdinalIgnoreCase)))
                {
                    labels.Add(l);
                }
            }
            if (step == DataProcessingStep.Preprocessing)
            {
                foreach (string l in new List<string> { "bubble", "duplicate", "object", "large" })
                {
                    if (!labels.OfType<string>().Any(label => label.Equals(l, StringComparison.OrdinalIgnoreCase)))
                    {
                        labels.Add(l);
                    }
                }
            }
            // generate buttons
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Num.Vector2(2, 2));
            foreach (var label in labels)
            {
                ImGui.PushID(label.ToString());
                if (ImGui.Button(label.ToString(), new Num.Vector2(100, 100)))
                {
                    // correct labels
                    App.DataManager.AssignCorrectedLabels(step, label.ToString(), App.ImageGridControl.SelectedImageGridIndices);
                    // clear list
                    App.ImageGridControl.SelectedImageGridIndices.Clear();
                    App.ImageGridControl.LastSelectedIndices.Clear();
                    // Close current popup
                    ImGui.CloseCurrentPopup();
                    // flag popup to false again
                    App.HotkeyManager.DisableHotkeys = false;
                }
                ImGui.PopID();

                // limit number of buttons to 3 per line
                if (_buttonCounter % 3 != 0)
                {
                    ImGui.SameLine();
                }

                // increase button counter
                _buttonCounter++;
            }
            ImGui.PopStyleVar();
        }
    }
}