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
using System;
using Microsoft.Xna.Framework;
using Num = System.Numerics;

namespace LabelChecker.Controls
{
    public class AboutWindow : Control
    {
        public bool AboutPopup = false;
        private const string LICENSE_TEXT = @"LabelChecker - A tool for checking and correcting image labels
Copyright (C) 2025

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

For the full license text, see the LICENSE file in the program directory
or visit: <https://www.gnu.org/licenses/gpl-3.0.html>";

        public override string Name => "AboutWindow";

        public AboutWindow(Application app) : base(app)
        {
        }

        public override void Update(GameTime time)
        {
        }

        public override void Draw(ImGuiIOPtr io)
        {
            if (AboutPopup)
            {
                ImGui.OpenPopup("About LabelChecker");

                Num.Vector2 center = new(io.DisplaySize.X * 0.5f, io.DisplaySize.Y * 0.5f);
                ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Num.Vector2(0.5f, 0.5f));

                if (ImGui.BeginPopupModal("About LabelChecker", ref AboutPopup, ImGuiWindowFlags.AlwaysAutoResize))
                {
                    ImGui.Text("LabelChecker");
                    ImGui.Separator();
                    ImGui.Text("Version: 1.0.1");
                    ImGui.Text("A tool for checking and correcting image labels.");
                    ImGui.Spacing();

                    ImGui.Text("Support & Community:");
                    ImGui.Text("For help, questions, or feature requests, join our Discord:");
                    if (ImGui.Button("Join Discord"))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "https://discord.gg/tGBg7z2hSU",
                            UseShellExecute = true
                        });
                    }
                    ImGui.Spacing();
                    ImGui.Spacing();

                    ImGui.Text("License:");
                    if (ImGui.BeginChild("License", new Num.Vector2(500, 200), true))
                    {
                        ImGui.TextWrapped(LICENSE_TEXT);
                        ImGui.EndChild();
                    }

                    ImGui.Separator();

                    if (ImGui.Button("Close"))
                    {
                        AboutPopup = false;
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.EndPopup();
                }
            }
        }
    }
}
