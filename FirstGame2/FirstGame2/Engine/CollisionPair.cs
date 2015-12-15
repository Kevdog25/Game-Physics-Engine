using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    class CollisionPair : IComparable<CollisionPair>
    {
        public ModelObject obj1;
        public PhysicsObject obj2;
        public double timeTillCollision;

        #region Constructor
        public CollisionPair(ModelObject inObj1,PhysicsObject inObj2,double inTime)
        {
            obj1 = inObj1;
            obj2 = inObj2;
            timeTillCollision = inTime;
        }
        #endregion

        #region Public Methods
        public int CompareTo(CollisionPair obj)
        {
            if (timeTillCollision < obj.timeTillCollision)
            {
                return 1;
            }
            else if (timeTillCollision > obj.timeTillCollision)
            {
                return -1;
            }

            return 0;
        }
        #endregion
    }
}
