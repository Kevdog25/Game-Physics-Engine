using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    /*
     * Class to organize lists of objects by their lower (or upper) bound.
     * Useful to check to see if an object is contained in the entire collision chain region.
     */
    public class CollisionChain
    {
        #region Fields
        public float LeadingEdge;
        public float TrailingEdge;
        public List<PhysicsObject> Chain;
        public int Length;
        #endregion


        /// <summary>
        /// Constructs a collision chain with trailing and leading edges.
        /// #Requires an object, and bounds to make sense.
        /// </summary>
        /// <param name="obj">Object to initialize it with</param>
        /// <param name="trailing">Trailing Edge of object bounds</param>
        /// <param name="leading">Leading Edge of the object bounds</param>
        public CollisionChain(PhysicsObject obj, float trailing, float leading)
        {
            TrailingEdge = trailing;
            LeadingEdge = leading;
            Chain = new List<PhysicsObject>();
            Chain.Add(obj);
            Length = 1;
        }

        #region Public Methods
        /// <summary>
        /// Adds an object to the collision chain.
        /// Updates: this.LeadingEdge
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="edge">Leading Edge of the object bounds.</param>
        public void AddFront(PhysicsObject obj, float edge)
        {
            if (edge > LeadingEdge)
            {
                LeadingEdge = edge;
            }
            Chain.Add(obj);
            Length++;
        }

        /// <summary>
        /// Adds an object to the back of a collision chain.
        /// Updates: this.TrailingEdge
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="edge">Trailing Edge of the object bounds.</param>
        public void AddBack(PhysicsObject obj, float edge)
        {
            if (edge < TrailingEdge)
            {
                TrailingEdge = edge;
            }
            Chain.Add(obj);
            Length++;
        }
        #endregion


        #region Comparisons
        /// <summary>
        /// Compare two objects by their yLeft bound
        /// </summary>
        /// <param name="obj1">obj 1</param>
        /// <param name="obj2">obj 2</param>
        /// <returns>returns -1 if obj2 > obj1 and 1 else</returns>
        public static int CompareObjectsByYLeft(PhysicsObject obj1, PhysicsObject obj2)
        {
            if (obj1.yLeft < obj2.yLeft)
            {
                return -1;
            }
            else if (obj1.yLeft > obj2.yLeft)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Compare two objects by their xLeft bound
        /// </summary>
        /// <param name="obj1">obj 1</param>
        /// <param name="obj2">obj 2</param>
        /// <returns>returns -1 if obj2 > obj1 and 1 else</returns>
        public static int CompareObjectsByZLeft(PhysicsObject obj1, PhysicsObject obj2)
        {
            if (obj1.zLeft < obj2.zLeft)
            {
                return -1;
            }
            else if(obj1.zLeft > obj2.zLeft)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        #endregion
    }
}
