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
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Num = System.Numerics;
using LabelChecker.IO;


namespace LabelChecker.Controls
{
    public class BackgroundReader(Application app) : Control(app)
    {
        private Application app = app;

        public override string Name => "BackgroundReader";
        private List<string> files = [];
        private int progress = 0;
        private string currentFile = "";
        private bool readingFile = false;
        private BackgroundWorker backgroundWorker = null;

        public override void Draw(ImGuiIOPtr iOPTr)
        {
            if (readingFile)
            {
                App.Utils.CenterWindow();
                ImGui.Begin("Reading LabelChecker file...", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar);
                ImGui.Text("Reading LabelChecker file...");
                ImGui.Text(currentFile);
                ImGui.ProgressBar((float)progress / 100, new Num.Vector2(500, 50));
                ImGui.End();
            }
        }
        public void ReadFile(string file)
        {
            files.Add(file);
            ReadFiles(files);
        }

        public void ReadFiles(List<string> files)
        {
            readingFile = true;
            CSVLabelCheckerFile newLabelCheckerFile = null;
            backgroundWorker = GetBackgroundWorker();
            backgroundWorker.DoWork += (sender, e) =>
            {
                int percentage = 0;
                foreach (var file in files)
                {
                    if (backgroundWorker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    backgroundWorker.ReportProgress(percentage, Path.GetFileName(file));
                    if (newLabelCheckerFile == null)
                    {
                        newLabelCheckerFile = CSVLabelCheckerFile.Read(file, app: app);
                    }
                    else
                    {
                        newLabelCheckerFile = CSVLabelCheckerFile.ReadAll(file, app: app, labelCheckerFile: newLabelCheckerFile);
                    }
                    if (newLabelCheckerFile == null)
                    {
                        backgroundWorker.CancelAsync();
                    }
                    percentage = (int)((float)(files.IndexOf(file) + 1) / files.Count * 100);
                    backgroundWorker.ReportProgress(percentage, Path.GetFileName(file));
                }
                Thread.Sleep(500);
                backgroundWorker.ReportProgress(100, "");
                e.Result = newLabelCheckerFile;
            };
            backgroundWorker.ProgressChanged += (sender, e) =>
            {
                progress = e.ProgressPercentage;
                currentFile = e.UserState as string;
            };
            backgroundWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Cancelled)
                {
                    // cancel reading file 
                }
                else if (e.Error == null)
                {
                    CSVLabelCheckerFile newLabelCheckerFile = e.Result as CSVLabelCheckerFile;
                    if (newLabelCheckerFile != null)
                    {
                        // set unique id for each data
                        int id = 1;
                        foreach (var data in newLabelCheckerFile.Data)
                        {
                            data.Id = id++;
                        }
                        App.ActiveLabelCheckerFile = newLabelCheckerFile;
                        if (files.Count == 1)
                        {
                            App.Utils.SaveLabelCheckerToRecentFiles(files[0]);
                        }
                    }
                    App.ActiveLabelCheckerFile = e.Result as CSVLabelCheckerFile;
                }
                else
                {
                    App.ErrorWindow.ErrorPopupName = "Exception";
                    App.ErrorWindow.ErrorPopupMessage = e.Error.Message;
                }
                ResetVariables();
            };
            backgroundWorker.RunWorkerAsync();
        }

        private BackgroundWorker GetBackgroundWorker()
        {
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;
            return backgroundWorker;
        }

        private void ResetVariables()
        {
            files.Clear();
            progress = 0;
            currentFile = "";
            readingFile = false;
            backgroundWorker.Dispose();
            backgroundWorker = null;
        }


    }
}
