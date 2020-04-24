using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopoCS.Shapes;
using SharpDX;
using SharpDX.Direct3D11;
namespace TopoCS
{
    class AxisRosette
    {
        private Triangle[] _Triangle;
        private VectorLine[] _Line;

        public AxisRosette()
        {

        }
        public AxisRosette(AxisRosette ax, Device device)
        {
            _Line = new VectorLine[3];
            _Triangle = new Triangle[3];

            _Line[0] = new VectorLine(ax._Line[0], device);
            _Line[1] = new VectorLine(ax._Line[1], device);
            _Line[2] = new VectorLine(ax._Line[2], device);

            _Triangle[0] = new Triangle(ax._Triangle[0], device);
            _Triangle[1] = new Triangle(ax._Triangle[1], device);
            _Triangle[2] = new Triangle(ax._Triangle[2], device);
        }

        public bool Initialize(D3DX direct, float lenght)
        {
            bool result = true;

            _Line = new VectorLine[3]; //0-X,1-Y,2-Z
            _Triangle = new Triangle[3];

            _Line[0] = new VectorLine();
            _Line[1] = new VectorLine();
            _Line[2] = new VectorLine();

            _Triangle[0] = new Triangle();
            _Triangle[1] = new Triangle();
            _Triangle[2] = new Triangle();

            result &= _Line[0].Initialize(direct.Device);
            result &= _Line[1].Initialize(direct.Device);
            result &= _Line[2].Initialize(direct.Device);

            result &= _Triangle[0].Initialize(direct.Device);
            result &= _Triangle[1].Initialize(direct.Device);
            result &= _Triangle[2].Initialize(direct.Device);

            if (!result) return false;

            _Line[0].SetPoints(new Vector3[] { new Vector3(0.0f, 0.0f, 0.0f), new Vector3(lenght, 0.0f, 0.0f) }, direct);
            _Line[0].Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            _Triangle[0].x = lenght;
            _Triangle[0].SetColor(direct.DeviceContext, new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
            _Triangle[0].twoWay = true;
            _Triangle[0].rotZ = (float)(-Math.PI / 2.0f);
            _Triangle[0].width = (int)(lenght / 10f);
            _Triangle[0].height = (int)(lenght / 10f);

            _Line[1].SetPoints(new Vector3[] { new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, lenght) }, direct);
            _Line[1].Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
            _Triangle[1].z = lenght;
            _Triangle[1].SetColor(direct.DeviceContext, new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
            _Triangle[1].twoWay = true;
            _Triangle[1].rotZ = (float)(Math.PI / 2.0f);
            _Triangle[1].rotY = (float)(Math.PI / 2.0f);
            _Triangle[1].width = (int)(lenght / 10f);
            _Triangle[1].height = (int)(lenght / 10f);

            _Line[2].SetPoints(new Vector3[] { new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, lenght, 0.0f) }, direct);
            _Line[2].Color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
            _Triangle[2].y = lenght;
            _Triangle[2].SetColor(direct.DeviceContext, new Vector4(0.0f, 0.0f, 1.0f, 1.0f));
            _Triangle[2].twoWay = true;
            _Triangle[2].width = (int)(lenght / 10f);
            _Triangle[2].height = (int)(lenght / 10f);

            return true;
        }
        public bool Render(DeviceContext deviceContext, MonocolorShader shader, Matrix worldMatrix, Matrix baseViewMatrix, Matrix orthoMatrix)
        {
            bool result = true;
            result &= _Line[0].Render(deviceContext, shader, worldMatrix, baseViewMatrix, orthoMatrix);
            result &= _Line[1].Render(deviceContext, shader, worldMatrix, baseViewMatrix, orthoMatrix);
            result &= _Line[2].Render(deviceContext, shader, worldMatrix, baseViewMatrix, orthoMatrix);

            result &= _Triangle[0].Render(deviceContext, shader, worldMatrix, baseViewMatrix, orthoMatrix);
            result &= _Triangle[1].Render(deviceContext, shader, worldMatrix, baseViewMatrix, orthoMatrix);
            result &= _Triangle[2].Render(deviceContext, shader, worldMatrix, baseViewMatrix, orthoMatrix);
            return result;
        }
        public void ChangeDevice(Device device)
        {
            _Line[0].ChangeDevice(device);
            _Line[1].ChangeDevice(device);
            _Line[2].ChangeDevice(device);
            _Triangle[0].ChangeDevice(device);
            _Triangle[1].ChangeDevice(device);
            _Triangle[2].ChangeDevice(device);
        }
        public void Shutdown()
        {
            _Line[0]?.Shutdown();
            _Line[1]?.Shutdown();
            _Line[2]?.Shutdown();
            _Line = null;

            _Triangle[0]?.Shutdown();
            _Triangle[1]?.Shutdown();
            _Triangle[2]?.Shutdown();

            _Triangle = null;

            return;
        }
    }
}
