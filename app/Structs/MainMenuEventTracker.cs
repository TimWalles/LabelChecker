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

namespace LabelChecker.Structs
{
    public struct MainMenuEventTracker
    {
        public MainMenuEventTracker()
        {
            FileMenu = new FileMenuEventTracker();
            EditMenu = new EditMenuEventTracker();
            DataMenu = new DataMenuEventTracker();
            LabelFileMenu = new LabelFileMenuEventTracker();
        }

        public FileMenuEventTracker FileMenu { get; set; }
        public EditMenuEventTracker EditMenu { get; set; }
        public DataMenuEventTracker DataMenu { get; set; }
        public LabelFileMenuEventTracker LabelFileMenu { get; set; }
    }
    public struct FileMenuEventTracker
    {
        public FileMenuEventTracker()
        {
            OpenLabelCheckerFile = false;
            OpenLabelCheckerFiles = false;
            SaveLabelCheckerFile = false;
            CloseApplication = false;
        }

        public bool OpenLabelCheckerFile { get; set; }
        public bool OpenLabelCheckerFiles { get; set; }
        public bool SaveLabelCheckerFile { get; set; }
        public bool CloseApplication { get; set; }
    }

    public struct EditMenuEventTracker
    {

    }

    public struct DataMenuEventTracker
    {
        public DataMenuEventTracker()
        {
            // TODO Make part of DataManager config structure
            IsDescendingOrder = false;
            CorrectLabel = false;
            ShowStats = false;
        }

        public bool IsDescendingOrder { get; set; }
        public bool CorrectLabel { get; set; }
        public bool ShowStats { get; set; }
    }



    public struct LabelFileMenuEventTracker
    {
        public LabelFileMenuEventTracker()
        {
            OpenLabelListFile = false;
            SaveLabelListFile = false;
            AddLabelToLabelList = false;
            RenameLabelInLabelList = false;
            RemoveLabelFromLabelList = false;
        }

        public bool OpenLabelListFile { get; set; }
        public bool SaveLabelListFile { get; set; }
        public bool AddLabelToLabelList { get; set; }
        public bool RenameLabelInLabelList { get; set; }
        public bool RemoveLabelFromLabelList { get; set; }

    }
}