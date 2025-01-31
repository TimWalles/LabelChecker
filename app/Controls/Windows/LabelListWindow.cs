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
using LabelChecker.Enums;
using Num = System.Numerics;
using Microsoft.Xna.Framework;

namespace LabelChecker.Controls
{
    public class LabelList(Application app) : Control(app)
    {

        public override string Name => "LabelList";

        public override void Update(GameTime time)
        {

        }

        public override void Draw(ImGuiIOPtr iOPtr)
        {

            if (App.DataProcessStep == DataProcessingStep.Classification)
            {
                if (App.ActiveLabelsFile == null)
                {
                    // create new active file is none is open
                    App.ActiveLabelsFile = CSVLabelFile.Create(this.App);
                }
            }

            if (App.ActiveLabelsFile != null)
            {
                // draw new box
                ImGui.SetNextWindowSize(new Num.Vector2(250, 600), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowPos(new Num.Vector2(1200, 80), ImGuiCond.FirstUseEver);
                ImGui.Begin("Label list", ImGuiWindowFlags.NoTitleBar);

                // reset window position if outside of viewport
                App.Utils.ResetWindowPosition("Label list");

                // add two columns
                ImGui.Columns(2, "##LabelGrid");

                // get enumerable list of label names and label codes
                IEnumerable<CSVLabelFile.LabelCode> LabelsCodes = App.LabelManager.GetLabelsAndCodes();
                foreach (var labelCode in LabelsCodes)
                {
                    ImGui.Text(labelCode.Label.ToString());
                    ImGui.NextColumn();
                    ImGui.Text(labelCode.Code.ToString());
                    ImGui.NextColumn();
                }
            }
        }
    }
}
