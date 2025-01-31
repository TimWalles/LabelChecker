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
using Microsoft.Xna.Framework;

namespace LabelChecker.Controls
{
    public class PageNrSelection(Application app) : Control(app)
    {
        public override string Name => "PageNrSelection";
        int pageNumber = 1;
        int maxPageNumbers = 1;
        public int PageNumber
        {
            get { return pageNumber; }
            set { pageNumber = value; }
        }
        public int MaxPageNumbers
        {
            get { return maxPageNumbers; }
            set { maxPageNumbers = value; }
        }

        public override void Update(GameTime time)
        {
            // If PageNumber is larger then MaxPageNumber, set PageNumber to be MaxPageNumber
            if (PageNumber > MaxPageNumbers)
            {
                PageNumber = MaxPageNumbers;
            }
        }

        public override void Draw(ImGuiIOPtr iOPtr)
        {

            // Page selection button
            // ImGui.SetNextWindowSize(new Num.Vector2(200, 75), ImGuiCond.FirstUseEver);
            ImGui.Begin("Go to page", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize);

            // reset window position if outside of viewport
            App.Utils.ResetWindowPosition("Go to page");

            ImGui.Text("Go to page number:");
            ImGui.SetNextItemWidth(60);
            if (ImGui.BeginCombo($"page of {MaxPageNumbers}", PageNumber.ToString()))
            {
                for (int n = 0; n < MaxPageNumbers; n++)
                {
                    if (ImGui.Selectable((n + 1).ToString()))
                    {
                        PageNumber = n + 1;
                    }
                }
                ImGui.EndCombo();
            }
        }
    }
}