using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Engine
{
    public interface ObjectCoupling
    {
        /// <summary>
        /// Gets the force on an object due to the coupling
        /// </summary>
        /// <returns>Returns the force as a 3 dimensional Catesian Vector</returns>
        Vector3 GetObjectForce();

    }
}
