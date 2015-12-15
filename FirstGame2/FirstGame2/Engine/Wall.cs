using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

namespace Engine
{
    public class Wall : PhysicsObject
    {
        #region Fields
        public Vector3 Normal;
        public Vector3 Center;
        public Vector3 U;
        public Vector3 V;
        public Vector3[] VertexList;
        public VertexPositionColor[] DrawingVertecies;
        public Int16[] DrawingIndecies;
        public Model Model = null;
        public Texture2D Texture = null;
        public Color Color;
        public bool Draw = false;
        #endregion


        #region Constructor for Invisible
        /// <summary>
        /// Creates a rectangular wall with a color based on dimensions, center and normal
        /// Makes one without a texture
        /// </summary>
        /// <param name="center">Center of the wall</param>
        /// <param name="width">Width of the wall. Axis given by u</param>
        /// <param name="height">Height of the wall. Axis given by normal X right</param>
        /// <param name="normal">The normal to the wall</param>
        /// <param name="u">Direction associated with width</param>
        public Wall(Vector3 center, float width, float height,Vector3 normal,Vector3 u)
        {
            //Initialize contact patners
            ContactPartners = new List<ContactPartner>();
            //Set it to immovable
            IsMovable = false;
            //Set the vertecies
            Center = center;
            U = u;
            V = Vector3.Cross(normal,u);
            Normal = normal;
            VertexList = new Vector3[4];
            VertexList[0] = center - width / 2 * u + height / 2 * V;
            VertexList[1] = center + width / 2 * u + height / 2 * V;
            VertexList[2] = center + width / 2 * u - height / 2 * V;
            VertexList[3] = center - width / 2 * u - height / 2 * V;

            //Set the boundaries for collision detection
            //No need to be a method call, since the object is immutable atm.
            #region Boundary Setting
            xRight = Center.X;
            for (int i = 0; i < VertexList.Count(); i++)
            {
                xRight = MathHelper.Max(xRight,VertexList[i].X);
            }
            xLeft = Center.X;
            for (int i = 0; i < VertexList.Count(); i++)
            {
                xLeft = MathHelper.Min(xLeft, VertexList[i].X);
            }
            yRight = Center.Y;
            for (int i = 0; i < VertexList.Count(); i++)
            {
                yRight = MathHelper.Max(yRight, VertexList[i].Y);
            }
            yLeft = Center.Y;
            for (int i = 0; i < VertexList.Count(); i++)
            {
                yLeft = MathHelper.Min(yLeft, VertexList[i].Y);
            }
            zRight = Center.Z;
            for (int i = 0; i < VertexList.Count(); i++)
            {
                zRight = MathHelper.Max(zRight, VertexList[i].Z);
            }
            zLeft = Center.Z;
            for (int i = 0; i < VertexList.Count(); i++)
            {
                zLeft = MathHelper.Min(zLeft, VertexList[i].Z);
            }
            #endregion
        }
        #endregion

        #region Constructor With Color
        /// <summary>
        /// Constructs a wall with a given color
        /// </summary>
        /// <param name="center">The center of the wall</param>
        /// <param name="color">The color of the wall</param>
        /// <param name="height">The height of the wall along the axis normal X u</param>
        /// <param name="normal">The normal to the wall</param>
        /// <param name="u">The direction associated with width</param>
        /// <param name="width">The width along the u-axis</param>
        public Wall(Vector3 center, float width, float height, Vector3 normal,Vector3 u,Color color)
            : this(center,width,height,normal,u)
        {
            Draw = true;
            setDrawingVertecies();
        }
        #endregion

        #region Public Methods
        private void setDrawingVertecies()
        {
            DrawingVertecies = new VertexPositionColor[4];
            DrawingIndecies = new Int16[6];
            for (int i = 0; i < VertexList.Length; i++)
            {
                DrawingVertecies[i].Position = VertexList[i];
                DrawingVertecies[i].Color = Color;
            }
            DrawingIndecies[0] = 0;
            DrawingIndecies[1] = 1;
            DrawingIndecies[2] = 2;
            DrawingIndecies[3] = 0;
            DrawingIndecies[4] = 2;
            DrawingIndecies[5] = 3;
        }

        /// <summary>
        /// Returns the shortest distance from the point to the plane
        /// </summary>
        /// <param name="point">Point in 3-space to calculate the distance to</param>
        /// <returns>Returns a float that is the distance to the plane</returns>
        public float GetShortestDist(Vector3 point)
        {
            return Math.Abs(Vector3.Dot(point-Center,Normal));
        }
        #endregion
    }
}
