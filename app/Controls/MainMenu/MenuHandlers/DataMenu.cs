using ImGuiNET;
using Num = System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace LabelChecker.Controls
{
    public class DataMenu(Application app) : Control(app)
    {
        public override string Name => "DataMenu";
        private string labelName = "";
        private string labelNameNew = "";
        private bool openDataStatsWindow = true;
        public override void Update(GameTime time)
        {
            // handle case where Popup is closed via X button
            if (!openDataStatsWindow)
            {
                App.HotkeyManager.DisableHotkeys = false;
                openDataStatsWindow = true;
            }
        }

        public override void Draw(ImGuiIOPtr iOPtr)
        {
            #region MainMenu -> Data
            var dataMenu = App.MainMenuControl.EventTracker.DataMenu;
            // rename label in data
            if (App.MainMenuControl.EventTracker.DataMenu.CorrectLabel)
            {
                dataMenu.CorrectLabel = false;
                ImGui.OpenPopup("Rename data label");
                ImGui.SetNextWindowSize(new Num.Vector2(300, 300), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowPos(new Num.Vector2(500, 500), ImGuiCond.FirstUseEver);
            }

            // show data stats
            if (App.MainMenuControl.EventTracker.DataMenu.ShowStats)
            {
                dataMenu.ShowStats = false;
                ImGui.OpenPopup("Stats");
                ImGui.SetNextWindowSize(new Num.Vector2(450, 900), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowPos(new Num.Vector2(500, 60), ImGuiCond.FirstUseEver);
            }
            App.MainMenuControl.EventTracker.DataMenu = dataMenu;
            #endregion MainMenu -> Data


            if (ImGui.BeginPopupModal("Rename data label"))
            {

                // disable hotkeys
                App.HotkeyManager.DisableHotkeys = true;

                // Type old label name
                ImGui.Text("Type current label name:");
                ImGui.SameLine();
                Util.InputTextWithHint("##labelName", "Type here...", ref labelName, 200, ImGuiInputTextFlags.AutoSelectAll);
                ImGui.NewLine();
                ImGui.Text("Or select current label name:");

                // generate a list of possible option buttons
                // add one columns
                ImGui.Columns(1, "##LabelGrid");
                foreach (var label in App.DataManager.SelectableLabels)
                {
                    // update as the user types the code
                    if (label.ToString().Contains(labelName))
                    {
                        ImGui.PushID(label.ToString());
                        if (ImGui.Button(label.ToString()))
                        {
                            // set selected label name when clicked on
                            labelName = label.ToString();
                        }
                        ImGui.PopID();
                        // step to next column
                        ImGui.NextColumn();
                    }
                }
                ImGui.NewLine();

                // check whether the _currentLabelName is equal to what is in the list
                if (App.DataManager.SelectableLabels.Contains(labelName))
                {
                    ImGui.Text("Type new label name:");
                    ImGui.SameLine();
                    Util.InputTextWithHint("##labelNameNew", "Type here...", ref labelNameNew, 200);
                }
                ImGui.NewLine();

                // rename current label into the new label
                if (App.DataManager.SelectableLabels.Contains(labelName) && labelNameNew != "")
                {
                    if (ImGui.Button("Rename") | Keyboard.GetState().IsKeyDown(Keys.Enter))
                    {
                        App.DataManager.RenameLabel(labelName, labelNameNew);

                        // reset variables
                        labelName = "";
                        labelNameNew = "";
                        // close popup and allow shortcut keys to be used again.
                        ImGui.CloseCurrentPopup();
                        App.HotkeyManager.DisableHotkeys = false;
                    }
                }



                // in case of unwanted clicking of an image.
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    App.ImageGridControl.SelectedImageGridIndices.Clear();
                    ImGui.CloseCurrentPopup();

                    // flag a popup to be closed
                    App.HotkeyManager.DisableHotkeys = false;
                }
                ImGui.EndPopup();
            }

            if (ImGui.BeginPopupModal("Stats", ref openDataStatsWindow))
            {
                // disable hotkeys
                App.HotkeyManager.DisableHotkeys = true;
                Dictionary<string, int> LabelCounts = App.DataManager.GetLabeledDataCounts(App.ActiveLabelCheckerFile.Data, out _);

                if (LabelCounts.Count == 0)
                {
                    ImGui.Text("No checked labels found in current file.");
                }

                ImGui.Columns(2);
                ImGui.Text("Label");
                ImGui.NextColumn();
                ImGui.Text("_Number of confirmed labels"); // the first character of the second column in the first row gets dropped for some reason... 
                ImGui.NextColumn();
                foreach (var LabelCount in LabelCounts)
                {
                    ImGui.Text(LabelCount.Key.ToString());
                    ImGui.NextColumn();
                    ImGui.Text(" " + LabelCount.Value.ToString()); // we add a white space to have everything outline under the header of the second column
                    ImGui.NextColumn();
                }

                if (ImGui.Button("Close", new Num.Vector2(100, 50)))
                {
                    ImGui.CloseCurrentPopup();
                    // flag a popup to be closed
                    App.HotkeyManager.DisableHotkeys = false;
                }

                // in case of unwanted clicking of an image.
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {

                    ImGui.CloseCurrentPopup();
                    // flag a popup to be closed
                    App.HotkeyManager.DisableHotkeys = false;
                }
                ImGui.EndPopup();
            }


        }
    }
}