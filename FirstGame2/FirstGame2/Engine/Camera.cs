using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Engine
{
    public class Camera : MovingObject
    {
        #region Fields
        public Vector3 Target;
        public Matrix ViewMatrix;
        public Matrix ProjectionMatrix;
        public Matrix WorldMatrix = Matrix.Identity;
        #endregion

        #region Private Components
        private float nearPlane;
        private float farPlane;
        private float aspectRatio;
        #endregion

        #region Properties
        //Updating the near plane, far plane, or aspect ratio 
        //causes the projection matrix to update
        public float NearPlane
        {
            get { return nearPlane; }
            set
            {
                nearPlane = value;
                ProjectionMatrix =
                    Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                    aspectRatio, nearPlane, farPlane);
            }
        }
        public float FarPlane
        {
            get { return farPlane; }
            set
            {
                farPlane = value;
                ProjectionMatrix =
                    Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                    aspectRatio, nearPlane, farPlane);
            }
        }
        public float AspectRatio
        {
            get { return aspectRatio; }
            set
            {
                aspectRatio = value;
                ProjectionMatrix =
                    Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                    aspectRatio, nearPlane, farPlane);
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Sets up a camera, as view matrix, and a projection matrix for screen mapping
        /// </summary>
        /// <param name="_position">The position of the camera.</param>
        /// <param name="_target">What the camera is looking at.</param>
        /// <param name="_aspectRatio">The viewport aspect ratio.</param>
        /// <param name="_nearPlane">The near plane for projection transformation.</param>
        /// <param name="_farPlane">The far plane for projection transformation.</param>
        public Camera(Vector3 _position, Vector3 _target,
            float _aspectRatio, float _nearPlane, float _farPlane)
            : base(_position)
        {
            VelocityDegredation = 0.85f;
            nearPlane = _nearPlane;
            farPlane = _farPlane;
            Target = _target;
            aspectRatio = _aspectRatio;
            ViewMatrix = Matrix.CreateLookAt(Position, Target, rotationMatrix.Up);
            ProjectionMatrix = 
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 
                aspectRatio, nearPlane, farPlane);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Updates the camera's position based on its current acceleration and velocity.
        /// </summary>
        /// <param name="dt">Time since last update</param>
        public override void UpdatePosition(float dt)
        {
            //Implement a Unit to meters type conversion so you can
            //work with normal recognizable numbers
            dt *= UnitToSpaceConversion;

            //Position is updated
            Position += (Velocity * dt  + (1/2)*(Acceleration * dt*dt))*Speed;
            Velocity += Acceleration * dt;
            RotationVelocity += RotationAcceleration * dt;
            AbsoluteRotation += RotationVelocity * dt;

            AbsoluteRotation.X = MathHelper.Clamp(AbsoluteRotation.X,
                -MathHelper.PiOver2+0.01f,
                MathHelper.PiOver2-0.01f);


            rotationMatrix = Matrix.CreateFromAxisAngle(Vector3.Right, AbsoluteRotation.X)
                * Matrix.CreateFromAxisAngle(Vector3.Forward, AbsoluteRotation.Z)
                * Matrix.CreateFromAxisAngle(Vector3.Up, AbsoluteRotation.Y);

            Target = Position + rotationMatrix.Forward;
            ViewMatrix = Matrix.CreateLookAt(Position, Target, rotationMatrix.Up);

            //Add some Friction to slow down
            Velocity = Velocity * VelocityDegredation;
            RotationVelocity = RotationVelocity * RotationDegredation;
        }
        #endregion

    }
}
