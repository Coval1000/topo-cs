using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopoCS
{
    class Position
    {
        public bool orbit;

        public Position()
        {
            orbit = false;
            _frameTime = 0.0f;

            _positionX = 0.0f;
            _positionY = 0.0f;
            _positionZ = 0.0f;

            _rotationX = 0.0f;
            _rotationY = 0.0f;
            _rotationZ = 0.0f;

            _forwardSpeed = 0.0f;
            _backwardSpeed = 0.0f;
            _upwardSpeed = 0.0f;
            _downwardSpeed = 0.0f;
            _leftTurnSpeed = 0.0f;
            _rightTurnSpeed = 0.0f;
            _lookUpSpeed = 0.0f;
            _lookDownSpeed = 0.0f;
            _leftSpeed = 0.0f;
            _rightSpeed = 0.0f;
            _moveSpeed = 0.0f;
            _offX = 0.0f;
            _offY = 0.0f;
            _offZ = 0.0f;
        }

        public void SetFrameTime(float time)
        {
            _frameTime = time;
            return;
        }
        public void GetRotation(out float y)
        {
            y = _rotationY;
            return;
        }
        public void SetPosition(float x, float y, float z)
        {
            _positionX = x + _offX;
            _positionY = y + _offY;
            _positionZ = z + _offZ;
            if (orbit) Sphere();
            return;
        }
        public void SetRotation(float x, float y, float z)
        {
            _rotationX = x;
            _rotationY = y;
            _rotationZ = z;
            if (orbit) Sphere();
            return;
        }
        public void SetOffSet(float x, float y, float z)
        {
            _positionX += x - _offX;
            _positionY += y - _offY;
            _positionZ += z - _offZ;
            _offX = x;
            _offY = y;
            _offZ = z;
            if (orbit) Sphere();
            return;
        }
        public void SetRadious(float r)
        {
            _radious = r;
            if (orbit) Sphere();
            return;
        }

        public void GetPosition(out float x, out float y, out float z)
        {
            x = _positionX;
            y = _positionY;
            z = _positionZ;
            return;
        }
        public void GetRotation(out float x, out float y, out float z)
        {
            x = _rotationX;
            y = _rotationY;
            z = _rotationZ;
            return;
        }
        public void GetOffSet(out float x, out float y, out float z)
        {
            x = _offX;
            y = _offY;
            z = _offZ;
            return;
        }
        public float GetRadious()
        {
            return _radious;
        }

        public void MoveOffSet(float x, float y, float z)
        {
            _offX = x;
            _offY = y;
            _offZ = z;
            _radious = (float)Math.Sqrt(Math.Pow(_positionX - _offX, 2) + Math.Pow(_positionY - _offY, 2) + Math.Pow(_positionZ - _offZ, 2));
            if (orbit) SpinCamera();
            return;
        }

        public void IncRadious(float dR)
        {
            _radious -= dR * 0.05f;
            Sphere();
            return;
        }
        public void OrbitCamera(int x, int y)
        {
            float speed = 0.2f;
            _rotationY += speed * x;
            _rotationX += speed * y;
            if (_rotationX > 90.0f) _rotationX = 90.0f;
            else if (_rotationX < -15.0f) _rotationX = -15.0f;
            Sphere();
            return;
        }

        public void MoveForward(bool keydown)
        {
            float radians;


            // Update the forward speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                _forwardSpeed += _frameTime * 0.001f;

                if (_forwardSpeed > (_frameTime * 0.03f))
                {
                    _forwardSpeed = _frameTime * 0.03f;
                }
            }
            else
            {
                _forwardSpeed -= _frameTime * 0.0007f;

                if (_forwardSpeed < 0.0f)
                {
                    _forwardSpeed = 0.0f;
                }
            }

            // Convert degrees to radians.
            radians = _rotationY * 0.0174532925f;

            // Update the position.
            _positionX += (float)Math.Sin(radians) * _forwardSpeed;
            _positionZ += (float)Math.Cos(radians) * _forwardSpeed;

            return;
        }
        public void MoveBackward(bool keydown)
        {
            float radians;


            // Update the backward speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                _backwardSpeed += _frameTime * 0.001f;

                if (_backwardSpeed > (_frameTime * 0.03f))
                {
                    _backwardSpeed = _frameTime * 0.03f;
                }
            }
            else
            {
                _backwardSpeed -= _frameTime * 0.0007f;

                if (_backwardSpeed < 0.0f)
                {
                    _backwardSpeed = 0.0f;
                }
            }

            // Convert degrees to radians.
            radians = _rotationY * 0.0174532925f;

            // Update the position.
            _positionX -= (float)Math.Sin(radians) * _backwardSpeed;
            _positionZ -= (float)Math.Cos(radians) * _backwardSpeed;

            return;
        }
        public void MoveForwBack(int val)
        {
            float radians, radiansPitch;

            if (val != 0)
            {
                _moveSpeed += val * _frameTime * 0.002f;
            }
            else
            {
                _moveSpeed = 0.0f;
            }

            radiansPitch = _rotationX * 0.0174532925f;
            _positionY -= (float)Math.Sin(radiansPitch) * _moveSpeed;

            radians = _rotationY * 0.0174532925f;
            _positionX += (float)Math.Cos(radiansPitch) * (float)Math.Sin(radians) * _moveSpeed;
            _positionZ += (float)Math.Cos(radiansPitch) * (float)Math.Cos(radians) * _moveSpeed;

            return;
        }
        public void MoveUpward(bool keydown)
        {
            // Update the upward speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                _upwardSpeed += _frameTime * 0.003f;

                if (_upwardSpeed > (_frameTime * 0.03f))
                {
                    _upwardSpeed = _frameTime * 0.03f;
                }
            }
            else
            {
                _upwardSpeed -= _frameTime * 0.002f;

                if (_upwardSpeed < 0.0f)
                {
                    _upwardSpeed = 0.0f;
                }
            }

            // Update the height position.
            _positionY += _upwardSpeed;

            return;
        }
        public void MoveDownward(bool keydown)
        {
            // Update the downward speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                _downwardSpeed += _frameTime * 0.003f;

                if (_downwardSpeed > (_frameTime * 0.03f))
                {
                    _downwardSpeed = _frameTime * 0.03f;
                }
            }
            else
            {
                _downwardSpeed -= _frameTime * 0.002f;

                if (_downwardSpeed < 0.0f)
                {
                    _downwardSpeed = 0.0f;
                }
            }

            // Update the height position.
            _positionY -= _downwardSpeed;

            return;
        }
        public void MoveLeft(bool keydown)
        {
            float radians;


            // Update the backward speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                _leftSpeed += _frameTime * 0.001f;

                if (_leftSpeed > (_frameTime * 0.03f))
                {
                    _leftSpeed = _frameTime * 0.03f;
                }
            }
            else
            {
                _leftSpeed -= _frameTime * 0.0007f;

                if (_leftSpeed < 0.0f)
                {
                    _leftSpeed = 0.0f;
                }
            }

            // Convert degrees to radians.
            radians = _rotationY * 0.0174532925f;

            // Update the position.
            _positionX -= (float)Math.Cos(radians) * _leftSpeed;
            _positionZ += (float)Math.Sin(radians) * _leftSpeed;

            return;
        }
        public void MoveRight(bool keydown)
        {
            float radians;


            // Update the backward speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                _rightSpeed += _frameTime * 0.001f;

                if (_rightSpeed > (_frameTime * 0.03f))
                {
                    _rightSpeed = _frameTime * 0.03f;
                }
            }
            else
            {
                _rightSpeed -= _frameTime * 0.0007f;

                if (_rightSpeed < 0.0f)
                {
                    _rightSpeed = 0.0f;
                }
            }

            // Convert degrees to radians.
            radians = _rotationY * 0.0174532925f;

            // Update the position.
            _positionX += (float)Math.Cos(radians) * _rightSpeed;
            _positionZ -= (float)Math.Sin(radians) * _rightSpeed;

            return;
        }
        public void TurnLeft(bool keydown)
        {
            // Update the left turn speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                _leftTurnSpeed += _frameTime * 0.01f;

                if (_leftTurnSpeed > (_frameTime * 0.15f))
                {
                    _leftTurnSpeed = _frameTime * 0.15f;
                }
            }
            else
            {
                _leftTurnSpeed -= _frameTime * 0.005f;

                if (_leftTurnSpeed < 0.0f)
                {
                    _leftTurnSpeed = 0.0f;
                }
            }

            // Update the rotation.
            _rotationY -= _leftTurnSpeed;

            // Keep the rotation in the 0 to 360 range.
            if (_rotationY < 0.0f)
            {
                _rotationY += 360.0f;
            }

            return;
        }
        public void TurnRight(bool keydown)
        {
            // Update the right turn speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                _rightTurnSpeed += _frameTime * 0.01f;

                if (_rightTurnSpeed > (_frameTime * 0.15f))
                {
                    _rightTurnSpeed = _frameTime * 0.15f;
                }
            }
            else
            {
                _rightTurnSpeed -= _frameTime * 0.005f;

                if (_rightTurnSpeed < 0.0f)
                {
                    _rightTurnSpeed = 0.0f;
                }
            }

            // Update the rotation.
            _rotationY += _rightTurnSpeed;

            // Keep the rotation in the 0 to 360 range.
            if (_rotationY > 360.0f)
            {
                _rotationY -= 360.0f;
            }

            return;
        }
        public void LookUpward(bool keydown)
        {
            // Update the upward rotation speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                _lookUpSpeed += _frameTime * 0.01f;

                if (_lookUpSpeed > (_frameTime * 0.15f))
                {
                    _lookUpSpeed = _frameTime * 0.15f;
                }
            }
            else
            {
                _lookUpSpeed -= _frameTime * 0.005f;

                if (_lookUpSpeed < 0.0f)
                {
                    _lookUpSpeed = 0.0f;
                }
            }

            // Update the rotation.
            _rotationX -= _lookUpSpeed;

            // Keep the rotation maximum 90 degrees.
            if (_rotationX < -90.0f)
            {
                _rotationX = -90.0f;
            }

            return;
        }
        public void LookDownward(bool keydown)
        {
            // Update the downward rotation speed movement based on the frame time and whether the user is holding the key down or not.
            if (keydown)
            {
                _lookDownSpeed += _frameTime * 0.01f;

                if (_lookDownSpeed > (_frameTime * 0.15f))
                {
                    _lookDownSpeed = _frameTime * 0.15f;
                }
            }
            else
            {
                _lookDownSpeed -= _frameTime * 0.005f;

                if (_lookDownSpeed < 0.0f)
                {
                    _lookDownSpeed = 0.0f;
                }
            }

            // Update the rotation.
            _rotationX += _lookDownSpeed;

            // Keep the rotation maximum 90 degrees.
            if (_rotationX > 90.0f)
            {
                _rotationX = 90.0f;
            }

            return;
        }
        public void SpinCamera()
        {
            double r;
            float x, z;
            x = _positionX - _offX;
            z = _positionZ - _offZ;
            r = (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(z, 2));
            _rotationY = (float)(Math.Atan2(x, z) * 180.0 / Math.PI + 180.0);
            _rotationX = (float)(Math.Atan2(_positionY - _offY, r) * 180.0 / Math.PI);
            return;
        }

        private void Sphere()
        {
            _positionX = _offX - _radious * (float)Math.Cos((-_rotationY + 90) * (Math.PI / 180.0)) * (float)Math.Cos(_rotationX * (Math.PI / 180.0));
            _positionY = _offY + _radious * (float)Math.Sin(_rotationX * (Math.PI / 180.0));
            _positionZ = _offZ - _radious * (float)Math.Sin((-_rotationY + 90) * (Math.PI / 180.0)) * (float)Math.Cos(_rotationX * (Math.PI / 180.0));
            return;
        }

        private float _frameTime;
        private float _radious;

        private float _offX, _offY, _offZ;
        private float _positionX, _positionY, _positionZ;
        private float _rotationX, _rotationY, _rotationZ;

        private float _leftSpeed, _rightSpeed;
        private float _forwardSpeed, _backwardSpeed;
        private float _upwardSpeed, _downwardSpeed;
        private float _leftTurnSpeed, _rightTurnSpeed;
        private float _lookUpSpeed, _lookDownSpeed;
        private float _moveSpeed;
    }
}
