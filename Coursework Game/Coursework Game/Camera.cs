using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Coursework_Game
{
    public class Camera
    {
        public Matrix camViewMatrix { get; set; } //Cameras view
        public Matrix camProjectionMatrix { get; set; }//Camera Projection Matrix
        public Matrix camRotationMatrix { get; set; } //Rotation Matrix for camera to reflect movement around Y Axis
        public Vector3 camPosition { get; set; } //Position of Camera in world
        public Vector3 camLookat { get; set; } //Where the camera is looking or pointing at
        Vector3 camTransform; //Used for repositioning the camer after it has been rotated
        public Vector3 camRotation { get; set; } //Cumulative rotation
        public Vector3 camTarget { get; set; }

        public Camera()
        {
            camPosition = new Vector3(0, 0, 0);
            camLookat = Vector3.Zero;
            camViewMatrix = Matrix.CreateLookAt(camPosition, camLookat, Vector3.Up);
            camRotation = Vector3.Zero;
        }

        public Camera(Vector3 startPosition)
        {
            camPosition = startPosition;
            camLookat = Vector3.Zero;
            camViewMatrix = Matrix.CreateLookAt(camPosition, camLookat, Vector3.Up);
            camRotation = Vector3.Zero;
        }

        public void MoveCamera(Vector3 amount)
        {
            camPosition += amount;
        }

        public void RotateCamera(Vector3 amount)
        {
            camRotation += amount;
        }

        public void Update()
        {
            camRotationMatrix = Matrix.CreateFromYawPitchRoll(camRotation.X, camRotation.Z, camRotation.Y);

            camTransform = Vector3.Transform(Vector3.Forward, camRotationMatrix);
            camLookat = camPosition + camTransform;
            camViewMatrix = Matrix.CreateLookAt(camPosition, camLookat, Vector3.Up);

        }
    }
}
