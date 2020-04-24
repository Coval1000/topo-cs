using SharpDX;

namespace TopoCS
{
    class Camera
    {

        public Camera()
        {
            Vector3 pos;
            pos.X = 0.0f;
            pos.Y = 0.0f;
            pos.Z = 0.0f;
            Position = pos;

            _Rotation.X = 0.0f;
            _Rotation.Y = 0.0f;
            _Rotation.Z = 0.0f;
        }

        public void Render()
        {
            Vector3 up, position, lookAt;
            float yaw, pitch, roll;
            Matrix rotationMatrix;

            up.X = 0.0f;
            up.Y = 1.0f;
            up.Z = 0.0f;

            position = Position;

            lookAt.X = 0.0f;
            lookAt.Y = 0.0f;
            lookAt.Z = 1.0f;

            pitch = Rotation.X * 0.0174532925f;
            yaw = Rotation.Y * 0.0174532925f;
            roll = Rotation.Z * 0.0174532925f;

            rotationMatrix = Matrix.RotationYawPitchRoll(yaw, pitch, roll);

            lookAt = Vector3.TransformCoordinate(lookAt, rotationMatrix);
            up = Vector3.TransformCoordinate(up, rotationMatrix);

            lookAt = position + lookAt;

            _viewMatrix = Matrix.LookAtLH(position, lookAt, up);

            return;
        }

        public Vector3 Position { get; set; }
        public Vector3 Rotation { 
            get 
            { 
                return _Rotation; 
            }
            set
            {
                if (value.Y > 360) value.Y %= 360.0f;
                else if (value.Y < 0) value.Y = value.Y % 360f + 360f;
                _Rotation = value;
            }
        }
        private Vector3 _Rotation;
        public Matrix _viewMatrix { get; private set; }
    }
}
