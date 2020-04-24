using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace TopoCS.Shapes
{
    abstract class Shape
    {
        [StructLayout(LayoutKind.Sequential)]
        protected struct VertexType
        {
            public Vector3 position;
        };

        protected Point position_;
        protected Vector4 color_;
        protected SharpDX.Direct3D11.Buffer vertexBuffer_, indexBuffer_;

        private readonly int indicesCount;
        private readonly int verticesCount;

        public bool draw;
        public float rotX, rotY, rotZ;
        public float x, y, z;
        public int width, height;
        public bool twoWay;

        public Shape(int indices, int vertices)
        {
            draw = true;
            rotX = 0;
            rotY = 0;
            rotZ = 0;
            width = 1;
            height = 1;
            x = 0;
            y = 0;
            z = 0;
            twoWay = false;
            indicesCount = indices;
            verticesCount = vertices;
        }

        public Shape(Shape shape, Device device)
        {
            position_ = shape.position_;
            color_ = shape.color_;

            indicesCount = shape.indicesCount;
            verticesCount = shape.verticesCount;

            draw = shape.draw;
            rotX = shape.rotX;
            rotY = shape.rotY;
            rotZ = shape.rotZ;
            x = shape.x;
            y = shape.y;
            z = shape.z;
            width = shape.width;
            height = shape.height;
            twoWay = shape.twoWay;        
            InitializeBuffer(device);
        }

        ~Shape()
        {
            Shutdown();
        }

        public void Shutdown()
        {
            vertexBuffer_?.Dispose();
            vertexBuffer_ = null;

            indexBuffer_?.Dispose();
            indexBuffer_ = null;
        }
        public bool Initialize(Device device)
        {
            return InitializeBuffer(device);
        }
        protected bool InitializeBuffer(Device device)
        {
            try
            {
                VertexType[] vertices = new VertexType[verticesCount];
                uint[] indices = new uint[indicesCount];

                CreateVertices(ref vertices);
                CreateIndices(ref indices);
                var vertexBufferDesc = new BufferDescription()
                {
                    Usage = ResourceUsage.Default,
                    SizeInBytes = Utilities.SizeOf<VertexType>() * verticesCount,
                    BindFlags = BindFlags.VertexBuffer,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };
                vertexBuffer_ = SharpDX.Direct3D11.Buffer.Create(device, vertices, vertexBufferDesc);

                var indexBufferDesc = new BufferDescription()
                {
                    Usage = ResourceUsage.Default,
                    SizeInBytes = Utilities.SizeOf<ulong>() * indicesCount,
                    BindFlags = BindFlags.IndexBuffer,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };
                indexBuffer_ = SharpDX.Direct3D11.Buffer.Create(device, indices, indexBufferDesc);
                indices = null;
                vertices = null;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool Render(DeviceContext deviceContext, MonocolorShader shader, Matrix worldMatrix, Matrix baseViewMatrix, Matrix orthoMatrix)
        {
            if (!draw) return true;
            int stride;
            int offset;

            stride = Utilities.SizeOf<VertexType>();
            offset = 0;

            deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding( vertexBuffer_, stride, offset));
            deviceContext.InputAssembler.SetIndexBuffer(indexBuffer_, SharpDX.DXGI.Format.R32_UInt, 0);
            deviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            var translateMatrix = Matrix.Scaling(width, height, 1);
            worldMatrix = Matrix.Multiply(worldMatrix, translateMatrix);
            Matrix.RotationYawPitchRoll(rotY, rotX, rotZ, out translateMatrix);
            worldMatrix = Matrix.Multiply(worldMatrix, translateMatrix);

            translateMatrix = Matrix.Translation(x, y, z);
            worldMatrix = Matrix.Multiply(worldMatrix, translateMatrix);

            return shader.Render(deviceContext, twoWay ? indicesCount : indicesCount / 2, worldMatrix, baseViewMatrix, orthoMatrix, color_);
        }
        protected bool UpdateBuffer(DeviceContext deviceContext)
        {
            try
            {
                VertexType[] vertices = new VertexType[verticesCount];
                CreateVertices(ref vertices);

                deviceContext.MapSubresource(vertexBuffer_, 0, MapMode.WriteDiscard, MapFlags.None, out DataStream mappedResource);
                mappedResource.WriteRange(vertices);
                deviceContext.UnmapSubresource(vertexBuffer_, 0);

                vertices = null;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void ChangeDevice(Device device)
        {
            Shutdown();
            InitializeBuffer(device);
        }
        public bool SetColor(DeviceContext deviceContext, float R, float G, float B, float A)
        {
            color_ =new Vector4(R, G, B, A);
            return UpdateBuffer(deviceContext);
        }
        public bool SetColor(DeviceContext deviceContext, Vector4 color)
        {
            color_ = color;
            return UpdateBuffer(deviceContext);
        }
        public bool SetColor(DeviceContext deviceContext, Vector3 color)
        {
            color_ = new Vector4(color.X, color.Y, color.Z, 1.0f);
            return UpdateBuffer(deviceContext);
        }
        public bool SetColor(DeviceContext deviceContext, Vector3 color, float alpha)
        {
            color_ = new Vector4(color.X, color.Y, color.Z, alpha);
            return UpdateBuffer(deviceContext);
        }
        public bool SetColor(DeviceContext deviceContext, int R, int G, int B, int A)
        {
            color_ = new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
            return UpdateBuffer(deviceContext);
        }
        public bool SetColor(DeviceContext deviceContext, int R, int G, int B)
        {
            color_ = new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, 1.0f);
            return UpdateBuffer(deviceContext);
        }
        public bool SetColor(DeviceContext deviceContext, Vector3 color, int alpha)
        {
            color_ = new Vector4(color.X, color.Y, color.Z, alpha / 255.0f);
            return UpdateBuffer(deviceContext);
        }

        protected abstract void CreateVertices(ref VertexType[] vertices);
	    protected abstract void CreateIndices(ref uint[] indices);
    }
}
