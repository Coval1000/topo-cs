using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace TopoCS
{
    class Plain
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct VertexType
        {
            public Vector3 position;
            public Vector4 color;
        };
        public enum ColorMode
        {
            GreyScale, HighContrast, HCGSHybrid, FullBit, SolidColor, Undefined, FullBitEyeCorrect
        };
        public enum DataMode
        {
            Extrapolator0, Extrapolator1, Extrapolator1Interpolated, Raw
        };
        public enum RenderStyle
        {
            Solid, Grid
        };

        public bool draw;
        public int TerrainWidth { get; private set; }
        public int TerrainHeight { get; private set; }
        private int _offSetX, _offSetY;
        private int _vertexCount;
        public int IndexCount { get; private set; }
        public int GridDensity { get; private set; }
        private DataMode PLOTTING;
        private RenderStyle RENDERING;
        private double _top, _bottom;
        private SharpDX.Direct3D11.Buffer _vertexBuffer, _indexBuffer;
        private Vector3[] HeightMap_;
        private ColorMode COLOR_MODE_;
        private VertexType[] Vertices_;
        public Color4 SolidColor
        {
            get { return _SolidColor; }
            set
            {
                _SolidColor = value;
                _UpdateColor = true;
            }
        }
        private Color4 _SolidColor;
        public bool IsValidated { 
            get 
            {
                return !(_UpdateColor || _UpdatePlot || _UpdateIndicies);
            }
        }
        private bool _UpdateColor;
        private bool _UpdatePlot;
        private bool _UpdateIndicies;
        public Plain()
        {
            COLOR_MODE_ = ColorMode.GreyScale;
            GridDensity = 1;
            PLOTTING = DataMode.Extrapolator0;
            _offSetX = 0;
            _offSetY = 0;
            RENDERING = RenderStyle.Solid;
            draw = false;
            _SolidColor = new Color4(0.0f, 0.75f, 0.75f, 1.0f);
        }
        public Plain(in Plain plain, in Device device)
        {
            draw = plain.draw;
            TerrainWidth = plain.TerrainWidth;
            TerrainHeight = plain.TerrainHeight;
            _offSetX = plain._offSetX;
            _offSetY = plain._offSetY;
            _vertexCount = plain._vertexCount;
            IndexCount = plain.IndexCount;
            GridDensity = plain.GridDensity;
            PLOTTING = plain.PLOTTING;
            RENDERING = plain.RENDERING;
            _top = plain._top;
            _bottom = plain._bottom;

            HeightMap_ = new Vector3[plain.HeightMap_.Length];
            Array.Copy(plain.HeightMap_, HeightMap_, plain.HeightMap_.Length);
            COLOR_MODE_ = plain.COLOR_MODE_;
            Vertices_ = new VertexType[plain.Vertices_.Length];
            Array.Copy(plain.Vertices_, Vertices_, plain.Vertices_.Length);
            _SolidColor = plain._SolidColor;
            _UpdateColor = plain._UpdateColor;
            _UpdateIndicies = plain._UpdateIndicies;
            _UpdatePlot = plain._UpdatePlot;
            FillVertexDesc(device);
            var indices = new uint[IndexCount];
            InitializeIndices(ref indices);
            FillIndexDesc(device, in indices);
        }
        ~Plain()
        {
            Shutdown();
        }
        #region Color Modes
        private Vector4 HighContrastBuffer(double heigh)
        {
            heigh = (heigh - _bottom) / (_top - _bottom);
            float color = (float)((heigh * 7) - (int)(heigh * 7));
            switch ((int)(heigh * 7))
            {
                case 0:
                    return new Vector4(0.0f, 0.0f, color, 1.0f);
                case 1:
                    return new Vector4(0.0f, color, 1.0f, 1.0f);
                case 2:
                    return new Vector4(0.0f, 1.0f, 1.0f - color, 1.0f);
                case 3:
                    return new Vector4(color, 1.0f, 0.0f, 1.0f);
                case 4:
                    return new Vector4(1.0f, 1.0f - color, 0.0f, 1.0f);
                case 5:
                    return new Vector4(1.0f, 0.0f, color, 1.0f);
                case 6:
                    return new Vector4(1.0f, color, 1.0f, 1.0f);
                case 7:
                    return new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                default:
                    return new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
            }
        }
        private Vector4 HybrydBuffer(double heigh)
        {
            float heighStd = (float)((heigh - _bottom) / (_top - _bottom));
            float color = (heighStd * 7) - (int)(heighStd * 7);
            switch ((int)(heighStd * 7))
            {
                case 0:
                    return new Vector4(0.0f, 0.0f, heighStd * color, 1.0f);
                case 1:
                    return new Vector4(0.0f, heighStd * color, heighStd, 1.0f);
                case 2:
                    return new Vector4(0.0f, heighStd, heighStd * (1.0f - color), 1.0f);
                case 3:
                    return new Vector4(heighStd * color, heighStd, 0.0f, 1.0f);
                case 4:
                    return new Vector4(heighStd, heighStd * (1.0f - color), 0.0f, 1.0f);
                case 5:
                    return new Vector4(heighStd, 0.0f, heighStd * color, 1.0f);
                case 6:
                    return new Vector4(heighStd, heighStd * color, heighStd, 1.0f);
                case 7:
                    return new Vector4(heighStd, heighStd, heighStd, 1.0f);
                default:
                    return new Vector4(heighStd, heighStd, heighStd, 0.0f);
            }
        }
        private Vector4 GreyScaleBuffer(double heigh)
        {
            float heighStd = (float)((heigh - _bottom) / (_top - _bottom));

            return new Vector4(heighStd, heighStd, heighStd, 1.0f);

        }
        private Vector4 FullBitBuffer(double heigh)
        {
            float heighStd = (float)((heigh - _bottom) / (_top - _bottom));
            float color = (heighStd * 7) - (int)(heighStd * 7);
            switch ((int)(heighStd * 7))
            {
                case 0:
                    return new Vector4(0.0f, 0.0f, color, 1.0f);
                case 1:
                    return new Vector4(0.0f, color, 1.0f - color, 1.0f);
                case 2:
                    return new Vector4(0.0f, 1.0f, color, 1.0f);
                case 3:
                    return new Vector4(color, 1.0f - color, 1.0f - color, 1.0f);
                case 4:
                    return new Vector4(1.0f, 0.0f, color, 1.0f);
                case 5:
                    return new Vector4(1.0f, color, 1.0f - color, 1.0f);
                case 6:
                    return new Vector4(1.0f, 1.0f, color, 1.0f);
                case 7:
                    return new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                default:
                    return new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
            }
        }
        private Vector4 FullBitEyeCorrectBuffer(double heigh)
        {
            float heighStd = (float)((heigh - _bottom) / (_top - _bottom));
            float color = (heighStd * 7) - (int)(heighStd * 7);
            switch ((int)(heighStd * 7))
            {
                case 0:
                    return new Vector4(0.0f, 0.0f, color, 1.0f);
                case 1:
                    return new Vector4(color, 0.0f, 1.0f - color, 1.0f);
                case 2:
                    return new Vector4(1.0f, 0.0f, color, 1.0f);
                case 3:
                    return new Vector4(1.0f - color, color, 1.0f - color, 1.0f);
                case 4:
                    return new Vector4(0.0f, 1.0f, color, 1.0f);
                case 5:
                    return new Vector4(color, 1.0f, 1.0f - color, 1.0f);
                case 6:
                    return new Vector4(1.0f, 1.0f, color, 1.0f);
                case 7:
                    return new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                default:
                    return new Vector4(1.0f, 1.0f, 1.0f, 0.0f);
            }
        }
        #endregion

        public int GetOffSetX()
        {
            return _offSetX;
        }
        public int GetOffSetY()
        {
            return _offSetY;
        }
        public float GetTerrainValue(int x, int y)
        {
            if (Vertices_ == null)
            {
                return 0;
            }
            return Vertices_[(TerrainHeight * x) + y].position.Y;
        }

        #region Initialize
        public bool Initialize(Device device, ref Vector3[] heightMap, int width, int height, float top, float bottom, RenderStyle REN, ColorMode colorMode)
        {
            TerrainWidth = width;
            TerrainHeight = height;
            _top = top;
            _bottom = bottom;
            HeightMap_ = new Vector3[TerrainWidth * TerrainHeight];
            Array.Copy( heightMap, HeightMap_, TerrainWidth * TerrainHeight);
            RENDERING = REN;
            if (colorMode != ColorMode.Undefined) COLOR_MODE_ = colorMode;
            return InitializeBuffers(device);
        }
        private bool InitializeBuffers(Device device)
        {
            try
            {
                if (!InitializePlot(out uint[] indices)) return false;
                UpdateColor(ref Vertices_);
                //UpdateGridIndices(*indicesGrid);

                FillVertexDesc(device);
                FillIndexDesc(device, in indices);
                indices = null;
                return true;
            }
            catch(ArgumentException e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK);
                return false;
            }
        }
        private bool InitializePlot(out uint[] indices)
        {
            int k = 0;
            switch (PLOTTING)
            {
                case DataMode.Extrapolator0:
                    k = 4;
                    break;
                case DataMode.Extrapolator1:
                    k = 1;
                    break;
                case DataMode.Extrapolator1Interpolated:
                    k = 4;
                    break;
            }
            _vertexCount = (TerrainWidth) * (TerrainHeight) * k;

            IndexCount = _vertexCount * 6;
            Vertices_ = null;

            Vertices_ = new VertexType[_vertexCount];

            indices = new uint[IndexCount];
            InitializeVertices();
            InitializeIndices(ref indices);
            return true;
        }
        private void InitializeVertices()
        {
            uint index = 0;
            switch (PLOTTING)
            {
                case DataMode.Extrapolator0:
                    for (int j = 0; j < (TerrainHeight); j++)
                    {
                        for (int i = 0; i < (TerrainWidth); i++)
                        {
                            index = (uint)((TerrainHeight * j) + i);
                            Vertices_[index * 4].position = new Vector3(HeightMap_[index].X - 0.5f, HeightMap_[index].Y, HeightMap_[index].Z - 0.5f);//UL
                            Vertices_[index * 4 + 1].position = new Vector3(HeightMap_[index].X + 0.5f, HeightMap_[index].Y, HeightMap_[index].Z - 0.5f);//UR
                            Vertices_[index * 4 + 2].position = new Vector3(HeightMap_[index].X - 0.5f, HeightMap_[index].Y, HeightMap_[index].Z + 0.5f);//BL
                            Vertices_[index * 4 + 3].position = new Vector3(HeightMap_[index].X + 0.5f, HeightMap_[index].Y, HeightMap_[index].Z + 0.5f);//BR
                        }
                    }
                    break;
                case DataMode.Extrapolator1:
                    for (int j = 0; j < (TerrainHeight); j++)
                    {
                        for (int i = 0; i < (TerrainWidth); i++)
                        {
                            index = (uint)((TerrainHeight * j) + i);
                            Vertices_[index].position = new Vector3(HeightMap_[index].X, HeightMap_[index].Y, HeightMap_[index].Z);
                        }
                    }
                    break;
                case DataMode.Extrapolator1Interpolated:
                    for (int j = 0; j < (TerrainHeight - 1); j++)
                    {
                        for (int i = 0; i < (TerrainWidth - 1); i++)
                        {
                            index = (uint)((TerrainHeight * j) + i);
                            Vertices_[index * 4].position = new Vector3(HeightMap_[index].X, HeightMap_[index].Y, HeightMap_[index].Z);//UL
                            Vertices_[index * 4 + 1].position = new Vector3(HeightMap_[index].X + 0.5f, (HeightMap_[index].Y + HeightMap_[index + 1].Y) / 2.0f, HeightMap_[index].Z);//UR
                            Vertices_[index * 4 + 2].position = new Vector3(HeightMap_[index].X, (HeightMap_[index].Y + HeightMap_[index + (uint)TerrainHeight].Y) / 2.0f, HeightMap_[index].Z + 0.5f);//BL
                            Vertices_[index * 4 + 3].position = new Vector3(HeightMap_[index].X + 0.5f, (HeightMap_[index].Y + HeightMap_[index + (uint)TerrainHeight].Y + HeightMap_[index + 1].Y + HeightMap_[index + (uint)TerrainHeight + 1].Y) / 4.0f, HeightMap_[index].Z + 0.5f);//BR
                        }
                        index = (uint)((TerrainHeight * j) + TerrainWidth - 1);
                        Vertices_[index * 4].position = new Vector3(HeightMap_[index].X, HeightMap_[index].Y, HeightMap_[index].Z);//UL
                        Vertices_[index * 4 + 1].position = new Vector3(HeightMap_[index].X, HeightMap_[index].Y, HeightMap_[index].Z);//UL
                        Vertices_[index * 4 + 2].position = new Vector3(HeightMap_[index].X, HeightMap_[index].Y, HeightMap_[index].Z);//UL
                        Vertices_[index * 4 + 3].position = new Vector3(HeightMap_[index].X, HeightMap_[index].Y, HeightMap_[index].Z);//UL
                    }
                    for (int i = 0; i < (TerrainWidth - 1); i++)
                    {
                        index = (uint)((TerrainHeight * (TerrainWidth - 1)) + i);
                        Vertices_[index * 4].position = new Vector3(HeightMap_[index].X, HeightMap_[index].Y, HeightMap_[index].Z);//UL
                        Vertices_[index * 4 + 1].position = new Vector3(HeightMap_[index].X + 0.5f, (HeightMap_[index].Y + HeightMap_[index + 1].Y) / 2.0f, HeightMap_[index].Z);//UR
                        Vertices_[index * 4 + 2].position = new Vector3(HeightMap_[index].X, HeightMap_[index].Y, HeightMap_[index].Z);//BL
                        Vertices_[index * 4 + 3].position = new Vector3(HeightMap_[index].X, HeightMap_[index].Y, HeightMap_[index].Z);//BR
                    }
                    ++index;
                    Vertices_[index * 4].position = new Vector3(HeightMap_[index].X, HeightMap_[index].Y, HeightMap_[index].Z);//UL
                    Vertices_[index * 4 + 1].position = new Vector3(HeightMap_[index].X, HeightMap_[index].Y, HeightMap_[index].Z);//UL
                    Vertices_[index * 4 + 2].position = new Vector3(HeightMap_[index].X, HeightMap_[index].Y, HeightMap_[index].Z);//UL
                    Vertices_[index * 4 + 3].position = new Vector3(HeightMap_[index].X, HeightMap_[index].Y, HeightMap_[index].Z);//UL
                    break;
                case DataMode.Raw:
                    break;
            }
        }
        private void InitializeIndices(ref uint[] indices)
        {
            switch (RENDERING)
            {
                case RenderStyle.Solid:

                    switch (PLOTTING)
                    {
                        case DataMode.Extrapolator0:
                            IndexRaw(ref indices);
                            break;
                        case DataMode.Extrapolator1:
                            IndexInterpolated0(ref indices);
                            break;
                        case DataMode.Extrapolator1Interpolated:
                            IndexInterpolated1(ref indices);
                            break;
                        case DataMode.Raw:
                            break;
                    }
                    break;

                case RenderStyle.Grid:

                    switch (PLOTTING)
                    {
                        case DataMode.Extrapolator0:
                            IndexGridRaw(ref indices);
                            break;
                        case DataMode.Extrapolator1:
                            IndexGridInterpolated0(ref indices);
                            break;
                        case DataMode.Extrapolator1Interpolated:
                            IndexGridInterpolated1(ref indices);
                            break;
                    }
                    break;
            }
        }
        #endregion
        #region IndexCreating
        private void IndexRaw(ref uint[] indices)
        {
            uint index = uint.MaxValue;
            uint[] node = new uint[4];
            for (int y = 0; y < (TerrainHeight - 1); ++y)
            {
                for (int x = 0; x < (TerrainWidth - 1); ++x)
                {
                    node[0] = (uint)(((TerrainHeight * y) + x) * 4);          // Upper left.
                    node[1] = (uint)(((TerrainHeight * y) + (x + 1)) * 4);      // Upper right.
                    node[2] = (uint)(((TerrainHeight * (y + 1)) + x) * 4);      // Bottom left.

                    indices[++index] = node[0] + 1;
                    indices[++index] = node[0] + 0;
                    indices[++index] = node[0] + 2;
                    indices[++index] = node[0] + 2;
                    indices[++index] = node[0] + 3;
                    indices[++index] = node[0] + 1;

                    indices[++index] = node[1] + 2;
                    indices[++index] = node[1] + 0;
                    indices[++index] = node[0] + 1;
                    indices[++index] = node[0] + 1;
                    indices[++index] = node[0] + 3;
                    indices[++index] = node[1] + 2;

                    indices[++index] = node[0] + 2;
                    indices[++index] = node[2] + 0;
                    indices[++index] = node[2] + 1;
                    indices[++index] = node[2] + 1;
                    indices[++index] = node[0] + 3;
                    indices[++index] = node[0] + 2;

                }
                node[0] = (uint)(((TerrainHeight * y) + TerrainWidth - 1) * 4);          // Upper left.
                node[2] = (uint)(((TerrainHeight * (y + 1)) + TerrainWidth - 1) * 4);
                indices[++index] = node[0] + 1;
                indices[++index] = node[0] + 0;
                indices[++index] = node[0] + 2;
                indices[++index] = node[0] + 2;
                indices[++index] = node[0] + 3;
                indices[++index] = node[0] + 1;

                indices[++index] = node[0] + 2;
                indices[++index] = node[2] + 0;
                indices[++index] = node[2] + 1;
                indices[++index] = node[2] + 1;
                indices[++index] = node[0] + 3;
                indices[++index] = node[0] + 2;

            }

            for (int x = 0; x < (TerrainWidth - 1); ++x)
            {
                node[0] = (uint)((TerrainHeight * (TerrainWidth - 1) + x) * 4);          // Upper left. x,y
                node[1] = (uint)(((TerrainHeight * (TerrainWidth - 1)) + (x + 1)) * 4);      // Upper right. x+1,y

                indices[++index] = node[0] + 1;
                indices[++index] = node[0] + 0;
                indices[++index] = node[0] + 2;
                indices[++index] = node[0] + 2;
                indices[++index] = node[0] + 3;
                indices[++index] = node[0] + 1;

                indices[++index] = node[1] + 2;
                indices[++index] = node[1] + 0;
                indices[++index] = node[0] + 1;
                indices[++index] = node[0] + 1;
                indices[++index] = node[0] + 3;
                indices[++index] = node[1] + 2;

            }
            node[0] = (uint)((TerrainHeight * TerrainWidth) * 4);          // Upper left. x,y

            indices[++index] = node[0] + 1;
            indices[++index] = node[0] + 0;
            indices[++index] = node[0] + 2;
            indices[++index] = node[0] + 2;
            indices[++index] = node[0] + 3;
            indices[++index] = node[0] + 1;
            return;
        }
        private void IndexGridRaw(ref uint[] indices)
        {
            uint index = uint.MaxValue;
            uint[] node = new uint[4];
            for (int y = 0; y < (TerrainHeight - 1); ++y)
            {
                for (int x = 0; x < (TerrainWidth - 1); ++x)
                {
                    node[0] = (uint)(((TerrainHeight * y) + x) * 4);          // Upper left.
                    node[1] = (uint)(((TerrainHeight * y) + (x + 1)) * 4);      // Upper right.
                    node[2] = (uint)(((TerrainHeight * (y + 1)) + x) * 4);      // Bottom left.

                    indices[++index] = node[0] + 1;
                    indices[++index] = node[0] + 0;
                    indices[++index] = node[0] + 2;
                    indices[++index] = node[0] + 2;
                    indices[++index] = node[0] + 3;
                    indices[++index] = node[0] + 1;

                    indices[++index] = node[1] + 2;
                    indices[++index] = node[1] + 0;
                    indices[++index] = node[0] + 1;
                    indices[++index] = node[0] + 1;
                    indices[++index] = node[0] + 3;
                    indices[++index] = node[1] + 2;

                    indices[++index] = node[0] + 2;
                    indices[++index] = node[2] + 0;
                    indices[++index] = node[2] + 1;
                    indices[++index] = node[2] + 1;
                    indices[++index] = node[0] + 3;
                    indices[++index] = node[0] + 2;

                }
                node[0] = (uint)(((TerrainHeight * y) + TerrainWidth - 1) * 4);          // Upper left.
                node[2] = (uint)(((TerrainHeight * (y + 1)) + TerrainWidth - 1) * 4);
                indices[++index] = node[0] + 1;
                indices[++index] = node[0] + 0;
                indices[++index] = node[0] + 2;
                indices[++index] = node[0] + 2;
                indices[++index] = node[0] + 3;
                indices[++index] = node[0] + 1;

                indices[++index] = node[0] + 2;
                indices[++index] = node[2] + 0;
                indices[++index] = node[2] + 1;
                indices[++index] = node[2] + 1;
                indices[++index] = node[0] + 3;
                indices[++index] = node[0] + 2;

            }

            for (int x = 0; x < (TerrainWidth - 1); ++x)
            {
                node[0] = (uint)((TerrainHeight * (TerrainWidth - 1) + x) * 4);          // Upper left. x,y
                node[1] = (uint)(((TerrainHeight * (TerrainWidth - 1)) + (x + 1)) * 4);      // Upper right. x+1,y

                indices[++index] = node[0] + 1;
                indices[++index] = node[0] + 0;
                indices[++index] = node[0] + 2;
                indices[++index] = node[0] + 2;
                indices[++index] = node[0] + 3;
                indices[++index] = node[0] + 1;

                indices[++index] = node[1] + 2;
                indices[++index] = node[1] + 0;
                indices[++index] = node[0] + 1;
                indices[++index] = node[0] + 1;
                indices[++index] = node[0] + 3;
                indices[++index] = node[1] + 2;

            }
            node[0] = (uint)((TerrainHeight * TerrainWidth) * 4);          // Upper left. x,y

            indices[++index] = node[0] + 1;
            indices[++index] = node[0] + 0;
            indices[++index] = node[0] + 2;
            indices[++index] = node[0] + 2;
            indices[++index] = node[0] + 3;
            indices[++index] = node[0] + 1;
            return;
        }
        private void IndexInterpolated0(ref uint[] indices)
        {
            uint index = uint.MaxValue;
            uint[] node = new uint[4];
            for (int j = 0; j < (TerrainHeight - 1); j++)
            {
                for (int i = 0; i < (TerrainWidth - 1); i++)
                {
                    node[0] = (uint)((TerrainHeight * j) + i);          // Upper left. a
                    node[1] = (uint)((TerrainHeight * j) + (i + 1));      // Upper right. a+1
                    node[2] = (uint)((TerrainHeight * (j + 1)) + i);      // Bottom left. x+a
                    node[3] = (uint)((TerrainHeight * (j + 1)) + (i + 1));  // Bottom right.  x+a+1

                    indices[++index] = node[2];
                    // Upper right. x+a+1
                    indices[++index] = node[3];
                    // Bottom left. a
                    indices[++index] = node[0];
                    // Bottom left. a
                    indices[++index] = node[0];
                    // Upper right. x+a+1
                    indices[++index] = node[3];
                    // Bottom right. a+1
                    indices[++index] = node[1];

                }
            }
            return;
        }
        private void IndexGridInterpolated0(ref uint[] indices)
        {
            uint index = uint.MaxValue;
            uint[] node = new uint[4];
            for (int j = 0; j < (TerrainHeight); j = j + GridDensity)
            {
                for (int i = 0; i < (TerrainWidth - 1); ++i)
                {
                    node[0] = (uint)((TerrainHeight * j) + i);
                    node[1] = (uint)((TerrainHeight * j) + i + 1);

                    indices[++index] = node[0];
                    indices[++index] = node[1];
                }
            }

            for (int j = 0; j < (TerrainWidth); j = j + GridDensity)
            {
                for (int i = 0; i < (TerrainHeight - 1); ++i)
                {
                    node[0] = (uint)((TerrainHeight * i) + j);
                    node[1] = (uint)((TerrainHeight * (i + 1)) + j);

                    indices[++index] = node[0];
                    indices[++index] = node[1];
                }
            }
        }
        private void IndexInterpolated1(ref uint[] indices)
        {
            uint index = uint.MaxValue;
            uint[] node = new uint[4];
            for (int y = 0; y < (TerrainHeight - 1); ++y)
            {
                for (int x = 0; x < (TerrainWidth - 1); ++x)
                {
                    node[0] = (uint)(((TerrainHeight * y) + x) * 4);          // Upper left.
                    node[1] = (uint)(((TerrainHeight * y) + (x + 1)) * 4);      // Upper right.
                    node[2] = (uint)(((TerrainHeight * (y + 1)) + x) * 4);      // Bottom left.
                    node[3] = (uint)(((TerrainHeight * (y + 1)) + x + 1) * 4);      // Bottom left.

                    indices[++index] = node[0];
                    indices[++index] = node[0] + 2;
                    indices[++index] = node[0] + 1;
                    indices[++index] = node[0] + 1;
                    indices[++index] = node[0] + 2;
                    indices[++index] = node[0] + 3;

                    indices[++index] = node[1] + 2;
                    indices[++index] = node[1];
                    indices[++index] = node[0] + 1;
                    indices[++index] = node[0] + 1;
                    indices[++index] = node[0] + 3;
                    indices[++index] = node[1] + 2;

                    indices[++index] = node[0] + 2;
                    indices[++index] = node[2];
                    indices[++index] = node[2] + 1;
                    indices[++index] = node[2] + 1;
                    indices[++index] = node[0] + 3;
                    indices[++index] = node[0] + 2;

                    indices[++index] = node[0] + 3;
                    indices[++index] = node[2] + 1;
                    indices[++index] = node[3];
                    indices[++index] = node[3];
                    indices[++index] = node[1] + 2;
                    indices[++index] = node[0] + 3;

                }
                node[0] = (uint)((TerrainHeight * y) + TerrainWidth - 1) * 4;          // Upper left.
                node[2] = (uint)((TerrainHeight * (y + 1)) + TerrainWidth - 1) * 4;
                indices[++index] = node[0];
                indices[++index] = node[0] + 2;
                indices[++index] = node[0] + 1;
                indices[++index] = node[0] + 1;
                indices[++index] = node[0] + 2;
                indices[++index] = node[0] + 3;

                indices[++index] = node[0] + 2;
                indices[++index] = node[2];
                indices[++index] = node[2] + 1;
                indices[++index] = node[2] + 1;
                indices[++index] = node[0] + 3;
                indices[++index] = node[0] + 2;

            }

            for (int x = 0; x < (TerrainWidth - 1); ++x)
            {
                node[0] = (uint)(TerrainHeight * (TerrainWidth - 1) + x) * 4;          // Upper left. x,y
                node[1] = (uint)((TerrainHeight * (TerrainWidth - 1)) + (x + 1)) * 4;      // Upper right. x+1,y

                indices[++index] = node[0];
                indices[++index] = node[0] + 2;
                indices[++index] = node[0] + 1;
                indices[++index] = node[0] + 1;
                indices[++index] = node[0] + 2;
                indices[++index] = node[0] + 3;

                indices[++index] = node[1] + 2;
                indices[++index] = node[1];
                indices[++index] = node[0] + 1;
                indices[++index] = node[0] + 3;
                indices[++index] = node[1] + 2;
                indices[++index] = node[0] + 1;

            }
            node[0] = (uint)(TerrainHeight * TerrainWidth) * 4;          // Upper left. x,y

            indices[++index] = node[0];
            indices[++index] = node[0] + 2;
            indices[++index] = node[0] + 1;
            indices[++index] = node[0] + 1;
            indices[++index] = node[0] + 2;
            indices[++index] = node[0] + 3;
            return;
        }
        private void IndexGridInterpolated1(ref uint[] indices)

        {
            uint index = uint.MaxValue;
            uint[] node = new uint[4];
            for (int y = 0; y < (TerrainHeight - 1); ++y)
            {
                for (int x = 0; x < (TerrainWidth - 1); ++x)
                {
                    node[0] = (uint)(((TerrainHeight * y) + x) * 4);          // Upper left.
                    node[1] = (uint)(((TerrainHeight * y) + (x + 1)) * 4);      // Upper right.
                    node[2] = (uint)(((TerrainHeight * (y + 1)) + x) * 4);      // Bottom left.
                    node[3] = (uint)(((TerrainHeight * (y + 1)) + x + 1) * 4);      // Bottom left.

                    indices[++index] = node[0];
                    indices[++index] = node[0] + 2;
                    indices[++index] = node[0] + 2;
                    indices[++index] = node[2];

                    indices[++index] = node[0];
                    indices[++index] = node[0] + 1;                    
                    indices[++index] = node[0] + 1;
                    indices[++index] = node[1];

                }
                node[0] = (uint)((TerrainHeight * y) + TerrainWidth - 1) * 4;          // Upper left.
                node[2] = (uint)((TerrainHeight * (y + 1)) + TerrainWidth - 1) * 4;
                indices[++index] = node[0];
                indices[++index] = node[0] + 2;
                indices[++index] = node[0] + 2;
                indices[++index] = node[2];

                indices[++index] = node[0];
                indices[++index] = node[0] + 1;
            }

            for (int x = 0; x < (TerrainWidth - 1); ++x)
            {
                node[0] = (uint)(TerrainHeight * (TerrainWidth - 1) + x) * 4;          // Upper left. x,y
                node[1] = (uint)((TerrainHeight * (TerrainWidth - 1)) + (x + 1)) * 4;      // Upper right. x+1,y

                indices[++index] = node[0];
                indices[++index] = node[0] + 2;

                indices[++index] = node[0];
                indices[++index] = node[0] + 1;
                indices[++index] = node[0] + 1;
                indices[++index] = node[1];

            }
            node[0] = (uint)(TerrainHeight * TerrainWidth) * 4;          // Upper left. x,y

            indices[++index] = node[0];
            indices[++index] = node[0] + 2;

            indices[++index] = node[0];
            indices[++index] = node[0] + 1;
            return;
        }
        #endregion
        private void FillVertexDesc(Device device)
        {
            var vertexBufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<VertexType>() * _vertexCount,
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };

            _vertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, Vertices_, vertexBufferDesc);
            return;
        }
        private void FillIndexDesc(Device device, in uint[] indices)
        {
            var indexBufferDesc = new BufferDescription()
            {
                Usage = RENDERING == RenderStyle.Solid ? ResourceUsage.Default : ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<uint>() * IndexCount,
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = RENDERING == RenderStyle.Solid ? CpuAccessFlags.None : CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };

            _indexBuffer = SharpDX.Direct3D11.Buffer.Create(device, indices, indexBufferDesc);
            return;
        }
        public bool SetMode(Device device, DataMode mode)
        {
            PLOTTING = mode;
            return UpdateDevice(device);
        }
        public void ShutdownBuffers()
        {
            _indexBuffer?.Dispose();
            _indexBuffer = null;

            _vertexBuffer?.Dispose();
            _vertexBuffer = null;
            return;
        }

        public bool Render(DeviceContext deviceContext, ColorShader colorShader, Matrix worldMatrixPlaine, Matrix viewMatrix, Matrix projectionMatrix)
        {
            Update(deviceContext); //todo: result handling
            if (!draw) return true;
            int stride = Utilities.SizeOf<VertexType>();
            int offset = 0;
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, stride, offset));
            deviceContext.InputAssembler.SetIndexBuffer(_indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);
            deviceContext.InputAssembler.PrimitiveTopology = 
                RENDERING == RenderStyle.Solid ? 
                SharpDX.Direct3D.PrimitiveTopology.TriangleList : 
                SharpDX.Direct3D.PrimitiveTopology.LineList;

            return colorShader.Render(deviceContext, IndexCount, worldMatrixPlaine, viewMatrix, projectionMatrix);
        }
        private bool Update(DeviceContext deviceContext)
        {
            if (_UpdateColor || _UpdatePlot)
            {

                if (Vertices_ == null)
                {
                    return false;
                }

                if (_UpdatePlot) UpdateTerrain(ref Vertices_);
                if (_UpdateColor) UpdateColor(ref Vertices_);
                try
                {
                    deviceContext.MapSubresource(_vertexBuffer, MapMode.WriteDiscard, MapFlags.None, out DataStream mappedResource);
                    mappedResource.WriteRange(Vertices_);
                    deviceContext.UnmapSubresource(_vertexBuffer, 0);
                }
                catch
                {
                    return false;
                }
            }

            if (_UpdateIndicies)
            {
                //unsigned long* indices;
                //indices = new unsigned long[_indexGridCount];
                //if (!indices)
                //{
                //	return false;
                //}

                ////UpdateGridIndices(indices);

                //result = D3Dx_->GetDeviceContext()->Map(_indexGridBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
                //if (FAILED(result))
                //{
                //	delete[] indices;
                //	indices = 0;
                //	return false;
                //}

                //// Copy the data into the vertex buffer.
                //memcpy(mappedResource.pData, (void*)indices, (sizeof(unsigned long) * _indexGridCount));

                //// Unlock the vertex buffer.
                //D3Dx_->GetDeviceContext()->Unmap(_indexGridBuffer, 0);
                //delete[] indices;
                //indices = 0;
            }
            return true;
        }
        private bool UpdateTerrain(ref VertexType[] vertices)
        {
            int index, i, j;

            for (j = 0; j < (TerrainHeight); j++)
            {
                for (i = 0; i < (TerrainWidth); i++)
                {
                    index = (TerrainHeight * j) + i;

                    vertices[index].position.Y = HeightMap_[index].Y;
                }
            }
            _UpdatePlot = false;
            return true;
        }
        private bool UpdateColor(ref VertexType[] vertices)
        {
            int index, i, j;
            int k;
            switch (PLOTTING)
            {
                case DataMode.Extrapolator0:
                    k = 2;
                    break;
                case DataMode.Extrapolator1:
                    k = 1;
                    break;
                case DataMode.Extrapolator1Interpolated:
                    k = 2;
                    break;
                default:
                    k = 0;
                    break;
            }
            for (j = 0; j < (TerrainHeight * k); j++)
            {
                for (i = 0; i < (TerrainWidth * k); i++)
                {
                    index = (TerrainHeight * j * k) + i;

                    switch (COLOR_MODE_)
                    {
                        case ColorMode.HighContrast:
                            vertices[index].color = HighContrastBuffer(Vertices_[index].position.Y);
                            break;

                        case ColorMode.HCGSHybrid:
                            vertices[index].color = HybrydBuffer(Vertices_[index].position.Y);
                            break;
                        case ColorMode.SolidColor:
                            vertices[index].color = _SolidColor;
                            break;

                        case ColorMode.FullBit:
                            vertices[index].color = FullBitBuffer(Vertices_[index].position.Y);
                            break;
                        case ColorMode.FullBitEyeCorrect:
                            vertices[index].color = FullBitEyeCorrectBuffer(Vertices_[index].position.Y);
                            break;
                        default:
                            vertices[index].color = GreyScaleBuffer(Vertices_[index].position.Y);
                            break;
                    }

                }
            }
            /*if (ClickPoint_.x >= 0)
            {
                vertices[(ClickPoint_.x * terrainHeight_) + ClickPoint_.y].color = D3DXVECTOR4(0.9f, 0.0f, 0.1f, 1.0f);
            }*/
            _UpdateColor = false;
            return true;
        }
        public void ChangeColorMode(ColorMode colormode)
        {
            if (colormode != ColorMode.Undefined) COLOR_MODE_ = colormode;
            _UpdateColor = true;
            return;
        }

        public void ChangeGridSize(int gridSize)
        {
            if (GridDensity + gridSize > 0)
                GridDensity += gridSize;
            _UpdateIndicies = true;
            return;
        }

        public bool UpdateDevice(Device device)
        {
            ShutdownBuffers();
            return InitializeBuffers(device);
        }

        public void Shutdown()
        {
            ShutdownBuffers();
            Vertices_ = null;
            HeightMap_ = null;
            return;
        }
    }
}
