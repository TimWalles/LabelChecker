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

using System.IO;
using System;


namespace LabelChecker.Settings
{
    public class UserSettings
    {
        private readonly string settingsFile;

        public UserSettings()
        {
            settingsFile = Path.Combine(GetSettingsDirectory(), "user_settings.txt");
        }

        public ImGuiSettings LoadSettings(Application app)
        {
            try
            {
                if (File.Exists(settingsFile))
                {
                    string data = File.ReadAllText(settingsFile);
                    return ImGuiSettings.FromStorageString(data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }

            return new ImGuiSettings();
        }

        public void SaveSettings(ImGuiSettings settings)
        {
            try
            {
                string directory = Path.GetDirectoryName(settingsFile);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(settingsFile, settings.ToStorageString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private static string GetSettingsDirectory()
        {
            string settingsDir;
            if (OperatingSystem.IsMacOS())
            {
                var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                settingsDir = Path.Combine(homeDir, "Library", "Application Support", "LabelChecker");

                if (!Directory.Exists(settingsDir))
                {
                    Directory.CreateDirectory(settingsDir);
                }

                // Set permissions explicitly
                try
                {
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "/bin/bash",
                            Arguments = $"-c \"chmod -R 755 '{settingsDir}'\"",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to set permissions: {ex.Message}");
                }
            }
            else
            {
                settingsDir = AppDomain.CurrentDomain.BaseDirectory;
            }

            return settingsDir;
        }
    }
}
