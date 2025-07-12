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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace LabelChecker.IO
{
    public class CSVLabelCheckerFile
    {
        public struct ColumnName
        {
            public string Name;
        }
        public class DataRow
        {
            // list of keys in CSVimage
            public string Name { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
            public string CollageFile { get; set; }
            public string ImageFilename { get; set; }
            public int Id { get; set; }
            public int GroupId { get; set; }
            public Guid Uuid { get; set; }
            public int SrcImage { get; set; }
            public int SrcX { get; set; }
            public int SrcY { get; set; }
            public int ImageX { get; set; }
            public int ImageY { get; set; }
            public int ImageW { get; set; }
            public int ImageH { get; set; }
            public string Timestamp { get; set; }
            public double ElapsedTime { get; set; }
            public double CalConst { get; set; }
            public int CalImage { get; set; }
            public double AbdArea { get; set; }
            public double AbdDiameter { get; set; }
            public double AbdVolume { get; set; }
            public double AspectRatio { get; set; }
            public double AvgBlue { get; set; }
            public double AvgGreen { get; set; }
            public double AvgRed { get; set; }
            public double BiovolumeCylinder { get; set; }
            public double BiovolumePSpheroid { get; set; }
            public double BiovolumeSphere { get; set; }
            public double Ch1Area { get; set; }
            public double Ch1Peak { get; set; }
            public double Ch1Width { get; set; }
            public double Ch2Area { get; set; }
            public double Ch2Peak { get; set; }
            public double Ch2Width { get; set; }
            public double Ch3Area { get; set; }
            public double Ch3Peak { get; set; }
            public double Ch3Width { get; set; }
            public double Ch2Ch1Ratio { get; set; }
            public double CircleFit { get; set; }
            public double Circularity { get; set; }
            public double CircularityHu { get; set; }
            public double Compactness { get; set; }
            public double Convexity { get; set; }
            public double ConvexPerimeter { get; set; }
            public double EdgeGradient { get; set; }
            public double Elongation { get; set; }
            public double EsdDiameter { get; set; }
            public double EsdVolume { get; set; }
            public double FdDiameter { get; set; }
            public double FeretMaxAngle { get; set; }
            public double FeretMinAngle { get; set; }
            public double FiberCurl { get; set; }
            public double FiberStraightness { get; set; }
            public double FilledArea { get; set; }
            public double FilterScore { get; set; }
            public double GeodesicAspectRatio { get; set; }
            public double GeodesicLength { get; set; }
            public double GeodesicThickness { get; set; }
            public double Intensity { get; set; }
            public double Length { get; set; }
            public int Ppc { get; set; }
            public double Perimeter { get; set; }
            public double RatioBlueGreen { get; set; }
            public double RatioRedBlue { get; set; }
            public double RatioRedGreen { get; set; }
            public double Roughness { get; set; }
            public double ScatterArea { get; set; }
            public double ScatterPeak { get; set; }
            public double SigmaIntensity { get; set; }
            public int SphereComplement { get; set; }
            public int SphereCount { get; set; }
            public int SphereUnknown { get; set; }
            public double SphereVolume { get; set; }
            public double SumIntensity { get; set; }
            public double Symmetry { get; set; }
            public double Transparency { get; set; }
            public double Width { get; set; }
            public string Preprocessing { get; set; }
            public string PreprocessingTrue { get; set; }
            public string LabelPredicted { get; set; }
            public double ProbabilityScore { get; set; }
            public string LabelTrue { get; set; }
            public double BiovolumeMS { get; set; }
            public double SurfaceAreaMS { get; set; }
        }

        public class DataFileMap
        {
            public string FilePath { get; set; }
            public int Id { get; set; }
        }
        public class FileDataMap
        {
            public List<Guid> Uuids { get; set; }
        }
        readonly Dictionary<string, ColumnName> ColumnNames = [];
        readonly Dictionary<Guid, DataRow> DataRows = [];
        readonly Dictionary<Guid, DataFileMap> DataFileMaps = [];
        readonly Dictionary<string, FileDataMap> FileDataMaps = [];
        readonly Application App;

        public List<DataRow> Data
        {
            get { return [.. DataRows.Values]; }
        }
        public List<FileDataMap> FileData
        {
            get { return [.. FileDataMaps.Values]; }
        }

        public bool GetImage(Guid id, out DataRow image)
        {
            image = null;
            if (DataRows.ContainsKey(id))
            {
                return DataRows.TryGetValue(id, out image);
            }
            return false;
        }

        public bool GetDataFileMap(Guid id, out DataFileMap dataFileMap)
        {
            dataFileMap = null;
            if (DataFileMaps.ContainsKey(id))
            {
                return DataFileMaps.TryGetValue(id, out dataFileMap);
            }
            return false;
        }

        private CSVLabelCheckerFile(Application app)
        {
            App = app;
        }
        public static CSVLabelCheckerFile Read(string path, Application app)
        {
            int currentLine = 0;
            var fullPath = Path.Combine(Environment.CurrentDirectory, path);
            var fileName = Path.GetFileName(fullPath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"The file '{fileName}' doesn't seem to exist.");

            var file = new CSVLabelCheckerFile(app);
            List<Guid> uuids = [];
            bool headerParsed = false;
            try
            {
                string sLine = "";
                using (var sr = new StreamReader(File.OpenRead(path)))
                {
                    while ((sLine = sr.ReadLine()) != null)
                    {
                        currentLine++;
                        if (!headerParsed)// first line of the .csv file is the header
                        {
                            // get keys from first line in .csv file
                            var headersRow = sLine.Replace("\"", "").Replace("\\", "").Split(',');
                            if (headersRow.Length == 1)
                            {
                                headersRow = sLine.Split(";");
                                if (headersRow.Length == 1)
                                {
                                    app.ErrorWindow.ErrorPopupMessage = $"{fileName} CSV seems to be in a wrong file format. Please use commas ',' or colons ';'.";
                                    break;
                                }
                            }

                            foreach (var key in headersRow)
                            {
                                file.ColumnNames.TryAdd(key, new ColumnName() { Name = key, });
                            }
                            headerParsed = true;
                            continue;
                        }

                        // read in image details
                        var dataRow = sLine.Replace("\"", "").Replace("\\", "").Split(',');
                        if (dataRow.Length != file.ColumnNames.Count)
                        {
                            dataRow = sLine.Replace("\"", "").Replace("\\", "").Split(';');
                            if (dataRow.Length != file.ColumnNames.Count)
                            {
                                app.ErrorWindow.ErrorPopupMessage = $"{fileName} CSV seems to be in a wrong file format. Please use commas ',' or colons ';'.";
                                break;
                            }
                        }
                        // generate LSTImage dictionary
                        var newImage = new DataRow();
                        var newFileMap = new DataFileMap();
                        var it = typeof(DataRow);
                        int colIndex = 0;

                        // loop over the image keys
                        foreach (var field in file.ColumnNames)
                        {
                            PropertyInfo csvProperty = null;
                            // get keys from LSTfile dictory
                            try
                            {
                                csvProperty = GetProperty(field.Key, it);
                            }
                            catch (ArgumentException ex)
                            {
                                app.ErrorWindow.ErrorPopupName = ex.GetType().Name;
                                app.ErrorWindow.ErrorPopupMessage = $"Error while parsing file {fileName} at line {currentLine} at column {field.Key}." + Environment.NewLine + Environment.NewLine + $"Unknown column name: {field.Key}";
                                return null;
                            }
                            if (csvProperty != null)
                            {
                                Type dataType = csvProperty.PropertyType;

                                // generate default values if null
                                if (string.IsNullOrEmpty(dataRow[colIndex]))
                                {
                                    dataRow[colIndex] = dataType switch
                                    {
                                        var t when t == typeof(int) => "0",
                                        var t when t == typeof(double) => "0.0",
                                        var t when t == typeof(string) => "",
                                        _ => ""
                                    };
                                }
                                try
                                {
                                    // parse data to correct type
                                    switch (dataType)
                                    {
                                        case var t when t == typeof(int):
                                            csvProperty.SetValue(newImage, int.Parse(dataRow[colIndex++]));
                                            break;
                                        case var t when t == typeof(double):
                                            csvProperty.SetValue(newImage, double.Parse(dataRow[colIndex++], NumberStyles.Any, CultureInfo.InvariantCulture));
                                            break;
                                        case var t when t == typeof(string):
                                            csvProperty.SetValue(newImage, dataRow[colIndex++]);
                                            break;
                                        case var t when t == typeof(Guid):
                                            csvProperty.SetValue(newImage, Guid.Parse(dataRow[colIndex++]));
                                            break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    app.ErrorWindow.ErrorPopupName = ex.GetType().Name;
                                    app.ErrorWindow.ErrorPopupMessage = $"Error while reading file {fileName} at line {currentLine} at column {field.Key}." + Environment.NewLine + Environment.NewLine + $"{ex.Message} Target data format: " + dataType;
                                    return null;
                                }
                            }//do something when its null
                            else
                            {
                                // throw new Exception($"Error while parsing file {fileName} at line {currentLine} at column {colIndex}.");
                                app.ErrorWindow.ErrorPopupName = "Parsing Error";
                                app.ErrorWindow.ErrorPopupMessage = $"Error while parsing file {fileName} at line {currentLine} at column {field.Key}.";
                                return null;
                            }
                        }


                        // if ID does not exist yet, add them to file variable
                        file.DataRows.TryAdd(newImage.Uuid, newImage);

                        // add file path to file variable
                        newFileMap.FilePath = fullPath;
                        newFileMap.Id = newImage.Id;

                        file.DataFileMaps.TryAdd(newImage.Uuid, newFileMap);

                        // add Uuid to list
                        uuids.Add(newImage.Uuid);
                    }
                }
                // add Uuids to file variable
                file.FileDataMaps.TryAdd(fullPath, new FileDataMap() { Uuids = uuids });

                // reset page index
                app.PageNrSelectionControl.PageNumber = 1;

                return file;
            }
            catch (Exception ex)
            {
                if (!headerParsed)
                {
                    // throw new Exception($"Error while parsing the file {fileName} at line {currentLine}. Please check your header.");
                    app.ErrorWindow.ErrorPopupName = ex.GetType().Name;
                    app.ErrorWindow.ErrorPopupMessage = $"Error while parsing the file {fileName} at line {currentLine}. Please check your header.";
                    return null;
                }
                else
                {
                    app.ErrorWindow.ErrorPopupName = ex.GetType().Name;
                    app.ErrorWindow.ErrorPopupMessage = $"Error while parsing the file {fileName} at line {currentLine}" + Environment.NewLine + Environment.NewLine + $"{ex.Message}";
                    return null;
                }
            }
        }

        public static CSVLabelCheckerFile ReadAll(string file_path, Application app, CSVLabelCheckerFile labelCheckerFile = null)
        {
            labelCheckerFile ??= new CSVLabelCheckerFile(app);
            var file = Read(file_path, app);
            if (file == null)
            {
                return null;
            }
            if (file.Data.Count > 0)
            {
                try
                {
                    foreach (var image in file.Data)
                    {
                        labelCheckerFile.DataRows.TryAdd(image.Uuid, image);
                        labelCheckerFile.DataFileMaps.TryAdd(image.Uuid, new DataFileMap()
                        {
                            FilePath = file.DataFileMaps[image.Uuid].FilePath,
                            Id = file.DataFileMaps[image.Uuid].Id
                        });
                    }

                    foreach (var fileDataMap in file.FileDataMaps)
                    {
                        labelCheckerFile.FileDataMaps.TryAdd(fileDataMap.Key, fileDataMap.Value);
                    }
                }
                catch (Exception ex)
                {
                    app.ErrorWindow.ErrorPopupName = ex.GetType().Name;
                    app.ErrorWindow.ErrorPopupMessage = $"Error while reading file {file_path}." + Environment.NewLine + Environment.NewLine + $"{ex.Message}";
                    return null;
                }
            }
            else
            {
                return null;
            }
            return labelCheckerFile;
        }

        public static CSVLabelCheckerFile ConcatenateFiles(List<CSVLabelCheckerFile> files, Application app)
        {
            var newFile = new CSVLabelCheckerFile(app);
            Parallel.ForEach(files, file =>
            {
                Parallel.ForEach(file.Data, image =>
                {
                    lock (newFile)
                    {
                        newFile.DataRows.TryAdd(image.Uuid, image);
                        newFile.DataFileMaps.TryAdd(image.Uuid, new DataFileMap() { FilePath = file.DataFileMaps[image.Uuid].FilePath });
                    }
                });

                Parallel.ForEach(file.FileDataMaps, fileDataMap =>
                {
                    lock (newFile)
                    {
                        newFile.FileDataMaps.TryAdd(fileDataMap.Key, fileDataMap.Value);
                    }
                });
            });
            return newFile;
        }


        static PropertyInfo GetProperty(string name, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type t)
        {
            foreach (var property in t.GetProperties())
            {
                var pName = property.Name.ToLower();
                if (pName == name.Replace("_", "").ToLower())
                {
                    return property;
                }
            }
            throw new ArgumentException($"Property {name} not found in {t.Name}");
        }

    }
}