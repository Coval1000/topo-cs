using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace TopoCS // todo: rework
{
    class Text
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SentenceType
        {
            public SharpDX.Direct3D11.Buffer vertexBuffer, indexBuffer;
            public int vertexCount, indexCount, maxLength;
            public float red, green, blue;
        };
        [StructLayout(LayoutKind.Sequential)]
        public struct VertexType
        {
            public Vector3 position;
            public Vector2 texture;
        };
        public float Width { get; private set; }
        public float Height { get; private set; }
        public Vector4 color;
        public Vector3 position;
        public float scale;
        
        private Font Font_;
        private FontShader FontShader_;

        private string text;
        private SentenceType sentence_;
        private const int _default_length = 32;

        public Text()
        {
            scale = 1.0f;
            color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            position = new Vector3(0.0f, 0.0f, 0.0f);
        }

        public Text(Text text, SharpDX.Direct3D11.Device device)
        {
            color = text.color;
            position = text.position;
            scale = text.scale;
            Font_ = new Font();
            Font_.Initialize(device, @"data\fontdata.txt", @"data\font.png");
            FontShader_ = new FontShader();
            FontShader_.Initialize(device);
            InitializeSentence(device, text.sentence_.maxLength);
            UpdateSentence(device.ImmediateContext, text.text);
        }

        public bool Initialize(D3DX d3dx)
        {
            Font_ = new Font();
            if (!Font_.Initialize(d3dx.Device, @"data\fontdata.txt", @"data\font.png"))
            {
                MessageBox.Show("Could not initialize the fontshader.", "Error", MessageBoxButtons.OK);
                return false;
            }

            FontShader_ = new FontShader();
            if (!FontShader_.Initialize(d3dx.Device))
            {
                MessageBox.Show("Could not initialize font font object.", "Error", MessageBoxButtons.OK);
                return false;
            }

            if (!InitializeSentence(d3dx.Device, _default_length)) return false;
            UpdateSentence(d3dx, " ");
            return true;
        }
        public void ChangeDevice(SharpDX.Direct3D11.Device device)
        {
            ReleaseSentence(sentence_);

            FontShader_?.Shutdown();
            FontShader_ = null;

            Font_?.Shutdown();
            Font_ = null;

            Font_ = new Font();
            if (!Font_.Initialize(device, @"data\fontdata.txt", @"data\font.png"))
            {
                MessageBox.Show("Could not initialize the fontshader.", "Error", MessageBoxButtons.OK);
                return;
            }

            FontShader_ = new FontShader();
            if (!FontShader_.Initialize(device))
            {
                MessageBox.Show("Could not initialize font font object.", "Error", MessageBoxButtons.OK);
                return;
            }

            InitializeSentence(device, sentence_.maxLength);
            UpdateSentence(device.ImmediateContext, text);
            return;
        }
        public void Shutdown()
        {
            ReleaseSentence(sentence_);

            /*if (_Description)
            {
                int i = 0;
                while (i < 25 && _Description[i])
                {
                    ReleaseSentence( &_Description[i]);
                    i++;
                }
                delete[] _Description;
                _Description = 0;
            }*/

            // Release the font shader object.

            FontShader_?.Shutdown();
            FontShader_ = null;

            Font_?.Shutdown();
            Font_ = null;
            return;
        }
        public bool Render(D3DX d3dx, Matrix worldMatrix, Matrix viewMatrix, Matrix orthoMatrix)
        {
            Matrix matrix;
            int stride, offset;


            stride = Utilities.SizeOf<VertexType>();
            offset = 0;

            d3dx.DeviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(sentence_.vertexBuffer, stride, offset));
            d3dx.DeviceContext.InputAssembler.SetIndexBuffer(sentence_.indexBuffer, Format.R32_UInt, 0);

            d3dx.DeviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            matrix = Matrix.Scaling(scale, scale, scale);
            worldMatrix = Matrix.Multiply(worldMatrix, matrix);

            //D3DXMatrixRotationYawPitchRoll(&matrix, rotY, rotX, rotZ);
            //D3DXMatrixMultiply(&worldMatrix, &worldMatrix, &matrix);

            matrix = Matrix.Translation(position.X, position.Y, position.Z);
            worldMatrix = Matrix.Multiply(worldMatrix, matrix);


            return FontShader_.Render(d3dx,
                sentence_.indexCount,
                worldMatrix,
                viewMatrix,
                orthoMatrix,
                Font_.GetTexture(),
                color);
        }
        private bool InitializeSentence(SharpDX.Direct3D11.Device device, int maxLength)
        {
            try
            {
                sentence_ = new SentenceType
                {
                    maxLength = maxLength,
                    vertexCount = 6 * maxLength,
                    indexCount = 6 * maxLength
                };

                VertexType[] vertices = new VertexType[sentence_.vertexCount];

                uint[] indices = new uint[sentence_.indexCount];

                for (int i = 0; i < sentence_.indexCount; ++i)
                {
                    indices[i] = (uint)i;
                }

                var vertexBufferDesc = new BufferDescription()
                {
                    Usage = ResourceUsage.Dynamic,
                    SizeInBytes = Utilities.SizeOf<VertexType>() * sentence_.vertexCount,
                    BindFlags = BindFlags.VertexBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };
                sentence_.vertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, vertices, vertexBufferDesc);


                var indexBufferDesc = new BufferDescription()
                {
                    Usage = ResourceUsage.Default,
                    SizeInBytes = Utilities.SizeOf<uint>() * sentence_.indexCount,
                    BindFlags = BindFlags.IndexBuffer,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };
                sentence_.indexBuffer = SharpDX.Direct3D11.Buffer.Create(device, indices, indexBufferDesc);

                vertices = null;
                indices = null;

                return true;
            }
            catch (ArgumentException e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK);
                return false;
            }
        }
        private bool UpdateSentence(DeviceContext deviceContext, string text)
        {

            this.text = text;
            if (text.Length > sentence_.maxLength) return false;

            Font_.BuildVertexArray(out List<Font.VertexType> vertices, text);
            if (vertices.Count == 0) return false;
            var coord = vertices[vertices.Count - 1].position;
            Height = coord.Y;
            Width = coord.X;

            deviceContext.MapSubresource(sentence_.vertexBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out DataStream mappedResource);
            mappedResource.WriteRange(vertices.ToArray());

            deviceContext.UnmapSubresource(sentence_.vertexBuffer, 0);


            vertices?.Clear();
            return true;
        }

        private void ReleaseSentence(SentenceType sentence)
        {
                sentence.vertexBuffer?.Dispose();
                sentence.vertexBuffer = null;

                sentence.indexBuffer?.Dispose();
                sentence.indexBuffer = null;
            return;
        }

        public bool SetText(DeviceContext deviceContext, string text)
        {
            return UpdateSentence(deviceContext, text);
        }
    }
}
