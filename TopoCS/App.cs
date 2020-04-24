using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpDX.IO;
using SharpDX.Direct2D1;
using SharpDX.WIC;
using SharpDX.DirectWrite;

namespace TopoCS
{
    class App
    {
        const bool FULL_SCREEN = false;
        const bool VSYNC_ENABLED = true;
        const float SCREEN_DEPTH = 100000.0f;
        const float SCREEN_NEAR = 0.1f;

        public Button.ClickDel fPTR;
        public Color4 BackgroundColour;

        private Input _Input;
        private D3DX _Direct3D;
        private Camera _Camera;
        private ColorShader _ColorShader;
        private Timer _Timer;
        private Position _Position;
        //private Fps _Fps;
        //private Cpu _Cpu;
        private FontShader FontShader_;
        private MonocolorShader _MonoShader;
        private string[] _Data;
        private string[] _DataHeader;
        private bool _gridOn;
        private bool _gpressed, _RenderMiniAxis;
        public bool PrintScreen;
        private bool mouseRightDown, mouseLeftDown;
        public bool manualSteering;
        public bool Focus;
        public int SaveWidth;
        public int SaveHeight;
        public Point CoordFix;
        private Plain.DataMode mode_;
        private int screenwidth_, screenheight_;
        private Plain.ColorMode COLOR_MODE_;
        private float rotation;
        private Matrix _baseViewMatrix;
        private IntPtr _hwnd;
        //private RenderTexture _RenderTexture;
        //private DebugWindow _DebugWindow;
        //private TextureShader _TextureShader;

        public Graph graph;
        //public Terrain _Terrain;
        #region Text
        private Text CpuUsage_;
        private Text FramesPerSecond_;
        private Text IntersectionDebug_;
        private Text SensivityZ_;
        private Text GridDensity_;
        #endregion
        //public Axle AxleX_, AxleY_, AxleZ_;
        private AxisRosette _AxisMini;
        #region Button
        private Button GridButton_;
        private Button OverdrivePButton_;
        private Button OverdriveNButton_;
        private Button CameraDebugButton_;
        private Button MiniAxisBgButton_;
        private Button LoadComMapButton_;
        private Button ModeButton_;
        private List<Button> ButtonList;
        #endregion
        private VectorLine _DebugLine;

        public App()
        {
            _gridOn = false;
            _gpressed = false;
            PrintScreen = false;
            mode_ = Plain.DataMode.Extrapolator0;
            COLOR_MODE_ = Plain.ColorMode.GreyScale;
            mouseRightDown = false;
            mouseLeftDown = false;
            manualSteering = false;
            rotation = 0.0f;
            _RenderMiniAxis = true;
            BackgroundColour = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            Focus = false;
            CoordFix = new Point(0, 0);
        }

        public bool Initialize(IntPtr hwnd, int screenWidth, int screenHeight, string path)
        {
            float cameraX, cameraY, cameraZ, rotX, rotY, rotZ;
            screenheight_ = screenHeight;
            screenwidth_ = screenWidth;
            SaveHeight = screenHeight;
            SaveWidth = screenWidth;
            _hwnd = hwnd;
            _Input = new Input();
            if (!_Input.Initialize(screenWidth, screenHeight))
            {
                MessageBox.Show("Could not initialize the input object.", "Error", MessageBoxButtons.OK);
                return false;
            }

            _Direct3D = new D3DX();
            if (!_Direct3D.Initialize(screenWidth, screenHeight, VSYNC_ENABLED, hwnd, FULL_SCREEN, SCREEN_DEPTH, SCREEN_NEAR))
            {
                MessageBox.Show("Could not initialize DirectX 11.", "Error", MessageBoxButtons.OK);
                return false;
            }

            _ColorShader = new ColorShader();
            if (!_ColorShader.Initialize(_Direct3D.Device))
            {
                MessageBox.Show("Could not initialize the color shader object.", "Error", MessageBoxButtons.OK);
                return false;
            }

            _MonoShader = new MonocolorShader();
            if (!_MonoShader.Initialize(_Direct3D.Device))
            {
                MessageBox.Show("Could not initialize the monocolor shader object.", "Error", MessageBoxButtons.OK);
                return false;
            }

            graph = new Graph();
            if (!graph.Initialize(_Direct3D, _DataHeader, _Data, path))
            {

                MessageBox.Show("Could not initialize the graph object.", "Error", MessageBoxButtons.OK);
                return false;
            }

            _AxisMini = new AxisRosette();
            if (!_AxisMini.Initialize(_Direct3D, 30.0f))
            {
                MessageBox.Show("Could not initialize the axis object.", "Error", MessageBoxButtons.OK);
                return false;
            }
            #region Camera
            _Camera = new Camera
            {
                Position = new Vector3(0.0f, 0.0f, -1.0f)
            };
            _Camera.Render();
            _baseViewMatrix = _Camera._viewMatrix;

            cameraX = 0;
            cameraY = 0;
            cameraZ = 0;

            rotX = 0.0f;
            rotY = 0.0f;
            rotZ = 0.0f;

            _Camera.Position = new Vector3(cameraX, cameraY, cameraZ);
            _Camera.Rotation = new Vector3(rotX, rotY, rotZ);

            _Position = new Position();

            _Position.SetPosition(cameraX, cameraY, cameraZ);
            _Position.SetRotation(rotX, rotY, rotZ);
            _Position.SetOffSet(graph.terrain.Width / 2.0f, 0, graph.terrain.Height / 2.0f);
            _Position.SetRadious(graph.terrain.Width / 2.0f);
            _Position.orbit = true;
            #endregion
            _Timer = new Timer();
            if (!_Timer.Initialize())
            {
                MessageBox.Show("Could not initialize the timer object.", "Error", MessageBoxButtons.OK);
                return false;
            }

            //// Create the fps object.
            //_Fps = new FpsClass;
            //if (!_Fps) return false;
            //_Fps.Initialize();

            //_Cpu = new CpuClass;
            //if (!_Cpu) return false;
            //_Cpu.Initialize();

            if (!InitializeText()) return false;

            _DebugLine = new VectorLine();
            if(!_DebugLine.Initialize(_Direct3D.Device)) return false;
            _DebugLine.Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

            if (!InitializeButtons()) return false;

            //_RenderTexture = new RenderTexture();
            //if (!_RenderTexture.Initialize(_Direct3D.Device, screenWidth, screenHeight)) return false;

            //_DebugWindow = new DebugWindowClass;
            //if (!_DebugWindow) return false;

            //result = _DebugWindow.Initialize(_Direct3D.GetDevice(), screenWidth, screenHeight, 100, 100);
            //if (!result)
            //{
            //    MessageBox.Show("Could not initialize the debug window object.", L"Error", MessageBoxButtons.OK);
            //    return false;
            //}

            //_TextureShader = new TextureShaderClass;
            //if (!_TextureShader) return false;

            //result = _TextureShader.Initialize(_Direct3D.GetDevice(), hwnd);
            //if (!result)
            //{
            //    MessageBox.Show("Could not initialize the texture shader object.", L"Error", MessageBoxButtons.OK);
            //    return false;
            //}
            return true;
        }
        private bool InitializeButtons()
        {
            bool result;
            ButtonList = new List<Button>();

            GridButton_ = new Button(this);
            fPTR = Button_Grid;
            result = GridButton_.Initialize(_Direct3D);
            if (!result)
            {
                MessageBox.Show("Could not initialize the button object.", "Error", MessageBoxButtons.OK);
                return false;
            }
            GridButton_.SetRectangle(new Int_RECT(screenwidth_ - 120, 20, 70, 15));
            GridButton_.textColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            GridButton_.SetRectangleColor(_Direct3D.DeviceContext, new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
            GridButton_.SetText(_Direct3D.DeviceContext, "Siatka");
            //GridButton_.OnClick(&fPTR);
            ButtonList.Add(GridButton_);
            //############
            //_OverdriveP
            //############
            OverdrivePButton_ = new Button(this);
            fPTR = Button_OverdriveP;
            result = OverdrivePButton_.Initialize(_Direct3D);
            if (!result)
            {
                MessageBox.Show("Could not initialize the button object.", "Error", MessageBoxButtons.OK);
                return false;
            }
            OverdrivePButton_.SetRectangle(new Int_RECT(screenwidth_ - 150, 40, 10, 8));
            OverdrivePButton_.textColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            OverdrivePButton_.SetRectangleColor(_Direct3D.DeviceContext, new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
            OverdrivePButton_.SetText(_Direct3D.DeviceContext, "+");
            ButtonList.Add(OverdrivePButton_);
            //############
            //_OverdriveN
            //############
            OverdriveNButton_ = new Button(this);
            fPTR = Button_OverdriveN;
            result = OverdriveNButton_.Initialize(_Direct3D);
            if (!result)
            {
                MessageBox.Show("Could not initialize the button object.", "Error", MessageBoxButtons.OK);
                return false;
            }
            OverdriveNButton_.SetRectangle(new Int_RECT(screenwidth_ - 150, 48, 10, 7));
            OverdriveNButton_.textColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            OverdriveNButton_.SetRectangleColor(_Direct3D.DeviceContext, new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
            OverdriveNButton_.SetText(_Direct3D.DeviceContext, "-");
            ButtonList.Add(OverdriveNButton_);
            //#########
            //Mini Axis
            //#########
            MiniAxisBgButton_ = new Button(this);

            fPTR = Button_MiniAxisBG;
            result = MiniAxisBgButton_.Initialize(_Direct3D);
            if (!result)
            {
                MessageBox.Show("Could not initialize the button object.", "Error", MessageBoxButtons.OK);
                return false;
            }
            MiniAxisBgButton_.SetRectangle(new Int_RECT(screenwidth_  - 70 , (screenheight_ - 70)/2, 70, 70));
            MiniAxisBgButton_.textColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            MiniAxisBgButton_.SetRectangleColor(_Direct3D.DeviceContext, new Vector4(0.9f, 0.9f, 0.9f, 0.5f));
            MiniAxisBgButton_.SetText(_Direct3D.DeviceContext, " ");
            ButtonList.Add(MiniAxisBgButton_);
            //###########
            //CameraDebug
            //###########
            CameraDebugButton_ = new Button(this);
            fPTR = Button_CameraDebug;
            result = CameraDebugButton_.Initialize(_Direct3D);
            if (!result)
            {
                MessageBox.Show("Could not initialize the button object.", "Error", MessageBoxButtons.OK);
                return false;
            }
            CameraDebugButton_.SetRectangle(new Int_RECT(screenwidth_ - 50, screenheight_ - 15, 50, 15));
            CameraDebugButton_.textColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            CameraDebugButton_.SetRectangleColor(_Direct3D.DeviceContext, new Vector4(0.6f, 0.6f, 0.6f, 1.0f));
            CameraDebugButton_.SetText(_Direct3D.DeviceContext, "Debug");
            ButtonList.Add(CameraDebugButton_);
            //##########
            //LoadComMap
            //##########
            LoadComMapButton_ = new Button(this);
            fPTR = Button_LoadComMap;
            result = LoadComMapButton_.Initialize(_Direct3D);
            if (!result)
            {
                MessageBox.Show("Could not initialize the button object.", "Error", MessageBoxButtons.OK);
                return false;
            }
            LoadComMapButton_.SetRectangle(new Int_RECT(screenwidth_ - 60, screenheight_ - 30, 60, 15));
            LoadComMapButton_.textColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            LoadComMapButton_.SetRectangleColor(_Direct3D.DeviceContext, new Vector4(0.6f, 0.8f, 0.8f, 1.0f));
            LoadComMapButton_.SetText(_Direct3D.DeviceContext, "Load Com");
            LoadComMapButton_.draw = true;
            ButtonList.Add(LoadComMapButton_);
            //
            //
            //
            ModeButton_ = new Button(this);
            fPTR = Button_Mode;
            result = ModeButton_.Initialize(_Direct3D);
            if (!result)
            {
                MessageBox.Show("Could not initialize the button object.", "Error", MessageBoxButtons.OK);
                return false;
            }
            ModeButton_.SetRectangle(new Int_RECT(screenwidth_ - 60, screenheight_ - 45, 60, 15));
            ModeButton_.textColor = new Vector4(0.8f, 1.0f, 1.8f, 1.0f);
            ModeButton_.SetRectangleColor(_Direct3D.DeviceContext, new Vector4(0.0f, 0.0f, 0.8f, 1.0f));
            ModeButton_.SetText(_Direct3D.DeviceContext, "Mode");
            ModeButton_.draw = true;
            ButtonList.Add(ModeButton_);
            return true;
        }
        private bool InitializeText()
        {
            bool result = true;

            CpuUsage_ = new Text();
            result &= CpuUsage_.Initialize(_Direct3D);
            CpuUsage_.color = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            CpuUsage_.position = new Vector3(-screenwidth_ / 2 + 20, screenheight_ / 2 - 40, 0);
            //CpuUsage_.SetText(_Cpu.GetCpuPercentage().ToString()));

            FramesPerSecond_ = new Text();
            result &= FramesPerSecond_.Initialize(_Direct3D);
            FramesPerSecond_.color = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            FramesPerSecond_.position = new Vector3(-screenwidth_ / 2 + 20, screenheight_ / 2 - 20, 0);
            //FramesPerSecond_.SetText(std::to_string(_Cpu.GetCpuPercentage()));

            IntersectionDebug_ = new Text();
            result &= IntersectionDebug_.Initialize(_Direct3D);
            IntersectionDebug_.color = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            IntersectionDebug_.position = new Vector3(-screenwidth_ / 2 + 300, screenheight_ / 2 - 20, 0);
            IntersectionDebug_.SetText(_Direct3D, " ");

            SensivityZ_ = new Text();
            result &= SensivityZ_.Initialize(_Direct3D);
            SensivityZ_.color = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            SensivityZ_.position = new Vector3(-screenwidth_ / 2 + screenwidth_ - 170, screenheight_ / 2 - 40, 0);
            SensivityZ_.SetText(_Direct3D, "Czulosc z: 1");

            GridDensity_ = new Text();
            result &= GridDensity_.Initialize(_Direct3D);
            GridDensity_.color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
            GridDensity_.position = new Vector3(-screenwidth_ / 2 + screenwidth_ - 170, screenheight_ / 2 - 20, 0);
            GridDensity_.SetText(_Direct3D, "Wielkosc siatki: 1");
            if (!result)
            {
                MessageBox.Show("Could not initialize the text object.", "Error", MessageBoxButtons.OK);
                return false;
            }
            return true;
        }
        private void ShutdownButtons()
        {
                GridButton_?.Shutdown();
                GridButton_ = null;
            
                OverdriveNButton_.Shutdown();
                OverdriveNButton_ = null;

                OverdrivePButton_.Shutdown();
                OverdrivePButton_ = null;

                LoadComMapButton_.Shutdown();
                LoadComMapButton_ = null;
        }
        public void Shutdown()
        {
            //_TextureShader?.Shutdown();
            //_TextureShader = null;


            //_DebugWindow?.Shutdown();
            //_DebugWindow = null;

            //_RenderTexture?.Shutdown();
            //_RenderTexture = null;

            _Data = null;
            _Data = null;

            _DataHeader = null;
            _DataHeader = null;

            CpuUsage_?.Shutdown();
            CpuUsage_ = null;



            _AxisMini?.Shutdown();
            _AxisMini = null;

            FontShader_?.Shutdown();
            FontShader_ = null;

            //_Cpu?.Shutdown();
            //_Cpu = null;

            //_Fps = null;

            _Position = null;

            _Timer = null;

            _ColorShader?.Shutdown();
            _ColorShader = null;

            _Camera = null;

            _Direct3D?.Shutdown();
            _Direct3D = null;

            _Input?.Shutdown();
            _Input = null;

            ShutdownButtons();
            return;
        }
        public bool Frame()
        {
            if (!HandleInput(_Timer.FrameTime)) return false;
            _Timer.Frame();
            //_Fps.Frame();
            //_Cpu.Frame();

            //result = FramesPerSecond_.SetText(std::to_string(_Fps.GetFps()));
            //if (!result) return false;

            //if (_Fps.GetFps() < 30) FramesPerSecond_.color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            //else if (_Fps.GetFps() < 60) FramesPerSecond_.color = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
            //else FramesPerSecond_.color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);

            //result = CpuUsage_.SetText(std::to_string(_Cpu.GetCpuPercentage()));
            //if (!result) return false;           
            if (!RenderGraphics()) return false;          
            return true;
        }

        public bool IsOPressed()
        {
            return _Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.O);
        }

        private bool HandleInput(float frameTime)
        {
            int mouseX, mouseY, mouseZ;
            float posX, posY, posZ, rotX, rotY, rotZ;
            if (!_Input.Frame()) return false;
            if (!Focus) 
            {
                _Input.ResetTranslation();
                return true; 
            }
            if (_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.Escape)) return false;
            mouseZ = _Input.MouseMoveZ();
            if (mouseZ != 0) _Position.IncRadious(mouseZ);

            if (_Input.IsLeftMouseButtonDown())
            {
                _Position.OrbitCamera(_Input.MouseMoveX(), _Input.MouseMoveY());
                _Position.GetPosition(out posX, out posY, out posZ);

                if (mouseLeftDown == false)
                {
                    mouseLeftDown = true;
                    _Input.GetMouseLocation(out mouseX, out mouseY);
                    mouseX -= CoordFix.X;
                    mouseY -= CoordFix.Y;
                    CpuUsage_.SetText(_Direct3D, mouseX.ToString() + "," + mouseY.ToString() + "........");
                    foreach(var button in ButtonList)
                    {
                        button.Click(mouseX, mouseY);
                    }
                }
            }
            else
            {
                _Input.ResetTranslation();
                mouseLeftDown = false;
            }

            if (_Input.IsRightMouseButtonDown())
            {
                if (mouseRightDown == false)
                {
                    _Input.GetMouseLocation(out mouseX, out mouseY);
                    mouseX -= CoordFix.X;
                    mouseY -= CoordFix.Y;
                    CpuUsage_.SetText(_Direct3D, mouseX.ToString() + "," + mouseY.ToString() + "........");
                    TestIntersection(mouseX, mouseY);
                    mouseRightDown = true;
                }

            }
            else mouseRightDown = false;

            _Position.SetFrameTime(frameTime);
            if (manualSteering)
            {
                _Position.TurnLeft(_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.Left));
                _Position.TurnRight(_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.Right));
                _Position.MoveForward(_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.W));
                _Position.MoveBackward(_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.S));
                _Position.MoveUpward(_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.E));
                _Position.MoveLeft(_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.A));
                _Position.MoveRight(_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.D));
                _Position.MoveDownward(_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.Q));
                _Position.LookUpward(_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.Up));
                _Position.LookDownward(_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.Down));
            }

            if (_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.G))
            {
                if (!_gpressed)
                {
                    _gpressed = true;
                    graph.terrain.RenderGrid = !graph.terrain.RenderGrid;
                }
            }
            else _gpressed = false;

            if (_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.PageUp))
            {
                //_Terrain.Plot.ChangeGridSize(1);
                //GridDensity_.SetText("Wielkosc siatki: " + _Terrain.Plot.GridDensity.ToString()); todo:
            }

            if (_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.PageDown))
            {
                //_Terrain.Plot.ChangeGridSize(-1);
                //GridDensity_.SetText("Wielkosc siatki: " + _Terrain.Plot.GridDensity.ToString()); todo:
            }
            #region TerrainColour
            if (_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.Z) && COLOR_MODE_ != Plain.ColorMode.GreyScale)
            {
                graph.terrain.Plot.ChangeColorMode(Plain.ColorMode.GreyScale);
                COLOR_MODE_ = Plain.ColorMode.GreyScale;
            }
            if (_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.X) && COLOR_MODE_ != Plain.ColorMode.HighContrast)
            {
                graph.terrain.Plot.ChangeColorMode(Plain.ColorMode.HighContrast);
                COLOR_MODE_ = Plain.ColorMode.HighContrast;
            }
            if (_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.C) && COLOR_MODE_ != Plain.ColorMode.HCGSHybrid)
            {
                graph.terrain.Plot.ChangeColorMode(Plain.ColorMode.HCGSHybrid);
                COLOR_MODE_ = Plain.ColorMode.HCGSHybrid;
            }
            if (_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.V) && COLOR_MODE_ != Plain.ColorMode.FullBit)
            {
                graph.terrain.Plot.ChangeColorMode(Plain.ColorMode.FullBit);
                COLOR_MODE_ = Plain.ColorMode.FullBit;
            }
            if (_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.B) && COLOR_MODE_ != Plain.ColorMode.FullBitEyeCorrect)
            {
                graph.terrain.Plot.ChangeColorMode(Plain.ColorMode.FullBitEyeCorrect);
                COLOR_MODE_ = Plain.ColorMode.FullBitEyeCorrect;
            }
            #endregion
            #region Sensivity
            if (_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.Minus))
            {
                graph.terrain.Sensivity += -0.05f;
                SensivityZ_.SetText(_Direct3D, "Czulosc Z: " + graph.terrain.Sensivity.ToString());
            }
            if (_Input._keyboardState.IsPressed(SharpDX.DirectInput.Key.Add))
            {
                graph.terrain.Sensivity += 0.05f;
                SensivityZ_.SetText(_Direct3D, "Czulosc Z: " + graph.terrain.Sensivity.ToString());
            }
            #endregion
            _Position.GetPosition(out posX, out posY, out posZ);
            _Position.GetRotation(out rotX, out rotY, out rotZ);

            _Camera.Position = new Vector3(posX, posY, posZ);
            _Camera.Rotation = new Vector3(rotX, rotY, rotZ);

            return true;
        }

        private bool RenderGraphics() //todo: refactor
        {
            bool result = true;
            if (PrintScreen)
            {
                PrintScreen = false;
                if (!RenderSceneToFile()) return false;
            }

            _Direct3D.BeginScene(BackgroundColour);

            // Render the scene as normal to the back buffer.
            //result = RenderScene();
            //if (!result) return false;

            _Camera.Render();

            var worldMatrix = _Direct3D.WorldMatrix;
            var viewMatrix = _Camera._viewMatrix;
            var projectionMatrix = _Direct3D.ProjectionMatrix;
            var orthoMatrix = _Direct3D.OrthoMatrix;

            var rotation = _Camera.Rotation;

            _Direct3D.TurnOnAlphaBlending();
            var deviceContext = _Direct3D.DeviceContext;


            graph.Render(_Direct3D, _ColorShader, _MonoShader, worldMatrix, viewMatrix, projectionMatrix, rotation);


            if (!_DebugLine.Render(deviceContext, _MonoShader, worldMatrix, viewMatrix, projectionMatrix)) return false;



            _Direct3D.TurnZBufferOff();

            foreach (var button in ButtonList)
            {
                result &= button.Render(_Direct3D, _MonoShader, worldMatrix, _baseViewMatrix, orthoMatrix);
            }
            if (!result) return false;
           
            if (_RenderMiniAxis)
            {
                var worldMatrixPlaine = worldMatrix;
                var transformMatrix = Matrix.RotationY(-rotation.Y * (float)(Math.PI / 180f));
                worldMatrixPlaine = Matrix.Multiply(worldMatrixPlaine, transformMatrix);
                transformMatrix = Matrix.RotationX(-rotation.X * (float)(Math.PI / 180f));
                worldMatrixPlaine = Matrix.Multiply(worldMatrixPlaine, transformMatrix);
                transformMatrix = Matrix.Translation(screenwidth_ / 2 - 35, 0, 30);
                worldMatrixPlaine = Matrix.Multiply(worldMatrixPlaine, transformMatrix);
                _AxisMini.Render(deviceContext, _MonoShader, worldMatrixPlaine, _baseViewMatrix, orthoMatrix);
            }

            //result = _DebugWindow.Render(_Direct3D.GetDeviceContext(), 50, 50);
            //if (!result) return false;

            //// Render the debug window using the texture shader.
            //result = _TextureShader.Render(_Direct3D.GetDeviceContext(), _DebugWindow.GetIndexCount(), worldMatrix, _baseViewMatrix,
            //    orthoMatrix, _RenderTexture.GetShaderResourceView());
            //if (!result)
            //{
            //    return false;
            //}

            _Direct3D.TurnOffAlphaBlending();

            _Direct3D.TurnZBufferOn();

            _Direct3D.EndScene();
            return true;
        }
        private bool RenderSceneToFile()
        {

            var direct3D = new D3DX();
            if (!direct3D.Initialize(SaveWidth, SaveHeight, VSYNC_ENABLED, _hwnd, FULL_SCREEN, SCREEN_DEPTH, SCREEN_NEAR))
            {
                MessageBox.Show("Could not initialize DirectX 11.", "Error", MessageBoxButtons.OK);
                return false;
            }

            var colorShader = new ColorShader();
            if (!colorShader.Initialize(direct3D.Device))
            {
                MessageBox.Show("Could not initialize the color shader object.", "Error", MessageBoxButtons.OK);
                return false;
            }

            var monocolorShader = new MonocolorShader();
            if (!monocolorShader.Initialize(direct3D.Device))
            {
                MessageBox.Show("Could not initialize the monocolor shader object.", "Error", MessageBoxButtons.OK);
                return false;
            }

            var graph = new Graph(in this.graph, direct3D.Device);
   

            direct3D.SetBackBufferRenderTarget();
            direct3D.BeginScene(BackgroundColour);
            #region Render
            _Camera.Render();

            var worldMatrix = direct3D.WorldMatrix;
            var viewMatrix = _Camera._viewMatrix;
            var projectionMatrix = direct3D.ProjectionMatrix;

            var rotation = _Camera.Rotation;

            direct3D.TurnOnAlphaBlending();

            if (!graph.Render(direct3D, colorShader, monocolorShader, worldMatrix, viewMatrix, projectionMatrix, rotation)) return false;
            #endregion
            SaveToFile(direct3D, direct3D.BackBuffer, SharpDX.WIC.PixelFormat.Format32bppRGB);

            graph.Dispose();
            colorShader.Shutdown();
            monocolorShader.Shutdown();
            direct3D.Shutdown();
            return true;
        }
        private void SaveToFile(D3DX direct3D, Texture2D texture, Guid format)
        {
            var stream = new System.IO.FileStream("Output.png", System.IO.FileMode.Create);
            var textureTarget = texture;
            var textureCopy = new Texture2D(direct3D.Device, new Texture2DDescription
            {
                Width = (int)textureTarget.Description.Width,
                Height = (int)textureTarget.Description.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = textureTarget.Description.Format,
                Usage = ResourceUsage.Staging,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None
            });
            direct3D.DeviceContext.CopyResource(textureTarget, textureCopy);

            var dataBox = direct3D.DeviceContext.MapSubresource(
                textureCopy,
                0,
                0,
                MapMode.Read,
                SharpDX.Direct3D11.MapFlags.None,
                out DataStream dataStream);

            var dataRectangle = new DataRectangle
            {
                DataPointer = dataStream.DataPointer,
                Pitch = dataBox.RowPitch
            };

            var imagingFactory = new ImagingFactory2();
            var bitmap = new SharpDX.WIC.Bitmap(
                imagingFactory,
                textureCopy.Description.Width,
                textureCopy.Description.Height,
                format,
                dataRectangle);

            using (var s = stream)
            {
                s.Position = 0;
                using (var bitmapEncoder = new PngBitmapEncoder(imagingFactory, s))
                {
                    using (var bitmapFrameEncode = new BitmapFrameEncode(bitmapEncoder))
                    {
                        bitmapFrameEncode.Initialize();
                        bitmapFrameEncode.SetSize(bitmap.Size.Width, bitmap.Size.Height);
                        var pixelFormat = SharpDX.WIC.PixelFormat.FormatDontCare;
                        bitmapFrameEncode.SetPixelFormat(ref pixelFormat);
                        bitmapFrameEncode.WriteSource(bitmap);
                        bitmapFrameEncode.Commit();
                        bitmapEncoder.Commit();
                        bitmapFrameEncode.Dispose();
                        bitmapEncoder.Dispose();
                    }
                }
            }

            direct3D.DeviceContext.UnmapSubresource(textureCopy, 0);
            textureCopy.Dispose();
            bitmap.Dispose();
            imagingFactory.Dispose();
            dataStream.Dispose();
            stream.Dispose();
        }

        private void TestIntersection(int mouseX, int mouseY)
        {
            Vector3 direction;
            Vector3[] Points = new Vector3[2];

            double fxAy, fxBy, fyAx, fyBx, fxAz, fxBz, fyAz, fyBz;

            // Move the mouse cursor coordinates into the -1 to +1 range.
            float PointX = ((2.0f * (float)mouseX) / (float)screenwidth_) - 1.0f;
            float PointY = (((2.0f * (float)mouseY) / (float)screenheight_) - 1.0f) * -1.0f;

            var camera_rotation = _Camera.Rotation;
            var origin = _Camera.Position;

            // Adjust the Points using the projection matrix to account for the aspect ratio of the viewport.
            var projectionMatrix = _Direct3D.ProjectionMatrix;
            PointX /= projectionMatrix.M11;
            PointY /= projectionMatrix.M22;

            // Get the inverse of the view matrix.
            var viewMatrix = _Camera._viewMatrix;
            var inverseViewMatrix = Matrix.Invert(viewMatrix);

            // Calculate the direction of the picking ray in view space.
            direction.X = (PointX * inverseViewMatrix.M11) + (PointY * inverseViewMatrix.M21) + inverseViewMatrix.M31;
            direction.Y = (PointX * inverseViewMatrix.M12) + (PointY * inverseViewMatrix.M22) + inverseViewMatrix.M32;
            direction.Z = (PointX * inverseViewMatrix.M13) + (PointY * inverseViewMatrix.M23) + inverseViewMatrix.M33;

            fxAy = direction.Z / direction.X;
            fxBy = origin.Z - origin.X * (direction.Z / direction.X);

            fyAx = direction.X / direction.Z;
            fyBx = origin.X - origin.Z * (direction.X / direction.Z);

            fyAz = direction.Y / direction.Z;
            fyBz = origin.Y - origin.Z * (direction.Y / direction.Z);

            fxAz = direction.Y / direction.X;
            fxBz = origin.Y - origin.X * (direction.Y / direction.X);

            double equation1, equation2, equation3, equation4;

            switch ((int)(camera_rotation.Y) / 45)
            {
                default:
                case 0:
                    equation1 = fxAy * 1000 + fxBy;
                    equation2 = fyAz * equation1 + fyBz;
                    equation3 = fxAy * -1000 + fxBy;
                    equation4 = fyAz * equation3 + fyBz;
                    Points[0] = new Vector3(-1000, (float)equation4, (float)equation3);
                    Points[1] = new Vector3(1000, (float)equation2, (float)equation1);
                    _DebugLine.SetPoints(Points, _Direct3D);
                    for (int y = 0; graph.terrain.Height > y; y++)
                    {
                        for (int i = 0; graph.terrain.Width > i; i++)
                        {
                            if (TestIntersection_Check(i, y, direction, origin)) return;
                        }
                    }
                    break;

                case 1:
                case 2:
                    equation1 = fyAx * 1000 + fyBx;
                    equation2 = fxAz * equation1 + fxBz;
                    equation3 = fyAx * -1000 + fyBx;
                    equation4 = fxAz * equation3 + fxBz;
                    Points[0] = new Vector3((float)equation3, (float)equation4, -1000);
                    Points[1] = new Vector3((float)equation1, (float)equation2, 1000);
                    _DebugLine.SetPoints( Points, _Direct3D);
                    for (int i = 0; graph.terrain.Height > i; i++)
                    {
                        for (int j = graph.terrain.Width - 1; 0 <= j; j--)
                        {
                            if (TestIntersection_Check(i, j, direction, origin)) return;
                        }
                    }
                    break;

                case 3:
                case 4:
                    equation1 = fxAy * 1000 + fxBy;
                    equation2 = fyAz * equation1 + fyBz;
                    equation3 = fxAy * -1000 + fxBy;
                    equation4 = fyAz * equation3 + fyBz;
                    Points[0] = new Vector3(-1000, (float)equation4, (float)equation3);
                    Points[1] = new Vector3(1000, (float)equation2, (float)equation1);
                    _DebugLine.SetPoints(Points, _Direct3D);
                    for (int j = graph.terrain.Width - 1; 0 <= j; j--)
                    {
                        for (int i = graph.terrain.Width - 1; 0 <= i; i--)
                        {
                            if (TestIntersection_Check(i, j, direction, origin)) return;
                        }
                    }
                    break;

                case 5:
                case 6:
                    equation1 = fyAx * 1000 + fyBx;
                    equation2 = fxAz * equation1 + fxBz;
                    equation3 = fyAx * -1000 + fyBx;
                    equation4 = fxAz * equation3 + fxBz;
                    Points[0] = new Vector3((float)equation3, (float)equation4, -1000);
                    Points[1] = new Vector3((float)equation1, (float)equation2, 1000);
                    _DebugLine.SetPoints( Points, _Direct3D);
                    for (int i = graph.terrain.Height - 1; 0 <= i; i--)
                    {
                        for (int j = 0; graph.terrain.Height > j; j++)
                        {
                            if (TestIntersection_Check(i, j, direction, origin)) return;
                        }
                    }
                    break;
            }

            /*switch ((int)(camera_rotation.Y)/90)
            {
            default:
            case 0:
                for (int j = 0; _Terrain.GetTerrainHeight() > j; j++)
                {
                    for (int i = 0; _Terrain.GetTerrainWidth() > i; i++)
                    {
                        if (TestIntersection_Check(i, j, direction, origin)) return;
                    }
                }
                break;

            case 1:
                for (int i = 0; _Terrain.GetTerrainHeight() > i; i++)
                {
                    for (int j = _Terrain.GetTerrainWidth() - 1; 0 <= j; j--)
                    {
                        if (TestIntersection_Check(i, j, direction, origin)) return;
                    }
                }
                break;

            case 2:
                for (int j = _Terrain.GetTerrainWidth() - 1; 0 <= j; j--)
                {
                    for (int i = _Terrain.GetTerrainWidth() - 1; 0 <= i; i--)
                    {
                        if (TestIntersection_Check(i, j, direction, origin)) return;
                    }
                }
                break;

            case 3:
                for (int i = _Terrain.GetTerrainHeight() - 1; 0 <= i; i--)
                {
                    for (int j = 0; _Terrain.GetTerrainHeight() > j; j++)
                    {
                        if (TestIntersection_Check(i, j, direction, origin)) return;
                    }
                }
                break;
            }
        */
            // If not then set the intersection to "No".
            IntersectionDebug_.SetText(_Direct3D, "false " + mouseX.ToString() + "," + mouseY.ToString());
            graph.terrain.HighlightNode(new Point( -1, -1 ));
            return;
        }

        private bool TestIntersection_Check(int i, int j, Vector3 direction, Vector3 origin)
        {

            // Get the world matrix and translate to the location of the sphere.
            var worldMatrix = _Direct3D.WorldMatrix;
            var translateMatrix = Matrix.Translation(i, graph.terrain.GetValue(j, i) * graph.terrain.Sensivity, j);
            worldMatrix = Matrix.Multiply(worldMatrix, translateMatrix);

            // Now get the inverse of the translated world matrix.
            var inverseWorldMatrix = Matrix.Invert(worldMatrix);

            // Now transform the ray origin and the ray direction from view space to world space.
            Vector3.TransformCoordinate(ref origin, ref inverseWorldMatrix, out Vector3 rayOrigin);
            Vector3.TransformNormal(ref direction, ref inverseWorldMatrix, out Vector3 rayDirection);

            // Normalize the ray direction.
            Vector3.Normalize(ref rayDirection, out rayDirection);

            if (RaySphereIntersect(rayOrigin, rayDirection, 0.5f))
            {
                IntersectionDebug_.SetText(_Direct3D, "true " + i.ToString() + ", " + graph.terrain.GetValue(j, i).ToString() + ", " + j.ToString());
                graph.terrain.HighlightNode(new Point( j, i ));
                _Position.MoveOffSet(i, graph.terrain.GetValue(j, i) * graph.terrain.Sensivity, j);
                return true;
            }
            return false;
        }

        private bool RaySphereIntersect(Vector3 rayOrigin, Vector3 rayDirection, float radius)
        {
            float a, b, c, discriminant;

            // Calculate the a, b, and c coefficients.
            a = (rayDirection.X * rayDirection.X) + (rayDirection.Y * rayDirection.Y) + (rayDirection.Z * rayDirection.Z);
            b = ((rayDirection.X * rayOrigin.X) + (rayDirection.Y * rayOrigin.Y) + (rayDirection.Z * rayOrigin.Z)) * 2.0f;
            c = ((rayOrigin.X * rayOrigin.X) + (rayOrigin.Y * rayOrigin.Y) + (rayOrigin.Z * rayOrigin.Z)) - (radius * radius);

            // Find the discriminant.
            discriminant = (b * b) - (4 * a * c);

            // if discriminant is negative the picking ray missed the sphere, otherwise it intersected the sphere.
            return discriminant >= 0.0f;
        }

        public void Resize(IntPtr hwnd, int screenWidth, int screenHeight)
        {
            screenheight_ = screenHeight;
            screenwidth_ = screenWidth;
            SaveHeight = screenHeight;
            SaveWidth = screenWidth;
            _hwnd = hwnd;

            _Input.ScreenHeight = screenHeight;
            _Input.ScreenWidth = screenWidth;

            _Direct3D?.Shutdown();
            _Direct3D = new D3DX();
            if (!_Direct3D.Initialize(screenWidth, screenHeight, VSYNC_ENABLED, hwnd, FULL_SCREEN, SCREEN_DEPTH, SCREEN_NEAR))
            {
                MessageBox.Show("Could not initialize DirectX 11.", "Error", MessageBoxButtons.OK);
            }

            _ColorShader?.Shutdown();
            _ColorShader = new ColorShader();
            if (!_ColorShader.Initialize(_Direct3D.Device))
            {
                MessageBox.Show("Could not initialize the color shader object.", "Error", MessageBoxButtons.OK);
            }

            _MonoShader?.Shutdown();
            _MonoShader = new MonocolorShader();
            if (!_MonoShader.Initialize(_Direct3D.Device))
            {
                MessageBox.Show("Could not initialize the monocolor shader object.", "Error", MessageBoxButtons.OK);
            }

            graph.terrain.ChangeDevice(_Direct3D);
            #region Axis

            graph.AxleX.ChangeDevice(_Direct3D.Device);
            graph.AxleY.ChangeDevice(_Direct3D.Device);
            graph.AxleZ.ChangeDevice(_Direct3D.Device);
            _AxisMini.ChangeDevice(_Direct3D.Device);
            #endregion

            #region Text
            CpuUsage_.ChangeDevice(_Direct3D.Device);
            FramesPerSecond_.ChangeDevice(_Direct3D.Device);
            IntersectionDebug_.ChangeDevice(_Direct3D.Device);
            SensivityZ_.ChangeDevice(_Direct3D.Device);
            GridDensity_.ChangeDevice(_Direct3D.Device);
            #endregion

            _DebugLine.ChangeDevice(_Direct3D.Device);
         
            foreach (var button in ButtonList)
            {
                button.ChangeDevice(_Direct3D.Device);
            }
        }

        #region Buttons
        public void Button_Grid()
        {
            _gridOn = !_gridOn;
            return;
        }
        public void Button_OverdriveP()
        {
            graph.terrain.Sensivity += +0.05f;
            SensivityZ_.SetText(_Direct3D, "Czulosc Z: " + graph.terrain.Sensivity.ToString());
            return;
        }
        public void Button_OverdriveN()
        {
            graph.terrain.Sensivity += -0.05f;
            SensivityZ_.SetText(_Direct3D, "Czulosc Z: " + graph.terrain.Sensivity.ToString());
            return;
        }
        public void Button_CameraDebug()
        {
            if (_DebugLine.draw)
            {
                _DebugLine.draw = false;
                CameraDebugButton_.SetRectangleColor(_Direct3D.DeviceContext, new Vector4(0.8f, 0.5f, 0.5f, 1.0f));
            }
            else
            {
                _DebugLine.draw = true;
                CameraDebugButton_.SetRectangleColor(_Direct3D.DeviceContext, new Vector4(0.5f, 0.8f, 0.5f, 1.0f));
            }
            PrintScreen = true;
            return;
        }
        public void Button_MiniAxisBG()//todo: add size relation
        {
            if (_RenderMiniAxis)
            {
                _RenderMiniAxis = false;
                MiniAxisBgButton_.SetPosition(screenwidth_ - 10, (screenheight_ - 70) / 2);
            }
            else
            {
                _RenderMiniAxis = true;
                MiniAxisBgButton_.SetPosition(screenwidth_ - 70, (screenheight_ - 70) / 2);
            }
            return;
        }
        public void Button_LoadComMap()
        {
            //_Terrain.LoadComMap();
            return;
        }
        public void Button_Mode()
        {
            switch (mode_)
            {
                case Plain.DataMode.Extrapolator1Interpolated:
                    mode_ = Plain.DataMode.Extrapolator0;
                    graph.terrain.SetMode(_Direct3D.Device, Plain.DataMode.Extrapolator0);
                    break;
                case Plain.DataMode.Extrapolator0:
                    mode_ = Plain.DataMode.Extrapolator1;
                    graph.terrain.SetMode(_Direct3D.Device, Plain.DataMode.Extrapolator1);
                    break;

                case Plain.DataMode.Extrapolator1:
                    mode_ = Plain.DataMode.Extrapolator1Interpolated;
                    graph.terrain.SetMode(_Direct3D.Device, Plain.DataMode.Extrapolator1Interpolated);
                    break;
            }
            return;
        }
        #endregion
        public void Screen(out int x, out int y)
        {
            x = screenwidth_;
            y = screenheight_;
            return;
        }

    }

    struct Int_RECT
    {
        public int top;
        public int left;
        public int height;
        public int width;

        public Int_RECT(int left, int top, int w, int h)
        {
            this.top = top;
            this.left = left;
            height = h;
            width = w;
        }

        public Int_RECT(Int_RECT rect)
        {
            this.top = rect.top;
            this.left = rect.left;
            this.height = rect.height;
            this.width = rect.width;
        }
    };
}
