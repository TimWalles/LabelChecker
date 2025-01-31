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
using Num = System.Numerics;
using Microsoft.Xna.Framework;

namespace LabelChecker.Controls
{
    public class FileMenu(Application app) : Control(app)
    {

        public override string Name => "FileMenu";
        private bool closeApplication = false;
        public override void Update(GameTime time)
        {

        }

        public override void Draw(ImGuiIOPtr iOPtr)
        {
            #region MainMenu -> file
            var fileMenu = App.MainMenuControl.EventTracker.FileMenu;

            // open file
            if (App.MainMenuControl.EventTracker.FileMenu.OpenLabelCheckerFile)
            {
                fileMenu.OpenLabelCheckerFile = !App.MainMenuControl.EventTracker.FileMenu.OpenLabelCheckerFile;
                ImGui.OpenPopup("please select file.");
            }

            // open folder
            if (App.MainMenuControl.EventTracker.FileMenu.OpenLabelCheckerFiles)
            {
                fileMenu.OpenLabelCheckerFiles = !App.MainMenuControl.EventTracker.FileMenu.OpenLabelCheckerFiles;
                ImGui.OpenPopup("please select folder.###");
            }

            // save as
            if (App.MainMenuControl.EventTracker.FileMenu.SaveLabelCheckerFile)
            {

                fileMenu.SaveLabelCheckerFile = !App.MainMenuControl.EventTracker.FileMenu.SaveLabelCheckerFile;
                ImGui.OpenPopup("select folder to save file.##");
            }

            // close application
            if (App.MainMenuControl.EventTracker.FileMenu.CloseApplication)
            {
                // Check whether a labelChecker file is open
                if (App.ActiveLabelCheckerFile != null)
                {
                    fileMenu.CloseApplication = !App.MainMenuControl.EventTracker.FileMenu.CloseApplication;
                    closeApplication = true;
                    ImGui.OpenPopup("Close application");
                }
                else
                {
                    // close application
                    ImGui.EndFrame();
                    Environment.Exit(0);
                }

            }

            var mainViewPort = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(new Num.Vector2(mainViewPort.Pos.X + mainViewPort.Size.X / 2, mainViewPort.Pos.Y + mainViewPort.Size.Y / 2), ImGuiCond.Appearing, new Num.Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowFocus();
            ImGui.SetNextWindowSize(new Num.Vector2(300, 100), ImGuiCond.FirstUseEver);
            if (ImGui.BeginPopupModal("Close application", ref closeApplication, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("Save data");
                if (ImGui.Button("Yes", new Num.Vector2(75, 50)))
                {
                    try
                    {
                        App.DataManager.Save(manualSave: true);
                    }
                    catch (Exception e)
                    {
                        App.ErrorWindow.ErrorPopupMessage = e.Message;
                    }
                    Environment.Exit(0);
                    ImGui.EndFrame();
                }
                ImGui.SameLine();
                if (ImGui.Button("No", new Num.Vector2(75, 50)))
                {
                    Environment.Exit(0);
                    ImGui.EndFrame();
                }
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Button, Color.Red.PackedValue);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Color.OrangeRed.PackedValue);
                ImGui.PushStyleColor(ImGuiCol.Text, Color.White.PackedValue);
                if (ImGui.Button("Cancel", new Num.Vector2(75, 50)))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.PopStyleColor(3);
                ImGui.EndPopup();
            }


            // update MainMenu event tracker
            App.MainMenuControl.EventTracker.FileMenu = fileMenu;
            #endregion MainMenu -> file
        }
    }
}
