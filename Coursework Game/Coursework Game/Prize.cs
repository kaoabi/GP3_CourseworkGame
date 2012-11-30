using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using JigLibX.Physics;
using JigLibX.Geometry;
using JigLibX.Collision;
using JigLibX.Math;

namespace Coursework_Game
{
    //Extended gameobject for the prize
    class Prize : GameObject
    {
        //Controller to allow this prize to float towards the UFO
        physController m_controller;
        //The body of the UFO to follow
        public Body m_UFOBody { get; set; }
        //Bool to see if the prize should go to the prize
        public bool moveToTarget { get; set; }

        public Prize(Game game)
            : base(game)
        {
            moveToTarget = false;
        }

        public Prize(Game game, Vector3 pos, Vector3 rot, Model model, bool textured, Matrix[] transforms, float scale, bool immovable)
            : base(game, pos, rot, model, textured, transforms, scale, immovable)
        {
            m_body.ApplyGravity = true;
            m_controller = new physController();
            m_controller.Initialize(m_body);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (moveToTarget)
            {
                //Same seeking code from the paper. Same as the one in the game class.
                Vector3 targetOffset = m_UFOBody.Position - m_body.Position + new Vector3(0,-5,0);
                float distance = targetOffset.Length();
                float rampedSpeed = GameVariables.beamForce * (distance / 10);
                float clippedSpeed = MathHelper.Min(rampedSpeed, GameVariables.beamForce);
                Vector3 desiredVelocity = (clippedSpeed/distance) * targetOffset;
                Vector3 steering = desiredVelocity - m_body.Velocity;

                m_controller.force0 = steering;
            }
        }

        public void Drop()
        {
            //Reset forces and stop it following the UFO
            m_body.ClearForces();
            m_body.Velocity = new Vector3(0, 0, 0);
            m_controller.force0 = new Vector3(0, 0, 0);
            moveToTarget = false;
        }
    }
}
