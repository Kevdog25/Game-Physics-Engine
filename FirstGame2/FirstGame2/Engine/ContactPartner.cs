using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Engine
{
    public class ContactPartner
    {
        public Vector3 RelativeContactPoint;
        public PhysicsObject Partner;

        public ContactPartner(PhysicsObject partner,Vector3 relative)
        {
            Partner = partner;
            RelativeContactPoint = relative;
        }
    }
}
