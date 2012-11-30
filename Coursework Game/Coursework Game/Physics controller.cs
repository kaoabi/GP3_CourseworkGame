using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using JigLibX.Physics;
using JigLibX.Geometry;
using JigLibX.Math;
using JigLibX.Utils;

namespace Coursework_Game
{
    //Controller to control physics on a body. Taken from the JiglibX wiki
    public class physController : Controller
    {
        private Body body0;
        public Vector3 force0 = new Vector3();
        public Vector3 torque0 = new Vector3();

        public physController()
        {
        }

        public void Initialize(Body body0)
        {
            EnableController();
            this.body0 = body0;
        }

        public override void UpdateController(float dt)
        {
            if (body0 == null)
                return;

            if (force0 != null && force0 != Vector3.Zero)
            {
                body0.AddWorldForce(force0);
                if (!body0.IsActive)
                    body0.SetActive();
            }
            if (torque0 != null && torque0 != Vector3.Zero)
            {
                body0.AddBodyTorque(torque0);
                if (!body0.IsActive)
                    body0.SetActive();
            }
        }
    }
}