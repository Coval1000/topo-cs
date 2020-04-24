using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Globalization;
namespace TopoCS
{
    class Font
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct FontType
        {
            public float left, right;
            public int size;
            public FontType(string fontData)
            {
                var data = fontData.Split(new[] { '\t' });
                left = float.Parse(data[data.Length - 3], CultureInfo.CreateSpecificCulture("en-US"));
                right = float.Parse(data[data.Length - 2], CultureInfo.CreateSpecificCulture("en-US"));
                size = int.Parse(data[data.Length - 1]);
            }
        };

        public struct VertexType
        {
            public Vector3 position;
            public Vector2 texture;
        };

        public List<FontType> Font_;
        private Texture _Texture;

        public Font()
        {
            _Texture = null;
        }
        public bool Initialize(Device device, string fontFilename, string textureFilename)
        {
            if (!LoadFontData(fontFilename)) return false;
            if (!LoadTexture(device, textureFilename)) return false;
            return true;
        }
        public void Shutdown()
        {
            ReleaseTexture();
            ReleaseFontData();

            return;
        }
        private bool LoadFontData(string filename)
        {
            try
            {
                var fontDataLines = File.ReadAllLines(filename);
                Font_ = new List<FontType>();
                foreach (var line in fontDataLines)
                    Font_.Add(new FontType(line));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void ReleaseFontData()
        {
            Font_?.Clear();
            Font_ = null;
            return;
        }
        private bool LoadTexture(Device device, string filename)
        {
            _Texture = new Texture();
            if (!_Texture.Initialize(device, filename))
            {
                return false;
            }
            return true;
        }
        private void ReleaseTexture()
        {
            _Texture?.Shutdown();
            _Texture = null;
            return;
        }
        public ShaderResourceView GetTexture()
        {
            return _Texture.GetTexture();
        }

        public void BuildVertexArray(out List<VertexType> vertices, string sentence)
        {

            vertices = new List<VertexType>();
            float drawY = 0;
            float drawX = 0;

            foreach (char ch in sentence)
            {
                var letter = ch - 32;
                letter %= 126;
                if (letter == 0)
                    drawX += 3;
                else
                {
                    BuildVertexArray(vertices, letter, ref drawX, ref drawY);

                    drawX += Font_[letter].size + 1;
                }
            }

            return;
        }
        private void BuildVertexArray(List<VertexType> vertices, int letter, ref float drawX, ref float drawY)
        {
            vertices.Add // Top left.
            (
                new VertexType()
                {
                    position = new Vector3(drawX, drawY, 0),
                    texture = new Vector2(Font_[letter].left, 0)
                }
            );
            vertices.Add // Bottom right.
            (
                new VertexType()
                {
                    position = new Vector3(drawX + Font_[letter].size, drawY - 16, 0),
                    texture = new Vector2(Font_[letter].right, 1)
                }
            );
            vertices.Add // Bottom left.
            (
                new VertexType()
                {
                    position = new Vector3(drawX, drawY - 16, 0),
                    texture = new Vector2(Font_[letter].left, 1)
                }
            );
            // Second triangle in quad.
            vertices.Add // Top left.
            (
                new VertexType()
                {
                    position = new Vector3(drawX, drawY, 0),
                    texture = new Vector2(Font_[letter].left, 0)
                }
            );
            vertices.Add // Top right.
            (
                new VertexType()
                {
                    position = new Vector3(drawX + Font_[letter].size, drawY, 0),
                    texture = new Vector2(Font_[letter].right, 0)
                }
            );
            vertices.Add // Bottom right.
            (
                new VertexType()
                {
                    position = new Vector3(drawX + Font_[letter].size, drawY - 16, 0),
                    texture = new Vector2(Font_[letter].right, 1)
                }
            );
        }
    }
}
