using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TopoCS
{
    class FontShader
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct ConstantBufferType
        {
            public Matrix world;
            public Matrix view;
            public Matrix projection;
        };
        [StructLayout(LayoutKind.Sequential)]
        private struct PixelBufferType
        {
            public Vector4 pixelColor;
        };
        VertexShader _vertexShader;
        PixelShader _pixelShader;
        InputLayout _layout;
        SharpDX.Direct3D11.Buffer _constantBuffer;
        SamplerState _sampleState;

        SharpDX.Direct3D11.Buffer _pixelBuffer;

        public FontShader()
        {
        }

        public bool Initialize(Device device)
        {
           return InitializeShader(device, @"data\font.vs", @"data\font.ps");
        }

        public void Shutdown()
        {
            ShutdownShader();
            return;
        }

        public bool Render(DeviceContext deviceContext, int indexCount, Matrix worldMatrix, Matrix viewMatrix,
    Matrix projectionMatrix, ShaderResourceView texture, Vector4 pixelColor)
        {
            if (!SetShaderParameters(deviceContext, worldMatrix, viewMatrix, projectionMatrix, texture, pixelColor))
            {
                return false;
            }
            RenderShader(deviceContext, indexCount);
            return true;
        }

        private bool InitializeShader(Device device, string vsFilename, string psFilename)
        {
            try
            {
                ShaderBytecode vertexShaderByteCode = ShaderBytecode.CompileFromFile(vsFilename, "FontVertexShader", "vs_5_0", ShaderFlags.EnableStrictness, EffectFlags.None);
                ShaderBytecode pixelShaderByteCode = ShaderBytecode.CompileFromFile(psFilename, "FontPixelShader", "ps_5_0", ShaderFlags.EnableStrictness, EffectFlags.None);

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
                        SemanticName = "TEXCOORD",
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
                    SizeInBytes = Utilities.SizeOf<ConstantBufferType>(), // was Matrix
                    BindFlags = BindFlags.ConstantBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };

                _constantBuffer = new SharpDX.Direct3D11.Buffer(device, constantBufferDesc);

                var samplerDesc = new SamplerStateDescription()
                {
                    Filter = Filter.MinMagMipLinear,
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    MipLodBias = 0,
                    MaximumAnisotropy = 1,
                    ComparisonFunction = Comparison.Always,
                    BorderColor = new Color4(0, 0, 0, 0),
                    MinimumLod = 0,
                    MaximumLod = float.MaxValue
                };

                _sampleState = new SamplerState(device, samplerDesc);

                var pixelBufferDesc = new BufferDescription()
                {
                    Usage = ResourceUsage.Dynamic,
                    SizeInBytes = Utilities.SizeOf<PixelBufferType>(),
                    BindFlags = BindFlags.ConstantBuffer,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,
                    StructureByteStride = 0
                };

                _pixelBuffer = new SharpDX.Direct3D11.Buffer(device, pixelBufferDesc);
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
            _pixelBuffer?.Dispose();
            _pixelBuffer = null;

            _sampleState?.Dispose();
            _sampleState = null;

            _constantBuffer?.Dispose();
            _constantBuffer = null;

            _layout?.Dispose();
            _layout = null;

            _pixelShader?.Dispose();
            _pixelShader = null;

            _vertexShader?.Dispose();
            _vertexShader = null;
            return;
        }

        private bool SetShaderParameters(DeviceContext deviceContext, Matrix worldMatrix, Matrix viewMatrix,
    Matrix projectionMatrix, ShaderResourceView texture, Vector4 pixelColor)
        {
            try
            {
                worldMatrix.Transpose();
                viewMatrix.Transpose();
                projectionMatrix.Transpose();

                deviceContext.MapSubresource(_constantBuffer, MapMode.WriteDiscard, MapFlags.None, out DataStream mappedResource);
                ConstantBufferType matrixBuffer = new ConstantBufferType()
                {
                    world = worldMatrix,
                    view = viewMatrix,
                    projection = projectionMatrix
                };
                mappedResource.Write(matrixBuffer);
                deviceContext.UnmapSubresource(_constantBuffer, 0);
                int bufferSlotNuber = 0;
                deviceContext.VertexShader.SetConstantBuffer(bufferSlotNuber, _constantBuffer);

                deviceContext.PixelShader.SetShaderResources(0, texture);
                deviceContext.MapSubresource(_pixelBuffer, MapMode.WriteDiscard, MapFlags.None, out mappedResource);
                mappedResource.Write(pixelColor);
                deviceContext.UnmapSubresource(_pixelBuffer, 0);
                deviceContext.PixelShader.SetConstantBuffer(bufferSlotNuber, _pixelBuffer);
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
            deviceContext.PixelShader.SetSamplers(0, _sampleState);           
            deviceContext.DrawIndexed(indexCount, 0, 0);

            return;
        }
    }
}
