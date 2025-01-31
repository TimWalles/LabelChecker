using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Num = System.Numerics;

namespace LabelChecker.Addons
{
    public class LabelFilePicker
    {
        static readonly Dictionary<object, LabelFilePicker> _labelFilePickers = new Dictionary<object, LabelFilePicker>();

        public string RootFolder;
        public string CurrentFolder;
        public string SelectedFile = "";
        public List<string> AllowedExtensions;
        public bool OnlyAllowFolders;
        private bool DisplayDrives = false;

        public static LabelFilePicker GetFolderPicker(object o, string startingPath)
            => GetFilePicker(o, startingPath, null, true);

        public static LabelFilePicker GetFilePicker(object o, string startingPath, string searchFilter = null, bool onlyAllowFolders = false)
        {
            if (File.Exists(startingPath))
            {
                startingPath = new FileInfo(startingPath).DirectoryName;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                startingPath = Environment.CurrentDirectory;
                if (string.IsNullOrEmpty(startingPath))
                    startingPath = AppContext.BaseDirectory;
            }

            if (!_labelFilePickers.TryGetValue(o, out LabelFilePicker fp))
            {
                fp = new LabelFilePicker();
                fp.RootFolder = new FileInfo(startingPath).DirectoryName; ;
                fp.CurrentFolder = startingPath;
                fp.OnlyAllowFolders = onlyAllowFolders;

                if (searchFilter != null)
                {
                    if (fp.AllowedExtensions != null)
                        fp.AllowedExtensions.Clear();
                    else
                        fp.AllowedExtensions = new List<string>();

                    fp.AllowedExtensions.AddRange(searchFilter.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                }

                _labelFilePickers.Add(o, fp);
            }

            return fp;
        }

        public static void RemoveFilePicker(object o) => _labelFilePickers.Remove(o);

        public bool Draw()
        {
            bool result = false;
            var di = new DirectoryInfo(CurrentFolder);

            if (ImGui.BeginChild("Navigation panel", new Num.Vector2(ImGui.GetContentRegionAvail().X * 0.2f, 400)))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, Color.GreenYellow.PackedValue);
                if (ImGui.Selectable("Desktop", false, ImGuiSelectableFlags.DontClosePopups))
                {
                    CurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                }
                if (ImGui.Selectable("Documents", false, ImGuiSelectableFlags.DontClosePopups))
                {
                    CurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                if (ImGui.Selectable("Pictures", false, ImGuiSelectableFlags.DontClosePopups))
                {
                    CurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                }
                if (ImGui.Selectable("Videos", false, ImGuiSelectableFlags.DontClosePopups))
                {
                    CurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                }
                if (ImGui.Selectable("Music", false, ImGuiSelectableFlags.DontClosePopups))
                {
                    CurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                }
                if (ImGui.Selectable("Favourites", false, ImGuiSelectableFlags.DontClosePopups))
                {
                    CurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
                }
                ImGui.PopStyleColor();
                ImGui.Text("--------------");
                ImGui.PushStyleColor(ImGuiCol.Text, Color.GreenYellow.PackedValue);
                if (ImGui.Selectable("This computer", false, ImGuiSelectableFlags.DontClosePopups))
                {
                    DisplayDrives = !DisplayDrives;
                }
                ImGui.PopStyleColor();
                ImGui.EndChild();
            }
            ImGui.SameLine();

            if (ImGui.BeginChildFrame(1, new Num.Vector2(400, 400)))
            {
                if (DisplayDrives)
                {// Display available drives (you can replace this with your own logic)
                    foreach (var drive in DriveInfo.GetDrives())
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, Color.Cyan.PackedValue);
                        if (ImGui.Selectable(drive.Name, false, ImGuiSelectableFlags.DontClosePopups))
                        {
                            CurrentFolder = drive.Name;
                            DisplayDrives = !DisplayDrives;
                        }
                        ImGui.PopStyleColor();
                    }
                }

                if (di.Exists && !DisplayDrives)
                {
                    // if (di.Parent != null && CurrentFolder != RootFolder)
                    if (di.Parent != null)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, Color.Yellow.PackedValue);
                        if (ImGui.Selectable("../", false, ImGuiSelectableFlags.DontClosePopups))
                            CurrentFolder = di.Parent.FullName;

                        ImGui.PopStyleColor();
                    }

                    var fileSystemEntries = GetFileSystemEntries(di.FullName);
                    foreach (var fse in fileSystemEntries)
                    {
                        if (Directory.Exists(fse))
                        {
                            var name = Path.GetFileName(fse);
                            ImGui.PushStyleColor(ImGuiCol.Text, Color.Yellow.PackedValue);
                            if (ImGui.Selectable(name + "/", false, ImGuiSelectableFlags.DontClosePopups))
                                CurrentFolder = fse;
                            ImGui.PopStyleColor();
                        }
                        else
                        {
                            var name = Path.GetFileName(fse);
                            bool isSelected = SelectedFile == fse;
                            if (ImGui.Selectable(name, isSelected, ImGuiSelectableFlags.DontClosePopups))
                                SelectedFile = fse;

                            if (ImGui.IsMouseDoubleClicked(0))
                            {
                                result = true;
                                ImGui.CloseCurrentPopup();
                            }
                        }
                    }
                }
            }
            ImGui.EndChildFrame();

            ImGui.Text("Current Folder: " + Path.GetFileName(RootFolder) + CurrentFolder.Replace(RootFolder, ""));

            ImGui.PushStyleColor(ImGuiCol.Button, Color.Red.PackedValue);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Color.OrangeRed.PackedValue);
            ImGui.PushStyleColor(ImGuiCol.Text, Color.White.PackedValue);
            if (ImGui.Button("Cancel"))
            {
                result = false;
                ImGui.CloseCurrentPopup();
            }
            ImGui.PopStyleColor(3);

            if (OnlyAllowFolders)
            {
                ImGui.SameLine();
                if (ImGui.Button("Open"))
                {
                    result = true;
                    SelectedFile = CurrentFolder;
                    ImGui.CloseCurrentPopup();
                }
            }
            else if (SelectedFile != null)
            {
                ImGui.SameLine();
                if (ImGui.Button("Open"))
                {
                    result = true;
                    ImGui.CloseCurrentPopup();
                }
            }

            return result;
        }

        bool TryGetFileInfo(string fileName, out FileInfo realFile)
        {
            try
            {
                realFile = new FileInfo(fileName);
                return true;
            }
            catch
            {
                realFile = null;
                return false;
            }
        }

        List<string> GetFileSystemEntries(string fullName)
        {
            var files = new List<string>();
            var dirs = new List<string>();

            foreach (var fse in Directory.GetFileSystemEntries(fullName, ""))
            {
                if (Directory.Exists(fse))
                {
                    bool isHidden = (File.GetAttributes(fse) & FileAttributes.Hidden) == FileAttributes.Hidden;
                    if (!isHidden)
                    {
                        dirs.Add(fse);
                    }
                }
                else if (!OnlyAllowFolders)
                {
                    if (AllowedExtensions != null)
                    {
                        var ext = Path.GetExtension(fse);
                        if (AllowedExtensions.Contains(ext))
                            files.Add(fse);
                    }
                    else
                    {
                        files.Add(fse);
                    }
                }
            }

            var ret = new List<string>(dirs);
            ret.AddRange(files);

            return ret;
        }

    }
}