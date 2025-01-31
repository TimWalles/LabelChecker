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
    public class PageNextPreviousSelection(Application app) : Control(app)
    {

        public override string Name => "PageNextPreviousSelection";


        public override void Update(GameTime time)
        {

        }

        public override void Draw(ImGuiIOPtr iOPtr)
        {
            if (App.ActiveLabelCheckerFile != null)
            {

                // set number of images to render in grid
                // ImGui.SetNextWindowSize(new Num.Vector2(600, 100), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowPos(new Num.Vector2(700, 800), ImGuiCond.FirstUseEver);
                ImGui.Begin("image page", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize);

                // reset window position if outside of viewport
                App.Utils.ResetWindowPosition("image page");

                // Previous button
                if (ImGui.Button("previous", new Num.Vector2(300, 75)))
                {
                    PrevPage();
                }
                ImGui.SameLine();
                // Next button
                if (ImGui.Button("next", new Num.Vector2(300, 75)))
                {
                    NextPage();
                }
                ImGui.End();
            }
        }

        public void NextPage()
        {
            // prevent infinite increasing of pages
            if (App.PageNrSelectionControl.PageNumber < App.PageNrSelectionControl.MaxPageNumbers)
            {
                App.PageNrSelectionControl.PageNumber++;
            }
            // clear image indices list
            App.ImageGridControl.ImageGridIndices.Clear();
            // clear unselected image grid indices list
            App.ImageGridControl.UnselectedImageGridIndices.Clear();

            // clear selected image indices list
            App.ImageGridControl.SelectedImageGridIndices.Clear();

            // save file
            try
            {
                App.DataManager.Save();
            }
            catch (System.Exception e)
            {
                App.ErrorWindow.ErrorPopupMessage = e.Message;
            }

        }

        public void PrevPage()
        {
            App.PageNrSelectionControl.PageNumber--;
            // prevent negative indexing
            if (App.PageNrSelectionControl.PageNumber <= 0)
            {
                App.PageNrSelectionControl.PageNumber = 1;
            }
            // clear image indices list
            App.ImageGridControl.ImageGridIndices.Clear();
            // clear unselected image grid indices list
            App.ImageGridControl.UnselectedImageGridIndices.Clear();
            // clear selected image indices list
            App.ImageGridControl.SelectedImageGridIndices.Clear();

            // save file
            try
            {
                App.DataManager.Save();
            }
            catch (System.Exception e)
            {
                App.ErrorWindow.ErrorPopupMessage = e.Message;
            }
        }

    }
}