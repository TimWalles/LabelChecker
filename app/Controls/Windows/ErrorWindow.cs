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
using Microsoft.Xna.Framework.Input;

namespace LabelChecker.Controls
{
    public class ErrorWindow(Application app) : Control(app)
    {
        private readonly Application app = app;

        public override string Name => "ErrorWindow";
        public string ErrorPopupName { get; set; } = "Error";
        public string ErrorPopupMessage { get; set; } = "";

        public override void Update(GameTime time)
        {

        }

        public override void Draw(ImGuiIOPtr iOPtr)
        {
            if (string.IsNullOrEmpty(ErrorPopupMessage))
            {
                return;
            }
            App.Utils.CenterWindow();
            if (ImGui.Begin("Error", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar))
            {
                // flag that there's a popup open
                app.HotkeyManager.DisableHotkeys = true;

                // Set cursor to center of the window
                var windowWidth = ImGui.GetWindowSize().X;
                var buttonWidth = 100; // Width of the button
                var buttonPosX = (windowWidth - buttonWidth) / 2;
                ImGui.SetCursorPosX(buttonPosX);

                ImGui.PushStyleColor(ImGuiCol.Text, new Num.Vector4(1, 0, 0, 1));
                ImGui.Text(ErrorPopupName);
                ImGui.PopStyleColor();

                ImGui.NewLine();
                ImGui.Text(ErrorPopupMessage);
                ImGui.NewLine();

                ImGui.SetCursorPosX(buttonPosX);
                if (ImGui.Button("OK", new Num.Vector2(100, 50)))
                {
                    ResetErrorPopup();

                    // flag popup to false again
                    app.HotkeyManager.DisableHotkeys = false;
                    ImGui.End();
                }

                // in case of unwanted clicking of an image.
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    ResetErrorPopup();
                    app.HotkeyManager.DisableHotkeys = false;
                    ImGui.End();
                }

                ImGui.End();
            }
        }

        private void ResetErrorPopup()
        {
            ErrorPopupName = null;
            ErrorPopupMessage = null;
        }
    }
}