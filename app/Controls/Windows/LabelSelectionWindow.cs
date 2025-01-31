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
using SEnum = System.Enum;
using System.Collections.Generic;
using Num = System.Numerics;
using LabelChecker.IO;
using Microsoft.Xna.Framework;
using LabelChecker.Enums;
using System.Linq;
using System;



namespace LabelChecker.Controls
{
    public class LabelSelection(Application app) : Control(app)
    {

        public override string Name => "LabelSelection";
        List<DataProcessingStep> headers = [];
        List<uint> headerIds = [];
        private Dictionary<string, bool> Checkboxes = [];
        private bool[] CheckboxReference;
        public bool Reinitialize { get; set; } = false;
        public List<string> SelectedLabel { get; set; } = [];
        public override void Update(GameTime time)
        {

        }

        public override void Draw(ImGuiIOPtr iOPtr)
        {
            if (App.ActiveLabelCheckerFile != null)
            {

                // list all types of data
                var t = typeof(CSVLabelCheckerFile.DataRow);
                // drawing the data processing box
                ImGui.SetNextWindowSize(new Num.Vector2(300, 100), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowPos(new Num.Vector2(50, 100), ImGuiCond.FirstUseEver);
                ImGui.Begin("Please select processing steps", ImGuiWindowFlags.NoTitleBar);

                // reset window position if outside of viewport
                App.Utils.ResetWindowPosition("Please select processing steps");

                // initialize only the first time
                if (headers.Count == 0)
                {
                    // get Collapsing header names
                    headers = GetProcessingStepHeaders(out _);
                    // get Collapsing header id's
                    headerIds = GetHeaderIDs(headers, out _);
                }

                // create collapsing header for each processing step
                foreach (DataProcessingStep header in headers)
                {
                    // skip default
                    if (header.Equals(DataProcessingStep.None))
                    {
                        continue;
                    }
                    // Cast to string
                    string headerName = header.ToString();
                    if (ImGui.CollapsingHeader(headerName))
                    {
                        try
                        {
                            // update processing step
                            App.Utils.UpdateProcessingStep(header);

                            // Set selection labels and Label Counts
                            App.DataManager.GetSelectableLabels(t, header);

                            // Initialize label checkboxes
                            InitializeCheckboxes(headerName, headerIds);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error during processing step selection: {ex.Message}");
                            // Reset processing step to none
                            App.Utils.UpdateProcessingStep(DataProcessingStep.None);
                            ImGui.TextColored(new Num.Vector4(1, 0, 0, 1), "Error processing images");
                        }
                    }
                }
                ImGui.End();
            }
        }

        private static List<DataProcessingStep> GetProcessingStepHeaders(out List<DataProcessingStep> processingStepHeaders)
        {
            // list of header ids
            processingStepHeaders = [];

            // get header ids from the different steps
            foreach (DataProcessingStep step in SEnum.GetValues<DataProcessingStep>())
            {
                // if header id isn't in the list, add it
                if (!processingStepHeaders.Contains(step))
                {
                    processingStepHeaders.Add(step);
                }
            }
            return processingStepHeaders;
        }

        private static List<uint> GetHeaderIDs(
            List<DataProcessingStep> headerList,
            out List<uint> headerIds
            )
        {
            // initialize list for header id's
            headerIds = [];

            foreach (DataProcessingStep header in headerList)
            {
                // get id
                uint id = ImGui.GetID(header.ToString());
                if (!headerIds.Contains(id))
                {
                    // if id isn't in the list yet, add it.
                    headerIds.Add(id);
                }
            }
            return headerIds;
        }

        private void InitializeCheckboxes(
            string header,
            List<uint> headerIds
            )
        {
            // close any open headers. If a header is being closed, reinitialize BoolArray is flagged.
            bool reinitializeBoolArray = CloseOpenHeaders(ImGui.GetID(header), headerIds);

            // only initialize if another header was closed, or if the checklist is empty (first time)
            if (reinitializeBoolArray || Checkboxes == null || Reinitialize || Checkboxes.Count == 0)
            {
                // initialize checkboxes
                InitializeCheckboxes();

                // to track which box is checked
                InitializeCheckboxReference(Checkboxes);

                // empty selected labels list
                SelectedLabel.Clear();

                // switch of new file
                Reinitialize = false;
            }
            // if DataManager.CachedLabels != checklist
            else if (Checkboxes.Count != 0)
                if (CheckboxLabelsChanged(Checkboxes) || Checkboxes.Count != App.DataManager.SelectableLabels.Count)
                {
                    // update checkbox labels
                    UpdateCheckbox();

                    // reinitialize checkbox reference
                    InitializeCheckboxReference(Checkboxes);
                }
            GetCheckBoxBool();
        }

        static bool CloseOpenHeaders(
            uint headerId,
            List<uint> ids
            )
        {
            // default is false
            bool reinitializeBoolArray = false;

            foreach (uint id in ids)
            {
                // get status of the header (open = 1, or closed = 0)
                int status = ImGui.GetStateStorage().GetInt(id);

                // if the id is not equal to that of the opened header and is open
                // close the header and generate a new checkbox array
                if (id != headerId & status == 1)
                {
                    // set other headers to close
                    ImGui.GetStateStorage().SetInt(id, 0);

                    // flag that a new checkbox bool array needs to be generated
                    reinitializeBoolArray = true;
                }
            }
            return reinitializeBoolArray;
        }

        private void InitializeCheckboxes()
        {
            // alphabetically sort labels
            App.DataManager.SelectableLabels.Sort();

            // assign labels to checkbox and set all to false
            Checkboxes = App.DataManager.SelectableLabels.ToDictionary(label => label.ToString(), label => false);
        }

        private void InitializeCheckboxReference(
            Dictionary<string, bool> Checkboxes
            )
        {
            // initialize bool array using the bool values in checkbox dictionary
            CheckboxReference = [.. Checkboxes.Values];
        }

        private bool CheckboxLabelsChanged(
            Dictionary<string, bool> checkboxes
        )
        {
            var checkboxLabels = checkboxes.Keys.ToList();
            var labelSet = checkboxLabels.ToHashSet();
            labelSet.SymmetricExceptWith(App.DataManager.SelectableLabels.Cast<string>());
            var diff = labelSet.ToList();
            if (diff.Count != 0)
            {
                return true;
            }
            return false;
        }

        private void UpdateCheckbox()
        {
            InitializeCheckboxes();

            // This stays equal if the label get renamed
            if (App.DataManager.SelectableLabels.Count != Checkboxes.Count)
            {
                // get index location of selected labels
                foreach (string label in SelectedLabel)
                {
                    // get index of selected label
                    int index = App.DataManager.SelectableLabels.IndexOf(label);
                    if (index != -1)
                        Checkboxes[(string)App.DataManager.SelectableLabels[index]] = true;
                }
            }
            // This stays equal if the label get removed
            else if (SelectedLabel.Count != 0)
            {
                foreach (string label in SelectedLabel.ToList())
                {
                    // get index of selected label
                    int index = App.DataManager.SelectableLabels.IndexOf(label);
                    if (index != -1) // if label is found
                        Checkboxes[(string)App.DataManager.SelectableLabels[index]] = true;
                    if (index == -1) // if label is not found
                    {
                        // remove label from selected labels list
                        SelectedLabel.Remove(label);
                    }
                }
            }
            else
            {
                // empty selected labels list
                SelectedLabel.Clear();
            }
        }

        private void GetCheckBoxBool()
        {
            int index = 0;

            foreach (var label in Checkboxes.Keys)
            {
                if (ImGui.Checkbox(label, ref CheckboxReference[index]))
                {
                    if (CheckboxReference[index])
                    {
                        // if selected then add to list
                        SelectedLabel.Add(label);
                    }
                    else
                    {
                        // if unselected then remove from the list
                        SelectedLabel.Remove(label);
                    }
                }
                index++;
            }
        }

    }
}