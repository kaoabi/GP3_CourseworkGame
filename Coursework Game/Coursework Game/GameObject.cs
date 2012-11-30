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
    class GameObject : DrawableGameComponent
    {
        public Vector3 m_GOPos { get; set; }
        public Vector3 m_GORot { get; set; }
        public Model m_GOModel { get; set; }
        public Matrix[] m_GOModelTransforms { get; set; }
        public float m_GOScale { get; set; }

        public Body m_body { get; set; }
        public CollisionSkin m_skin { get; set; }
        public bool m_textured { get; set; }

        public Vector3 centerOfMass { get; set; }

        public GameObject(Game game):base(game)
        {
            m_GOPos = new Vector3(0, 0, 0);
            m_GOModel = null;
            m_GOModelTransforms = null;
            m_GOScale = 1;
            m_textured = false;

            m_body = new Body();
            m_body.Immovable = true;
            m_body.Position = new Vector3(0, 0, 0);

            this.m_skin = new CollisionSkin(m_body);
            this.m_body.CollisionSkin = this.m_skin;

            this.m_body.MoveTo(this.m_GOPos, Matrix.Identity);
            m_body.EnableBody();

            ResetSkinAndMass();
        }

        public GameObject(Game game, Vector3 pos, Vector3 rot, Model model, bool textured, Matrix[] transforms, float scale , bool immovable) : base(game)
        {
            m_GOPos = pos;
            m_GORot = rot;
            m_GOModel = model;
            m_GOModelTransforms = transforms;
            m_GOScale = scale;
            m_textured = textured;
            m_body = new Body();
            m_body.Immovable = immovable;
            m_body.Position = pos;

            this.m_skin = new CollisionSkin(m_body);
            this.m_body.CollisionSkin = this.m_skin;

            this.m_body.MoveTo(this.m_GOPos, Matrix.Identity);
            m_body.EnableBody();
        }

        public void Move(Vector3 amount)
        {
            if (!m_body.IsActive)
            {
                m_body.SetActive();
            }
            
            this.m_body.MoveTo(m_body.Position+=amount, m_body.Orientation);
            ResetSkinAndMass();
        }

        public void Rotate(Vector3 amount)
        {
            m_GORot += amount;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (m_body != null)
            {
                m_body.UpdatePosition((float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond);
            }

            if (m_skin != null)
            {
                m_skin.UpdateWorldBoundingBox();
            }
        }

        public void ResetSkinAndMass()
        {
            centerOfMass = SetMass(1.0f);

            this.m_skin.ApplyLocalTransform(new Transform(-centerOfMass, Matrix.Identity));
        }

        private Vector3 SetMass(float mass)
        {
            PrimitiveProperties primitiveProperties = new PrimitiveProperties(
                PrimitiveProperties.MassDistributionEnum.Solid,
                PrimitiveProperties.MassTypeEnum.Mass, mass);

            float junk;
            Vector3 com;
            Matrix it;
            Matrix itCoM;

            m_skin.GetMassProperties(primitiveProperties, out junk, out com, out it, out itCoM);

            m_body.BodyInertia = itCoM;
            m_body.Mass = junk;

            return com;
        }

        public void AddPhysicsPrimitive(Primitive geom, MaterialProperties properties)
        {
            m_skin.AddPrimitive(geom, properties);

            ResetSkinAndMass();
        }

        public virtual void Render(DebugDrawer debugDrawer, Camera cam)
        {
            foreach (ModelMesh mesh in m_GOModel.Meshes)
            {
                //This is where the mesh orientation is set
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    if (m_body.CollisionSkin != null)
                        effect.World = m_GOModelTransforms[mesh.ParentBone.Index] * Matrix.CreateScale(m_GOScale) * m_body.CollisionSkin.GetPrimitiveLocal(0).Transform.Orientation * m_body.Orientation * Matrix.CreateFromYawPitchRoll(m_GORot.X, m_GORot.Y, m_GORot.Z) * Matrix.CreateTranslation(m_body.Position);
                    else
                        effect.World = m_GOModelTransforms[mesh.ParentBone.Index] * Matrix.CreateScale(m_GOScale) * m_body.Orientation * Matrix.CreateFromYawPitchRoll(m_GORot.X, m_GORot.Y, m_GORot.Z) * Matrix.CreateTranslation(m_body.Position);

                    effect.Projection = cam.camProjectionMatrix;
                    effect.View = cam.camViewMatrix;

                    //render fog for all models.
                    effect.SpecularPower = 2;
                    effect.FogEnabled = true;
                    effect.FogColor = Color.Black.ToVector3();
                    effect.FogStart = 70;
                    effect.FogEnd = 80;

                    if (m_textured)
                    {
                        effect.TextureEnabled = true;
                    }
                    else
                    {
                        effect.TextureEnabled = false;
                    }

                }

                //Draw the mesh, will use the effects set above.
                mesh.Draw();
            }
        }
    }
}
