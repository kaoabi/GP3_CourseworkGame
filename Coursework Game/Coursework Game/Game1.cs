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

using JigLibX.Math;
using JigLibX.Utils;
using JigLibX.Physics;
using JigLibX.Geometry;
using JigLibX.Collision;

namespace Coursework_Game
{
    

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //Font
        SpriteFont fontToUse;

        //Camera variables
        Camera m_mainCamera;

        Camera m_FirstPersonCamera;
        Camera m_ThirdPersonCamera;
        
        //Models
        private Model m_UFOCatcherBoxModel;
        private Model m_GlassModel;
        private Model m_UFOModel;
        private Model m_prize;
        private Model m_Floor;

        //GameObjects incase each one specificly needed.
        GameObject m_UFOCatcherBox;
        GameObject m_Glass;
        UFO m_UFO;
        Floor m_prizeFloor;

        List<GameObject> m_GameObjects;

        List<Prize> m_prizesList;
        //Physics world
        PhysicsSystem m_world;

        //Physics controller to move ufo with forces
        physController m_UFOController;

        //Draws physics collision skins
        DebugDrawer debugDrawer;
        DebugDrawer debugDrawerFirst;

        //Render collision skins
        bool m_PhysicsDebug;
        bool m_debugMode;

        // The aspect ratio determines how to scale 3d to 2d projection.
        private float aspectRatio;

        //The amount of money the player has which is the same as lives/trys
        private int m_Money;
        private int m_prizesObtained;
        private int m_coinsInserted;
        private bool m_coinInsertedOnceChecker;

        public e_UFOStates m_ufoState;

        public Song m_mainTheme;
        public SoundEffect m_insertCoinSE;
        public SoundEffect m_UFObeamingSE;
        public bool m_playSounds;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //Turn on and off rendering bounding boxes
            m_PhysicsDebug = false;
            m_debugMode = false;

            //Starting amount of money
            m_Money = 5;

            //The amount of prizes won
            m_prizesObtained = 0;

            //Current state of the UFO
            m_ufoState = e_UFOStates.e_freeRoam;

            //Checker to make sure only 1 coin/money is inserted at a time.
            m_coinInsertedOnceChecker = true;

            //Initalize lists
            m_GameObjects = new List<GameObject>();
            m_prizesList = new List<Prize>();

            //Turn on and off sounds
            m_playSounds = true;
        }

        //Return the camera from the game class
        public Camera getcam(){return m_mainCamera;}

        private void InitializeTransform()
        {
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            
            //Set up Third person camera
            m_ThirdPersonCamera = new Camera(new Vector3(0, 18.5f, 59.5f));
            m_ThirdPersonCamera.camRotation = new Vector3(0, 0, -0.17f);
            m_ThirdPersonCamera.camProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), aspectRatio, 0.1f, 500.0f);

            //Set up First person camera
            m_FirstPersonCamera = new Camera(new Vector3(-16, 30, 6));
            m_FirstPersonCamera.camRotation = new Vector3(0, 0.3f, -1.5f);
            m_FirstPersonCamera.camProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), aspectRatio, 0.1f, 500.0f);

            //Set main camera to the third person camera
            m_mainCamera = m_ThirdPersonCamera;
        }

        private void InitalizePhysics()
        {
            //Setting up the physics engine
            m_world = new PhysicsSystem();
            m_world.CollisionSystem = new CollisionSystemSAP();

            //Setting up gravity force and direction
            m_world.Gravity = new Vector3(0, -9.81f, 0);
        }
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            InitializeTransform();
            InitalizePhysics();

            //Set up debug drawer for rendering physics boxes.
            debugDrawer = new DebugDrawer(this, m_ThirdPersonCamera);
            debugDrawer.Enabled = true;
            Components.Add(debugDrawer);

            debugDrawerFirst = new DebugDrawer(this, m_FirstPersonCamera);
            debugDrawerFirst.Enabled = true;
            Components.Add(debugDrawerFirst);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            //Load font
            fontToUse = Content.Load<SpriteFont>("Game font");

            //Load models
            m_UFOCatcherBoxModel = Content.Load<Model>("True crane game with prize block");
            m_GlassModel = Content.Load<Model>("Glass");
            m_UFOModel = Content.Load<Model>("UFO");
            m_prize = Content.Load<Model>("Prize model");
            m_Floor = Content.Load<Model>("Floor");

            //Set up the Game objects
            m_UFOCatcherBox = new GameObject(this, new Vector3(0, 0, 0), new Vector3(0, 0, 0), m_UFOCatcherBoxModel, true,SetupEffectTransformDefaults(m_UFOCatcherBoxModel, true), 10, true);
            m_Glass = new GameObject(this, new Vector3(0, 0, 0), new Vector3(0, 0, 0), m_GlassModel, true, SetupEffectTransformDefaults(m_GlassModel, true), 10, true);
            m_UFO = new UFO(this, new Vector3(-16, 26, 6), new Vector3(0, 0, 0), m_UFOModel, true, SetupEffectTransformDefaults(m_UFOModel, true), 4, false);
            m_prizeFloor = new Floor(this, new Vector3(-12, -30, 0), new Vector3(0, 0, 0), m_Floor, true, SetupEffectTransformDefaults(m_Floor, true), 700, true);

            //Add bounding box for the UFO
            m_UFO.AddPhysicsPrimitive(new Box(new Vector3(-16, 26, 6), Matrix.Identity, new Vector3(7.0f,5.0f,7.0f)), new MaterialProperties(0.0f,0.0f,0.0f));
            //Register collision skin to check for collisions
            m_UFO.m_skin.callbackFn += (UFOCollisionDetection);
            
            //Add bounding box to check for the prizes
            m_prizeFloor.AddPhysicsPrimitive(new Box(m_prizeFloor.m_body.Position, Matrix.Identity, new Vector3(40.0f, 10.0f, 40.0f)), new MaterialProperties(0.0f, 0.4f, 0.9f));
            //Register collision skin to check for collisions
            m_prizeFloor.m_skin.callbackFn += (PrizeCollisionDetection);
            //Register prize floor in XNA component system
            Components.Add(m_prizeFloor);

            //Add objects to list in the right order from back to front
            m_GameObjects.Add(m_prizeFloor);
            m_GameObjects.Add(m_UFOCatcherBox);
            m_GameObjects.Add(m_UFO);

            //Random number generator
            Random rand = new Random();

            //Generate a prize and add it to both lists
            for (int i = 0; i < GameVariables.numberOfPrizes; i++)
            {
                Prize prize = new Prize(this, new Vector3(5 + rand.Next(20), 0, -(float)rand.Next(12)), new Vector3(0, 0, 0), m_prize, true, SetupEffectTransformDefaults(m_prize, true), 8, false);
                prize.AddPhysicsPrimitive(new Box(prize.m_body.Position, Matrix.Identity, new Vector3(4.0f, 1.5f, 7.0f)), new MaterialProperties(0.0f, 0.4f, 0.9f));
                Components.Add(prize);
                m_prizesList.Add(prize);
                m_GameObjects.Add(prize);
            }
            //Add glass
            m_GameObjects.Add(m_Glass);

            //Create collision boundarys for the walls of the box
            CreateMachineWalls();

            //Add UFO to Component system
            Components.Add(m_UFO);
            
            //Create a controller to enable forces to be applyed to a body(part of the physics engine)
            m_UFOController = new physController();
            m_UFOController.Initialize(m_UFO.m_body);

            //Load music
            m_mainTheme = Content.Load<Song>("F03");

            MediaPlayer.Play(m_mainTheme);
            MediaPlayer.IsRepeating = true;

            //Load sound effects
            m_insertCoinSE = Content.Load<SoundEffect>("Insert Coin Sound Effect");
            m_UFObeamingSE = Content.Load<SoundEffect>("reimanzeta"); 
            // TODO: use this.Content to load your game content here
        }

        private Matrix[] SetupEffectTransformDefaults(Model myModel, bool textured)
        {
            Matrix[] absoluteTransforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(absoluteTransforms);
            
            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.Projection = m_mainCamera.camProjectionMatrix;
                    effect.View = m_mainCamera.camViewMatrix;
                   
                    //checks if textured or not before applying
                    if (textured)
                    {
                        effect.TextureEnabled = true;
                    }
                    else
                    {
                        effect.TextureEnabled = false;
                    }
                }
            }
            return absoluteTransforms;
        }

        private void writeText(string msg, Vector2 msgPos, Color msgColour)
        {
            spriteBatch.Begin();
            string output = msg;
            // Find the center of the string
            Vector2 FontOrigin = fontToUse.MeasureString(output) / 2;
            Vector2 FontPos = msgPos;
            // Draw the string
            spriteBatch.DrawString(fontToUse, output, FontPos, msgColour);
            spriteBatch.End();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected void Input()
        {
            KeyboardInput();
            GamepadInput();
        }

        protected void KeyboardInput()
        {
            // TODO: Add your update logic here
            
            KeyboardState keyboardState = Keyboard.GetState();

            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            //Exit the game
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }


            if (m_debugMode == true)
            {
                if (keyboardState.IsKeyDown(Keys.A))
                {
                    //Move camera left
                    m_mainCamera.MoveCamera(new Vector3(-0.5f, 0, 0));
                }
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    //Move camera right
                    m_mainCamera.MoveCamera(new Vector3(0.5f, 0, 0));
                }
                if (keyboardState.IsKeyDown(Keys.W))
                {
                    //Move camera up
                    m_mainCamera.MoveCamera(new Vector3(0, 0.5f, 0));
                }
                if (keyboardState.IsKeyDown(Keys.S))
                {
                    //Move camera Down
                    m_mainCamera.MoveCamera(new Vector3(0, -0.5f, 0));
                }
                if (keyboardState.IsKeyDown(Keys.F))
                {
                    //Move camera Back
                    m_mainCamera.MoveCamera(new Vector3(0, 0, 0.5f));
                }
                if (keyboardState.IsKeyDown(Keys.R))
                {
                    ////Move camera forwards
                    m_mainCamera.MoveCamera(new Vector3(0, 0, -0.5f));
                }


                if (keyboardState.IsKeyDown(Keys.U))
                {
                    //Rotate camera left
                    m_mainCamera.RotateCamera(new Vector3(GameVariables.camRotationSpeed, 0, 0));
                }
                if (keyboardState.IsKeyDown(Keys.J))
                {
                    //Rotate camera right
                    m_mainCamera.RotateCamera(new Vector3(-GameVariables.camRotationSpeed, 0, 0));
                }
                if (keyboardState.IsKeyDown(Keys.O))
                {
                    //Pan camera up
                    m_mainCamera.RotateCamera(new Vector3(0, 0, GameVariables.camRotationSpeed));
                }
                if (keyboardState.IsKeyDown(Keys.L))
                {
                    //Pan camera down
                    m_mainCamera.RotateCamera(new Vector3(0, 0, -GameVariables.camRotationSpeed));
                }

                // Change camera type
                if (keyboardState.IsKeyDown(Keys.C))
                {
                    if (m_mainCamera == m_FirstPersonCamera)
                    {
                        m_mainCamera = m_ThirdPersonCamera;
                    }
                    else if (m_mainCamera == m_ThirdPersonCamera)
                    {
                        m_mainCamera = m_FirstPersonCamera;
                    }
                }

                // Render collision skins (key 0)
                if (keyboardState.IsKeyDown(Keys.D0))
                {
                    m_PhysicsDebug = !m_PhysicsDebug;
                }

                // Toggle music (key 8)
                if (keyboardState.IsKeyDown(Keys.D8))
                {
                    m_playSounds = !m_playSounds;

                    if (m_playSounds)
                    {
                        MediaPlayer.Play(m_mainTheme);
                    }
                    else if (!m_playSounds)
                    {
                        MediaPlayer.Stop();
                    }
                }
            }

            if (!gamePadState.IsConnected)
            {
                //Insert a coin/money once each time the key is pressed
                if (keyboardState.IsKeyDown(Keys.B) && m_coinInsertedOnceChecker == true)
                {
                    //If the player has money
                    if (m_Money > 0)
                    {
                        //Play sound effect and manage the money
                        if (m_playSounds)
                            m_insertCoinSE.Play();

                        m_coinsInserted++;
                        m_Money--;
                        m_coinInsertedOnceChecker = false;
                    }
                }
                else if (keyboardState.IsKeyUp(Keys.B))
                {
                    //Allow money to be inserted if the key is released
                    m_coinInsertedOnceChecker = true;
                }
            }

            if (keyboardState.IsKeyDown(Keys.D9))
            {
                m_debugMode = !m_debugMode;
            }
            
        }

        protected void GamepadInput()
        {
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            if (gamePadState.IsConnected)
            {
                //Insert a coin/money once each time the key is pressed
                if (gamePadState.Buttons.Y == ButtonState.Pressed && m_coinInsertedOnceChecker == true)
                {
                    //If the player has money
                    if (m_Money > 0)
                    {
                        //Play sound effect and manage the money
                        if (m_playSounds)
                            m_insertCoinSE.Play();

                        m_coinsInserted++;
                        m_Money--;
                        m_coinInsertedOnceChecker = false;
                    }
                }
                else if (gamePadState.Buttons.Y == ButtonState.Released)
                {
                    //Allow money to be inserted if the key is released
                    m_coinInsertedOnceChecker = true;
                }
            }
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //Handle input
            Input();

            //FSM of the UFO logic and inputs
            UFOLogic();

            //Update all game objects
            foreach(GameObject ob in m_GameObjects)
            {
                ob.Update(gameTime);
            }

            //Set First person camera to the body of the UFO if it is currently being used.
            if (m_mainCamera == m_FirstPersonCamera)
            {
                m_FirstPersonCamera.camPosition = m_UFO.m_body.Position;
            }
            
            //Update camera
            m_mainCamera.Update();

            base.Update(gameTime);

            //Update the physics engine with the time step
            float timeStep = (float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;
            m_world.Integrate(timeStep);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            //Set blend state to allow transparency
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            //Allow stencil tests
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //Iterate through the objects
            foreach (GameObject ob in m_GameObjects)
            {
                //Render the object
                ob.Render(debugDrawer, m_mainCamera);

                //Render the physics bounding boxes if turned on
                if (ob.m_skin != null && ob.m_skin.NumPrimitives > 0  && m_PhysicsDebug == true)
                {
                    //Change depending on camera used
                    if (m_mainCamera == m_ThirdPersonCamera)
                    {
                        VertexPositionColor[] skinWireframe = ob.m_skin.GetLocalSkinWireframe();
                        ob.m_body.TransformWireframe(skinWireframe);
                        debugDrawer.DrawShape(skinWireframe);
                    }
                    else if (m_mainCamera == m_FirstPersonCamera)
                    {
                        VertexPositionColor[] skinWireframe = ob.m_skin.GetLocalSkinWireframe();
                        ob.m_body.TransformWireframe(skinWireframe);
                        debugDrawerFirst.DrawShape(skinWireframe);
                    }
                }
            }

            //Display information
            writeText("Press 9 to go into Debug mode.", new Vector2(5, 5), Color.Gold);
            writeText("Remaining money: " + m_Money, new Vector2(5, 20), Color.Gold);
            writeText("Prizes obtained: " + m_prizesObtained, new Vector2(5, 35), Color.Gold);

            if (m_debugMode)
            {
                writeText("WASD to move the third person", new Vector2(360, 5), Color.Gold);
                writeText("camera UP,DOWN,LEFT,RIGHT", new Vector2(380, 20), Color.Gold);
                writeText("R/F to move FORWARDS and BACK", new Vector2(360, 35), Color.Gold);
                writeText("U/J to pan the third person ", new Vector2(360, 50), Color.Gold);
                writeText("camera left and right", new Vector2(380, 65), Color.Gold);
                writeText("O/L to pan the third person", new Vector2(360, 80), Color.Gold);
                writeText("camera up and down", new Vector2(380, 95), Color.Gold);
                writeText("0 to turn toggle physics rendering", new Vector2(360, 110), Color.Gold);
                writeText("C to swap cameras", new Vector2(360, 125), Color.Gold);
                writeText("8 to turn on and off music", new Vector2(360, 140), Color.Gold);
            }
            else
            {
                GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

                if (gamePadState.IsConnected)
                {
                    writeText("Use the DPad to move the UFO", new Vector2(360, 5), Color.Gold);
                    writeText("around when you have inserted money", new Vector2(360, 20), Color.Gold);
                    writeText("Press the Y button to insert money", new Vector2(360, 35), Color.Gold);
                    writeText("Press the A button to grab ", new Vector2(360, 50), Color.Gold);
                }
                else
                {
                    writeText("Use the arrow keys to move the UFO", new Vector2(360, 5), Color.Gold);
                    writeText("around when you have inserted money", new Vector2(360, 20), Color.Gold);
                    writeText("Press the B key to insert money", new Vector2(360, 35), Color.Gold);
                    writeText("Press space to grab ", new Vector2(360, 50), Color.Gold);
                }
            }
            
            base.Draw(gameTime);
        }

        public void UFOLogic()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            //Grabbing dropping states for logic
            if (m_ufoState == e_UFOStates.e_freeRoam)
            {
                //If there is atleast 1 coin inserted allow the player to move the UFO
                if (m_coinsInserted > 0)
                {
                    if (keyboardState.IsKeyDown(Keys.Left) || gamePadState.DPad.Left == ButtonState.Pressed)
                    {
                        m_UFOController.force0 = new Vector3(-GameVariables.ufoMoveSpeed, 0, 0);
                    }
                    else if (keyboardState.IsKeyDown(Keys.Right) || gamePadState.DPad.Right == ButtonState.Pressed)
                    {
                        m_UFOController.force0 = new Vector3(GameVariables.ufoMoveSpeed, 0, 0);
                    }
                    else if (keyboardState.IsKeyDown(Keys.Up) || gamePadState.DPad.Up == ButtonState.Pressed)
                    {
                        m_UFOController.force0 = new Vector3(0, 0, -GameVariables.ufoMoveSpeed);
                    }
                    else if (keyboardState.IsKeyDown(Keys.Down) || gamePadState.DPad.Down == ButtonState.Pressed)
                    {
                        m_UFOController.force0 = new Vector3(0, 0, GameVariables.ufoMoveSpeed);
                    }

                    //Set the UFO to grab
                    if (keyboardState.IsKeyDown(Keys.Space) || gamePadState.Buttons.A == ButtonState.Pressed)
                    {
                        //Clear forces and set all forces to 0 so the UFO doesn't move based on previous moving forces.
                        m_UFO.m_body.ClearForces();
                        m_UFO.m_body.Velocity = new Vector3(0, 0, 0);
                        m_UFOController.force0 = Vector3.Zero;

                        //Remove a try/coin
                        m_coinsInserted--;

                        //Play grabbing Sound effect
                        if (m_playSounds)
                            m_UFObeamingSE.Play();

                        //Change state to grabbing
                        m_ufoState = e_UFOStates.e_grabbing;
                    }
                }
            }
            else if (m_ufoState == e_UFOStates.e_grabbing)
            {
                m_mainCamera = m_FirstPersonCamera;
                //Move UFO down
                m_UFOController.force0 = new Vector3(0, -GameVariables.grabSpeed, 0);
            }
            else if (m_ufoState == e_UFOStates.e_moveBackUp)
            {
                //If the body of the UFO is not at/around the original position
                if (m_UFO.m_body.Position.Y < m_UFO.m_GOPos.Y)
                {
                    //Move it back up
                    m_UFOController.force0 = new Vector3(0, GameVariables.grabSpeed, 0);
                }
                //If it has 
                else if (m_UFO.m_body.Position.Y >= m_UFO.m_GOPos.Y)
                {
                    //Clear all forces again so it doesn't move higher
                    m_UFO.m_body.ClearForces();
                    m_UFO.m_body.Velocity = new Vector3(0, 0, 0);
                    m_UFOController.force0 = Vector3.Zero;
                    //Change state to move across
                    m_ufoState = e_UFOStates.e_moveBackAcross;
                }
            }
            else if (m_ufoState == e_UFOStates.e_moveBackAcross)
            {
                //Seek to the original position with slowing down algorithm based on the steering paper
                Vector3 targetOffset = m_UFO.m_GOPos - m_UFO.m_body.Position;
                float distance = targetOffset.Length();
                float rampedSpeed = GameVariables.ufoMoveSpeed * (distance/50);
                float clippedSpeed = MathHelper.Min(rampedSpeed, GameVariables.ufoMoveSpeed);
                Vector3 desiredVelocity = (clippedSpeed/distance) * targetOffset;
                Vector3 steering = desiredVelocity - m_UFO.m_body.Velocity;

                m_UFOController.force0 = steering;

                //If the UFO is within the distance from the starting point
                if (distance <= 0.5f)
                {
                    //Clear forces
                    m_UFO.m_body.ClearForces();
                    m_UFO.m_body.Velocity = new Vector3(0, 0, 0);
                    m_UFOController.force0 = Vector3.Zero;
                    //Change state to drop the prizes
                    m_ufoState = e_UFOStates.e_drop;
                }

            }
            else if (m_ufoState == e_UFOStates.e_drop)
            {
                //Drop all prizes. The ones grabbed will fall while the ones not grabbed will stay still
                foreach (Prize ob in m_prizesList)
                {
                    ob.Drop();
                }
                m_mainCamera = m_ThirdPersonCamera;
                //Allow the player to move again
                m_ufoState = e_UFOStates.e_freeRoam;
            }
        }

        //Collision function for the UFO
        public bool UFOCollisionDetection(CollisionSkin owner, CollisionSkin collidee)
        {
            //If the UFO collides with the bottom of the box
            if (collidee.Equals(m_UFOCatcherBox.m_skin) && m_ufoState == e_UFOStates.e_grabbing)
            {
                m_ufoState = e_UFOStates.e_moveBackUp;
                return true;
            }
            //If the UFO collides with a prize
            foreach( Prize ob in m_prizesList)
            {
                if( collidee.Equals(ob.m_skin))
                {
                    //Move the ufo back and 
                    m_ufoState = e_UFOStates.e_moveBackUp;
                    ob.m_UFOBody = m_UFO.m_body;
                    ob.moveToTarget = true;
                    return true;
                }
            }
            // all other collisions will be handled by physicengine
            return true;
        }

        public bool PrizeCollisionDetection(CollisionSkin owner, CollisionSkin collidee)
        {
            //If the UFO collides with the bottom of the box
            if (collidee.Equals(m_UFO.m_skin) && m_ufoState == e_UFOStates.e_grabbing)
            {
                m_ufoState = e_UFOStates.e_moveBackUp;
                return true;
            }
            // here is handled what happens if your Object collides with another special Object (= OtherObject)
            foreach (Prize ob in m_prizesList)
            {
                if (collidee.Equals(ob.m_skin))
                {
                    ob.m_body = null;
                    ob.m_skin = null;
                    m_GameObjects.Remove(ob);
                    m_prizesList.Remove(ob);
                    Components.Remove(ob);
                    m_prizesObtained++;
                    return true;
                }
            }
            // all other collisions will be handled by physicengine
            return true;
        }

        public void CreateMachineWalls()
        {
            Components.Add(m_UFOCatcherBox);

            //PlayArea
           /* //FrontWall
            m_UFOCatcherBox.m_skin.AddPrimitive(new Box(new Vector3(-36.5f, -10, 12), Matrix.Identity, new Vector3(70, 50, 0)), new MaterialProperties(0.0f, 0.0f, 0.0f));
            //BackWall
            m_UFOCatcherBox.m_skin.AddPrimitive(new Box(new Vector3(-36.5f, -10, -12), Matrix.Identity, new Vector3(70, 50, 0)), new MaterialProperties(0.0f, 0.0f, 0.0f));
            //LeftWall
            m_UFOCatcherBox.m_skin.AddPrimitive(new Box(new Vector3(-27.5f, -10, -16), Matrix.Identity, new Vector3(0, 50, 40)), new MaterialProperties(0.0f, 0.0f, 0.0f));
            //RightWall
            m_UFOCatcherBox.m_skin.AddPrimitive(new Box(new Vector3(27.5f, -10, -16), Matrix.Identity, new Vector3(0, 50, 40)), new MaterialProperties(0.0f, 0.0f, 0.0f));
            */
            
            //RightSection
            m_UFOCatcherBox.m_skin.AddPrimitive(new Box(new Vector3(-11.5f, -9, -16), Matrix.Identity, new Vector3(60, 0, 40)), new MaterialProperties(0.0f, 0.2f, 0.4f));
            //LeftSection
            m_UFOCatcherBox.m_skin.AddPrimitive(new Box(new Vector3(-29.5f, -9, -16), Matrix.Identity, new Vector3(7.5f, 0, 40)), new MaterialProperties(0.0f, 0.2f, 0.4f));
            //TopSection
            m_UFOCatcherBox.m_skin.AddPrimitive(new Box(new Vector3(-22f, -9, -20f), Matrix.Identity, new Vector3(30, 0, 20)), new MaterialProperties(0.0f, 0.2f, 0.4f));
            //BottomSection
            m_UFOCatcherBox.m_skin.AddPrimitive(new Box(new Vector3(-22f, -9, 10f), Matrix.Identity, new Vector3(30, 0, 20)), new MaterialProperties(0.0f, 0.2f, 0.4f));

            //TopHoleSection
            m_UFOCatcherBox.m_skin.AddPrimitive(new Box(new Vector3(-22f, -37.3f, 0), Matrix.Identity, new Vector3(11,30, 0)), new MaterialProperties(0.0f, 0.2f, 0.4f));
            //BottomHoleSection
            m_UFOCatcherBox.m_skin.AddPrimitive(new Box(new Vector3(-22f, -37.3f, 10), Matrix.Identity, new Vector3(11, 30, 0)), new MaterialProperties(0.0f, 0.2f, 0.4f));
            //LeftHoleSection
            m_UFOCatcherBox.m_skin.AddPrimitive(new Box(new Vector3(-22f, -37.3f, 0), Matrix.Identity, new Vector3(0, 30, 10)), new MaterialProperties(0.0f, 0.2f, 0.4f));
            //RightHoleSection
            m_UFOCatcherBox.m_skin.AddPrimitive(new Box(new Vector3(-11f, -37.3f, 0), Matrix.Identity, new Vector3(0, 30, 10)), new MaterialProperties(0.0f, 0.2f, 0.4f));

            //Glass boundarys
            Components.Add(m_Glass);
            //FrontWall
            m_Glass.m_skin.AddPrimitive(new Box(new Vector3(-36.5f, -10, 12), Matrix.Identity, new Vector3(70, 50, 0)), new MaterialProperties(0.0f, 0.0f, 0.0f));
            //BackWall
            m_Glass.m_skin.AddPrimitive(new Box(new Vector3(-36.5f, -10, -12), Matrix.Identity, new Vector3(70, 50, 0)), new MaterialProperties(0.0f, 0.0f, 0.0f));
            //LeftWall
            m_Glass.m_skin.AddPrimitive(new Box(new Vector3(-27.5f, -10, -16), Matrix.Identity, new Vector3(0, 50, 40)), new MaterialProperties(0.0f, 0.0f, 0.0f));
            //RightWall
            m_Glass.m_skin.AddPrimitive(new Box(new Vector3(27.5f, -10, -16), Matrix.Identity, new Vector3(0, 50, 40)), new MaterialProperties(0.0f, 0.0f, 0.0f));
        }
    }
}

