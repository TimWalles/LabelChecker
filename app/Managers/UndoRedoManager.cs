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

using System.Collections.Generic;
using LabelChecker.Enums;

namespace LabelChecker.Manager
{
    public class UndoRedoManager(Application app)
    {
        Dictionary<int, Dictionary<List<string>, List<Dictionary<DataProcessingStep, string>>>> _undoChanges = new Dictionary<int, Dictionary<List<string>, List<Dictionary<DataProcessingStep, string>>>>();
        Dictionary<int, Dictionary<List<string>, List<Dictionary<DataProcessingStep, string>>>> _redoChanges = new Dictionary<int, Dictionary<List<string>, List<Dictionary<DataProcessingStep, string>>>>();

        readonly int _nrOfChanges = 50;
        int _undoIndex = 0;
        int _oldUndoIndex;
        int _redoIndex = 0;
        int _oldRedoIndex;

        public int NrOfChanges => _nrOfChanges;
        public int UndoIndex
        {
            get => _undoIndex;
            set => _undoIndex = value;
        }
        public int OldUndoIndex
        {
            get => _oldUndoIndex;
            set => _oldUndoIndex = value;
        }
        public int OldRedoIndex
        {
            get => _oldRedoIndex;
            set => _oldRedoIndex = value;
        }
        public int RedoIndex
        {
            get => _redoIndex;
            set => _redoIndex = value;
        }
        public Dictionary<int, Dictionary<List<string>, List<Dictionary<DataProcessingStep, string>>>> UndoChanges
        {
            get => _undoChanges;
            set => _undoChanges = value;
        }
        public Dictionary<int, Dictionary<List<string>, List<Dictionary<DataProcessingStep, string>>>> RedoChanges
        {
            get => _redoChanges;
            set => _redoChanges = value;
        }

        public Application App { get; } = app;
    }
}