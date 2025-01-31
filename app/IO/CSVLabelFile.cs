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
using System.Collections.Generic;

namespace LabelChecker.IO
{
    public class CSVLabelFile
    {
        public struct ColumnName
        {
            public string Name;
        }
        public class LabelCode
        {
            // list of keys in CSVimage
            public string Label { get; set; }
            public string Code { get; set; }

        }
        public Dictionary<string, ColumnName> ColumnNames = [];

        public Dictionary<string, LabelCode> LabelAndCode = [];

        readonly Application App;

        public List<LabelCode> LabelsAndCodes
        {
            get { return [.. LabelAndCode.Values]; }
        }
        private CSVLabelFile(Application app)
        {
            App = app;
        }



        public static CSVLabelFile Read(string path, Application app)
        {
            // set filepath
            var fullPath = Path.Combine(Environment.CurrentDirectory, path);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException();

            var file = new CSVLabelFile(app);

            string sLine = "";
            using (var sr = new StreamReader(File.OpenRead(path)))
            {
                // first line of the .csv file is the header
                bool headerParsed = false;

                while ((sLine = sr.ReadLine()) != null)
                {
                    if (!headerParsed)
                    {
                        // get column headers from first line in .csv file
                        var headersRow = sLine.Split(',');
                        if (headersRow.Length == 1)
                        {
                            headersRow = sLine.Split(";");
                            if (headersRow.Length == 1)
                            {
                                throw new Exception("CSV seems to be in a wrong file format. Please use commas ',' or colons ';'.");
                            }
                        }

                        foreach (var header in headersRow)
                        {
                            file.ColumnNames.TryAdd(header, new ColumnName() { Name = header });
                        }
                        headerParsed = true;
                        continue;
                    }
                    string[] labelCode = sLine.Split(',');

                    if (labelCode.Length != file.ColumnNames.Count)
                    {
                        labelCode = sLine.Split(';');
                        if (labelCode.Length != file.ColumnNames.Count)
                        {
                            throw new Exception("CSV seems to be in a wrong file format. Please use commas ',' or colons ';'.");
                        }
                    }
                    LabelCode labelCodes = new() { Label = labelCode[0], Code = labelCode[1] };

                    // if ID doesn't exist yet, add them to file variable
                    file.LabelAndCode.TryAdd(labelCodes.Label, labelCodes);
                }
            }
            return file;
        }

        public static CSVLabelFile Create(Application app)
        {
            // add keys label and code header
            CSVLabelFile file = new(app);
            file.ColumnNames.Add("label", new ColumnName() { Name = "label" });
            file.ColumnNames.Add("code", new ColumnName() { Name = "code" });
            return file;
        }
    }
}