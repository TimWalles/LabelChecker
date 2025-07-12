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

using System.IO;
using System.Collections.Concurrent;
using System.Drawing;
using LabelChecker.IO;
using System;

namespace LabelChecker.Manager
{
    public class TextureManager(Application app)
    {
        readonly Application _app = app;
        readonly ConcurrentDictionary<string, ImGuiTexture2D> _textures = new ConcurrentDictionary<string, ImGuiTexture2D>();
        readonly int MaxNrOfTextures = 1000;

        public bool TryGetTexture(Guid id, out ImGuiTexture2D texture, out RectangleF dimension)
        {
            texture = null;
            dimension = new RectangleF(0, 0, 0, 0);
            if (_app.ActiveLabelCheckerFile != null)
            {
                if (_app.ActiveLabelCheckerFile.GetImage(id, out CSVLabelCheckerFile.DataRow dataRow) && _app.ActiveLabelCheckerFile.GetDataFileMap(id, out CSVLabelCheckerFile.DataFileMap dataFilePath))
                {
                    dimension = new RectangleF(dataRow.ImageX, dataRow.ImageY, dataRow.ImageW, dataRow.ImageH);
                    // If the image is already loaded, return this
                    if (!string.IsNullOrEmpty(dataRow.ImageFilename) && _textures.TryGetValue(dataRow.ImageFilename, out ImGuiTexture2D value))
                    {
                        texture = value;
                        return true;
                    }
                    // If the collage already loaded return this
                    if (_textures.ContainsKey(dataRow.CollageFile))
                    {
                        texture = _textures[dataRow.CollageFile];
                        return true;
                    }

                    // Load the image or collage
                    string path;
                    if (string.IsNullOrEmpty(dataRow.ImageFilename))
                    {
                        // collages are inside the same folder
                        path = Path.Combine(Path.GetDirectoryName(dataFilePath.FilePath), dataRow.CollageFile);
                    }
                    else
                    {
                        // Try original folder (sample name)
                        var baseDir = Path.GetDirectoryName(dataFilePath.FilePath);
                        var sampleFolder = Path.Combine(baseDir, dataRow.Name);
                        var sampleImagesFolder = Path.Combine(baseDir, dataRow.Name + " Images");
                        var imagePath = Path.Combine(sampleFolder, dataRow.ImageFilename);
                        var imagePathWithImages = Path.Combine(sampleImagesFolder, dataRow.ImageFilename);
                        if (File.Exists(imagePath))
                        {
                            path = imagePath;
                        }
                        else if (File.Exists(imagePathWithImages))
                        {
                            path = imagePathWithImages;
                        }
                        else
                        {
                            // fallback to original logic (sample name folder)
                            path = imagePath;
                        }
                    }

                    // catch if the collages aren't with the .csv data
                    if (!File.Exists(path))
                    {
                        return false;
                    }
                    // load collage into ImGuiTexture
                    var newTex = ImGuiTexture2D.FromImage(path, _app.GraphicsDevice, _app.Renderer);

                    if (newTex != null)
                    {
                        texture = newTex;
                        _ = string.IsNullOrEmpty(dataRow.ImageFilename)
                        ? _textures.TryAdd(dataRow.CollageFile, newTex)
                        : _textures.TryAdd(dataRow.ImageFilename, newTex);
                        return true;
                    }

                    // remove the oldest texture if the limit is reached
                    if (_textures.Count >= MaxNrOfTextures)
                    {
                        foreach (var key in _textures.Keys)
                        {
                            if (TryRemoveTexture(key))
                            {
                                break;
                            }
                        }
                    }

                }
            }
            return false;
        }

        private bool TryRemoveTexture(string key)
        {
            return _textures.TryRemove(key, out _);
        }
    }
}