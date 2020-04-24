using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using TopoCS.Shapes;
namespace TopoCS
{
    class Button
    {
        public Vector4 textColor;

        public bool draw;
        private Point position_;
        private int _width, _height;

        private Text Label;
        private TopoCS.Shapes.Rectangle _Rectangle;
        private int screenWidth_, screenHeight_;
        public delegate void ClickDel();
        ClickDel _Click;
        private App _App;

        public Button(App app)
        {
            _App = app;
            _App.Screen(out screenWidth_, out screenHeight_);
            textColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            draw = true;
        }

        public Button(Button button, D3DX direct)
        {
            textColor = button.textColor;
            draw = button.draw;
            position_ = button.position_;
            _width = button._width;
            _height = button._height;

            Label = new Text(button.Label, direct);
            _Rectangle = new Shapes.Rectangle(button._Rectangle, direct);
            _Click = button._Click;
            _App = button._App;
            _App.Screen(out screenWidth_, out screenHeight_);
        }

        ~Button()
        {
            Shutdown();
        }
        public bool Initialize(D3DX direct)
        {

            _Rectangle = new TopoCS.Shapes.Rectangle();
            if (!_Rectangle.Initialize(direct.Device))
            {
                return false;
            }
            _Rectangle.twoWay = true;

            Label = new Text();
            if (!Label.Initialize(direct))
            {
                return false;
            }
            Label.SetText(direct, " ");
            Label.color = textColor;
            _Click = _App.fPTR;

            return true;
        }
        public bool Render(D3DX direct, MonocolorShader shader, Matrix worldMatrix, Matrix baseViewMatrix, Matrix orthoMatrix)
        {
            if (!draw) return true;
            bool result;
            result = _Rectangle.Render(direct, shader, worldMatrix, baseViewMatrix, orthoMatrix);

            result &= Label.Render(direct, worldMatrix, baseViewMatrix, orthoMatrix);

            return result;
        }
        public bool Hover(int x, int y)
        {
            if (x >= _Rectangle.x &&
                x <= _Rectangle.x + _width &&
                y >= _Rectangle.y &&
                y <= _Rectangle.y + _height) return true;
            else return false;

        }
        public bool Click(int x, int y)
        {
            if (x >= position_.X &&
                x <= position_.X + _width &&
                y >= position_.Y &&
                y <= position_.Y + _height)
            {
                _Click();
                return true;
            }
            else return false;
        }
        public void SetText(DeviceContext deviceContext, string text)
        {
            Label.SetText(deviceContext, text);
            return;
        }
        public void SetPosition(int x, int y)
        {
            _Rectangle.x = x - (screenWidth_ - _width) / 2;
            _Rectangle.y = -y + (screenHeight_ - _height) / 2;
            position_.X = x;
            position_.Y = y;
            Label.position.X = x - screenWidth_ / 2;
            Label.position.Y = -y + screenHeight_ / 2;
            return;
        }
        public void SetRectangleColor(DeviceContext deviceContext, Vector4 color)
        {
            _Rectangle.SetColor(deviceContext, color);
            return;
        }
        public void SetRectangle(Int_RECT rect)
        {
            _width = rect.width;
            _height = rect.height;
            _Rectangle.height = _height;
            _Rectangle.width = _width;
            SetPosition(rect.left, rect.top);
            return;
        }
        public void OnClick(ClickDel del)
        {
            _Click = del;
            return;
        }
        public void Shutdown()
        {
            Label?.Shutdown();
            Label = null;

            _Rectangle?.Shutdown();
            _Rectangle = null;
        }
        public void ChangeDevice(Device device)
        {
            Label.ChangeDevice(device);
            _Rectangle.ChangeDevice(device);
        }

    }
}
