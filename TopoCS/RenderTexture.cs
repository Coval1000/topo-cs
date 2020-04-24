using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace TopoCS
{
    class RenderTexture
    {

        private Texture2D _renderTargetTexture;
        private RenderTargetView _renderTargetView;
        private ShaderResourceView _shaderResourceView;

        public RenderTexture()
        {
        }

        public bool Initialize(SharpDX.Direct3D11.Device device, int textureWidth, int textureHeight)
        {
            try
            {
                _renderTargetTexture = new Texture2D(device, new Texture2DDescription()
                {
                    Width = textureWidth,
                    Height = textureHeight,
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                    Usage = ResourceUsage.Default,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = Format.R32G32B32A32_Float,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0),
                });

                _renderTargetView = new RenderTargetView(device, _renderTargetTexture, new RenderTargetViewDescription()
                {
                    Format = Format.R32G32B32A32_Float,
                    Dimension = RenderTargetViewDimension.Texture2D,
                    Texture2D =
                {
                    MipSlice = 0,
                }
                });

                _shaderResourceView = new ShaderResourceView(device, _renderTargetTexture, new ShaderResourceViewDescription()
                {
                    Format = Format.R32G32B32A32_Float,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D =
                {
                    MostDetailedMip = 0,
                    MipLevels = 1,
                }
                });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Shutdown()
        {
            _shaderResourceView?.Dispose();
            _shaderResourceView = null;

            _renderTargetView?.Dispose();
            _renderTargetView = null;

            _renderTargetTexture?.Dispose();
            _renderTargetTexture = null;

            return;
        }

        public void SetRenderTarget(DeviceContext deviceContext, DepthStencilView depthStencilView)
        {
            // Bind the render target view and depth stencil buffer to the output render pipeline.
            deviceContext.OutputMerger.SetRenderTargets(depthStencilView, _renderTargetView);
            return;
        }

        public void ClearRenderTarget(DeviceContext deviceContext, DepthStencilView depthStencilView,
            Color4 color)
        {
            deviceContext.ClearRenderTargetView(_renderTargetView, color);
            deviceContext.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);

            return;
        }

        public ShaderResourceView GetShaderResourceView()
        {
            return _shaderResourceView;
        }

        public Texture2D GetRenderTargetTexture()
        {
            return _renderTargetTexture;
        }

        public void ToBitmap(DeviceContext deviceContext)
        {
            _ = deviceContext.MapSubresource(_renderTargetTexture, 0, MapMode.Write, 0, out _);
            deviceContext.UnmapSubresource(_renderTargetTexture, 0);
        }
    }
}
