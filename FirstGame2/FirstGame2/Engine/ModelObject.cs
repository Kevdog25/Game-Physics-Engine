using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Engine
{
    public class ModelObject : MovingObject
    {
        #region Fields
        public Model Model;
        public float Radius;
        public float ElapsedTime = 0;
        public int ObjectID;
        #endregion

        #region Private Attributes
        private float velocityBoundScale = 100/1000.0f;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a movable object
        /// </summary>
        /// <param name="_position">Position of object in world space</param>
        /// <param name="mod">Model to place at position</param>
        public ModelObject(Vector3 _position,Model mod,float mass) : base(_position)
        {
            Model = mod;
            Mass = mass;
            BoundingSphere = mod.Meshes[0].BoundingSphere;
            Radius = BoundingSphere.Radius;
            SetBounds();
        }
        #endregion

        #region Public Methods
        public override void UpdatePosition(float dt)
        {
            SetBounds();
            base.UpdatePosition(dt);
            ElapsedTime += dt;
        }

        /// <summary>
        /// Draws the object to the world
        /// </summary>
        /// <param name="camera">Camera to draw to</param>
        public override void Draw(Camera camera)
        {
            Matrix[] transforms = new Matrix[Model.Bones.Count];
            Model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    effect.World = camera.WorldMatrix 
                        * transforms[mesh.ParentBone.Index]
                        * Matrix.CreateTranslation(Position);
                    effect.View = camera.ViewMatrix;
                    effect.Projection = camera.ProjectionMatrix;
                }
                mesh.Draw();
            }
        }
        #endregion

        #region Private Methods
        private void SetBounds()
        {
            //for debugging
            float scale = 1f;
            //^
            this.xLeft = Position.X - (Radius + scale * Math.Abs(Velocity.X * velocityBoundScale));
            this.xRight = Position.X + (Radius + scale * Math.Abs(Velocity.X * velocityBoundScale));
            this.yLeft = Position.Y - (Radius + scale * Math.Abs(Velocity.Y * velocityBoundScale));
            this.yRight = Position.Y + (Radius + scale * Math.Abs(Velocity.Y * velocityBoundScale));
            this.zLeft = Position.Z - (Radius + scale * Math.Abs(Velocity.Z * velocityBoundScale));
            this.zRight = Position.Z + (Radius + scale * Math.Abs(Velocity.Z * velocityBoundScale));
        }
        #endregion
    }
}
