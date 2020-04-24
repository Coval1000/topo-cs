using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TopoCS.Shaders
{
    class PlotShader
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct MatrixBufferType
        {
            public Matrix world;
            public Matrix view;
            public Matrix projection;
        };

        private VertexShader _vertexShader;
        private PixelShader _pixelShader;
        private InputLayout _layout;
        private SharpDX.Direct3D11.Buffer _matrixBuffer;

        public PlotShader()
        {
        }
        public bool Initialize(Device device, IntPtr hwnd)
        {
            return InitializeShader(device, hwnd, @"data\plot.vs", @"data\plot.ps");
        }
        public void Shutdown()
        {
            ShutdownShader();
            return;
        }
        public bool Render(DeviceContext deviceContext, int indexCount, Matrix worldMatrix, Matrix viewMatrix,
    Matrix projectionMatrix, bool isSecondaryColor = false)
        {
            if (!SetShaderParameters(deviceContext, worldMatrix, viewMatrix, projectionMatrix, isSecondaryColor))
            {
                return false;
            }
            RenderShader(deviceContext, indexCount);
            return true;
        }
        private bool InitializeShader(Device device, IntPtr hwnd, string vsFilename, string psFilename)
        {
            try
            {
                ShaderBytecode vertexShaderByteCode = ShaderBytecode.CompileFromFile(vsFilename, "PlotVertexShader", "vs_5_0", ShaderFlags.EnableStrictness, EffectFlags.None);
                ShaderBytecode pixelShaderByteCode = ShaderBytecode.CompileFromFile(psFilename, "PlotPixelShader", "ps_5_0", ShaderFlags.EnableStrictness, EffectFlags.None);

                _vertexShader = new VertexShader(device, vertexShaderByteCode);
                _pixelShader = new PixelShader(device, pixelShaderByteCode);

                InputElement[] inputElements = new InputElement[]
                    {
                    new InputElement()
                    {
                        SemanticName = "POSITION",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32_Float,
                        Slot = 0,
                        AlignedByteOffset = 0,
                        Classification = InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    },
                    new InputElement()
                    {
                        SemanticName = "COLOR",
                        SemanticIndex = 0,
                        Format = SharpDX.DXGI.Format.R32G32B32A32_Float,
                        Slot = 0,
                        AlignedByteOffset = InputElement.AppendAligned,
                        Classification = InputClassification.PerVertexData,
                        InstanceDataStepRate = 0
                    }
                    };

                _layout = new InputLayout(device, ShaderSignature.GetInputSignature(vertexShaderByteCode), inputElements);

                vertexShaderByteCode.Dispose();
                pixelShaderByteCode.Dispose();

                BufferDescription constantBufferDesc = new BufferDescription()
                {
                    Usage = ResourceUsage.Dynamic,
                    SizeInBytes = Utilities.SizeOf<MatrixBufferType>(), // was Matrix
                    BindFlags = BindFlags.ConstantBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };

                _matrixBuffer = new SharpDX.Direct3D11.Buffer(device, constantBufferDesc);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing shader. Error is " + ex.Message);
                return false;
            }
        }
        private void ShutdownShader()
        {

            _matrixBuffer?.Dispose();
            _matrixBuffer = null;

            _layout?.Dispose();
            _layout = null;

            _pixelShader?.Dispose();
            _pixelShader = null;

            _vertexShader?.Dispose();
            _vertexShader = null;
            return;
        }
        private bool SetShaderParameters(DeviceContext deviceContext, Matrix worldMatrix, Matrix viewMatrix,
    Matrix projectionMatrix, bool gridColor)
        {
            try
            {
                worldMatrix.Transpose();
                viewMatrix.Transpose();
                projectionMatrix.Transpose();

                deviceContext.MapSubresource(_matrixBuffer, MapMode.WriteDiscard, MapFlags.None, out DataStream mappedResource);
                MatrixBufferType matrixBuffer = new MatrixBufferType()
                {
                    world = worldMatrix,
                    view = viewMatrix,
                    projection = projectionMatrix,
                };
                mappedResource.Write(matrixBuffer);
                deviceContext.UnmapSubresource(_matrixBuffer, 0);
                int bufferSlotNuber = 0;
                deviceContext.VertexShader.SetConstantBuffer(bufferSlotNuber, _matrixBuffer);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void RenderShader(DeviceContext deviceContext, int indexCount)
        {
            deviceContext.InputAssembler.InputLayout = _layout;
            deviceContext.VertexShader.Set(_vertexShader);
            deviceContext.PixelShader.Set(_pixelShader);
            deviceContext.DrawIndexed(indexCount, 0, 0);

            return;
        }
    }
}
