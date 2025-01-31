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

using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;
using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Num = System.Numerics;
using ImGuiNET;
using LabelChecker.IO;
using LabelChecker.Enums;

namespace LabelChecker.Controls
{
    public class ImageGrid(Application app) : Control(app)
    {
        public override string Name => "ImageGrid";
        public List<int> ImageGridIndices = [];
        public List<int> SelectedImageGridIndices = [];
        public List<int> UnselectedImageGridIndices = [];
        public List<int> LabelledIndices = [];
        public List<int> LastSelectedIndices = [];
        public bool ScaleImage = false;
        public bool Magnification = false;
        public bool ShowFileInfo = false;
        public bool ShowParameterValue = false;
        public string ParameterValueName = "";
        public int MagnificationFactor = 1;
        public Num.Vector2 WindowSize = new(800, 600);
        public Num.Vector2 WindowPosition = new(300, 50);
        int gridCounter = 1;
        Num.Vector2 imageSize = new(0, 0);
        Color WhiteTransparent = new(255, 255, 255, 100);
        static Dictionary<object, Color> label_colour_map = null;


        public override void Update(GameTime time)
        {

        }
        public override void Draw(ImGuiIOPtr ioPtr)
        {
            if (App.ActiveLabelCheckerFile != null)
            {
                //Console.WriteLine(App.OpenFlowCamPicker.CurrentFolder);
                // get image list to render
                App.DataManager.GetDataRows(App.ActiveLabelCheckerFile.Data, App.DataProcessStep, out List<CSVLabelCheckerFile.DataRow> dataRows);

                // update number of pages for go to button
                App.PageNrSelectionControl.MaxPageNumbers = (int)Math.Round((dataRows.Count / (App.SetRowColumnControl.ColumnNumbers * App.SetRowColumnControl.RowNumbers)) + .9);

                // Image grid display
                ImGui.SetNextWindowSize(WindowSize, ImGuiCond.Once);
                ImGui.SetNextWindowPos(WindowPosition, ImGuiCond.Once);

                // get window title
                string WindowTitle = string.IsNullOrEmpty(App.OpenLabelCheckerFilePicker.SelectedFile)
                    ? "Selected Folders"
                    : Path.GetFileNameWithoutExtension(App.OpenLabelCheckerFilePicker.SelectedFile);

                if (ImGui.Begin(WindowTitle, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse))
                {
                    // reset window position if outside of viewport
                    App.Utils.ScaleWindowToViewport(WindowTitle);

                    // Get the window size
                    WindowSize = ImGui.GetWindowSize();
                    App.Utils.SaveWindowSize(WindowSize);


                    // Get the window position
                    WindowPosition = ImGui.GetWindowPos();
                    App.Utils.SaveWindowPosition(WindowPosition);

                    // only display images when a processing step is selected
                    if (!App.DataProcessStep.Equals(DataProcessingStep.None))
                    {
                        UnselectedImageGridIndices.Clear();
                        ImageGridIndices.Clear();
                        LabelledIndices.Clear();
                        // number of images to render
                        int maxImageNumbers = App.SetRowColumnControl.ColumnNumbers * App.SetRowColumnControl.RowNumbers * App.PageNrSelectionControl.PageNumber;
                        int imageIndices = App.SetRowColumnControl.ColumnNumbers * App.SetRowColumnControl.RowNumbers * (App.PageNrSelectionControl.PageNumber - 1);

                        // get window dimensions
                        var windowSize = ImGui.GetWindowSize();

                        // get image sizes (we take a bit of the bottom as )
                        Num.Vector2 columnSize = new(windowSize.X / App.SetRowColumnControl.ColumnNumbers, windowSize.Y * .95f / App.SetRowColumnControl.RowNumbers);

                        // loop over the number of images to render at index (idx) in lst file
                        for (int idx = imageIndices; idx < maxImageNumbers; idx++)
                        {
                            // note '<' because idx indexes at 0, whereas ID starts at 1.
                            if (idx < dataRows.Count)
                            {
                                // Get image ID
                                int selectedImageId = dataRows[idx].Id;
                                string selectedImage = selectedImageId.ToString();

                                // Because ID starts at 1, while indexing of a list start at 0, we offset by -1.
                                int imageIndex = selectedImageId - 1;

                                // get predicted and true label
                                string predictedLabel = GetPredictedLabel(dataRows, idx, out _);
                                string trueLabel = GetTrueLabel(dataRows, idx, out _);

                                // get image label to display (either predicted or true)
                                string imageLabel = GetImageLabel(predictedLabel, trueLabel);

                                // set the color of the image 
                                switch (App.DataProcessStep)
                                {
                                    case DataProcessingStep.Preprocessing:
                                        // check if label colour map has changed
                                        if (label_colour_map == null || label_colour_map != GetLabelColorMap(App.DataManager.SelectableLabels))
                                        {
                                            label_colour_map = GetLabelColorMap(App.DataManager.SelectableLabels);
                                        }
                                        SetLabelColor(imageIndex, trueLabel, SelectedImageGridIndices, predictedLabel, label_colour_map);
                                        break;
                                    case DataProcessingStep.Classification:
                                        // (blue=default; orange=selected; and green=processed)
                                        SetLabelColor(imageIndex, trueLabel, SelectedImageGridIndices);
                                        break;
                                    default:
                                        // (blue=default; orange=selected; and green=processed)
                                        SetLabelColor(imageIndex, trueLabel, SelectedImageGridIndices);
                                        break;
                                }

                                // if labelled, add to list of labelled indices
                                if (!string.IsNullOrEmpty(trueLabel))
                                {
                                    if (!LabelledIndices.Contains(imageIndex))
                                    {
                                        LabelledIndices.Add(imageIndex);
                                    }
                                }

                                // list of image grid indices
                                if (!ImageGridIndices.Contains(imageIndex))
                                {
                                    ImageGridIndices.Add(imageIndex);
                                }

                                // add imageIndex to ImageIndices list if the image hasn't been selected
                                if (!SelectedImageGridIndices.Contains(imageIndex))
                                {
                                    UnselectedImageGridIndices.Add(imageIndex);
                                }

                                if (!string.IsNullOrEmpty(selectedImage))
                                {
                                    try
                                    {
                                        // texture == image
                                        App.TextureManager.TryGetTexture(dataRows[idx].Uuid, out ImGuiTexture2D texture, out RectangleF dimensions);
                                        if (texture == null)
                                        {
                                            // to avoid all text turning the same color or background color
                                            ImGui.PopStyleColor(2);

                                            // generate default text
                                            ImGui.Button("Failed to load image!", columnSize);
                                        }
                                        else
                                        {
                                            // TODO keep aspect ratios
                                            int width = texture.Width;
                                            int height = texture.Height;
                                            float x = dimensions.X;
                                            float y = dimensions.Y;
                                            float dWidth = dimensions.Width;
                                            float dHeight = dimensions.Height;
                                            // var pos = ioPtr.MousePos;

                                            // create a child frame of the max calculated imageSize
                                            ImGui.BeginChildFrame((uint)selectedImageId, columnSize, ImGuiWindowFlags.NoDecoration);

                                            // Allow to draw things on top of each other inside the child frame
                                            ImGui.SetItemAllowOverlap();

                                            // get begin position for drawing labels on top of the image
                                            var beginPosition = ImGui.GetCursorPos();

                                            // get image size
                                            imageSize = GetImageSize(dWidth, dHeight, columnSize, ScaleImage);

                                            // set cursor position to draw image in the middle of the frame
                                            SetImagePosition(imageSize, columnSize, beginPosition);

                                            // draw image
                                            ImGui.Image(texture, imageSize, //h+w of image to render
                                                new Num.Vector2(x / width, y / height),//src X and Y coordinates within the textures
                                            new Num.Vector2((x + dWidth) / width, (y + dHeight) / height));//src rect width X height
                                            // // Set ID for the imageButton
                                            ImGui.PushID(selectedImage);

                                            // set the button to be transparent, text transparent and when hovered transparent white
                                            ImGui.PushStyleColor(ImGuiCol.Button, Color.Transparent.PackedValue);
                                            ImGui.PushStyleColor(ImGuiCol.Text, Color.Transparent.PackedValue);
                                            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, WhiteTransparent.PackedValue);
                                            ImGui.SetCursorPos(beginPosition);

                                            // draw the button
                                            if (ImGui.Button(" ", columnSize))
                                            {
                                                int indexOfImage = ImageGridIndices.IndexOf(imageIndex);
                                                if (!SelectedImageGridIndices.Remove(imageIndex))
                                                {
                                                    SelectedImageGridIndices.Add(imageIndex);
                                                }
                                                // track up to two last selected indices in case of a shift select
                                                if (Manager.HotkeyManager.Select())
                                                {
                                                    if (!LastSelectedIndices.Remove(indexOfImage))
                                                    {
                                                        LastSelectedIndices.Add(indexOfImage);
                                                    }

                                                    if (LastSelectedIndices.Count > 2)
                                                    {
                                                        // remove the first to last selection
                                                        LastSelectedIndices.RemoveAt(LastSelectedIndices.Count - 2);
                                                    }
                                                }
                                            }

                                            if (ImGui.IsItemHovered() & (Magnification | ShowFileInfo))
                                            {
                                                ImGui.BeginTooltip();
                                                // Show image information
                                                if (ShowFileInfo)
                                                {
                                                    ImGui.PushStyleColor(ImGuiCol.Text, Color.White.PackedValue);
                                                    ImGui.Text($"Image ID: {dataRows[idx].Id}");
                                                    ImGui.Text($"Image UUID: {dataRows[idx].Uuid}");
                                                    ImGui.Text($"Sample name: {dataRows[idx].Name}");
                                                    ImGui.PopStyleColor();
                                                }
                                                // Show magnified image
                                                if (Magnification)
                                                {
                                                    ImGui.Image(texture, GetImageSize(dWidth, dHeight, new Num.Vector2(
                                                    (float)(dWidth * MagnificationFactor < App.GraphicsDevice.Adapter.CurrentDisplayMode.Width * 0.5
                                                        ? dWidth * MagnificationFactor
                                                        : App.GraphicsDevice.Adapter.CurrentDisplayMode.Width * 0.5),
                                                    (float)(dHeight * MagnificationFactor < App.GraphicsDevice.Adapter.CurrentDisplayMode.Width * 0.5
                                                        ? dHeight * MagnificationFactor
                                                        : App.GraphicsDevice.Adapter.CurrentDisplayMode.Width * 0.5)), true), //h+w of image to render
                                            new Num.Vector2(x / width, y / height),//src X and Y coordinates within the textures
                                            new Num.Vector2((x + dWidth) / width, (y + dHeight) / height));
                                                }
                                                ImGui.EndTooltip();

                                            }
                                            // pop the three styles from the button 
                                            ImGui.PopStyleColor(3);

                                            // pop unique ID of the button
                                            ImGui.PopID();
                                            // Get end position                                        
                                            var endPosition = ImGui.GetCursorPos();

                                            if (ShowParameterValue & !string.IsNullOrEmpty(ParameterValueName))
                                            {
                                                // get parameter value
                                                string parameterValue = App.DataManager.GetPropertyValue(dataRows[idx], ParameterValueName);
                                                string parameterValueLabel = $"{ParameterValueName}: {parameterValue}";

                                                // set font size 
                                                ImGui.SetWindowFontScale(App.MainMenuControl.LabelFontSize);

                                                // text to overlay the image
                                                SetParameterPosition(parameterValueLabel, columnSize, beginPosition);
                                                ImGui.PushStyleColor(ImGuiCol.Text, Color.Yellow.PackedValue);
                                                ImGui.Text(parameterValueLabel);
                                                ImGui.PopStyleColor();

                                                // reset cursor position
                                                ImGui.SetCursorPos(endPosition);
                                            }

                                            // if selected, resize font to fit automatic
                                            if (App.MainMenuControl.AutoFontSize)
                                            {
                                                SetFontSize(imageLabel, columnSize);
                                            }
                                            else
                                            {
                                                // set font size 
                                                ImGui.SetWindowFontScale(App.MainMenuControl.LabelFontSize);
                                            }

                                            // text to overlay the image
                                            SetLabelPosition(imageLabel, columnSize, beginPosition);
                                            ImGui.Text(imageLabel);

                                            // to avoid all text turning the same color or background color
                                            ImGui.PopStyleColor(2);

                                            // reset cursor position
                                            ImGui.SetCursorPos(endPosition);

                                            ImGui.EndChildFrame();

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // to avoid all text turning the same color or background color
                                        ImGui.PopStyleColor(2);

                                        // Show error message
                                        ImGui.Button($"Error loading image: {ex.Message}", columnSize);

                                        // Log the error
                                        Console.WriteLine($"Error loading image: {ex.Message}");
                                    }
                                }
                            }
                            // keep generating frames on the same line until number of columns is reached
                            if (gridCounter != App.SetRowColumnControl.ColumnNumbers)
                            {
                                // keep printing on the same line and increment the counter
                                ImGui.SameLine(0f, 0f);
                                gridCounter++;
                            }
                            else
                            {
                                // reset counter
                                gridCounter = 1;
                            }

                        }
                    }
                    else
                    {
                        // instruct user to select processing step
                        ImGui.Text("Please select processing step");
                    }
                }
                ImGui.End();
            }
        }

        private string GetPredictedLabel(
            List<CSVLabelCheckerFile.DataRow> objectData,
            int id,
            out string label
            )
        {
            // create empty string
            label = "";

            switch (App.DataProcessStep)
            {
                case DataProcessingStep.Preprocessing:
                    label = objectData[id].Preprocessing.ToString();
                    return label;
                case DataProcessingStep.Classification:
                    label = objectData[id].LabelPredicted.ToString();
                    return label;
                default:
                    label = "";
                    return label;
            }
        }
        private string GetTrueLabel(
            List<CSVLabelCheckerFile.DataRow> objectData,
            int id,
            out string label
            )
        {
            switch (App.DataProcessStep)
            {
                case DataProcessingStep.Preprocessing:
                    label = objectData[id].PreprocessingTrue.ToString();
                    return label;
                case DataProcessingStep.Classification:
                    label = objectData[id].LabelTrue.ToString();
                    return label;
                default:
                    label = "";
                    return label;
            }
        }


        private static string GetImageLabel(
            string predictedLabel,
            string trueLabel
        )
        {
            if (!string.IsNullOrEmpty(trueLabel))
                return trueLabel;
            return predictedLabel;
        }

        private void SetLabelColor(
            int imageID,
            string trueLabel,
            List<int> selectedLabels,
            string predictedLabel = null,
            Dictionary<object, Color> label_colour_map = null
        )
        {
            // if image is selected by user
            if (selectedLabels.Contains(imageID))
            {
                // set color orange
                ImGui.PushStyleColor(ImGuiCol.Text, Color.DarkOrange.PackedValue);
                ImGui.PushStyleColor(ImGuiCol.FrameBg, Color.Wheat.PackedValue);
                return;
            }
            // if a true label has been set
            if (!string.IsNullOrEmpty(trueLabel))
            {
                switch (App.DataProcessStep)
                {
                    case DataProcessingStep.Preprocessing:
                        // set color green
                        ImGui.PushStyleColor(ImGuiCol.Text, label_colour_map[trueLabel].PackedValue);
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Num.Vector4(172f / 255f, 225f / 255f, 175f / 255, 1.0f));
                        return;
                    case DataProcessingStep.Classification:
                        // set color green
                        ImGui.PushStyleColor(ImGuiCol.Text, Color.LightSeaGreen.PackedValue);
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Num.Vector4(172f / 255f, 225f / 255f, 175f / 255, 1.0f));
                        return;
                }

            }
            // if a predicted label has been set
            if (!string.IsNullOrEmpty(predictedLabel) && label_colour_map != null)
            {
                // set color according to label
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, label_colour_map[predictedLabel].PackedValue);
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, Color.SlateGray.PackedValue);
                }
                return;
            }
            // default color
            ImGui.PushStyleColor(ImGuiCol.Text, Color.DarkBlue.PackedValue);
            ImGui.PushStyleColor(ImGuiCol.FrameBg, Color.SlateGray.PackedValue);
            return;

        }

        private Dictionary<object, Color> GetLabelColorMap(List<object> labels)
        {
            Dictionary<object, Color> labelColorMap = [];

            for (int i = 0; i < labels.Count; i++)
            {
                Color labelColor;
                // Use default colors
                labelColor = App.DataManager.SelectableLabels[i] switch
                {
                    "bubble" => Color.Red,
                    "duplicate" => Color.BlueViolet,
                    "large" => Color.DarkOrange,
                    "object" => Color.Yellow,
                    "small" => Color.DarkGreen,

                    _ => Color.Black, // Add a default case to handle all other values
                };
                if (labelColor == Color.Black)
                {
                    labelColor = (i % 8) switch
                    {
                        0 => Color.DarkRed,
                        1 => Color.LightBlue,
                        2 => Color.Purple,
                        3 => Color.Orange,
                        4 => Color.Pink,
                        5 => Color.LightGreen,
                        6 => Color.DarkBlue,
                        _ => Color.Black,
                    };
                }
                labelColorMap.Add(labels[i], labelColor);
            }

            return labelColorMap;
        }

        private static Num.Vector2 GetImageSize(float imageWidth, float imageHeight, Num.Vector2 frameSize, bool scaleImage)
        {
            frameSize *= .90f;
            if (imageWidth < frameSize.X & imageHeight < frameSize.Y & !scaleImage)
            {
                Num.Vector2 imageSize = new(imageWidth, imageHeight);
                return imageSize;
            }

            // to retain aspect ratios
            float _imageWidth = frameSize.Y / imageHeight * imageWidth;
            float _imageHeight = frameSize.X / imageWidth * imageHeight;

            // if image is higher than wide 
            if (imageWidth < imageHeight)
            {
                // check whether image width is not larger then frame
                if (_imageWidth > frameSize.X)
                {
                    // add correction factor to ensure the button size is being maintained
                    float factor = frameSize.X / _imageWidth;
                    Num.Vector2 imageSize = new(_imageWidth * factor, frameSize.Y * factor);
                    return imageSize;
                }
                else
                {
                    Num.Vector2 imageSize = new(_imageWidth, frameSize.Y);
                    return imageSize;
                }
            }
            else
            {
                if (_imageHeight > frameSize.Y)
                {
                    float factor = frameSize.Y / _imageHeight;
                    Num.Vector2 imageSize = new(frameSize.X * factor, _imageHeight * factor);
                    return imageSize;
                }
                else
                {
                    Num.Vector2 imageSize = new(frameSize.X, _imageHeight);
                    return imageSize;
                }
            }
        }

        private static void SetImagePosition(Num.Vector2 imageSize, Num.Vector2 frameSize, Num.Vector2 beginPosition)
        {
            // centers in x direction
            float centerFrameWidth = frameSize.X / 2;
            float centerImageWidth = imageSize.X / 2;

            // centers in y direction
            float centerFrameHeight = frameSize.Y / 2;
            float centerImageHeight = imageSize.Y / 2;

            // setting top left starting point
            ImGui.SetCursorPos(new Num.Vector2(beginPosition.X + (centerFrameWidth - centerImageWidth), beginPosition.Y + (centerFrameHeight - centerImageHeight)));
        }

        private void SetFontSize(string label, Num.Vector2 frameSize)
        {
            // get text size
            Num.Vector2 textSize = ImGui.CalcTextSize(label);


            float newFontSize = App.MainMenuControl.LabelFontSize;
            // decrease font till fits
            while (textSize.X > frameSize.X)
            {
                // decrease font size by 10%
                newFontSize -= .1f;

                // set new font size 
                ImGui.SetWindowFontScale(newFontSize);

                // update text size
                textSize = ImGui.CalcTextSize(label);
            }
        }

        private static void SetLabelPosition(string label, Num.Vector2 frameSize, Num.Vector2 startPosition)
        {
            // get text size
            Num.Vector2 textSize = ImGui.CalcTextSize(label);

            // centers in x direction
            float centerCorX = frameSize.X / 2;
            float centerLabel = textSize.X / 2;

            // write location y direction
            float locY = startPosition.Y + ((frameSize.Y * .9f) - textSize.Y);
            // location to write
            if (centerCorX - centerLabel >= 0)
            {
                // center text
                ImGui.SetCursorPos(new Num.Vector2(startPosition.X + (centerCorX - centerLabel), locY));
            }
            else
            {
                // if text is to large set to start of frame
                ImGui.SetCursorPos(new Num.Vector2(startPosition.X, locY));
            }

        }

        private static void SetParameterPosition(string value, Num.Vector2 frameSize, Num.Vector2 startPosition)
        {
            // get text size
            Num.Vector2 textSize = ImGui.CalcTextSize(value);

            // centers in x direction
            float centerCorX = frameSize.X / 2;
            float centerLabel = textSize.X / 2;

            // write location y direction
            float locY = startPosition.Y + ((frameSize.Y * .1f) - textSize.Y);
            // location to write
            if (centerCorX - centerLabel >= 0)
            {
                // center text
                ImGui.SetCursorPos(new Num.Vector2(startPosition.X + (centerCorX - centerLabel), locY));
            }
            else
            {
                // if text is to large set to start of frame
                ImGui.SetCursorPos(new Num.Vector2(startPosition.X, locY));
            }

        }

    }
}