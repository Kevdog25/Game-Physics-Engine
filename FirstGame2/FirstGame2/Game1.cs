using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Engine;

namespace FirstGame2
{

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        EngineHead engine;
        SpriteFont Font;
        ModelObject Ball;
        ModelObject Ball2;
        List<Wall> Walls;
        VertexPositionColor[] VertextList;
        KeyboardState oldKeyboard;
        KeyboardState keyboard;
        ModelObject CameraModel;
        Camera Camera;

        SoundEffect CollisionSoundEffect;

        List<ModelObject> ListOfObjects;

        private float totalTime = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Camera = EngineHead.Start(this);
            engine = EngineHead.Instance;

            Walls = new List<Wall>();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            CollisionSoundEffect = this.Content.Load<SoundEffect>("Sounds/boing_spring");
            ListOfObjects = new List<ModelObject>();
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //this.IsFixedTimeStep = false;

            //PlaceObjectsRandomly();
            //PlaceObjectsDeliberately();
            PlaceObjectsInOrbit();
            //PlaceObjectsInCoupledLattice();
            //TestHarmonicError();
            //Walls.Add(EngineHead.AddWall(new Vector3(30, 0, 30), 120f, 120f, Vector3.Up, Vector3.Right, Color.BlueViolet));
            //Walls.Add(EngineHead.AddWall(new Vector3(0, 30, 30), 30f, 30f, Vector3.Right, Vector3.Down, Color.CadetBlue));
            //Walls.Add(EngineHead.AddWall(new Vector3(60, 30, 30), 30f, 30f, Vector3.Left, Vector3.Up, Color.CadetBlue));
            //Walls.Add(EngineHead.AddWall(new Vector3(30, 60, 30), 30f, 30f, Vector3.Down, Vector3.Left));
            //Walls.Add(EngineHead.AddWall(new Vector3(30, 30, 0), 30f, 30f, Vector3.Backward, Vector3.Right));
            //Walls.Add(EngineHead.AddWall(new Vector3(30, 30, 60), 30f, 30f, Vector3.Forward, Vector3.Left));
            Font = this.Content.Load<SpriteFont>("Font");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            oldKeyboard = keyboard;
            keyboard = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                keyboard.IsKeyDown(Keys.Escape))
                this.Exit();
            // TODO: Add your update logic here
            if (keyboard.IsKeyDown(Keys.G) && !oldKeyboard.IsKeyDown(Keys.G))
            {
                SetObjects();
            }
            if (keyboard.IsKeyDown(Keys.F) && !oldKeyboard.IsKeyDown(Keys.F))
            {
                graphics.ToggleFullScreen();
            }


            #region Simulation Error Analysis
            float frameTime = gameTime.ElapsedGameTime.Milliseconds;
            int updateIterations = 1;
            float dt = frameTime / updateIterations;
            for (int i = 0; i < updateIterations; i++)
            {
                EngineHead.Update(keyboard, mouse, dt);
            }
            //totalTime += dt / (1000f / updateIterations);
            //ListOfObjects[2].Position = new Vector3(5, 10 + 2*(float)Math.Cos(10 * totalTime), 0);
            #endregion

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Add a HUD to the game
            spriteBatch.Begin();
            spriteBatch.DrawString(Font,
                ListOfObjects[2].Position.ToString(),
                new Vector2(30, 60),
                Color.White);

            spriteBatch.DrawString(Font,
                ListOfObjects[1].Position.ToString(),
                new Vector2(30, 90),
                Color.White);
            spriteBatch.DrawString(Font,
                (ListOfObjects[2].Position.Y-ListOfObjects[1].Position.Y).ToString(),
                new Vector2(30, 120),
                Color.White);
            spriteBatch.End();

            //Reset the graphicsDevice for 3D
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            EngineHead.Draw();

            BasicEffect wallEffect = new BasicEffect(GraphicsDevice);
            foreach (Wall wall in Walls)
            {
                if (wall.Draw)
                {
                    //VertexBuffer vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 4, BufferUsage.WriteOnly);
                    //vertexBuffer.SetData<VertexPositionColor>(wall.DrawingVertecies);
                    wallEffect.World = Matrix.Identity;
                    wallEffect.View = Camera.ViewMatrix;
                    wallEffect.Projection = Camera.ProjectionMatrix;
                    wallEffect.VertexColorEnabled = true;
                    //GraphicsDevice.SetVertexBuffer(vertexBuffer);
                    //RasterizerState rasterizerState = new RasterizerState();
                    //rasterizerState.CullMode = CullMode.None;
                    //GraphicsDevice.RasterizerState = rasterizerState;

                    foreach (EffectPass pass in wallEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList,
                            wall.DrawingVertecies, 0, 4, wall.DrawingIndecies, 0, 2);
                    }
                }
            }

            base.Draw(gameTime);
        }

        /// <summary>
        /// Sets some objects up
        /// </summary>
        public void SetObjects()
        {
            EngineHead.AddModel(this.Content.Load<Model>("Models/ball"),
                new Vector3(0, 5, -10), 10f).Acceleration.Y = -1f;

            Ball.Position = new Vector3(10, 0, -10);
            Ball.Velocity = new Vector3(-1.8f, 0, 0);
            Ball.Acceleration.Y = -1f;
            Ball2.Position = new Vector3(-10, 1, -11);
            Ball2.Velocity = new Vector3(1f, 0, 0);
            Ball2.Acceleration.Y = -1f;

        }

        /// <summary>
        /// Plays the collision sound effect.
        /// </summary>
        public void PlayCollisionSound()
        {
            CollisionSoundEffect.Play();
        }

        /// <summary>
        /// Places objects randomly and adds them to the list of objects.
        /// </summary>
        public void PlaceObjectsRandomly()
        {

            Random rnd = new Random();
            while (ListOfObjects.Count < 50)
            {
                bool canPlaceHere = true;
                Vector3 tryHere = new Vector3((float)rnd.NextDouble() * 50 + 5,
                                                (float)rnd.NextDouble() * 50 + 5,
                                                (float)rnd.NextDouble() * 50 + 5);
                foreach (ModelObject obj in ListOfObjects)
                {
                    if ((obj.Position - tryHere).Length() < 3)
                    {
                        canPlaceHere = false;
                    }
                }
                if (canPlaceHere)
                {
                    ModelObject obj = EngineHead.AddModel(this.Content.Load<Model>("Models/ball"),
                        tryHere,
                        (float)rnd.NextDouble() * 30);
                    obj.Mass = (float)rnd.NextDouble() * 10;
                    obj.ObjectID = ListOfObjects.Count;
                    ListOfObjects.Add(obj);
                }
            }
        }

        /// <summary>
        /// Used to place objects in a specific place with intent
        /// </summary>
        public void PlaceObjectsDeliberately()
        {
            ListOfObjects.Add(EngineHead.AddModel(this.Content.Load<Model>("Models/ball"),
                new Vector3(40, 70, 30), 10));
            //ListOfObjects[0].Velocity = new Vector3(-30, 0 ,0);
            ListOfObjects.Add(EngineHead.AddModel(this.Content.Load<Model>("Models/ball"),
                new Vector3(20, 60, 30), 10));
            //ListOfObjects[1].Velocity = new Vector3(30, 0, 0);
            //ListOfObjects.Add(EngineHead.AddModel(this.Content.Load<Model>("Models/ball_large"),
            //    new Vector3(30, 30, 30), 1000));

            ListOfObjects[0].Couplings.Add(new SpringCoupling(ListOfObjects[0], ListOfObjects[1], 10, 15));
            ListOfObjects[1].Couplings.Add(new SpringCoupling(ListOfObjects[1], ListOfObjects[0], 10, 15));

            for (int i = 0; i < ListOfObjects.Count; i++)
            {
                ListOfObjects[i].ObjectID = i;
            }
        }

        public void PlaceObjectsInOrbit()
        {
            //Place the center object
            ListOfObjects.Add(EngineHead.AddModel(this.Content.Load<Model>("Models/ball_large"),
                new Vector3(30, 30, 30), 1000));
            //ListOfObjects[0].BoundingSphere.Radius = 5.5f;

            Random rnd = new Random();
            double cosTheta;
            double phi;
            float R = 50;

            int numberOfObjects = 201;

            while (ListOfObjects.Count < numberOfObjects)
            {
                cosTheta = rnd.NextDouble() * 2 - 1;
                phi = rnd.NextDouble() *2* Math.PI;

                bool canPlaceHere = true;
                Vector3 tryHere = new Vector3();
                tryHere.X = (float)(Math.Sqrt(1 - cosTheta * cosTheta) * Math.Cos(phi) * R);
                tryHere.Y = (float)(Math.Sqrt(1 - cosTheta * cosTheta) * Math.Sin(phi) * R);
                tryHere.Z = (float)cosTheta * R;
                tryHere += ListOfObjects[0].Position;

                foreach (ModelObject obj in ListOfObjects)
                {
                    if ((obj.Position - tryHere).Length() < 3)
                    {
                        canPlaceHere = false;
                    }
                }

                if (canPlaceHere)
                {
                    ModelObject obj = EngineHead.AddModel(this.Content.Load<Model>("Models/ball"),
                        tryHere,
                        (float)rnd.NextDouble() * 30);
                    obj.Mass = (float)rnd.NextDouble()*0.01f;
                    obj.ObjectID = ListOfObjects.Count;
                    obj.Velocity.X = (float)(cosTheta * Math.Cos(phi));
                    obj.Velocity.Z = (float)(-Math.Sqrt(1-cosTheta*cosTheta));
                    obj.Velocity.Y = (float)(cosTheta * Math.Sin(phi));
                    obj.Velocity *= (float)rnd.NextDouble() * 20 - 40;
                    ListOfObjects.Add(obj);

                }
            }

        }

        public void PlaceObjectsInCoupledLattice()
        {
            Vector3 startingPosition = new Vector3(0,10,0);
            int numberWide = 10, numberLong = 1, numberHigh = 10;
            float spacing = 10;

            for (int w = 0; w < numberWide; w++ )
            {
                for (int l = 0; l < numberLong;l++ )
                {
                    for (int h = 0; h < numberHigh;h++ )
                    {
                        Vector3 position = startingPosition;
                        position += spacing * w * Vector3.Right;
                        position += spacing * l * Vector3.Forward;
                        position += spacing * h * Vector3.Up;
                        ListOfObjects.Add(EngineHead.AddModel(this.Content.Load<Model>("Models/ball"),
                            position, 10));
                    }
                }
            }

            CoupleAdjacentObjectsWithSprings(1000, spacing, spacing + 0.1f);
            ListOfObjects[0].Position += new Vector3(0,0,0);
            ListOfObjects.Add(EngineHead.AddModel(this.Content.Load<Model>("Models/ball"),
                new Vector3(10, 30, 30), 10));
            ListOfObjects[ListOfObjects.Count - 1].Velocity = new Vector3(0, 0, -3);

        }

        public void CoupleAdjacentObjectsWithSprings(float k, float l,float couplingThreshold)
        {
            for (int i = 0; i < ListOfObjects.Count; i++)
            {
                for (int j = i+1; j < ListOfObjects.Count; j++)
                {
                    if ((ListOfObjects[j].Position - ListOfObjects[i].Position).Length()
                        < couplingThreshold)
                    {
                        ListOfObjects[i].Couplings.Add(
                            new SpringCoupling(ListOfObjects[i],
                                                ListOfObjects[j],
                                                k, l));
                        ListOfObjects[j].Couplings.Add(
                            new SpringCoupling(ListOfObjects[j],
                                                ListOfObjects[i],
                                                k, l));
                    }
                }
            }
        }

        public void TestHarmonicError()
        {
            float displacement = 2;
            ListOfObjects.Add(EngineHead.AddModel(this.Content.Load<Model>("Models/ball"),
                new Vector3(0, 0, 0), 1));
            ListOfObjects.Add(EngineHead.AddModel(this.Content.Load<Model>("Models/ball"),
                new Vector3(0, 10+displacement, 0), 1));
            ListOfObjects[1].Couplings.Add(new SpringCoupling(ListOfObjects[1], ListOfObjects[0], 100,10));
            ListOfObjects.Add(EngineHead.AddModel(this.Content.Load<Model>("Models/ball"),
                new Vector3(0, 10 + displacement, 0), 1));
            ListOfObjects[2].Position = new Vector3(5, 12, 0);
        }
    }
}
