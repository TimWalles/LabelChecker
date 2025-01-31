using System;
using Microsoft.Xna.Framework.Graphics;
using LabelChecker.ImageLoading;

namespace LabelChecker
{
    public class ImGuiTexture2D : Texture2D
    {
        IntPtr _imgGuiTexPtr;
        readonly ImGuiRenderer _renderer;

        public static ImGuiTexture2D FromImage(string path, GraphicsDevice device, ImGuiRenderer renderer)
        {
            ImGuiTexture2D tex = null;
            var loader = new SkiaImageLoader();

            try
            {
                var (pixels, width, height) = loader.LoadImage(path);
                tex = new ImGuiTexture2D(device, width, height, renderer);
                tex.SetData(pixels);
                tex.Bind();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image {path}: {ex.Message}");
            }

            return tex;
        }

        public ImGuiTexture2D(GraphicsDevice device, int width, int height, ImGuiRenderer renderer)
            : base(device, width, height)
        {
            _renderer = renderer;
        }

        public ImGuiTexture2D(GraphicsDevice device, int width, int height, ImGuiRenderer renderer, SurfaceFormat format)
            : base(device, width, height, false, format)
        {
            _renderer = renderer;
        }

        public static implicit operator IntPtr(ImGuiTexture2D tex)
        {
            return tex._imgGuiTexPtr;
        }

        public void Bind()
        {
            _imgGuiTexPtr = _renderer.BindTexture(this);
        }
    }
}
