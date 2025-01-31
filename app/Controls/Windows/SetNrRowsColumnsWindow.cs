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
using Num = System.Numerics;
using Microsoft.Xna.Framework;

namespace LabelChecker.Controls
{
    public class SetRowsColumns(Application app) : Control(app)
    {
        int _columnNumbers = 5;
        int _rowNumbers = 5;

        public int RowNumbers
        {
            get { return _rowNumbers; }
            set { _rowNumbers = value; }
        }
        public int ColumnNumbers
        {
            get { return _columnNumbers; }
            set { _columnNumbers = value; }
        }
        public override string Name => "SetRowsColumns";

        public override void Update(GameTime time)
        {

        }

        public override void Draw(ImGuiIOPtr iOPtr)
        {
            // set number of images to render in grid
            ImGui.SetNextWindowPos(new Num.Vector2(1200, 50), ImGuiCond.FirstUseEver);
            ImGui.Begin("Image grid control", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);

            // reset window position if outside of viewport
            App.Utils.ResetWindowPosition("Image grid control");
            // number of columns in image grid
            ImGui.Text("Columns");
            if (ImGui.Button("-")) { ColumnNumbers--; }
            ImGui.SameLine();
            ImGui.Text(ColumnNumbers.ToString());
            ImGui.SameLine();
            if (ImGui.Button("+")) { ColumnNumbers++; }

            // number of rows in image grid
            ImGui.Text("Rows");
            if (ImGui.Button("-##")) { RowNumbers--; }
            ImGui.SameLine();
            ImGui.Text(RowNumbers.ToString());
            ImGui.SameLine();
            if (ImGui.Button("+##")) { RowNumbers++; }
        }
    }
}