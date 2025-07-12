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

using System;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using LabelChecker.Enums;
using LabelChecker.IO;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace LabelChecker.Manager
{
    public class DataManager(Application app)
    {
        long lastTimeSaved = 0;
        readonly long minTimeBetweenSaves = 300;
        readonly Application App = app;
        public List<object> SelectableLabels { get; set; } = [];
        public Dictionary<string, int> LabelCounts { get; set; }

        public void GetSelectableLabels([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, DataProcessingStep dataProcessingStep)
        {
            if (App.ActiveLabelCheckerFile != null)
            {
                switch (dataProcessingStep)
                {
                    case DataProcessingStep.Preprocessing:
                        {
                            List<object> labels = GetLabels(type.GetProperties(), nameof(CSVLabelCheckerFile.DataRow.Preprocessing), nameof(CSVLabelCheckerFile.DataRow.PreprocessingTrue), out _);
                            // only update if list has new items
                            var set = new HashSet<object>(SelectableLabels);
                            if (!set.SetEquals(labels))
                            {
                                SelectableLabels = labels;
                            }
                            break;
                        }
                    case DataProcessingStep.Classification:
                        {
                            List<object> labels = GetLabels(type.GetProperties(), nameof(CSVLabelCheckerFile.DataRow.LabelPredicted), nameof(CSVLabelCheckerFile.DataRow.LabelTrue), out _);
                            // only update if list has new items
                            var set = new HashSet<object>(SelectableLabels);
                            if (!set.SetEquals(labels))
                            {
                                SelectableLabels = labels;
                            }
                            break;
                        }
                }
            }
        }
        private List<object> GetLabels(PropertyInfo[] properties, string predictedColumnName, string trueColumnName, out List<object> labels)
        {
            labels = [];
            // First list all true labels
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.Equals(trueColumnName))
                {
                    // Get unique/ distinct values inside that dictionary
                    labels = GetTrueLabels(property, out _);
                }
            }

            // update true label list with predicted labels
            foreach (PropertyInfo property in properties)
            {
                if (property.Name.Equals(predictedColumnName))
                {
                    labels = UpdateTrueLabels(property, labels);
                }
            }
            return labels;
        }

        private List<object> GetTrueLabels(PropertyInfo property, out List<object> labels)
        {
            labels = [.. App.ActiveLabelCheckerFile.Data
                .Select(dict => property.GetValue(dict, null))
                .Distinct()
                .Order()];
            labels.Remove("");
            labels.Remove("not_included");
            return labels;
        }


        private List<object> UpdateTrueLabels(PropertyInfo property, List<object> existingLabels)
        {
            HashSet<object> existingSet = new(existingLabels);

            List<object> tempList = [.. App.ActiveLabelCheckerFile.Data
                .Select(dict => property.GetValue(dict, null))
                .Distinct()
                .Order()];
            tempList.Remove("");
            tempList.Remove("not_included");

            foreach (object item in tempList)
            {
                if (!existingSet.Contains(item))
                {
                    existingLabels.Add(item);
                }
            }

            return existingLabels;
        }
        public bool GetDataRows(List<CSVLabelCheckerFile.DataRow> dataRows, DataProcessingStep processingStep, out List<CSVLabelCheckerFile.DataRow> imagesToRender)
        {
            imagesToRender = [];

            if (App.ActiveLabelCheckerFile == null)
                return false;

            bool checkedOnly = App.MainMenuControl.CheckedOnly;
            bool notCheckedOnly = App.MainMenuControl.NotCheckedOnly;


            if (App.LabelSelectionControl.SelectedLabel.Count == 0)
            {
                switch (processingStep)
                {
                    case DataProcessingStep.Preprocessing:

                        Func<CSVLabelCheckerFile.DataRow, bool> preprocessingFilter = notCheckedOnly
                            ? data => string.IsNullOrEmpty(data.PreprocessingTrue)
                            : data => !checkedOnly || !string.IsNullOrEmpty(data.PreprocessingTrue);
                        imagesToRender.AddRange(dataRows.Where(preprocessingFilter));
                        break;



                    case DataProcessingStep.Classification:
                        Func<CSVLabelCheckerFile.DataRow, bool> taxonomyFilter = notCheckedOnly
                            ? data =>
                                data.Preprocessing.Equals("object", StringComparison.CurrentCultureIgnoreCase) && data.PreprocessingTrue.Equals("object", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrEmpty(data.LabelTrue) || // both are object
                                data.Preprocessing.Equals("object", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrEmpty(data.PreprocessingTrue) && string.IsNullOrEmpty(data.LabelTrue) || // only predicted is object
                                data.PreprocessingTrue.Equals("object", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrEmpty(data.LabelTrue) || // only true is object
                                string.IsNullOrEmpty(data.Preprocessing) && string.IsNullOrEmpty(data.PreprocessingTrue) && string.IsNullOrEmpty(data.LabelTrue)// both are empty
                            : data =>
                                (data.Preprocessing.Equals("object", StringComparison.CurrentCultureIgnoreCase) && data.PreprocessingTrue.Equals("object", StringComparison.CurrentCultureIgnoreCase) && (!checkedOnly || !string.IsNullOrEmpty(data.LabelTrue))) || // both are object
                                (data.Preprocessing.Equals("object", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrEmpty(data.PreprocessingTrue) && (!checkedOnly || !string.IsNullOrEmpty(data.LabelTrue))) || // only predicted is object
                                (data.PreprocessingTrue.Equals("object", StringComparison.CurrentCultureIgnoreCase) && (!checkedOnly || !string.IsNullOrEmpty(data.LabelTrue))) || // only true is object
                                (string.IsNullOrEmpty(data.Preprocessing) && string.IsNullOrEmpty(data.PreprocessingTrue) && (!checkedOnly || !string.IsNullOrEmpty(data.LabelTrue))); // both are empty
                        imagesToRender.AddRange(dataRows.Where(taxonomyFilter));
                        break;
                }
                imagesToRender = OrderData(imagesToRender, out _);
                return true;
            }
            else
            {
                switch (processingStep)
                {
                    case DataProcessingStep.Preprocessing:
                        Func<CSVLabelCheckerFile.DataRow, bool> preprocessingFilter = notCheckedOnly
                            ? data => App.LabelSelectionControl.SelectedLabel.Contains(data.Preprocessing) && string.IsNullOrEmpty(data.PreprocessingTrue)
                            : data => (App.LabelSelectionControl.SelectedLabel.Contains(data.Preprocessing) || App.LabelSelectionControl.SelectedLabel.Contains(data.PreprocessingTrue))
                                    && (!checkedOnly || !string.IsNullOrEmpty(data.PreprocessingTrue));
                        imagesToRender.AddRange(dataRows.Where(preprocessingFilter));
                        break;

                    case DataProcessingStep.Classification:
                        Func<CSVLabelCheckerFile.DataRow, bool> taxonomyFilter = notCheckedOnly
                            ? data =>
                                data.Preprocessing.Equals("object", StringComparison.CurrentCultureIgnoreCase) && data.PreprocessingTrue.Equals("object", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrEmpty(data.LabelTrue) && App.LabelSelectionControl.SelectedLabel.Contains(data.LabelPredicted) || // both are object
                                data.Preprocessing.Equals("object", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrEmpty(data.PreprocessingTrue) && string.IsNullOrEmpty(data.LabelTrue) && App.LabelSelectionControl.SelectedLabel.Contains(data.LabelPredicted) || // only predicted is object
                                data.PreprocessingTrue.Equals("object", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrEmpty(data.LabelTrue) && App.LabelSelectionControl.SelectedLabel.Contains(data.LabelPredicted) || // only true is object
                                string.IsNullOrEmpty(data.Preprocessing) && string.IsNullOrEmpty(data.PreprocessingTrue) && string.IsNullOrEmpty(data.LabelTrue) && App.LabelSelectionControl.SelectedLabel.Contains(data.LabelPredicted) // both are empty
                            : data =>
                                (data.Preprocessing.Equals("object", StringComparison.CurrentCultureIgnoreCase) && data.PreprocessingTrue.Equals("object", StringComparison.CurrentCultureIgnoreCase) && (App.LabelSelectionControl.SelectedLabel.Contains(data.LabelPredicted) || App.LabelSelectionControl.SelectedLabel.Contains(data.LabelTrue)) && (!checkedOnly || !string.IsNullOrEmpty(data.LabelTrue))) || // both are object
                                (data.Preprocessing.Equals("object", StringComparison.CurrentCultureIgnoreCase) && string.IsNullOrEmpty(data.PreprocessingTrue) && (App.LabelSelectionControl.SelectedLabel.Contains(data.LabelPredicted) || App.LabelSelectionControl.SelectedLabel.Contains(data.LabelTrue)) && (!checkedOnly || !string.IsNullOrEmpty(data.LabelTrue))) || // only predicted is object
                                (data.PreprocessingTrue.Equals("object", StringComparison.CurrentCultureIgnoreCase) && (App.LabelSelectionControl.SelectedLabel.Contains(data.LabelPredicted) || App.LabelSelectionControl.SelectedLabel.Contains(data.LabelTrue)) && (!checkedOnly || !string.IsNullOrEmpty(data.LabelTrue))) || // only true is object
                                (string.IsNullOrEmpty(data.Preprocessing) && string.IsNullOrEmpty(data.PreprocessingTrue) && (App.LabelSelectionControl.SelectedLabel.Contains(data.LabelPredicted) || App.LabelSelectionControl.SelectedLabel.Contains(data.LabelTrue)) && (!checkedOnly || !string.IsNullOrEmpty(data.LabelTrue))); // both are empty
                        imagesToRender.AddRange(dataRows.Where(taxonomyFilter));
                        break;
                }
                imagesToRender = OrderData(imagesToRender, out _);
            }
            return true;
        }
        public Dictionary<string, int> GetLabeledDataCounts(List<CSVLabelCheckerFile.DataRow> dataRows, out Dictionary<string, int> LabelCounts)
        {
            LabelCounts = [];
            if (App.ActiveLabelCheckerFile == null)
            {
                return LabelCounts;
            }
            // get all Taxonomical labels
            GetSelectableLabels(typeof(CSVLabelCheckerFile.DataRow), DataProcessingStep.Classification);

            // get all true labels and counts
            foreach (var label in SelectableLabels)
            {
                LabelCounts.Add(label.ToString(), dataRows.FindAll(data => (data.PreprocessingTrue.Equals("object", StringComparison.CurrentCultureIgnoreCase) || (data.Preprocessing.Equals("object", StringComparison.CurrentCultureIgnoreCase) && data.PreprocessingTrue == "")) && data.LabelTrue == label.ToString()).Count());
            }
            return LabelCounts;
        }

        private List<CSVLabelCheckerFile.DataRow> OrderData(List<CSVLabelCheckerFile.DataRow> imagesToRender, out List<CSVLabelCheckerFile.DataRow> sortedList)
        {
            sortedList = imagesToRender;
            // sort the image list
            sortedList = App.MainMenuControl.EventTracker.DataMenu.IsDescendingOrder
                ? [.. imagesToRender.OrderByDescending(image => App.MainMenuControl.SelectedProperty != null ? App.MainMenuControl.SelectedProperty.GetValue(image, null) : image.Id)]
                : [.. imagesToRender.OrderBy(image => App.MainMenuControl.SelectedProperty != null ? App.MainMenuControl.SelectedProperty.GetValue(image, null) : image.Id)];

            return sortedList;
        }

        public void RenameLabel(string currentLabel, string newLabel)
        {
            List<CSVLabelCheckerFile.DataRow> dataLabelsToRename =
            [
                .. App.ActiveLabelCheckerFile.Data.FindAll(image => image.LabelPredicted == currentLabel || image.LabelTrue == currentLabel).ToList(),
            ];

            foreach (var dataLabel in dataLabelsToRename)
            {
                // offset data ID by one to get index number
                int id = dataLabel.Id - 1;

                // rename LabelPredicted
                if (App.ActiveLabelCheckerFile.Data[id].LabelPredicted.ToString() == currentLabel)
                {
                    App.ActiveLabelCheckerFile.Data[id].LabelPredicted = newLabel;
                }
                // rename LabelTrue
                if (App.ActiveLabelCheckerFile.Data[id].LabelTrue.ToString() == currentLabel)
                {
                    App.ActiveLabelCheckerFile.Data[id].LabelTrue = newLabel;
                }
            }
            // update Selectable labels
            GetSelectableLabels(typeof(CSVLabelCheckerFile.DataRow), App.DataProcessStep);
        }

        public string GetPropertyValue(CSVLabelCheckerFile.DataRow data, string propertyName)
        {
            PropertyInfo property = typeof(CSVLabelCheckerFile.DataRow).GetProperty(propertyName);
            object value = property.GetValue(data, null);
            if (value is double d)
            {
                return d.ToString("F3", CultureInfo.InvariantCulture);
            }
            return value?.ToString();
        }

        public void Save(bool manualSave = false)
        {
            DateTime dt = DateTime.UtcNow;
            var currentTime = dt.ToUnixTimestamp();
            var deltaTime = currentTime - lastTimeSaved;
            if (deltaTime < minTimeBetweenSaves && !manualSave)
            {
                //we won't save to often. so return for now.
                return;
            }
            IEnumerable<object> columns = [];
            var properties = typeof(CSVLabelCheckerFile.DataRow).GetProperties();
            Parallel.ForEach(App.ActiveLabelCheckerFile.FileData, fileDataMap =>
                {
                    List<CSVLabelCheckerFile.DataRow> dataRows = App.ActiveLabelCheckerFile.Data.FindAll(data => fileDataMap.Uuids.Contains(data.Uuid));
                    List<int> originalIds = [];
                    List<string> filePaths = [];
                    foreach (CSVLabelCheckerFile.DataRow data in dataRows)
                    {
                        App.ActiveLabelCheckerFile.GetDataFileMap(data.Uuid, out CSVLabelCheckerFile.DataFileMap dataFilePath);
                        filePaths.Add(dataFilePath.FilePath);
                        originalIds.Add(dataFilePath.Id);
                    }
                    List<string> filePath = filePaths.Distinct().ToList();
                    if (filePath.Count > 1)
                    {
                        throw new InvalidOperationException("Failed to save data: multiple file paths found.");
                    }
                    dataRows.Sort((a, b) => a.Id.CompareTo(b.Id));
                    SaveData(filePath[0], dataRows, originalIds);
                }
            );

            lastTimeSaved = currentTime;
            App.SaveWindow.SaveMessage = manualSave
                ? $"Last manual save at {dt.ToLocalTime()}"
                : $"Last auto save at {dt.ToLocalTime()}";
            App.HotkeyManager.DisableHotkeys = false;
        }

        private static void SaveData(string filePath, List<CSVLabelCheckerFile.DataRow> dataRows, List<int> originalIds)
        {
            var delimiter = ",";
            IEnumerable<object> columns = [];
            var properties = typeof(CSVLabelCheckerFile.DataRow).GetProperties();
            using (StreamWriter sw = new(filePath, false))
            {
                // Column header
                var header = properties.Select(n => n.Name)
                .Aggregate((a, b) => a + delimiter + b);
                sw.WriteLine(header);

                // data per column
                foreach (var property in properties)
                {

                    // Get all values from column
                    var column = property.Name == "Id"
                        ? originalIds.Select(id => (object)id.ToString()).ToList()
                        : dataRows.Select(dict => property.GetValue(dict, null)).ToList();

                    // If no data in columns yet, write column data to column
                    if (!columns.Any())
                    {
                        columns = column;
                    }
                    else
                    {
                        // merge data delimitated by delimiter
                        columns = columns.Zip(column, (List1, List2) => Convert.ToString(List1, CultureInfo.InvariantCulture) + delimiter + Convert.ToString(List2, CultureInfo.InvariantCulture));
                    }
                }
                // write all lines to file
                foreach (var line in columns)
                {
                    sw.WriteLine(line);
                }
            }
        }


        // assign corrected labels to an image 
        public void AssignCorrectedLabels(DataProcessingStep step, string label, List<int> imageIndices)
        {
            // update undo dictionary
            AssignUndoChange(step, imageIndices);

            // go over each item in the list
            foreach (int item in imageIndices)
            {
                // Assign new label to Quality check
                if (!string.IsNullOrEmpty(label))
                {
                    switch (step)
                    {
                        case DataProcessingStep.Preprocessing:
                            App.ActiveLabelCheckerFile.Data[item].PreprocessingTrue = label.ToString();
                            break;
                        case DataProcessingStep.Classification:
                            App.ActiveLabelCheckerFile.Data[item].LabelTrue = label.ToString();
                            // if labelPredicted is empty, assign same label to it
                            if (App.ActiveLabelCheckerFile.Data[item].LabelPredicted == "")
                            {
                                App.ActiveLabelCheckerFile.Data[item].LabelPredicted = label.ToString();
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    // Assign output label to Quality check
                    switch (step)
                    {
                        case DataProcessingStep.Preprocessing:
                            App.ActiveLabelCheckerFile.Data[item].PreprocessingTrue = App.ActiveLabelCheckerFile.Data[item].Preprocessing;
                            break;
                        case DataProcessingStep.Classification:
                            App.ActiveLabelCheckerFile.Data[item].LabelTrue = App.ActiveLabelCheckerFile.Data[item].LabelPredicted;
                            break;
                        default:
                            break;
                    }
                }
            }
            // reset redo changes
            App.UndoRedoManager.RedoIndex = 0;
            App.UndoRedoManager.OldRedoIndex = 0;
            App.UndoRedoManager.RedoChanges.Clear();
            App.HotkeyManager.DisableHotkeys = !App.HotkeyManager.DisableHotkeys;
        }

        public void AssignUndoChange(DataProcessingStep step, List<int> imageIndices)
        {
            // initialize list for storing old labels
            List<Dictionary<DataProcessingStep, string>> oldLabels = [];

            // go over each item in the list
            foreach (int item in imageIndices)
            {
                // Assign output label to Quality check
                switch (step)
                {
                    case DataProcessingStep.Preprocessing:
                        oldLabels.Add(new Dictionary<DataProcessingStep, string> { { step, App.ActiveLabelCheckerFile.Data[item].PreprocessingTrue } });
                        break;
                    case DataProcessingStep.Classification:
                        oldLabels.Add(new Dictionary<DataProcessingStep, string> { { step, App.ActiveLabelCheckerFile.Data[item].LabelTrue } });
                        break;
                    default:
                        break;
                }
            }

            // if key already exists, remove it
            if (App.UndoRedoManager.UndoChanges.ContainsKey(App.UndoRedoManager.UndoIndex))
            {
                App.UndoRedoManager.UndoChanges.Remove(App.UndoRedoManager.UndoIndex);
            }

            // log old data
            App.UndoRedoManager.UndoChanges.Add(App.UndoRedoManager.UndoIndex, []);
            App.UndoRedoManager.UndoChanges[App.UndoRedoManager.UndoIndex].Add(imageIndices.ConvertAll(c => c.ToString()), oldLabels);

            // increment dictionary index only storing last 10 changes
            App.UndoRedoManager.UndoIndex += 1;
            if (App.UndoRedoManager.UndoIndex > App.UndoRedoManager.NrOfChanges)
            {
                App.UndoRedoManager.UndoIndex = 0;
            }
            // track last changed storage and increment only when NrOfChanges have been filled
            if (App.UndoRedoManager.OldUndoIndex == App.UndoRedoManager.UndoIndex)
            {
                App.UndoRedoManager.OldUndoIndex += 1;
                if (App.UndoRedoManager.OldUndoIndex > App.UndoRedoManager.NrOfChanges)
                {
                    App.UndoRedoManager.OldUndoIndex = 0;
                }
            }
        }

        public void UndoChanges()
        {
            // initialize list for storing current labels for redoing change
            List<string> currentIndices = [];
            List<Dictionary<DataProcessingStep, string>> currentLabels = [];

            // get current index
            int index = App.UndoRedoManager.UndoIndex - 1;
            if (index < 0)
            {
                index = App.UndoRedoManager.NrOfChanges - 1;
            }

            // Ensure not looping back to beginning of undo list and whether the key exists
            if (App.UndoRedoManager.UndoIndex != App.UndoRedoManager.OldUndoIndex && App.UndoRedoManager.UndoChanges.ContainsKey(index))
            {
                // step one index back
                App.UndoRedoManager.UndoIndex = index;

                // get changed indexes and old labels
                Dictionary<List<string>, List<Dictionary<DataProcessingStep, string>>> dict =
                    new(App.UndoRedoManager.UndoChanges[App.UndoRedoManager.UndoIndex]);

                foreach (var item in dict)
                {
                    var indicesAndLabels = item.Key.Zip(item.Value, (i, l) => new { index = i, labelDict = l });
                    foreach (var il in indicesAndLabels)
                    {
                        currentIndices.Add(il.index);
                        var currentLabel = new Dictionary<DataProcessingStep, string>();

                        foreach (var stepLabel in il.labelDict)
                        {
                            var step = stepLabel.Key;
                            var label = stepLabel.Value;

                            // Store current value for redo
                            switch (step)
                            {
                                case DataProcessingStep.Preprocessing:
                                    currentLabel[step] = App.ActiveLabelCheckerFile.Data[Int32.Parse(il.index)].PreprocessingTrue;
                                    App.ActiveLabelCheckerFile.Data[Int32.Parse(il.index)].PreprocessingTrue = label;
                                    break;
                                case DataProcessingStep.Classification:
                                    currentLabel[step] = App.ActiveLabelCheckerFile.Data[Int32.Parse(il.index)].LabelTrue;
                                    App.ActiveLabelCheckerFile.Data[Int32.Parse(il.index)].LabelTrue = label;
                                    break;
                            }
                        }
                        currentLabels.Add(currentLabel);
                    }
                }
                // save undo changes for redoing
                AssignRedoChange(currentIndices, currentLabels);
            }
        }

        public void AssignRedoChange(List<string> imageIndices, List<Dictionary<DataProcessingStep, string>> imageLabels)
        {
            // if key already exists, remove it
            if (App.UndoRedoManager.RedoChanges.ContainsKey(App.UndoRedoManager.RedoIndex))
            {
                App.UndoRedoManager.RedoChanges.Remove(App.UndoRedoManager.RedoIndex);
            }

            // log new data
            App.UndoRedoManager.RedoChanges.Add(App.UndoRedoManager.RedoIndex, []);
            App.UndoRedoManager.RedoChanges[App.UndoRedoManager.RedoIndex].Add(imageIndices, imageLabels);

            // increment dictionary index only storing last changes up to NrOfChanges
            App.UndoRedoManager.RedoIndex += 1;
            if (App.UndoRedoManager.RedoIndex > App.UndoRedoManager.NrOfChanges)
            {
                App.UndoRedoManager.RedoIndex = 0;
            }
            // track last changed storage and increment only when NrOfChanges have been filled
            if (App.UndoRedoManager.OldRedoIndex == App.UndoRedoManager.RedoIndex)
            {
                App.UndoRedoManager.OldRedoIndex += 1;
                if (App.UndoRedoManager.OldRedoIndex > App.UndoRedoManager.NrOfChanges)
                {
                    App.UndoRedoManager.OldRedoIndex = 0;
                }
            }
        }

        public void ReDoChanges()
        {
            // get current index
            int index = App.UndoRedoManager.RedoIndex - 1;
            if (index < 0)
            {
                index = App.UndoRedoManager.NrOfChanges - 1;
            }

            // Ensure not looping back to beginning of redo list and whether the key exists
            if (App.UndoRedoManager.RedoIndex != App.UndoRedoManager.OldRedoIndex && App.UndoRedoManager.RedoChanges.ContainsKey(index))
            {
                // step one index back
                App.UndoRedoManager.RedoIndex = index;

                // get changed indexes and labels to restore
                Dictionary<List<string>, List<Dictionary<DataProcessingStep, string>>> dict =
                    new(App.UndoRedoManager.RedoChanges[App.UndoRedoManager.RedoIndex]);

                foreach (var item in dict)
                {
                    var indicesAndLabels = item.Key.Zip(item.Value, (i, l) => new { index = i, labelDict = l });
                    foreach (var il in indicesAndLabels)
                    {
                        foreach (var stepLabel in il.labelDict)
                        {
                            var step = stepLabel.Key;
                            var label = stepLabel.Value;

                            switch (step)
                            {
                                case DataProcessingStep.Preprocessing:
                                    App.ActiveLabelCheckerFile.Data[Int32.Parse(il.index)].PreprocessingTrue = label;
                                    break;
                                case DataProcessingStep.Classification:
                                    App.ActiveLabelCheckerFile.Data[Int32.Parse(il.index)].LabelTrue = label;
                                    break;
                            }
                        }
                    }
                }

                // update undo index
                App.UndoRedoManager.UndoIndex += 1;
                if (App.UndoRedoManager.UndoIndex > App.UndoRedoManager.NrOfChanges)
                {
                    App.UndoRedoManager.UndoIndex = 0;
                }
            }
        }
    }
}
