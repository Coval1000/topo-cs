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
    class D3DX
    {
        private bool _vsync_enabled;

        public int VideoCardMemory { get; private set; }
        public string VideoCardDescription { get; private set; }
        private SwapChain _swapChain;
        public SharpDX.Direct3D11.Device Device { get; private set; }
        public DeviceContext DeviceContext { get; private set; }
        private RenderTargetView _renderTargetView;
        public Texture2D DepthStencilBuffer { get; private set; }
        public Texture2D BackBuffer { get; private set; }
        private DepthStencilState _depthStencilState;
        public DepthStencilView DepthStencilView { get; private set; }
        private RasterizerState _rasterState;

        public Matrix ProjectionMatrix { get; private set; }
        public Matrix WorldMatrix { get; private set; }
        public Matrix OrthoMatrix { get; private set; }

        private DepthStencilState _depthDisabledStencilState;
        private BlendState _alphaEnableBlendingState;
        private BlendState _alphaDisableBlendingState;

        public D3DX()
        {

        }
        public bool Initialize(int screenWidth, int screenHeight, bool vsync, IntPtr hwnd, bool fullscreen,
                          float screenDepth, float screenNear)
        {
            try
            {
                var factory = new Factory1();
                var adapter = factory.GetAdapter1(0);
                var adapterOutput = adapter.GetOutput(0);
                var displayModeList = adapterOutput.GetDisplayModeList(Format.R8G8B8A8_UNorm, DisplayModeEnumerationFlags.Interlaced);

                _vsync_enabled = vsync;

                var rational = new Rational(0, 1);
                if (_vsync_enabled)
                {
                    foreach (var mode in displayModeList)
                    {
                        if (mode.Width == screenWidth && mode.Height == screenHeight)
                        {
                            rational = new Rational(mode.RefreshRate.Numerator, mode.RefreshRate.Denominator);
                            break;
                        }
                    }
                }
                VideoCardMemory = adapter.Description.DedicatedVideoMemory >> 10 >> 10;
                VideoCardDescription = adapter.Description.Description.Trim('\0');

                SharpDX.Direct3D11.Device.CreateWithSwapChain(
                    DriverType.Hardware,
                    DeviceCreationFlags.None,
                    new SwapChainDescription()
                    {
                        BufferCount = 1,
                        ModeDescription = new ModeDescription(screenWidth, screenHeight, rational, Format.R8G8B8A8_UNorm),
                        Usage = Usage.RenderTargetOutput,
                        OutputHandle = hwnd,
                        SampleDescription = new SampleDescription(1, 0),
                        IsWindowed = fullscreen,
                        Flags = SwapChainFlags.None,
                        SwapEffect = SwapEffect.Discard
                    },
                    out var device,
                    out _swapChain);

                Device = device;

                DeviceContext = Device.ImmediateContext;

                BackBuffer = Texture2D.FromSwapChain<Texture2D>(_swapChain, 0);
                _renderTargetView = new RenderTargetView(Device, BackBuffer);                

                DepthStencilBuffer = new Texture2D(Device, new Texture2DDescription()
                {
                    Width = screenWidth,
                    Height = screenHeight,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.D24_UNorm_S8_UInt,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.DepthStencil,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                });

                _depthStencilState = new DepthStencilState(Device, new DepthStencilStateDescription()
                {
                    IsDepthEnabled = true,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthComparison = Comparison.Less,
                    IsStencilEnabled = true,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0xFF,
                    FrontFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Increment,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always
                    },
                    BackFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Decrement,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always
                    }
                });
                DeviceContext.OutputMerger.SetDepthStencilState(_depthStencilState, 1);


                DepthStencilView = new DepthStencilView(Device, DepthStencilBuffer, new DepthStencilViewDescription()
                {
                    Format = Format.D24_UNorm_S8_UInt,
                    Dimension = DepthStencilViewDimension.Texture2D,
                    Texture2D = new DepthStencilViewDescription.Texture2DResource()
                    {
                        MipSlice = 0
                    }
                });

                DeviceContext.OutputMerger.SetTargets(DepthStencilView, _renderTargetView);

                _rasterState = new RasterizerState(Device, new RasterizerStateDescription()
                {
                    IsAntialiasedLineEnabled = false,
                    CullMode = CullMode.Back,
                    DepthBias = 0,
                    DepthBiasClamp = .0f,
                    IsDepthClipEnabled = true,
                    FillMode = FillMode.Solid,
                    IsFrontCounterClockwise = false,
                    IsMultisampleEnabled = false,
                    IsScissorEnabled = false,
                    SlopeScaledDepthBias = .0f
                });
                DeviceContext.Rasterizer.State = _rasterState;

                DeviceContext.Rasterizer.SetViewport(0, 0, screenWidth, screenHeight, 0, 1);

                ProjectionMatrix = Matrix.PerspectiveFovLH((float)(Math.PI / 4), ((float)screenWidth / (float)screenHeight), screenNear, screenDepth);
                WorldMatrix = Matrix.Identity;
                OrthoMatrix = Matrix.OrthoLH((float)screenWidth, (float)screenHeight, screenNear, screenDepth);


                _depthDisabledStencilState = new DepthStencilState(Device, new DepthStencilStateDescription()
                {
                    IsDepthEnabled = false,
                    DepthWriteMask = DepthWriteMask.All,
                    DepthComparison = Comparison.Less,
                    IsStencilEnabled = true,
                    StencilReadMask = 0xFF,
                    StencilWriteMask = 0xFF,
                    FrontFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Increment,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always
                    },
                    BackFace = new DepthStencilOperationDescription()
                    {
                        FailOperation = StencilOperation.Keep,
                        DepthFailOperation = StencilOperation.Decrement,
                        PassOperation = StencilOperation.Keep,
                        Comparison = Comparison.Always
                    }
                });

                var blendStateDesc = new BlendStateDescription();
                blendStateDesc.RenderTarget[0].IsBlendEnabled = true;
                blendStateDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
                blendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
                blendStateDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                blendStateDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
                blendStateDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
                blendStateDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                blendStateDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;


                _alphaEnableBlendingState = new BlendState(Device, blendStateDesc);

                blendStateDesc.RenderTarget[0].IsBlendEnabled = false;

                _alphaDisableBlendingState = new BlendState(Device, blendStateDesc);

                factory.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Shutdown()
        {
            // Before shutting down set to windowed mode or when you release the swap chain it will throw an exception.
            _swapChain?.SetFullscreenState(false, null);

            _alphaEnableBlendingState?.Dispose();
            _alphaEnableBlendingState = null;

            _alphaDisableBlendingState?.Dispose();
            _alphaDisableBlendingState = null;

            BackBuffer?.Dispose();
            BackBuffer = null;

            _depthDisabledStencilState?.Dispose();
            _depthDisabledStencilState = null;

            _rasterState?.Dispose();
            _rasterState = null;

            DepthStencilView?.Dispose();
            DepthStencilView = null;

            _depthStencilState?.Dispose();
            _depthStencilState = null;

            DepthStencilBuffer?.Dispose();
            DepthStencilBuffer = null;

            _renderTargetView?.Dispose();
            _renderTargetView = null;

            DeviceContext?.Dispose();
            DeviceContext = null;

            Device?.Dispose();
            Device = null;

            BackBuffer?.Dispose();
            BackBuffer = null;

            _swapChain?.Dispose();
            _swapChain = null;
        }
        public void BeginScene(float red, float green, float blue, float alpha)
        {
            BeginScene(new Color4(red, green, blue, alpha));
            return;
        }
        public void BeginScene(Color4 givenColour)
        {
            DeviceContext.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth, 1, 0);
            DeviceContext.ClearRenderTargetView(_renderTargetView, givenColour);
        }
        public void EndScene()
        {
           if (_vsync_enabled)
                _swapChain.Present(1, PresentFlags.None);
            else
                _swapChain.Present(0, PresentFlags.None);
        }
        public void TurnZBufferOn()
        {
            DeviceContext.OutputMerger.SetDepthStencilState(_depthStencilState, 1);
        }
        public void TurnZBufferOff()
        {
            DeviceContext.OutputMerger.SetDepthStencilState(_depthDisabledStencilState, 1);
        }
        public void TurnOnAlphaBlending()
        {
            var blendFactor = new Color4(0, 0, 0, 0);
            DeviceContext.OutputMerger.SetBlendState(_alphaEnableBlendingState, blendFactor, -1);
        }
        public void TurnOffAlphaBlending()
        {
            var blendFactor = new Color4(0, 0, 0, 0);
            DeviceContext.OutputMerger.SetBlendState(_alphaDisableBlendingState, blendFactor, -1);
        }
        public void SetBackBufferRenderTarget()
        {         
            DeviceContext.OutputMerger.SetTargets(DepthStencilView, _renderTargetView);
            return;
        }

        public static implicit operator SharpDX.Direct3D11.Device(D3DX d) => d.Device;
        public static implicit operator DeviceContext(D3DX d) => d.DeviceContext;
    }
}

