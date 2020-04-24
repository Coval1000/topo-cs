using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TopoCS
{
    class VectorLine
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct VertexType
        {
            public Vector3 position;
        };
        public bool draw;

        public PrimitiveTopology drawtype;
        public Vector4 Color;

        private SharpDX.Direct3D11.Buffer _vertexBuffer, _indexBuffer;
        private VertexType[] Point_;
        public int VerticesCount { get; private set; }
        private int[] indices_;
        private bool isBufferBigEnough;
        public VectorLine()
        {
            draw = true;
            drawtype = PrimitiveTopology.LineList;
            VerticesCount = 2;
            
        }
        public VectorLine(VectorLine vec, SharpDX.Direct3D11.Device device)
        {
            Color = vec.Color;
            draw = vec.draw;
            drawtype = vec.drawtype;
            Point_ = new VertexType[vec.Point_.Length];
            Array.Copy(vec.Point_, Point_, vec.Point_.Length);

            VerticesCount = vec.VerticesCount;
            indices_ = new int[vec.indices_.Length];
            Array.Copy(vec.indices_, indices_, vec.indices_.Length);
            isBufferBigEnough = vec.isBufferBigEnough;
            InitializeBuffers(device);
        }

        ~VectorLine()
        {
            Shutdown();
        }

        public bool Initialize(SharpDX.Direct3D11.Device device)
        {
            Point_ = new VertexType[VerticesCount];
            indices_ = new int[VerticesCount];
            Point_[0].position = new Vector3(0f, 0f, 0f);
            Point_[1].position = new Vector3(0f, 0f, 0f);
            indices_[0] = 0;
            indices_[1] = 1;
            return InitializeBuffers(device);
        }
        private bool InitializeBuffers(SharpDX.Direct3D11.Device device)
        {        
            var vertexBufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<VertexType>() * VerticesCount * 2,
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };
            _vertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, Point_, vertexBufferDesc);

            var indexBufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = Utilities.SizeOf<int>() * VerticesCount * 2,
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };
            _indexBuffer = SharpDX.Direct3D11.Buffer.Create(device, indices_, indexBufferDesc);
            isBufferBigEnough = true;
            return true;
        }
        public bool Render(DeviceContext deviceContext, MonocolorShader shader, Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            bool result = true;
            if (draw)
            {
                RenderBuffers(deviceContext);
                result = shader.Render(deviceContext, VerticesCount, worldMatrix, viewMatrix, projectionMatrix, Color);
            }

            return result;
        }
        public bool SetPoint(int index, Vector3 pos, D3DX d3dx)
        {
            if (index >= VerticesCount) return false;
            Point_[index].position = pos;
            return UpdateBuffer(d3dx);
        }
        public bool SetPoint(int index, float x, float y, float z, D3DX d3dx)
        {
            return SetPoint(index, new Vector3(x, y, z), d3dx);
        }
        public bool SetPoints(Vector3[] points, D3DX d3dx)
        {
            Point_ = null;
            indices_ = null;
            if (VerticesCount != points.Length)
            {
                isBufferBigEnough = false;
                VerticesCount = points.Length;
            }
            Point_ = new VertexType[VerticesCount];
            indices_ = new int[VerticesCount];
            for (int i = 0; i < VerticesCount; ++i)
            {
                Point_[i].position = points[i];
                indices_[i] = i;
            }
            return UpdateBuffer(d3dx);
        }
        private void RenderBuffers(DeviceContext deviceContext)
        {
            int stride;
            int offset;

            stride = Utilities.SizeOf<VertexType>();
            offset = 0;
            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, stride, offset));
            deviceContext.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R32_UInt, 0);
            deviceContext.InputAssembler.PrimitiveTopology = drawtype;
            return;
        }
        public void Shutdown()
        {
            ShutdownBuffers();

            Point_ = null;
            indices_ = null;
            return;
        }
        public void ChangeDevice(SharpDX.Direct3D11.Device device)
        {
            ShutdownBuffers();
            InitializeBuffers(device);
            return;
        }
        private void ShutdownBuffers()
        {
            _indexBuffer?.Dispose();
            _indexBuffer = null;

            _vertexBuffer?.Dispose();
            _vertexBuffer = null;
            return;
        }
        private bool UpdateBuffer(D3DX d3dx)
        {
            if (isBufferBigEnough)
            {
                d3dx.DeviceContext.MapSubresource(_vertexBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out DataStream mappedResource);
                mappedResource.WriteRange(Point_);
                d3dx.DeviceContext.UnmapSubresource(_vertexBuffer, 0);

                d3dx.DeviceContext.MapSubresource(_indexBuffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out mappedResource);
                mappedResource.WriteRange(indices_);
                d3dx.DeviceContext.UnmapSubresource(_indexBuffer, 0);
            }
            else
            {
                ShutdownBuffers();
                InitializeBuffers(d3dx.Device);//todo: add error handling
            }
            return true;
        }
    }
}
