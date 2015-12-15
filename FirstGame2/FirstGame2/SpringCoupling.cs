using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Engine;
namespace FirstGame2
{
    class SpringCoupling : ObjectCoupling
    {
        #region Private Physical Properties
        private ModelObject CoupledObject;
        private ModelObject CoupledToObject;
        private float k;
        private float l0;
        #endregion

        /// <summary>
        /// Initiate a spring coupling to an other object.
        /// Returns force based on a harmonic oscillator potential in correspondence with Hooke's law
        /// f = -kx
        /// </summary>
        /// <param name="coupledObject">The object that is coupled</param>
        /// <param name="coupleTo">The object to couple to</param>
        /// <param name="springConstant">The strength of the spring</param>
        /// <param name="naturalLength">The natural length of the spring</param>
        public SpringCoupling(ModelObject coupledObject,ModelObject coupleTo, float springConstant, float naturalLength)
        {
            CoupledObject = coupledObject;
            CoupledToObject = coupleTo;
            k = springConstant;
            l0 = naturalLength;
        }

        /// <summary>
        /// Calcultes the force vector based on the relative positions of the objects.
        /// </summary>
        /// <returns></returns>
        public Vector3 GetObjectForce()
        {
            Vector3 force0;
            //Get the direction of the coupling force
            Vector3 direction = CoupledObject.Position - CoupledToObject.Position;

            //Find the magnitude of the coupling force
            float relativeDistance;
            relativeDistance = direction.Length();
            //Normalize the direction
            direction.Normalize();

            force0 = -k * (relativeDistance - l0) * direction;
            return force0;
        }
    }
}
