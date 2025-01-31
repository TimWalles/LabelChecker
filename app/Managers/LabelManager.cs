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
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static LabelChecker.IO.CSVLabelFile;

namespace LabelChecker.Manager
{
    public class LabelManager(Application app)
    {
        readonly Application App = app;

        public List<object> GetLabels([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, out List<object> labelNames)
        {
            labelNames = [];
            foreach (var property in type.GetProperties())
            {
                // first option is for going of the preprocessed data
                if (property.Name.Contains("Label"))
                {
                    // Get unique/ distinct values inside that dictionary
                    labelNames = App.ActiveLabelsFile.LabelsAndCodes.Select(dict => property.GetValue(dict, null)).ToList();
                }
            }
            return labelNames;
        }

        public List<object> GetCodes([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, out List<object> codeNumbers)
        {
            codeNumbers = [];
            foreach (var property in type.GetProperties())

            {
                // first option is for going of the preprocessed data
                if (property.Name.Contains("Code"))
                {
                    // Get unique/ distinct values inside that dictionary
                    codeNumbers = App.ActiveLabelsFile.LabelsAndCodes.Select(dict => property.GetValue(dict, null)).ToList();
                }
            }
            return codeNumbers;
        }

        public List<LabelCode> GetLabelsAndCodes()
        {
            return App.ActiveLabelsFile.LabelsAndCodes;
        }

        private static string GetLabelFromCode(string value, List<LabelCode> labelsCodes, out string name)
        {
            {
                name = "";
                var matchingLabelCode = labelsCodes.FirstOrDefault(labelCode => labelCode.Code.ToString() == value);
                return matchingLabelCode?.Label ?? name;
            }
        }

        private static string GetLabelFromLabel(string value, List<LabelCode> labelsCodes, out string name)
        {
            {
                name = "";
                var matchingLabelCode = labelsCodes.FirstOrDefault(labelCode => labelCode.Label.Contains(value, StringComparison.CurrentCultureIgnoreCase));
                return matchingLabelCode?.Label ?? name;
            }
        }

        public string GetSelectedLabel(string value, out string name)
        {
            name = "";
            List<LabelCode> labelsCodes = GetLabelsAndCodes();

            // check value is a code number
            bool isCodeNr = int.TryParse(value, out _);
            // assign matching code number if code
            if (isCodeNr)
            {
                name = GetLabelFromCode(value, labelsCodes, out _);
            }
            // if not a code, get matching label
            else
            {
                name = GetLabelFromLabel(value, labelsCodes, out _);
            }

            // if code but empty, check if number occures in label
            if (isCodeNr && string.IsNullOrEmpty(name))
            {
                name = GetLabelFromLabel(value, labelsCodes, out _);
            }
            return name;
        }

        private static LabelCode CreateLabelCode(string label, string code, out LabelCode labelCode)
        {
            labelCode = new LabelCode { Label = label, Code = code };
            return labelCode;
        }

        public void Create(string label, string code)
        {
            LabelCode labelCodes = CreateLabelCode(label, code, out _);
            // if ID doesn't exist yet, add them to file variable
            App.ActiveLabelsFile.LabelAndCode.TryAdd(labelCodes.Label, labelCodes);
        }

        public void Update(string label, string newLabel)
        {
            LabelCode labelCodes = CreateLabelCode(newLabel, App.ActiveLabelsFile.LabelAndCode.GetValueOrDefault(label).Code, out _);
            App.ActiveLabelsFile.LabelAndCode.Remove(label);
            App.ActiveLabelsFile.LabelAndCode.TryAdd(labelCodes.Label, labelCodes);
        }
        public void Delete(string label)
        {
            LabelCode labelCodes = CreateLabelCode(label, App.ActiveLabelsFile.LabelAndCode.GetValueOrDefault(label).Code, out _);
            App.ActiveLabelsFile.LabelAndCode.Remove(labelCodes.Label);
        }

        public void Save()
        {
            var delimiter = ",";
            IEnumerable<object> columns = [];
            var properties = typeof(LabelCode).GetProperties();
            var labelsList = App.ActiveLabelsFile.LabelsAndCodes;
            using (StreamWriter sw = new(App.OpenLabelPicker.SelectedFile, false))
            {
                // Column header
                var header = properties.Select(n => n.Name)
                .Aggregate((a, b) => a + delimiter + b);
                sw.WriteLine(header);

                // data per column
                foreach (var property in properties)
                {
                    // Get all values from column
                    var column = labelsList.Select(dict => property.GetValue(dict, null)).ToList();

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
    }
}