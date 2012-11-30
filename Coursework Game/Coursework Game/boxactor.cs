using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using JigLibX.Physics;
using JigLibX.Collision;
using JigLibX.Geometry;
using JigLibX.Math;
using Microsoft.Xna.Framework.Graphics;

namespace Coursework_Game
{
    public class BoxActor : DrawableGameComponent
    {
        private Vector3 position, scale;

        public Model model { get; set; }

        public Body Body { get; private set; }
        public CollisionSkin Skin { get; private set; }

        public BoxActor(Game game, Vector3 position, Vector3 scale)
            : base(game)
        {
            this.position = position;
            this.scale = scale;

            this.Body = new Body();
            this.Skin = new CollisionSkin(this.Body);

            this.Body.CollisionSkin = this.Skin;

            Box box = new Box(Vector3.Zero, Matrix.Identity, scale);
            this.Skin.AddPrimitive(box, new MaterialProperties(
                0.8f, // elasticity
                0.8f, // static roughness
                0.7f  // dynamic roughness
                ));

            Vector3 com = SetMass(1.0f);

            this.Body.MoveTo(this.position, Matrix.Identity);

            this.Skin.ApplyLocalTransform(new Transform(-com, Matrix.Identity));
            this.Body.EnableBody();
        }

        private Vector3 SetMass(float mass)
        {
            PrimitiveProperties primitiveProperties = new PrimitiveProperties(
                PrimitiveProperties.MassDistributionEnum.Solid,
                PrimitiveProperties.MassTypeEnum.Mass,
                mass);

            float junk;
            Vector3 com;
            Matrix it, itCom;

            this.Skin.GetMassProperties(primitiveProperties, out junk, out com, out it, out itCom);

            this.Body.BodyInertia = itCom;
            this.Body.Mass = junk;

            return com;
        }

        protected override void LoadContent()
        {
            this.model = Game.Content.Load<Model>("UFO");
        }

        private Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(scale) * this.Skin.GetPrimitiveLocal(0).Transform.Orientation * this.Body.Orientation * Matrix.CreateTranslation(this.Body.Position);
        }

        public override void Draw(GameTime gameTime)
        {
            Game1 game = (Game1)this.Game;

            Matrix[] transforms = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(transforms);

            Matrix worldMatrix = this.GetWorldMatrix();

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = game.getcam().camViewMatrix;
                    effect.Projection = game.getcam().camProjectionMatrix;
                }
                mesh.Draw();
            }
        }
    }
}