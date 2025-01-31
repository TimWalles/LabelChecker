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

using Microsoft.Xna.Framework.Input;
using LabelChecker.Controls;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using System;
using Emgu.CV.OCR;

namespace LabelChecker.Manager
{
    public class HotkeyManager(Application app) : Control(app)
    {
        public override string Name => "HotkeyHandler";
        public bool DisableHotkeys { get; set; } = false;
        private bool EventTriggered = false;
        public static bool IsAnyKeyDown()
        {
            var values = Enum.GetValues<Keys>();
            foreach (var v in values)
            {
                if (((Keys)v) != Keys.None && Keyboard.GetState().IsKeyDown((Keys)v))
                    return true;
            }
            return false;
        }

        public static bool IsCtrlKeyDown()
        {

            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? Keyboard.GetState().IsKeyDown(Keys.LeftWindows) || Keyboard.GetState().IsKeyDown(Keys.RightWindows)
                : Keyboard.GetState().IsKeyDown(Keys.LeftControl) || Keyboard.GetState().IsKeyDown(Keys.RightControl);
        }

        const Keys AcceptAllLabelsKey = Keys.A;
        const Keys NextPage = Keys.Right;
        const Keys NextPageSpace = Keys.Space;
        const Keys PrevPage = Keys.Left;

        #region Hotkeys
        private static bool OpenFileBrowser()
        {
            bool result = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                // OS
                ? Keyboard.GetState().IsKeyDown(Keys.O) &&
                  (Keyboard.GetState().IsKeyDown(Keys.LeftWindows) ||
                   Keyboard.GetState().IsKeyDown(Keys.RightWindows))
                // Windows
                : Keyboard.GetState().IsKeyDown(Keys.O) &&
                  (Keyboard.GetState().IsKeyDown(Keys.LeftControl) ||
                   Keyboard.GetState().IsKeyDown(Keys.RightControl));
            return result;
        }
        private static bool Save()
        {
            bool result = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            // OS
            ? Keyboard.GetState().IsKeyDown(Keys.S) &&
                (Keyboard.GetState().IsKeyDown(Keys.LeftWindows) ||
                Keyboard.GetState().IsKeyDown(Keys.RightWindows))
            // Windows
            : Keyboard.GetState().IsKeyDown(Keys.S) &&
                (Keyboard.GetState().IsKeyDown(Keys.LeftControl) ||
                Keyboard.GetState().IsKeyDown(Keys.RightControl));
            return result;
        }

        public static bool Select()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
            {
                return true;
            }
            return false;
        }
        private static bool SelectAll()
        {
            bool result = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            // OS
            ? Keyboard.GetState().IsKeyDown(Keys.A) &&
                (Keyboard.GetState().IsKeyDown(Keys.LeftWindows) ||
                Keyboard.GetState().IsKeyDown(Keys.RightWindows))
            // Windows
            : Keyboard.GetState().IsKeyDown(Keys.A) &&
                (Keyboard.GetState().IsKeyDown(Keys.LeftControl) ||
                Keyboard.GetState().IsKeyDown(Keys.RightControl));
            return result;
        }
        private static bool Undo()
        {
            bool result = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            // OS
            ? Keyboard.GetState().IsKeyDown(Keys.Z) &&
                (Keyboard.GetState().IsKeyDown(Keys.LeftWindows) ||
                Keyboard.GetState().IsKeyDown(Keys.RightWindows))
            // Windows
            : Keyboard.GetState().IsKeyDown(Keys.Z) &&
                (Keyboard.GetState().IsKeyDown(Keys.LeftControl) ||
                Keyboard.GetState().IsKeyDown(Keys.RightControl));
            return result;
        }
        private static bool Redo()
        {
            bool result = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            // OS
            ? Keyboard.GetState().IsKeyDown(Keys.Y) &&
                (Keyboard.GetState().IsKeyDown(Keys.LeftWindows) ||
                Keyboard.GetState().IsKeyDown(Keys.RightWindows))
            // Windows
            : Keyboard.GetState().IsKeyDown(Keys.Y) &&
                (Keyboard.GetState().IsKeyDown(Keys.LeftControl) ||
                Keyboard.GetState().IsKeyDown(Keys.RightControl));
            return result;
        }
        #endregion Hotkeys

        public override void Update(GameTime time)
        {
            KeyboardState state = Keyboard.GetState();
            if (!DisableHotkeys && !EventTriggered)
            {
                if (OpenFileBrowser())
                {
                    DisableHotkeys = !DisableHotkeys;
                    App.Utils.OpenFileBrowser();
                }
                if (Save())
                {
                    EventTriggered = !EventTriggered;
                    try
                    {
                        DisableHotkeys = !DisableHotkeys;
                        App.DataManager.Save(manualSave: true);
                    }
                    catch (Exception e)
                    {
                        App.ErrorWindow.ErrorPopupMessage = e.Message;
                    }
                }
                if (Select())
                {
                    App.Utils.SelectImages();

                }
                if (SelectAll())
                {
                    DisableHotkeys = !DisableHotkeys;
                    App.Utils.SelectAll();
                }
                if (Undo())
                {
                    EventTriggered = !EventTriggered;
                    App.DataManager.UndoChanges();
                }
                if (Redo())
                {
                    EventTriggered = !EventTriggered;
                    App.DataManager.ReDoChanges();
                }
                // Accept all (remaining) label predictions
                if (state.IsKeyDown(AcceptAllLabelsKey) && (!SelectAll()))
                {
                    // assign all(remaining) labels as correct
                    DisableHotkeys = !DisableHotkeys;
                    App.Utils.AcceptAll();
                }

                // Next page
                if (state.IsKeyDown(NextPage) || state.IsKeyDown(NextPageSpace))
                {
                    EventTriggered = !EventTriggered;
                    App.PageNextPreviousSelectionControl.NextPage();
                }

                // Previous page
                if (state.IsKeyDown(PrevPage))
                {
                    EventTriggered = !EventTriggered;
                    App.PageNextPreviousSelectionControl.PrevPage();
                }
            }
            // Trigger event only once
            if (!IsAnyKeyDown())
            {
                EventTriggered = false;
            }
        }
    }
}