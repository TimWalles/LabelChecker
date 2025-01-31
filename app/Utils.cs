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
using System.Linq;
using LabelChecker.Enums;
using Num = System.Numerics;

namespace LabelChecker.Utilities
{
    public class Utils(Application app)
    {
        readonly Application App = app;
        public void UpdateProcessingStep(DataProcessingStep processingStep)
        {
            App.DataProcessStep = processingStep;
        }

        public void CenterWindow()
        {
            var windowViewPort = ImGui.GetWindowViewport();
            ImGui.SetNextWindowPos(new Num.Vector2(windowViewPort.Pos.X + windowViewPort.Size.X / 2, windowViewPort.Pos.Y + windowViewPort.Size.Y / 2), ImGuiCond.Appearing, new Num.Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowFocus();
        }

        public void ResetWindowPosition(string windowName)
        {
            var windowViewPort = ImGui.GetWindowViewport();
            var currentWindowPos = ImGui.GetWindowPos();
            var currentWindowSize = ImGui.GetWindowSize();
            if (currentWindowPos.X + currentWindowSize.X > windowViewPort.Size.X || currentWindowPos.Y + currentWindowSize.Y > windowViewPort.Size.Y || currentWindowPos.X < 0 || currentWindowPos.Y < 0)
            {
                int defaultX = 0;
                int defaultY = 40; // offset for the menubar
                int offset = 20;
                switch (windowName)
                {
                    case "image page":
                        defaultX = (int)(windowViewPort.Size.X / 2 - currentWindowSize.X / 2);
                        defaultY = windowViewPort.Size.Y * 0.95f + currentWindowSize.Y > windowViewPort.Size.Y
                            ? (int)(windowViewPort.Size.Y * 0.95f - currentWindowSize.Y)
                            : (int)(windowViewPort.Size.Y * 0.95f);
                        break;
                    case "Go to page":
                        break;
                    case "Label list":
                        defaultX = (int)(windowViewPort.Size.X - currentWindowSize.X);
                        defaultY = (int)(windowViewPort.Size.Y - currentWindowSize.Y + offset);
                        break;
                    case "Image grid control":
                        defaultX = (int)(windowViewPort.Size.X - currentWindowSize.X + offset);
                        break;
                    case "Please select processing steps":
                        defaultY = (int)(windowViewPort.Size.Y - currentWindowSize.Y + offset);
                        break;
                }
                ImGui.SetWindowPos(windowName, new Num.Vector2(defaultX, defaultY));
            }
        }

        public void ScaleWindowToViewport(string windowName)
        {
            var windowViewPort = ImGui.GetWindowViewport();
            var currentWindowPos = ImGui.GetWindowPos();
            var currentWindowSize = ImGui.GetWindowSize();
            if (currentWindowSize.X >= windowViewPort.Size.X || currentWindowSize.Y >= windowViewPort.Size.Y)
            {
                ImGui.SetWindowSize(windowName, new Num.Vector2((int)windowViewPort.Size.X * 0.75f, (int)windowViewPort.Size.Y * 0.75f));
                ImGui.SetWindowPos(windowName, new Num.Vector2((int)windowViewPort.Size.X * 0.125f, (int)windowViewPort.Size.Y * 0.125f));
            }
        }

        public void OpenFileBrowser()
        {
            var fileMenuEventTracker = App.MainMenuControl.EventTracker.FileMenu;
            // set Filetype
            App.Filetype = Filetype.LabelCheckerFile;

            // open file browser
            fileMenuEventTracker.OpenLabelCheckerFile = !App.MainMenuControl.EventTracker.FileMenu.OpenLabelCheckerFile;

            App.MainMenuControl.EventTracker.FileMenu = fileMenuEventTracker;
        }

        public void AcceptAll()
        {
            // get all unlabeled indices
            List<int> unlabeledIndices = App.ImageGridControl.UnselectedImageGridIndices.Except(App.ImageGridControl.LabelledIndices).ToList();
            App.DataManager.AssignCorrectedLabels(App.DataProcessStep, "", unlabeledIndices);
        }

        public void SelectImages()
        {
            List<int> selectedImageIndices = [];
            if (App.ImageGridControl.LastSelectedIndices.Count == 2 && App.ImageGridControl.SelectedImageGridIndices.Count > 0)
            {
                // descending selection
                if (App.ImageGridControl.LastSelectedIndices[0] > App.ImageGridControl.LastSelectedIndices[1])
                {
                    foreach (int index in Enumerable.Range(App.ImageGridControl.LastSelectedIndices[1], App.ImageGridControl.LastSelectedIndices[0] - App.ImageGridControl.LastSelectedIndices[1] + 1))
                    {
                        selectedImageIndices.Add(App.ImageGridControl.ImageGridIndices[index]);
                    }
                }
                else
                // ascending selection
                {
                    foreach (int index in Enumerable.Range(App.ImageGridControl.LastSelectedIndices[0], App.ImageGridControl.LastSelectedIndices[1] - App.ImageGridControl.LastSelectedIndices[0] + 1))
                    {
                        selectedImageIndices.Add(App.ImageGridControl.ImageGridIndices[index]);
                    }
                }

                if (selectedImageIndices.Except(App.ImageGridControl.SelectedImageGridIndices).Any())
                {
                    App.ImageGridControl.SelectedImageGridIndices = selectedImageIndices;
                    foreach (int index in App.ImageGridControl.SelectedImageGridIndices)
                    {
                        App.ImageGridControl.UnselectedImageGridIndices.Remove(index);
                    }
                }
            }
        }

        public void SelectAll()
        {
            foreach (var item in App.ImageGridControl.UnselectedImageGridIndices)
            {
                App.ImageGridControl.SelectedImageGridIndices.Add(item);
            }
            foreach (var item in App.ImageGridControl.SelectedImageGridIndices)
            {
                App.ImageGridControl.UnselectedImageGridIndices.Remove(item);
            }
        }

        public void SaveLabelCheckerToRecentFiles(string filePath)
        {
            // Initialize list if not done
            if (App.imGuiSettings.RecentLabelCheckerFiles == null)
            {
                List<string> recentFiles = [filePath];
                App.imGuiSettings.RecentLabelCheckerFiles = recentFiles;
            }
            // if item isn't in the list, insert on the first spot
            else if (!App.imGuiSettings.RecentLabelCheckerFiles.Contains(filePath))
            {
                App.imGuiSettings.RecentLabelCheckerFiles.Insert(0, filePath);
            }
            else
            {
                UpdateLabelCheckerToRecentFiles(filePath);
            }

            // First-in-First-out when more than 10 recent files in the list
            if (App.imGuiSettings.RecentLabelCheckerFiles.Count > 10)
            {
                App.imGuiSettings.RecentLabelCheckerFiles.SkipLast(1).ToList();
            }

            // save new user settings
            App.UserSettings.SaveSettings(App.imGuiSettings);
        }

        public void UpdateLabelCheckerToRecentFiles(string filePath)
        {
            var idx = App.imGuiSettings.RecentLabelCheckerFiles.FindIndex(x => x == filePath);
            if (idx >= 0)
            {
                App.imGuiSettings.RecentLabelCheckerFiles.RemoveAt(idx);
                App.imGuiSettings.RecentLabelCheckerFiles.Insert(0, filePath);
            }
            // save new user settings
            App.UserSettings.SaveSettings(App.imGuiSettings);
        }

        public void SaveLabelToRecentFiles(string filePath)
        {
            // Initialize list if not done
            if (App.imGuiSettings.RecentLabelFiles == null)
            {
                List<string> recentFiles = [filePath];
                App.imGuiSettings.RecentLabelFiles = recentFiles;
            }
            // if item isn't in the list, insert on the first spot
            else if (!App.imGuiSettings.RecentLabelFiles.Contains(filePath))
            {
                App.imGuiSettings.RecentLabelFiles.Insert(0, filePath);
            }

            // First-in-First-out when more than 10 recent files in the list
            if (App.imGuiSettings.RecentLabelFiles.Count > 10)
            {
                App.imGuiSettings.RecentLabelFiles.SkipLast(1).ToList();
            }
            else
            {
                UpdateLabelCheckerToRecentFiles(filePath);
            }

            // save new user settings
            App.UserSettings.SaveSettings(App.imGuiSettings);
        }

        public void UpdateLabelToRecentFiles(string filePath)
        {
            var idx = App.imGuiSettings.RecentLabelFiles.FindIndex(x => x == filePath);
            if (idx >= 0)
            {
                App.imGuiSettings.RecentLabelFiles.RemoveAt(idx);
                App.imGuiSettings.RecentLabelFiles.Insert(0, filePath);
            }
            // save new user settings
            App.UserSettings.SaveSettings(App.imGuiSettings);
        }

        public void SaveFullScreenMode(bool fullScreenMode)
        {
            // update full screen mode setting
            App.imGuiSettings.FullScreenMode = fullScreenMode;
            // save new user settings
            App.UserSettings.SaveSettings(App.imGuiSettings);
        }

        public void SaveAutoFontSize(bool autoFontSize)
        {
            // update auto font size setting
            App.imGuiSettings.AutoFontSize = autoFontSize;
            // save new user settings
            App.UserSettings.SaveSettings(App.imGuiSettings);
        }

        public void SaveFontSize(float fontSize)
        {
            // update font size setting
            App.imGuiSettings.FontSize = fontSize;
            // save new user settings
            App.UserSettings.SaveSettings(App.imGuiSettings);
        }

        public void SaveShowFileInfo(bool showFileInfo)
        {
            // update font size setting
            App.imGuiSettings.ShowFileInfo = showFileInfo;
            // save new user settings
            App.UserSettings.SaveSettings(App.imGuiSettings);
        }

        public void SaveImageScale(bool imageScale)
        {
            // update font size setting
            App.imGuiSettings.ScaleImage = imageScale;
            // save new user settings
            App.UserSettings.SaveSettings(App.imGuiSettings);
        }

        public void SaveShowParameterValue(bool showParameterValue, string parameterValue)
        {
            // update font size setting
            App.imGuiSettings.ShowParameterValue = showParameterValue;
            App.imGuiSettings.ParameterValue = parameterValue;
            // save new user settings
            App.UserSettings.SaveSettings(App.imGuiSettings);
        }

        public void SaveMagnification(bool magnification, int magnificationFactor)
        {
            // update font size setting
            App.imGuiSettings.Magnification = magnification;
            App.imGuiSettings.MagnificationFactor = magnificationFactor;
            // save new user settings
            App.UserSettings.SaveSettings(App.imGuiSettings);
        }

        public void SaveShowNumpad(bool showNumpad)
        {
            // update font size setting
            App.imGuiSettings.ShowNumpad = showNumpad;
            // save new user settings
            App.UserSettings.SaveSettings(App.imGuiSettings);
        }

        public void SaveWindowSize(Num.Vector2 windowSize)
        {
            // update font size setting
            App.imGuiSettings.WindowSize = windowSize;
            // save new user settings
            App.UserSettings.SaveSettings(App.imGuiSettings);
        }

        public void SaveWindowPosition(Num.Vector2 windowPosition)
        {
            // update font size setting
            App.imGuiSettings.WindowPosition = windowPosition;
            // save new user settings
            App.UserSettings.SaveSettings(App.imGuiSettings);
        }
    }
}