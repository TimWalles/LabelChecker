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
    public class SaveWindow(Application app) : Control(app)
    {
        private readonly Application app = app;

        public override string Name => "SaveWindow";
        public string SaveMessage { get; set; } = "";

        public override void Update(GameTime time)
        {

        }

        public override void Draw(ImGuiIOPtr iOPtr)
        {
            if (string.IsNullOrEmpty(SaveMessage))
            {
                return;
            }
            ImGui.SetNextWindowPos(new Num.Vector2(iOPtr.DisplaySize.X - 10, iOPtr.DisplaySize.Y - 10), ImGuiCond.Always, new Num.Vector2(1, 1));
            if (ImGui.Begin("SaveWindow", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize))
            {
                ImGui.Text(SaveMessage);
                ImGui.End();
            }
        }
    }
}