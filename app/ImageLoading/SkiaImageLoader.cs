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
using System.Drawing;
using System.IO;
using SkiaSharp;
using BitMiracle.LibTiff.Classic;
using System.Runtime.InteropServices;

namespace LabelChecker.ImageLoading
{
    public class SkiaImageLoader : IImageLoader
    {
        public (byte[] pixels, int width, int height) LoadImage(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Image file not found: {path}");

            // Special handling for TIFF files
            if (path.EndsWith(".tif", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase))
            {
                return LoadTiffImage(path);
            }

            // Handle other image formats with SkiaSharp
            using var stream = File.OpenRead(path);
            using var bitmap = SKBitmap.Decode(stream);
            if (bitmap == null)
                throw new InvalidOperationException($"Failed to decode image: {path}");

            return ConvertToRGBABytes(bitmap);
        }

        private (byte[] pixels, int width, int height) LoadTiffImage(string path)
        {
            using var tiff = Tiff.Open(path, "r") ?? throw new InvalidOperationException($"Failed to open TIFF file: {path}");
            int width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

            // Read the image into RGBA format
            int[] raster = new int[width * height];
            if (!tiff.ReadRGBAImageOriented(width, height, raster, Orientation.TOPLEFT))
                throw new InvalidOperationException($"Failed to read TIFF image data: {path}");

            // Convert from ABGR int array to RGBA byte array
            byte[] pixels = new byte[width * height * 4];
            for (int i = 0; i < width * height; i++)
            {
                int pixel = raster[i];
                pixels[i * 4 + 0] = (byte)((pixel >> 0) & 0xFF);  // R (was B)
                pixels[i * 4 + 1] = (byte)((pixel >> 8) & 0xFF);  // G (unchanged)
                pixels[i * 4 + 2] = (byte)((pixel >> 16) & 0xFF); // B (was R)
                pixels[i * 4 + 3] = (byte)((pixel >> 24) & 0xFF); // A (unchanged)
            }

            return (pixels, width, height);
        }

        private (byte[] pixels, int width, int height) ConvertToRGBABytes(SKBitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            // Create SKImage from bitmap
            using var image = SKImage.FromBitmap(bitmap);
            if (image == null)
                throw new InvalidOperationException("Failed to create image from bitmap");

            // Get the image info
            var info = new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Rgba8888, SKAlphaType.Premul);

            // Create pixel array
            var pixels = new byte[info.Width * info.Height * 4];

            // Create a surface and draw the image to it
            using var surface = SKSurface.Create(info);
            if (surface == null)
                throw new InvalidOperationException("Failed to create surface");

            var canvas = surface.Canvas;
            canvas.Clear();
            canvas.DrawImage(image, 0, 0);
            canvas.Flush();

            // Read pixels from the surface
            var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                var success = surface.PeekPixels().ReadPixels(info, handle.AddrOfPinnedObject(), info.RowBytes);
                if (!success)
                    throw new InvalidOperationException("Failed to read pixel data");
            }
            finally
            {
                handle.Free();
            }

            return (pixels, bitmap.Width, bitmap.Height);
        }

        public RectangleF GetImageDimensions(string path)
        {
            if (path.EndsWith(".tif", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase))
            {
                using var tiff = Tiff.Open(path, "r");
                if (tiff == null)
                    throw new InvalidOperationException($"Failed to open TIFF file: {path}");

                int width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                int height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                return new RectangleF(0, 0, width, height);
            }

            using var codec = SKCodec.Create(path);
            if (codec == null)
                throw new InvalidOperationException($"Failed to get image dimensions: {path}");

            return new RectangleF(0, 0, codec.Info.Width, codec.Info.Height);
        }
    }
}