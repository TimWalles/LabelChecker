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
using LabelChecker.Enums;
using Num = System.Numerics;

namespace LabelChecker.Manager
{
    public class ThemeManager(Application app)
    {
        readonly Application App = app;
        private ImGuiTheme currentTheme;

        public void SetTheme(ImGuiTheme theme)
        {
            currentTheme = theme;
            ApplyTheme();

            // update and save new user settings
            App.imGuiSettings.Theme = currentTheme;
            App.UserSettings.SaveSettings(App.imGuiSettings);
        }

        private void ApplyTheme()
        {
            switch (currentTheme)
            {
                case ImGuiTheme.Dark:
                    App.clear_color = new Num.Vector3(0.18f, 0.18f, 0.18f);
                    ImGui.StyleColorsDark();
                    break;
                case ImGuiTheme.White:
                    App.clear_color = new Num.Vector3(0.39f, 0.58f, 0.92f);
                    ImGui.StyleColorsLight();
                    break;
                case ImGuiTheme.Classic:
                    App.clear_color = new Num.Vector3(114f / 255f, 144f / 255f, 154f / 255f);
                    ImGui.StyleColorsClassic();
                    break;
                case ImGuiTheme.Monokai:
                    App.clear_color = new Num.Vector3(0.153f, 0.157f, 0.133f);
                    StyleColorMonokai();
                    break;
                default:
                    App.clear_color = new Num.Vector3(114f / 255f, 144f / 255f, 154f / 255f);
                    ImGui.StyleColorsDark();
                    break;
            }
        }

        private void StyleColorMonokai()
        {
            var monokaiStyle = ImGui.GetStyle();


            monokaiStyle.Colors[(int)ImGuiCol.Text] = new Num.Vector4(248f / 255f, 248f / 255f, 242f / 255f, 1.0f);
            monokaiStyle.Colors[(int)ImGuiCol.CheckMark] = new Num.Vector4(248f / 255f, 248f / 255f, 242f / 255f, 1.0f);

            monokaiStyle.Colors[(int)ImGuiCol.WindowBg] = new Num.Vector4(65f / 255f, 67f / 255f, 57f / 255f, 1.0f);
            monokaiStyle.Colors[(int)ImGuiCol.MenuBarBg] = new Num.Vector4(65f / 255f, 67f / 255f, 57f / 255f, 1.0f);
            monokaiStyle.Colors[(int)ImGuiCol.Header] = new Num.Vector4(117f / 255f, 113f / 255f, 94f / 255f, 1.0f);
            monokaiStyle.Colors[(int)ImGuiCol.HeaderActive] = new Num.Vector4(141f / 255f, 137f / 255f, 123f / 255f, 1.0f);
            monokaiStyle.Colors[(int)ImGuiCol.HeaderHovered] = new Num.Vector4(135f / 255f, 131f / 255f, 120f / 255f, 1.0f);

            monokaiStyle.Colors[(int)ImGuiCol.FrameBg] = new Num.Vector4(89f / 255f, 91f / 255f, 81f / 255f, 1.0f);
            monokaiStyle.Colors[(int)ImGuiCol.FrameBgHovered] = new Num.Vector4(82f / 255f, 84f / 255f, 74f / 255f, 1.0f);
            monokaiStyle.Colors[(int)ImGuiCol.FrameBgActive] = new Num.Vector4(89f / 255f, 91f / 255f, 81f / 255f, 1.0f);

            monokaiStyle.Colors[(int)ImGuiCol.ResizeGrip] = new Num.Vector4(117f / 255f, 113f / 255f, 94f / 255f, 1.0f);
            monokaiStyle.Colors[(int)ImGuiCol.ResizeGripActive] = new Num.Vector4(141f / 255f, 137f / 255f, 123f / 255f, 1.0f);
            monokaiStyle.Colors[(int)ImGuiCol.ResizeGripHovered] = new Num.Vector4(135f / 255f, 131f / 255f, 120f / 255f, 1.0f);

            monokaiStyle.Colors[(int)ImGuiCol.Tab] = new Num.Vector4(117f / 255f, 113f / 255f, 94f / 255f, 1.0f);
            monokaiStyle.Colors[(int)ImGuiCol.TabActive] = new Num.Vector4(141f / 255f, 137f / 255f, 123f / 255f, 1.0f);
            monokaiStyle.Colors[(int)ImGuiCol.TabHovered] = new Num.Vector4(135f / 255f, 131f / 255f, 120f / 255f, 1.0f);

            monokaiStyle.Colors[(int)ImGuiCol.TitleBg] = new Num.Vector4(117f / 255f, 113f / 255f, 94f / 255f, 1.0f);
            monokaiStyle.Colors[(int)ImGuiCol.TitleBgActive] = new Num.Vector4(141f / 255f, 137f / 255f, 123f / 255f, 1.0f);

            monokaiStyle.Colors[(int)ImGuiCol.Button] = new Num.Vector4(117f / 255f, 113f / 255f, 94f / 255f, 1.0f);
            monokaiStyle.Colors[(int)ImGuiCol.ButtonActive] = new Num.Vector4(141f / 255f, 137f / 255f, 123f / 255f, 1.0f);
            monokaiStyle.Colors[(int)ImGuiCol.ButtonHovered] = new Num.Vector4(135f / 255f, 131f / 255f, 120f / 255f, 1.0f);








        }
    }
}