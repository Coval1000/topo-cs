using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
namespace TopoCS
{
    class Terrain
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public float Top { get; private set; }
        public float Bottom { get; private set; }
        private Vector3[] HeightMap_;
        private Point ClickPoint_;

        public Plain Plot;
        public Plain Grid;

        public float Sensivity;
        public bool RenderPlot
        {
            get { return Plot.draw; }
            set { Plot.draw = value; }
        }
        public bool RenderGrid
        {
            get { return Grid.draw; }
            set { Grid.draw = value; }
        }

        public Terrain()
        {
            ClickPoint_ = new Point(-1, -1);
            Sensivity = 1.0f;
        }      
        public Terrain(in Terrain terrain, in Device device)
        {
            Width = terrain.Width;
            Height = terrain.Height;
            Top = terrain.Top;
            Bottom = terrain.Bottom;
            HeightMap_ = new Vector3[terrain.HeightMap_.Length];
            Array.Copy(terrain.HeightMap_, HeightMap_, terrain.HeightMap_.Length);
            ClickPoint_ = terrain.ClickPoint_;
            Sensivity = terrain.Sensivity;
            Plot = new Plain(terrain.Plot, device);
            Grid = new Plain(terrain.Grid, device);
        }
        ~Terrain()
        {
            Dispose();
        }
        public float GetValue(int X, int Y)
        {
            return HeightMap_[Width * Y + X].Y;
        }
        public bool Initialize(Device device, string[] header, string[] data, string heightMapFilename, Plain.ColorMode colorMode = Plain.ColorMode.GreyScale)
        {
            bool result = true;
            Grid = new Plain();
            Plot = new Plain();
            if (!LoadHeightMap(header, data, heightMapFilename)) return false;
            result &= Plot.Initialize(device, ref HeightMap_, Width, Height, Top, Bottom, Plain.RenderStyle.Solid, colorMode);
            result &= Grid.Initialize(device, ref HeightMap_, Width, Height, Top, Bottom, Plain.RenderStyle.Grid, Plain.ColorMode.SolidColor);


            return result;
        }

        public bool Initialize(Device device, int width, int height, Vector3[] heightMap, Plain.ColorMode colorMode = Plain.ColorMode.GreyScale)
        {
            bool result = true;
            Width = width;
            Height = height;

            HeightMap_ = new Vector3[Width * Height];
            HeightMap_ = heightMap;
            result &= Grid.Initialize(device, ref HeightMap_, Width, Height, Top, Bottom, Plain.RenderStyle.Grid, Plain.ColorMode.SolidColor);
            result &= Plot.Initialize(device, ref HeightMap_, Width, Height, Top, Bottom, Plain.RenderStyle.Solid, colorMode);

            if (!result) return false;
            return true;
        }

        private bool LoadHeightMap(string[] header, string[] data, string filename)
        {

            int i, j, index;
            byte[] fileData = File.ReadAllBytes(filename);
            header = new string[25];
            data = new string[25];
            i = 0;
            Width = 0;
            Height = 0;



            ulong chNumber = 0;
            char input = (char)fileData[chNumber];
            while (input != '[')
            {
                input = (char)fileData[++chNumber];
            }
            while (true)
            {
                while (input != ']')
                {
                    input = (char)fileData[++chNumber];
                    if (input == '\n') continue;
                    header[i] += input;
                }
                header[i].Remove(header[i].Length - 1, 1);
                if (header[i].Contains("Liczba Kolumn") == true)
                {
                    //fin >> terrainWidth_;
                    data[i] = Width.ToString();
                }
                if (header[i].Contains("Liczba Linii") == true)
                {
                    //fin >> terrainHeight_;
                    data[i] = Height.ToString();
                }
                if (header[i].Contains("Dane") == true)
                {
                    header[i] = "";
                    break;
                }

                while (input != '[')
                {
                    input = (char)fileData[++chNumber];
                    if (input == '\n') continue;
                    data[i] += input;
                }
                data[i].Remove(data[i].Length - 1, 1); ;
                ++i;
            }
            Height = 256;
            Width = 256;
            if (Width == 0) Width = Height;
            HeightMap_ = new Vector3[Width * Height];

            Top = 0;
            Bottom = 0;
            for (j = 0; j < Height; ++j)
            {
                for (i = 0; i < Width; ++i)
                {

                    index = (Height * j) + i;

                    HeightMap_[index].X = (float)i;
                    //fin >> HeightMap_[index].Y;
                    HeightMap_[index].Y = (float)(Math.Sin(i * 1.5) * Math.Cos(j * 1.5));
                    HeightMap_[index].Z = (float)j;
                    //input = (char)fileData[++chNumber];
                    if (HeightMap_[index].Y > Top) Top = HeightMap_[index].Y;
                    else if (HeightMap_[index].Y < Bottom) Bottom = HeightMap_[index].Y;

                }
            }
            return true;
        }

        public bool Render(DeviceContext deviceContext, ColorShader colorShader, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            var transformMatrix = Matrix.Scaling(1.0f, Sensivity, 1.0f);
            worldMatrix = Matrix.Multiply(worldMatrix, transformMatrix);
            var result = Plot.Render(deviceContext, colorShader, worldMatrix, viewMatrix, projectionMatrix);

            transformMatrix = Matrix.Translation(0.0f, 0.0078125f, 0.0f);
            worldMatrix = Matrix.Multiply(worldMatrix, transformMatrix);
            result &= Grid.Render(deviceContext, colorShader, worldMatrix, viewMatrix, projectionMatrix);
            return result;
        }

        public bool SetMode(Device device, Plain.DataMode mode)
        {
            var result = Plot.SetMode(device, mode);
            result &= Grid.SetMode(device, mode);
            return result;
        }
        public void Dispose()
        {
            Plot?.Shutdown();
            Plot = null;
            Grid?.Shutdown();
            Grid = null;

            HeightMap_ = null;
            return;
        }

        public bool HighlightNode(Point node)
        {
            if (node == ClickPoint_) return true;

            if (node.Y < Width &&
                node.X < Width &&
                node.X >= 0 &&
                node.Y >= 0)
            {
                ClickPoint_ = node;
            }
            else
            {
                ClickPoint_ = new Point(-1, -1);
            }
            //result = Update(true, false, false);

            return true;
        }

        public bool ChangeDevice(Device device)
        {
            var result =  Plot.UpdateDevice(device);
            return result & Grid.UpdateDevice(device);
        }
    }
}
