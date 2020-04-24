using SharpDX;
using SharpDX.DirectInput;
using System.Windows.Forms;
using System;

namespace TopoCS
{
    class Input
    {
        public DirectInput _directInput;
        public Keyboard _keyboard;
        public Mouse _mouse;

        public KeyboardState _keyboardState;
        public MouseState _mouseState;

        public int ScreenWidth, ScreenHeight;
        public int _mouseX, _mouseY;
        public int _mouseXv, _mouseYv, _mouseZv;
        public Input()
        {
            _keyboardState = null;
        }
        internal bool Initialize(int screenWidth, int screenHeight)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;

            _mouseX = 0;
            _mouseY = 0;
            _mouseXv = 0;
            _mouseYv = 0;
            _mouseZv = 0;


            _directInput = new DirectInput();
            _keyboard = new Keyboard(_directInput);
            _keyboard.Properties.BufferSize = 256;

            //_keyboard.SetCooperativeLevel(hwnd, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);

            try
            {
                _keyboard.Acquire();
            }
            catch (SharpDXException sEx)
            {
                if (sEx.ResultCode.Failure)
                    return false;
            }

            _mouse = new Mouse(_directInput);
            _mouse.Properties.AxisMode = DeviceAxisMode.Relative;

            //_mouse.SetCooperativeLevel(hwnd, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);

            try
            {
                _mouse.Acquire();
            }
            catch (SharpDXException sEx)
            {
                if (sEx.ResultCode.Failure)
                    return false;
            }
            return true;
        }
        public void Shutdown()
        {
            _mouse?.Unacquire();
            _mouse?.Dispose();
            _mouse = null;
            // Release the keyboard.
            _keyboard?.Unacquire();
            _keyboard?.Dispose();
            _keyboard = null;
            // Release the main interface to direct input.
            _directInput?.Dispose();
            _directInput = null;
        }
        public bool Frame()
        {
            if (!ReadKeyboard())
                return false;

            if (!ReadMouse())
                return false;

            ProcessInput();

            return true;
        }

        public void GetMouseLocation(out int mouseX, out int mouseY)
        {
            mouseX = _mouseX;
            mouseY = _mouseY;
        }

        public bool IsLeftMouseButtonDown()
        {
            return (bool)_mouseState.Buttons.GetValue(0);
        }
        public bool IsRightMouseButtonDown()
        {
            return (bool)_mouseState.Buttons.GetValue(1);
        }
        public int MouseMoveX()
        {
            int i = _mouseXv;
            _mouseXv = 0;
            return i;
        }
        public int MouseMoveY()
        {
            int i = _mouseYv;
            _mouseYv = 0;
            return i;
        }
        public int MouseMoveZ()
        {
            int i = _mouseZv;
            _mouseZv = 0;
            return i;
        }
        public void ResetTranslation()
        {
            _mouseXv = 0;
            _mouseYv = 0;
            _mouseZv = 0;
        }
        private bool ReadKeyboard()
        {
            var resultCode = ResultCode.Ok;
            if (_keyboardState != null) _keyboardState = null;
            _keyboardState = new KeyboardState();

            try
            {
                _keyboard.GetCurrentState(ref _keyboardState);
            }
            catch (SharpDX.SharpDXException ex)
            {
                resultCode = ex.Descriptor; // ex.ResultCode;
                return false;
            }

            if (resultCode == ResultCode.InputLost || resultCode == ResultCode.NotAcquired)
            {
                _keyboard.Acquire();
            }
            else if (resultCode != ResultCode.Ok)
            {
                return false;
            }

            return true;
        }
        private bool ReadMouse()
        {
            var resultCode = ResultCode.Ok;

            _mouseState = new MouseState();
            try
            {
                _mouse.GetCurrentState(ref _mouseState);
            }
            catch (SharpDX.SharpDXException ex)
            {
                resultCode = ex.Descriptor; // ex.ResultCode;

                if (ex.ResultCode.Failure)
                {
                    if (ex.ResultCode == ResultCode.InputLost || ex.ResultCode == ResultCode.NotAcquired)
                    {
                        _mouse.Acquire();
                    }
                    else
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            if (resultCode == ResultCode.InputLost || resultCode == ResultCode.NotAcquired)
                _mouse.Acquire();
            else if (resultCode != ResultCode.Ok)
                return false;

            return true;
        }
        private void ProcessInput()
        {
            if (_mouseState != null)
            {
                _mouseXv += _mouseState.X;
                _mouseYv += _mouseState.Y;
                _mouseZv += _mouseState.Z;
            }
            _mouseX = Control.MousePosition.X;
            _mouseY = Control.MousePosition.Y;

            if (_mouseX < 0) { _mouseX = 0; }
            if (_mouseY < 0) { _mouseY = 0; }

            //if (_mouseX > screenWidth_)  { _mouseX = screenWidth_; }
            //if (_mouseY > screenHeight_) { _mouseY = screenHeight_; }//todo: make on fix input

            return;
        }


    }
}
