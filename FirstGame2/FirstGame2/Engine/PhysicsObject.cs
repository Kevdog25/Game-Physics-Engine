using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class PhysicsObject
    {
        #region Fields
        public bool IsMovable;
        public ContactGroup ContactGroup = null;
        public List<ContactPartner> ContactPartners;
        public List<ObjectCoupling> Couplings;

        #region Bounds
        public float xLeft;
        public float xRight;
        public float yLeft;
        public float yRight;
        public float zLeft;
        public float zRight;
        #endregion
        #endregion

        public bool isContacting(PhysicsObject obj)
        {
            foreach (ContactPartner contactingObj in ContactPartners)
            {
                if (contactingObj.Partner.Equals(obj))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Updates the position of the object
        /// does nothing if the object is static
        /// </summary>
        /// <param name="dt">Update time</param>
        public virtual void UpdatePosition(float dt) { }

        /// <summary>
        /// Updates the velocity of the object
        /// does nothing if the object is static
        /// </summary>
        /// <param name="dt"></param>
        public virtual void UpdateVelocity(float dt) { }
    }
}
