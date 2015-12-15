using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Engine
{
    public abstract class MovingObject : PhysicsObject
    {
        #region Fields
        public Matrix rotationMatrix;

        public Vector3 Acceleration;
        public Vector3 PreviousAccel;
        public Vector3 Velocity;
        public Vector3 Position;
        public Vector3 RotationAcceleration;
        public Vector3 RotationVelocity;
        public Vector3 Rotation;
        public Vector3 AbsoluteRotation;
        public float Speed;
        public float Mass;
        public float Charge;
        public BoundingSphere BoundingSphere;

        #endregion

        #region Protected
        protected float VelocityDegredation = 0.98f;
        protected const float UnitToSpaceConversion = 1/1000f;
        protected const float RotationDegredation = 0.5f;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates an object with moving functionalities
        /// </summary>
        /// <param name="_position">Position of the object in world space</param>
        public MovingObject(Vector3 _position)
        {
            ContactPartners = new List<ContactPartner>();
            Couplings = new List<ObjectCoupling>();
            IsMovable = true;
            Speed = 1f;
            Position = _position;
            rotationMatrix = Matrix.Identity;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Evolves a generic object for time dt
        /// Updates: this
        /// Requires: dt less than this.FrameTime
        /// </summary>
        /// <param name="gameTime">GameTime for physics</param>
        public override void UpdatePosition(float dt)
        {
            //Implement a Unit to meters type conversion so you can
            //work with normal recognizable numbers
            dt *= UnitToSpaceConversion;
            double realDt = dt;

            //Position is updated with the initial velocity
            //realDt is a double value that improves the precision of the calculations.
            //If float is used, then the error builds
            Position += (Velocity * dt + (1.0f / 2.0f) * (Acceleration * (float)(realDt * realDt)));

            //Copy the current accel to the previous accel (used for velocity updating)
            PreviousAccel = Acceleration;

            RotationVelocity += RotationAcceleration * dt;
            AbsoluteRotation += RotationVelocity * dt;

            //Add some Friction to slow down
            //Velocity = Velocity * VelocityDegredation;
            RotationVelocity = RotationVelocity * RotationDegredation;

            rotationMatrix = Matrix.CreateFromAxisAngle(Vector3.Right, AbsoluteRotation.X)
                * Matrix.CreateFromAxisAngle(Vector3.Forward, AbsoluteRotation.Z)
                * Matrix.CreateFromAxisAngle(Vector3.Up, AbsoluteRotation.Y);
            BoundingSphere.Center = Position;
            
        }

        /// <summary>
        /// Updates the velocity by 'integrating' the acceleration
        /// Updates: this.Velocity
        /// </summary>
        /// <param name="dt">The small interval over which to integrate</param>
        public override void UpdateVelocity(float dt)
        {
            Velocity += (PreviousAccel + Acceleration) * dt / 2000.0f;
        }

        public virtual void Draw(Camera camera)
        {
        }

        #endregion
    }
}
