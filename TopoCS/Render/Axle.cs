using System;
using SharpDX.Direct3D11;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
namespace TopoCS
{
    class Axle
    {
        public enum DIVISION_TYPE { Const_Value, Const_Amount };

        public Vector3 Direction
        {
            get { return _Direction; }
            set
            {
                _Direction = value;
                Validated = false;
            }
        }
        private Vector3 _Direction;    
        public Vector3 Position
        {
            get { return _Position; }
            set
            {
                _Position = value;
                Validated = false;
            }
        }
        private Vector3 _Position;
        private Vector3 textposition_;
        public Vector3 TextOffset { 
            get { return _TextOffset; } 
            set
            {
                _TextOffset = value;
                Validated = false;
                return;
            } }
        private Vector3 _TextOffset;
        public string Unit
        {
            get { return _Unit; }
            set
            {
                _Unit = value;
                Validated = false;
            }
        }
        private string _Unit;
        public float Lenght
        {
            get { return _Lenght; }
            set 
            {
                _Lenght = value;
                Validated = false;
            }
        }
        private float _Lenght;
        private float maxValue_;
        private float minValue_;
        private float supportDivision_;
        private float supportDivisionLenght_;
        private float mainDivision_;
        private float mainDivisionLenght_;
        private DIVISION_TYPE division_type_;
        private Text Label_;
        private VectorLine Axle_;
        public bool Validated { get; private set;}

        public Axle()
        {
            _Unit = "";
            maxValue_ = 255;
            minValue_ = 0;
            supportDivision_ = 1;
            mainDivision_ = 1;
            supportDivisionLenght_ = 1;
            mainDivisionLenght_ = 2;
            _Lenght = 255;
            division_type_ = DIVISION_TYPE.Const_Amount;
            Validated = false;
            _Direction = new Vector3(0.0f, 0.0f, 0.0f);
            _Position = new Vector3(0.0f, 0.0f, 0.0f);
            _TextOffset = new Vector3(0.0f, 0.0f, 0.0f);
            textposition_ = new Vector3(0.0f, 0.0f, 0.0f);
        }

        public Axle(in Axle axle, in Device device)
        {
            _Direction = axle._Direction;
            _Position = axle._Position;
            textposition_ = axle.textposition_;
            _TextOffset = axle._TextOffset;
            _Unit = axle._Unit;
            Lenght = axle._Lenght;
            maxValue_ = axle.maxValue_;
            minValue_ = axle.minValue_;
            supportDivision_ = axle.supportDivision_;
            supportDivisionLenght_ = axle.supportDivisionLenght_;
            mainDivision_ = axle.mainDivision_;
            mainDivisionLenght_ = axle.mainDivisionLenght_;
            division_type_ = axle.division_type_;
            Label_ = new Text(axle.Label_, device);
            Axle_ = new VectorLine(axle.Axle_, device);
            Validated = axle.Validated;
        }
        public bool Initialize(in D3DX direct)
        {
            bool result = true;

            Axle_ = new VectorLine();
            Label_ = new Text();
            result &= Axle_.Initialize(direct.Device);

            result &= Label_.Initialize(direct);
            if (!result) return false;

            Label_.SetText(direct, "test");
            Label_.color = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            Axle_.Color = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            Axle_.SetPoints(new Vector3[] { new Vector3(0f, 0f, 0f), new Vector3(_Lenght, 0f, 0f) }, direct);
            return true;
        }
        public void ChangeDevice(in Device device)
        {
            Axle_.ChangeDevice(device);
            Label_.ChangeDevice(device);
            return;
        }
        public bool Render(in D3DX d3dx, in MonocolorShader shader, Matrix worldMatrix, Matrix viewMatrix, Matrix orthoMatrix)
        {
            Matrix transformMatrix;
            Validate(d3dx);
            transformMatrix = Matrix.RotationYawPitchRoll(_Direction.Y, _Direction.X, _Direction.Z);
            worldMatrix = Matrix.Multiply(worldMatrix, transformMatrix);

            return Axle_.Render(d3dx.DeviceContext, shader, worldMatrix, viewMatrix, orthoMatrix);
        }

        public bool RenderBanerLabel(in D3DX d3dx, Matrix worldMatrix, Matrix viewMatrix, Matrix orthoMatrix, Vector3 cameraRotation)
        {
            Validate(d3dx);
            Matrix transformMatrix;

            transformMatrix = Matrix.RotationYawPitchRoll(
                cameraRotation.Y * (float)Math.PI / 180f,
                cameraRotation.X * (float)Math.PI / 180f,
                cameraRotation.Z * (float)Math.PI / 180f);

            worldMatrix = Matrix.Multiply(worldMatrix, transformMatrix);

            //todo: direction * rotation


            return Label_.Render(d3dx, worldMatrix, viewMatrix, orthoMatrix);
        }
        public void Dispose()
        {
            Axle_?.Shutdown();
            Axle_ = null;

            Label_?.Shutdown();
            Label_ = null;
            return;
        }

        public void SetValuesRange(float min, float max, string unit)
        {
            maxValue_ = max;
            minValue_ = min;
            _Unit = unit;
            Validated = false;
            return;
        }

        public void SetTextSize(float size)
        {
            Label_.scale = size;
            return;
        }
        public void SetSupportDivision(float dv)
        {
            supportDivision_ = dv;
            Validated = false;
            return;
        }
        public void SetSupportDivisionLenght(float dvl)
        {
            supportDivisionLenght_ = dvl;
            Validated = false;
            return;
        }
        public void SetMainDivision(float dv)
        {
            mainDivision_ = dv;
            Validated = false;
            return;
        }
        public void SetMainDivisionLenght(float dvl)
        {
            mainDivisionLenght_ = dvl;
            Validated = false;
            return;
        }
        public void SetDivisionType(DIVISION_TYPE typ)
        {
            division_type_ = typ;
            Validated = false;
            return;
        }
        public bool Validate(in D3DX d3dx)
        {
            if (Validated) return true;
            
            bool result;
            switch (division_type_)
            {
                case DIVISION_TYPE.Const_Amount:
                    result = DivideCosntAmount(d3dx);
                    break;
                case DIVISION_TYPE.Const_Value:
                    result = DivideCosntValue();
                    break;
                default:
                    return false;
            }            
            TransformLabelPosition();
            Label_.SetText(d3dx, maxValue_.ToString() + _Unit);
            Validated = true;
            return result;
        }
        private void TransformLabelPosition()
        {
            var transformVectQuaternion = new Quaternion(
                _Lenght / 2f + _TextOffset.X + _Position.X,
                _TextOffset.Y + _Position.Y,
                _TextOffset.Z + _Position.Z + mainDivisionLenght_, 0.0f);
            Quaternion.RotationYawPitchRoll(-_Direction.Y, _Direction.X, _Direction.Z, out Quaternion transformQuaternionInv);
            Quaternion.Normalize(ref transformQuaternionInv, out transformQuaternionInv);
            Quaternion.Invert(ref transformQuaternionInv, out Quaternion transformQuaternion);

            var qResult = transformQuaternion * transformVectQuaternion * transformQuaternionInv;
            Label_.position = new Vector3(qResult.X, qResult.Y - Label_.Height / 2f, qResult.Z);
            return;
        }
        private bool DivideCosntAmount(in D3DX d3dx)
        {
            float maindivide, supportdivide;
            int i = 0;
            int points = 2 + (int)mainDivision_ + ((int)supportDivision_ - 1) * (int)mainDivision_; //ilość segmentów, nie podziałek. 1 główna + 2 na końcach
            bool result;
            points *= 2;
            maindivide = _Lenght / (int)mainDivision_;
            supportdivide = _Lenght / ((int)mainDivision_ * (int)supportDivision_);

            Vector3[] vects = new Vector3[points];

            vects[i] = new Vector3(0f, 0f, 0f) + _Position;
            vects[++i] = new Vector3(_Lenght, 0f, 0f) + _Position;

            vects[++i] = new Vector3(0f, 0f, 0f) + _Position;
            vects[++i] = new Vector3(0f, 0f, mainDivisionLenght_) + _Position;

            vects[++i] = new Vector3(_Lenght, 0f, 0f) + _Position;
            vects[++i] = new Vector3(_Lenght, 0f, mainDivisionLenght_) + _Position;

            for (int d = (int)supportDivision_ * (int)mainDivision_ - 1; d > 0; --d)
            {
                for (int s = (int)supportDivision_; s > 0; --d, --s)
                {
                    vects[++i] = new Vector3(d * supportdivide, 0f, 0f) + _Position;
                    vects[++i] = new Vector3(d * supportdivide, 0f, supportDivisionLenght_) + _Position;

                }
                if (d > 0)
                {
                    vects[++i] = new Vector3(d * supportdivide, 0f, 0f) + _Position;
                    vects[++i] = new Vector3(d * supportdivide, 0f, mainDivisionLenght_) + _Position;
                }
            }

            result = Axle_.SetPoints(vects, d3dx);
            return result;
        }
        private bool DivideCosntValue()
        {
            return false;
        }
    }
}
