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
    class UFO : GameObject
    {
        public UFO(Game game) : base(game)
        {

        }

        public UFO(Game game, Vector3 pos, Vector3 rot, Model model, bool textured, Matrix[] transforms, float scale, bool immovable) : base(game, pos,  rot,  model, textured, transforms, scale, immovable)
        {
            m_body.ApplyGravity = false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            m_body.SetOrientation(Matrix.Identity);

            RotateUFO();
        }

        public void RotateUFO()
        {
            Rotate(new Vector3(MathHelper.ToRadians(2), 0, 0));
        }
    }
}
