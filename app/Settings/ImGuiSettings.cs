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

using LabelChecker.Enums;
using System;
using System.Collections.Generic;
using Num = System.Numerics;

namespace LabelChecker.Settings
{
    public class ImGuiSettings
    {
        public ImGuiSettings()
        {
            // Initialize collections
            RecentLabelCheckerFiles = new List<string>();
            RecentLabelFiles = new List<string>();
        }

        // Properties with default values
        public ImGuiTheme Theme { get; set; } = ImGuiTheme.Default;
        public bool AutoFontSize { get; set; } = false;
        public float FontSize { get; set; } = 1.0f;
        public List<string> RecentLabelCheckerFiles { get; set; }
        public List<string> RecentLabelFiles { get; set; }
        public bool ScaleImage { get; set; } = false;
        public bool Magnification { get; set; } = false;
        public int MagnificationFactor { get; set; } = 2;
        public float WindowSizeX { get; set; } = 800;
        public float WindowSizeY { get; set; } = 600;
        public float WindowPositionX { get; set; } = 300;
        public float WindowPositionY { get; set; } = 50;
        public bool FullScreenMode { get; set; } = false;
        public bool ShowFileInfo { get; set; } = false;
        public bool ShowParameterValue { get; set; } = false;
        public string ParameterValue { get; set; } = "";
        public bool ShowNumpad { get; set; } = false;

        // Helper methods for Vector2
        public Num.Vector2 WindowSize
        {
            get => new(WindowSizeX, WindowSizeY);
            set
            {
                WindowSizeX = value.X;
                WindowSizeY = value.Y;
            }
        }

        public Num.Vector2 WindowPosition
        {
            get => new(WindowPositionX, WindowPositionY);
            set
            {
                WindowPositionX = value.X;
                WindowPositionY = value.Y;
            }
        }

        // Simple string representation for file storage
        public string ToStorageString()
        {
            var lines = new List<string>
            {
                $"Theme={Theme}",
                $"AutoFontSize={AutoFontSize}",
                $"FontSize={FontSize}",
                $"ScaleImage={ScaleImage}",
                $"Magnification={Magnification}",
                $"MagnificationFactor={MagnificationFactor}",
                $"WindowSizeX={WindowSizeX}",
                $"WindowSizeY={WindowSizeY}",
                $"WindowPositionX={WindowPositionX}",
                $"WindowPositionY={WindowPositionY}",
                $"FullScreenMode={FullScreenMode}",
                $"RecentLabelCheckerFiles={string.Join("|", RecentLabelCheckerFiles)}",
                $"RecentLabelFiles={string.Join("|", RecentLabelFiles)}",
                $"ShowFileInfo={ShowFileInfo}",
                $"ShowParameterValue={ShowParameterValue}",
                $"ParameterValue={ParameterValue}",
                $"ShowNumpad={ShowNumpad}",
            };
            return string.Join("\n", lines);
        }

        public static ImGuiSettings FromStorageString(string data)
        {
            var settings = new ImGuiSettings();
            if (string.IsNullOrEmpty(data)) return settings;

            foreach (var line in data.Split('\n'))
            {
                var parts = line.Split('=');
                if (parts.Length != 2) continue;

                var key = parts[0];
                var value = parts[1];

                switch (key)
                {
                    case "Theme": settings.Theme = Enum.Parse<ImGuiTheme>(value); break;
                    case "AutoFontSize": settings.AutoFontSize = bool.Parse(value); break;
                    case "FontSize": settings.FontSize = float.Parse(value); break;
                    case "ScaleImage": settings.ScaleImage = bool.Parse(value); break;
                    case "Magnification": settings.Magnification = bool.Parse(value); break;
                    case "MagnificationFactor": settings.MagnificationFactor = int.Parse(value); break;
                    case "WindowSizeX": settings.WindowSizeX = float.Parse(value); break;
                    case "WindowSizeY": settings.WindowSizeY = float.Parse(value); break;
                    case "WindowPositionX": settings.WindowPositionX = float.Parse(value); break;
                    case "WindowPositionY": settings.WindowPositionY = float.Parse(value); break;
                    case "FullScreenMode": settings.FullScreenMode = bool.Parse(value); break;
                    case "RecentLabelCheckerFiles": settings.RecentLabelCheckerFiles = new List<string>(value.Split('|')); break;
                    case "RecentLabelFiles": settings.RecentLabelFiles = new List<string>(value.Split('|')); break;
                    case "ShowFileInfo": settings.ShowFileInfo = bool.Parse(value); break;
                    case "ShowParameterValue": settings.ShowParameterValue = bool.Parse(value); break;
                    case "ParameterValue": settings.ParameterValue = value; break;
                    case "ShowNumpad": settings.ShowNumpad = bool.Parse(value); break;
                }
            }
            return settings;
        }
    }
}
