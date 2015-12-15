using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine
{
    public class ContactGroup
    {
        #region Fields
        List<PhysicsObject> ObjectList = new List<PhysicsObject>();
        #endregion

        #region Constructor
        public ContactGroup(PhysicsObject obj1,PhysicsObject obj2)
        {
            ObjectList.Add(obj1);
            ObjectList.Add(obj2);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds the object to the object list
        /// </summary>
        /// <param name="obj">The object to add</param>
        public void Add(PhysicsObject obj)
        {
            ObjectList.Add(obj);
        }

        /// <summary>
        /// Combines the contact groups
        /// </summary>
        /// <param name="group">The group to add to #this</param>
        public void Combine(ContactGroup group)
        {
            foreach(PhysicsObject obj in group.ObjectList)
            {
                this.Add(obj);
            }
        }
        #endregion
    }
}
