using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using FirstGame2;

namespace Engine
{
    public class EngineHead
    {
        #region Fields
        private static EngineHead instance;
        private static Game1 game;
        private static GraphicsDevice graphics;

        public static Camera camera;
        private static float FrameTime;

        private static List<ModelObject> AllMovingObjects;
        private static List<PhysicsObject> AllObjects;
        private static int CenterOfScreenX;
        private static int CenterOfScreenY;
        private static List<Wall> AllWalls;

        //Collision handling
        static List<CollisionPair> CollisionPairs = new List<CollisionPair>();
        static List<ContactGroup> ContactGroups = new List<ContactGroup>();
        static List<ModelObject> ObjectsX;
        static List<ModelObject> ObjectsY;
        static List<ModelObject> ObjectsZ;

        //Debuging
        private static float ReportTime;
        //private static ErrorReport ErrorReport;
        #endregion

        public static EngineHead Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EngineHead();
                    return instance;
                }
                else
                    throw new Exception("EngineHead is already instantiated");
            }
        }

        #region Constructor
        private EngineHead()
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts the Engine to begin rendering
        /// </summary>
        /// <param name="_game">Game class used to control the game</param>
        public static Camera Start(Game1 _game)
        {
            game = _game;
            graphics = _game.GraphicsDevice;
            CenterOfScreenX = graphics.Viewport.Width / 2;
            CenterOfScreenY = graphics.Viewport.Height / 2;
            camera = new Camera(new Vector3(30, 30, 150f), Vector3.Zero,
                (float)16/9, 0.1f, 10000f);
            ErrorReport.FileName = "ErrorReport.txt";
            AllMovingObjects = new List<ModelObject>();
            AllObjects = new List<PhysicsObject>();
            AllWalls = new List<Wall>();
            Debug.WriteLine("Debug has begun.");
            Debug.Indent();
            
            return camera;
        }

        /// <summary>
        /// Controls the positions and physics of each Object in the world
        /// </summary>
        /// <param name="_keyboard">Current KeyboardState</param>
        /// <param name="mouse">Current MouseState</param>
        /// <param name="gameTime">GameTime to control the physics</param>
        public static void Update(KeyboardState keyboard, MouseState mouse, float millisecondGameTime)
        {
            ErrorReport.SubmitReport(millisecondGameTime + " ********* New Frame ************");
            ReportTime += millisecondGameTime;
            FrameTime = millisecondGameTime;

            #region Camera acceleration from input
            //Default 0, and accel is added based on user input.
            //Each unit of accel corresponds to 1000 units of ingame accel
            camera.Acceleration = Vector3.Zero;
            if (keyboard.IsKeyDown(Keys.W))
                camera.Acceleration += camera.rotationMatrix.Forward;
            if (keyboard.IsKeyDown(Keys.A))
                camera.Acceleration += camera.rotationMatrix.Left;
            if (keyboard.IsKeyDown(Keys.S))
                camera.Acceleration += camera.rotationMatrix.Backward;
            if (keyboard.IsKeyDown(Keys.D))
                camera.Acceleration += camera.rotationMatrix.Right;
            if (keyboard.IsKeyDown(Keys.Space))
                camera.Acceleration += camera.rotationMatrix.Up;
            if (keyboard.IsKeyDown(Keys.LeftShift))
                camera.Acceleration += camera.rotationMatrix.Down;
            if (keyboard.IsKeyDown(Keys.LeftControl))
                camera.Speed = 200f;
            else
                camera.Speed = 100;
            #endregion

            #region Camera rotation from input
            camera.RotationAcceleration.Y = -(mouse.X - CenterOfScreenX) * MathHelper.Pi;
            camera.RotationAcceleration.X = -(mouse.Y - CenterOfScreenY) * MathHelper.Pi;
            #endregion

            //Update the camera and reset the mouse to the center
            camera.UpdatePosition(FrameTime);
            Mouse.SetPosition(CenterOfScreenX,CenterOfScreenY);
            //Update the rest of the models
            var watch = new Stopwatch();
            watch.Start();
            CollideObjects2();
            watch.Stop();
            ErrorReport.SubmitReport("Time taken to collide: " + watch.ElapsedMilliseconds.ToString());

            //for (int i = 0; i < AllMovingObjects.Count; i++)
            //{
            //    for (int j = i + 1; j < AllMovingObjects.Count; j++)
            //    {
            //        if ((AllMovingObjects[i].Position - AllMovingObjects[j].Position).Length() <
            //            AllMovingObjects[i].BoundingSphere.Radius + AllMovingObjects[j].BoundingSphere.Radius)
            //        {
            //            ErrorReport.SubmitReport("Error: Intersecting objects at time: " + gameTime.TotalGameTime.Milliseconds.ToString());
            //        }
            //    }
            //}
        }

        //V0.1
        /// <summary>
        /// Calls the update method for all models in AllModels 
        /// </summary>
        /// <param name="dt">Time to evolve the models for</param>
        private static void UpdateAllModels(float dt)
        {
            FrameTime -= dt;
            foreach (ModelObject obj in AllMovingObjects)
            {
                obj.UpdatePosition(dt);
            }
        }

        //V0.2
        /// <summary>
        /// Updates all models in the list. Handles immovable objects defined by the isMovable property.
        /// </summary>
        /// <param name="objects">List of objects to update</param>
        /// <param name="dt">How long to update them for</param>
        private static void UpdateModelPosition(List<PhysicsObject> objects, float dt)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].IsMovable)
                {
                    objects[i].UpdatePosition(dt);
                }
            }
        }

        //V0.2
        /// <summary>
        /// Updates the Accelerations and velocities of all the objects
        /// </summary>
        /// <param name="dt"></param>
        private static void UpdateModelVelocity(float dt)
        {
            UpdateModelAcceleration();
            for (int i = 0; i < AllMovingObjects.Count; i++)
            {
                AllMovingObjects[i].UpdateVelocity(dt);
            }
        }

        /*
         * REMOVE THIS METHOD. THIS IS A CLEAR VIOLATION OF THE SCHEME
         */
        /// <summary>
        /// Draws all ModelObjects in the current list of objects 
        /// 
        /// </summary>
        public static void Draw()
        {
            foreach (ModelObject obj in AllMovingObjects)
            {
                foreach (ModelMesh mesh in obj.Model.Meshes)
                {
                    Matrix[] transforms = new Matrix[obj.Model.Bones.Count];
                    obj.Model.CopyAbsoluteBoneTransformsTo(transforms);
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.EnableDefaultLighting();
                        effect.World = obj.rotationMatrix
                            * transforms[mesh.ParentBone.Index]
                            * Matrix.CreateTranslation(obj.Position);
                        effect.View = camera.ViewMatrix;
                        effect.Projection = camera.ProjectionMatrix;
                    }
                    mesh.Draw();
                }
            }

        }

        /// <summary>
        /// Adds a model to the physics object list
        /// </summary>
        /// <param name="model">Microsoft.Xna.Framework.Model to place at the position</param>
        /// <param name="pos">Microsoft.Xna.Framework.Vector3 that describes the position in 3 space</param>
        /// <param name="mass">The mass of the object in the world for physics calculations</param>
        public static ModelObject AddModel(Model model, Vector3 pos, float mass)
        {
            ModelObject obj = new ModelObject(pos, model, mass);
            AllMovingObjects.Add(obj);
            AllObjects.Add(obj);
            //ObjectsX.Add(obj);
            //ObjectsY.Add(obj);
            //ObjectsZ.Add(obj);
            return AllMovingObjects[AllMovingObjects.Count-1];
        }

        //V0.1
        /// <summary>
        /// Identifies the collisions with a model and 
        /// updates the model accordingly.
        /// </summary>
        /// <param name="model">The model object being tested</param>
        public static void TestCollision()
        {
            ModelObject obj1;
            ModelObject obj2;
            Wall wall;
            int NumberOfRootFindingCalls = 0;
            for (int i = 0; i < AllMovingObjects.Count; i++)
            {
                obj1 = AllMovingObjects[i];
                //Check for collisions with other objects
                for (int j = i + 1; j < AllMovingObjects.Count; j++)
                {
                    obj2 = AllMovingObjects[j];
                    if (obj1.xLeft <= obj2.xRight && obj1.xRight >= obj2.xLeft)
                    {
                        if (obj1.yLeft <= obj2.yRight && obj1.yRight >= obj2.yLeft)
                        {
                            if (obj1.zLeft <= obj2.zRight && obj1.zRight >= obj2.zLeft)
                            {
                                NumberOfRootFindingCalls++;
                                ConsiderCollisionObjObj(obj1, obj2, FrameTime / 1000.0,CollisionPairs);
                            }
                        }
                    }
                }
                //Check for collisions with walls 
                for (int k = 0; k < AllWalls.Count; k++)
                {
                    wall = AllWalls[k];
                    ConsiderCollisionObjWall(obj1,wall,FrameTime / 1000.0,CollisionPairs);
                }
            }
            //ErrorReport.SubmitReport("Number of root finder calls: " + NumberOfRootFindingCalls);
        }

        /// <summary>
        /// Considers a collision between two objects within a time frame.
        /// Adds it to the collision list to be handled
        /// </summary>
        /// <param name="obj1">Object 1 to test</param>
        /// <param name="obj2">Object 2 to test</param>
        /// <param name="timeFrame">Time frame to test it in</param>
        /// <param name="collisionPairs">List to add the collision to if there is one.</param>
        private static void ConsiderCollisionObjObj(ModelObject obj1,ModelObject obj2,double timeFrame,
            List<CollisionPair> collisionPairs)
        {
            if (obj1.isContacting(obj2) || obj2.isContacting(obj1))
            {
                return;
            }
            double timeToCollision;
            Vector3 aRel = obj1.Acceleration - obj2.Acceleration;
            Vector3 vRel = obj1.Velocity - obj2.Velocity;
            Vector3 pRel = obj1.Position - obj2.Position;
            bool clipping = false;
            double d = obj1.BoundingSphere.Radius + obj2.BoundingSphere.Radius;
            double[] quarticCoefficients = {
                                           (pRel.LengthSquared() - d*d),
                                           (2*Vector3.Dot(pRel,vRel)),
                                           Vector3.Dot(pRel,vRel) + vRel.LengthSquared(),
                                           Vector3.Dot(vRel,aRel),
                                           aRel.LengthSquared()/4.0
                                           };
            if(pRel.Length() < d)
            {
                clipping = true;
                for(int i = 1;i<quarticCoefficients.Length;i+=2)
                {
                    quarticCoefficients[i] *= -1;
                }
            }

            double[] positiveRange = {0,timeFrame};
            timeToCollision = RootFinder.findFirstRealRoot(quarticCoefficients,positiveRange);
            if (timeToCollision != -1)
            {
                if(clipping)
                {
                    timeToCollision *= -1;
                }
                collisionPairs.Add(new CollisionPair(obj1, obj2, timeToCollision));
            }
        }

        /// <summary>
        /// Considers a collision between an object and a wall.
        /// Adds it to the collision list to be handled
        /// </summary>
        /// <param name="obj">The ovject to test</param>
        /// <param name="wall">The wall to test</param>
        /// <param name="timeFrame">Time frame to test it in</param>
        /// <param name="collisionPairs">List to add the potential collision pair to.</param>
        private static void ConsiderCollisionObjWall(ModelObject obj, Wall wall, double timeFrame,
            List<CollisionPair> collisionPairs)
        {
            if (obj.isContacting(wall) || wall.isContacting(obj))
            {
                return;
            }

            Vector3 n = wall.Normal;
            Vector3 planeCollisionPoint;
            double rRelDotn = Vector3.Dot(obj.Position - wall.Center,n);
            double vDotn = Vector3.Dot(obj.Velocity, n);
            double aDotn = Vector3.Dot(obj.Acceleration,n);
            double discrim = vDotn*vDotn - 2 * (rRelDotn-obj.BoundingSphere.Radius) * aDotn;
            double timeTillCollision = 0;
            bool willHitPlane = false;

            #region Hitting The Plane
            //If there is virtually no accelleration
            if (Math.Abs(aDotn) < 1e-10)
            {
                timeTillCollision = -(rRelDotn-obj.BoundingSphere.Radius) / vDotn;
                if (timeTillCollision >= 0 && timeTillCollision <= timeFrame)
                {
                    willHitPlane = true;
                }
            }
            //If there are roots
            else if (discrim >= 0)
            {
                discrim = Math.Sqrt(discrim);
                double tPlus = (-vDotn + discrim)/aDotn;
                double tMinus = (-vDotn - discrim)/aDotn;
                
                if((0 < tMinus && tMinus < timeFrame))
                {
                    willHitPlane = true;
                    timeTillCollision = tMinus;
                } 
                else if((0 <= tPlus && tPlus <= timeFrame))
                {
                    willHitPlane = true;
                    timeTillCollision = tPlus;
                }
            }
            #endregion

            //Now we find where exactly it his the plane and determine whether or not that is in the wall.
            if (willHitPlane)
            {
                //Find the point on the plane where the object will collide
                planeCollisionPoint = obj.Position - n * obj.BoundingSphere.Radius + obj.Velocity * (float)timeTillCollision
                    + (1 / 2f) * obj.Acceleration * (float)(timeTillCollision * timeTillCollision);
                bool insideThePlane = false;
                double totalAngle = 0;
                Vector3 vertex1ToPoint;
                Vector3 vertex2ToPoint;

                //Find if it is inside the plane or not by examining the vertecies and angles between them
                for (int i = 0; i < wall.VertexList.Count(); i++)
                {
                    vertex1ToPoint = wall.VertexList[i] - planeCollisionPoint;
                    vertex2ToPoint = wall.VertexList[(i + 1) % wall.VertexList.Count()] - planeCollisionPoint;
                    totalAngle += Math.Acos(
                        Vector3.Dot(vertex1ToPoint, vertex2ToPoint)
                        / (vertex1ToPoint.Length() * vertex2ToPoint.Length()));
                }

                if (Math.Abs(totalAngle - MathHelper.TwoPi) < 1e-5)
                {
                    insideThePlane = true;
                }

                if (insideThePlane)
                {
                    collisionPairs.Add(new CollisionPair(obj, wall, timeTillCollision));
                }
            }
        }

        //V0.1
        /// <summary>
        /// Uses the collisionPairs list to determine and handle collisions
        /// Updates: AllObjects
        /// Clears: collisionPairs
        /// </summary>
        private static void HandleCollision()
        {
            ErrorReport.SubmitReport("Handling Collisions. Size of CollisionPairs: " + CollisionPairs.Count() 
                + "\n\tTime remaining to next frame: " + FrameTime);
            //First order the list by lowest time
            CollisionPairs.Sort();
            //Next, process first collision and update the remaining objects
            CollisionPair cPair = CollisionPairs.ElementAt(0);
            UpdateAllModels((float)cPair.timeTillCollision);
            if (cPair.obj2.IsMovable)
                KineticCollision(cPair.obj1, (ModelObject)cPair.obj2);
            else
                WallCollision(cPair.obj1,(Wall)cPair.obj2);
            CollisionPairs.Clear();
        }

        //V0.2
        /// <summary>
        /// Uses the collisionPairs list to determine and handle collisions
        /// Clears: collisionPairs
        /// Updates: involvedObjects
        /// </summary>
        /// <param name="collisionPairs">List of colliding objects.</param>
        /// <param name="involvedObjects">Total objects involved in this collision handing.</param>
        private static float HandleCollision(List<CollisionPair> collisionPairs,List<PhysicsObject> involvedObjects)
        {
            ErrorReport.SubmitReport("Handling Collisions. Size of CollisionPairs: " + collisionPairs.Count());
            //First order the list by lowest time
            collisionPairs.Sort();

            //Next, process first collision and update the remaining objects
            CollisionPair cPair = collisionPairs[0];
            UpdateModelPosition(involvedObjects,(float)cPair.timeTillCollision);

            if (cPair.obj2.IsMovable)
                KineticCollision(cPair.obj1, (ModelObject)cPair.obj2);
            else
                WallCollision(cPair.obj1, (Wall)cPair.obj2);
            collisionPairs.Clear();
            //game.PlayCollisionSound();

            return (float)cPair.timeTillCollision;
        }

        /// <summary>
        /// Collides 2 objects
        /// Updates: obj1 and obj2
        /// </summary>
        /// <param name="obj1">Projectile</param>
        /// <param name="obj2">Target</param>
        public static void KineticCollision(MovingObject obj1, MovingObject obj2)
        {
            float stictionThreshold = 0f;
            //First we move to the center of mass reference frame for simplicity
            //Vcm = (m1V1+m2V2)/(m1+m2)
            Vector3 Vcm = (obj1.Mass * obj1.Velocity + obj2.Mass * obj2.Velocity)
                / (obj1.Mass + obj2.Mass);
            Vector3 V1cm = obj1.Velocity - Vcm;
            Vector3 V2cm = obj2.Velocity - Vcm;
            //Now, the momenta are equal and opposite, we reflect both velocities over
            //the plane of collision
            if ((V1cm - V2cm).Length()  < stictionThreshold )
            {
                V1cm = Vector3.Zero;
                V2cm = Vector3.Zero;
                //ContactGroup group1 = obj1.ContactGroup;
                //ContactGroup group2 = obj2.ContactGroup;
                //if (group1 == null && group2 == null)
                //{
                //    ContactGroup newGroup = new ContactGroup(obj1, obj2);
                //    ContactGroups.Add(newGroup);
                //    obj1.ContactGroup = newGroup;
                //    obj2.ContactGroup = newGroup;
                //}
                //else if (group1 == null && group2 != null)
                //{
                //    group2.Add(obj1);
                //    obj1.ContactGroup = obj1.ContactGroup;
                //}
                //else if (group1 != null && group2 == null)
                //{
                //    group1.Add(obj2);
                //    obj2.ContactGroup = obj1.ContactGroup;
                //}
                //else
                //{
                //    group1.Combine(group2);
                //    obj2.ContactGroup = group1;
                //}
            }
            else
            {
                Vector3 planeNormal = (obj1.Position - obj2.Position);
                planeNormal.Normalize();
                V1cm = Vector3.Reflect(V1cm, planeNormal);
                V2cm = Vector3.Reflect(V2cm, planeNormal);
            }
            //Now, move back to the world frame
            obj1.Velocity = V1cm + Vcm;
            obj2.Velocity = V2cm + Vcm;
            //Lose some Energy.
            //obj1.Velocity = obj1.Velocity * 0.95f;
            //obj2.Velocity = obj2.Velocity * 0.95f;
            
        }

        /// <summary>
        /// Updates the objects velocity based on the collision
        /// Updates: obj
        /// </summary>
        /// <param name="obj">The object to collide</param>
        /// <param name="wall">The wall it is colliding with</param>
        private static void WallCollision(ModelObject obj, Wall wall)
        {
            double stictionThreshold = 10;
            float energyLoss = 0.95f;
            Vector3 vNorm = Vector3.Dot(obj.Velocity, wall.Normal) * wall.Normal;
            obj.Velocity -= vNorm;
            if (Math.Abs(obj.Velocity.Length()) < stictionThreshold)
            {
                obj.Velocity = Vector3.Zero;
            }
            if (Math.Abs(vNorm.Length()) > stictionThreshold)
            {
                obj.Velocity -= energyLoss * vNorm;
            }
        }

        /// <summary>
        /// Applies arbitrary forces/accels
        /// </summary>
        public static void UpdateModelAcceleration()
        {
            foreach (MovingObject obj1 in AllMovingObjects)
            {
                obj1.Acceleration = Vector3.Zero;
                //obj1.Acceleration += 100* Vector3.Down;

                //Add the forces due to object couplings
                for (int i = 0; i < obj1.Couplings.Count; i++)
                {
                    obj1.Acceleration += obj1.Couplings[i].GetObjectForce() / obj1.Mass;
                }

                foreach (MovingObject obj2 in AllMovingObjects)
                {
                    Vector3 rRel = obj2.Position - obj1.Position;
                    if (!rRel.Equals(Vector3.Zero))
                    {
                        float r = MathHelper.Clamp(rRel.Length(), 0.1f, 1000000f);
                        rRel.Normalize();
                        //Gravity
                        obj1.Acceleration += obj2.Mass * rRel * (50 / (r * r));
                    }
                    //Lorentz Forces
                    //Vector3 E = new Vector3(0, 1, 0);
                    //Vector3 B = new Vector3(0, 0, 1);
                    //obj1.Acceleration += 10 * E / obj1.Mass;
                    //obj1.Acceleration += Vector3.Cross(obj1.Velocity, B) * 5 / obj1.Mass;
                }
            }
        }

        //V0.0
        /// <summary>
        /// Applies the normal and friction forces to objects
        /// </summary>
        /// <param name="i">Index of object 1</param>
        /// <param name="j">Index of object 2</param>
        public static void ApplyContactForce(int i, int j)
        {
            Vector3 r = AllMovingObjects[i].Position - AllMovingObjects[j].Position;
            r.Normalize();
            AllMovingObjects[i].Acceleration -= Vector3.Dot(AllMovingObjects[i].Acceleration, r) * r;
            AllMovingObjects[j].Acceleration -= Vector3.Dot(AllMovingObjects[j].Acceleration, r) * r;
        }

        #region AddWalls
        /// <summary>
        /// Adds a wall to the wall list for collision detection
        /// </summary>
        /// <param name="vertex1">First vertex for the wall</param>
        /// <param name="vertex2">Vertex that is diagonal from 1</param>
        /// <param name="vertex3">A third to define the plane of the wall</param>
        public static Wall AddWall(Vector3 center, float width, float height, Vector3 normal, Vector3 u)
        {
            Wall wall = new Wall(center, width, height, normal, u);
            AllWalls.Add(wall);
            AllObjects.Add(wall);
            return wall;
        }

        public static Wall AddWall(Vector3 center, float width, float height, Vector3 normal, Vector3 u, Color color)
        {
            Wall wall = new Wall(center, width, height, normal, u, color);
            AllWalls.Add(wall);
            AllObjects.Add(wall);
            return wall;
        }
        #endregion
        #endregion

        #region Utility Methods
        //V0.1
        /// <summary>
        /// Tests and collides all physics objects
        /// </summary>
        private static void CollideObjects1()
        {
            //Apply forces
            UpdateModelAcceleration();
            //Test for collisions
            TestCollision();
            //Place holder so it doesnt freeze in contact scenarios
            int iter = 0;
            while (CollisionPairs.Count > 0 && iter < 50)
            {

                HandleCollision();
                TestCollision();
                UpdateModelAcceleration();
                iter++;
            }
            UpdateAllModels(FrameTime);
        }

        //V0.2
        /// <summary>
        /// Tests and collides all physics objects
        /// </summary>
        private static void CollideObjects2()
        {
            List<CollisionChain> properChains;
            List<CollisionPair> currentPairs;
            //Update the forces
            UpdateModelVelocity(FrameTime);
            //Get the proper chains

            //For Debug
            if (ReportTime > 2900)
            {
                int m = 0;
            }
            properChains = GetProperCollisionChains();

            ErrorReport.SubmitReport("Number of proper chains is: " + properChains.Count);
            for (int i = 0; i < properChains.Count; i++)
            {
                float workingFrameTime = FrameTime;
                if (properChains[i].Length > 1)
                {
                    currentPairs = TestCollision2(properChains[i].Chain);
                    int iter = 0;
                    while (currentPairs.Count > 0 && iter < 5)
                    {
                        workingFrameTime -= HandleCollision(currentPairs,properChains[i].Chain);
                        //UpdateModelAcceleration();
                        currentPairs = TestCollision2(properChains[i].Chain);
                        iter++;
                    }
                    UpdateModelPosition(properChains[i].Chain, workingFrameTime);
                }
                else
                {
                    properChains[i].Chain[0].UpdatePosition(workingFrameTime);
                }
            }

        }

        //V0.2
        /// <summary>
        /// Tests for any collisions within collision groups
        /// Requires: objectList is sorted from smallest to largest zLeft bound
        /// </summary>
        /// <param name="objectList">The physics object list to test within</param>
        /// <returns>Returns a list of collision pairs</returns>
        private static List<CollisionPair> TestCollision2(List<PhysicsObject> objectList)
        {
            ModelObject obj1 = null;
            ModelObject obj2 = null;
            List<CollisionPair> collisionPairs = new List<CollisionPair>();
            Wall wall = null;
            int NumberOfRootFindingCalls = 0;

            for (int i = 0; i < objectList.Count; i++)
            {
                if (objectList[i].IsMovable)
                {
                    obj1 = (ModelObject)objectList[i];
                }
                else
                {
                    wall = (Wall)objectList[i];
                }
                int j = i+1;
                //Check for collisions with other objects
                while(j < objectList.Count && objectList[i].zRight > objectList[j].zLeft)
                {
                    if (objectList[j].IsMovable)
                    {
                        obj2 = (ModelObject)objectList[j];
                        if (objectList[i].IsMovable)
                        {
                            if (obj1.xLeft <= obj2.xRight && obj1.xRight >= obj2.xLeft)
                            {
                                if (obj1.yLeft <= obj2.yRight && obj1.yRight >= obj2.yLeft)
                                {
                                    NumberOfRootFindingCalls++;
                                    ConsiderCollisionObjObj(obj1, obj2, FrameTime / 1000.0, collisionPairs);
                                }
                            }
                        }
                        else
                        {
                            if (obj2.xLeft <= wall.xRight && obj2.xRight >= wall.xLeft)
                            {
                                if (obj2.yLeft <= wall.yRight && obj2.yRight >= wall.yLeft)
                                {
                                    NumberOfRootFindingCalls++;
                                    ConsiderCollisionObjWall(obj2, wall, FrameTime / 1000.0, collisionPairs);
                                }
                            }
                        }
                    }
                    else if(objectList[i].IsMovable)
                    {
                        wall = (Wall)objectList[j];
                        if (obj1.xLeft <= wall.xRight && obj1.xRight >= wall.xLeft)
                        {
                            if (obj1.yLeft <= wall.yRight && obj1.yRight >= wall.yLeft)
                            {
                                NumberOfRootFindingCalls++;
                                ConsiderCollisionObjWall(obj1, wall, FrameTime / 1000.0, collisionPairs);
                               
                            }
                        }
                    }
                    j++;
                }
            }
            //ErrorReport.SubmitReport("Number of root finder calls: " + NumberOfRootFindingCalls);
            return collisionPairs;
        }

        //V0.2
        /// <summary>
        /// Finds the proper chains of collisions in 3D
        /// Updates: AllObjects
        /// 
        /// Ensures: 
        /// AllObjects is sorted by the object xLeft paramater.
        /// GetProperCollisionChains is the set of sets of ojects that may collide with each other.
        /// </summary>
        /// <returns>A list of chains of collision systems to handle</returns>
        private static List<CollisionChain> GetProperCollisionChains()
        {
            List<CollisionChain> properChains = new List<CollisionChain>();
            List<CollisionChain> xChains = new List<CollisionChain>();
            List<CollisionChain> xyChains = new List<CollisionChain>();
            CollisionChain currentChain;

            #region Break into chains in x
            //First make sure AllObjects is sorted properly.
            SortAllObjectsByX();
            //Then create collision chains from the sorted list.
            for (int i = 0; i < AllObjects.Count; i++)
            {
                //Start a new collision chain
                currentChain = new CollisionChain(AllObjects[i],
                    AllObjects[i].xLeft,
                    AllObjects[i].xRight);
                //Run through the rest of the objects to see when the overlap stops
                while (i < AllObjects.Count - 1 && currentChain.LeadingEdge >= AllObjects[i + 1].xLeft)
                {
                    i++;
                    currentChain.AddFront(AllObjects[i],AllObjects[i].xRight);
                }

                //Add the resultant chain to the list if it needs further testing.
                //Else, just skip it to the final list.
                if (currentChain.Length > 1)
                {
                    xChains.Add(currentChain);
                }
                else if(currentChain.Length == 1)
                {
                    properChains.Add(currentChain);
                }
            }
            #endregion

            #region Break into chains in x and y
                //Now, check each chain in X to find chains in x and y
                for (int i = 0; i < xChains.Count; i++)
                {
                    xChains[i].Chain.Sort(CollisionChain.CompareObjectsByYLeft);
                    for (int j = 0; j < xChains[i].Length; j++)
                    {
                        //Start a new chain
                        currentChain = new CollisionChain(xChains[i].Chain[j],
                            xChains[i].Chain[j].yLeft,
                            xChains[i].Chain[j].yRight);

                        while (j < xChains[i].Length - 1 && currentChain.LeadingEdge >= xChains[i].Chain[j + 1].yLeft)
                        {
                            j++;
                            currentChain.AddFront(xChains[i].Chain[j], xChains[i].Chain[j].yRight);
                        }
                        //Add the resultant chain to the list if it needs further testing.
                        //Else, just skip it to the final list.
                        if (currentChain.Length > 1)
                        {
                            xyChains.Add(currentChain);
                        }
                        else if (currentChain.Length == 1)
                        {
                            properChains.Add(currentChain);
                        }
                    }
                }
            #endregion

            #region Break into chains in x, y and z (Proper chains)
            //Now, check each chain in X and Y to find chains in x,y, and z
            for (int i = 0; i < xyChains.Count; i++)
            {
                xyChains[i].Chain.Sort(CollisionChain.CompareObjectsByZLeft);
                for (int j = 0; j < xyChains[i].Length; j++)
                {
                    //Start a new chain
                    currentChain = new CollisionChain(xyChains[i].Chain[j],
                        xyChains[i].Chain[j].zLeft,
                        xyChains[i].Chain[j].zRight);

                    //Check for continued overlap
                    while (j < xyChains[i].Length - 1 && currentChain.LeadingEdge >= xyChains[i].Chain[j + 1].zLeft)
                    {
                        j++;
                        currentChain.AddFront(xyChains[i].Chain[j], xyChains[i].Chain[j].zRight);
                    }

                    //Add it to the final chain
                    properChains.Add(currentChain);
                }
            }
            #endregion

            //Finally return proper chains
            return properChains;
        }

        //V0.2
        #region Sorting
        /// <summary>
        /// Sorts the a list of physics objects by their relative y positions
        /// Updates: chain
        /// 
        /// Ensures: For all Xi in AllObjects, Xi+1.yLeft > Xi.yLeft
        /// </summary>
        /// <param name="chain">The list of objects to sort</param>
        private static void SortCollisionChainByY(List<ModelObject> chain)
        {
            int j;
            ModelObject temp;
            for (int i = 1; i < chain.Count; i++)
            {
                temp = chain[i];
                j = i;
                while (j > 0 && chain[j - 1].yLeft > temp.yLeft)
                {
                    chain[j] = chain[j - 1];
                    j--;
                }
                chain[j] = temp;
            }
        }

        /// <summary>
        /// Sorts the a list of physics objects by their relative y positions
        /// Updates: chain
        /// 
        /// Ensures: For all Xi in AllObjects, Xi+1.zLeft > Xi.zLeft
        /// </summary>
        /// <param name="chain">The list of objects to sort</param>
        private static void SortCollisionChainByZ(List<ModelObject> chain)
        {
            int j;
            ModelObject temp;
            for (int i = 1; i < chain.Count; i++)
            {
                temp = chain[i];
                j = i;
                while (j > 0 && chain[j - 1].yLeft > temp.yLeft)
                {
                    chain[j] = chain[j - 1];
                    j--;
                }
                chain[j] = temp;
            }
        }

        /// <summary>
        /// Sorts AllObjects by their xLeft bound.
        /// Updates: AllObjects
        /// Ensures: For all Xi in AllObjects, Xi+1.xLeft > Xi.xLeft
        /// </summary>
        private static void SortAllObjectsByX()
        {
            //Since AllObjects is likely sorted already, use a simple insertion sort to check and correct.
            int j;
            PhysicsObject temp;
            for (int i = 1; i < AllObjects.Count; i++)
            {
                temp = AllObjects[i];
                j = i;
                while (j > 0 && AllObjects[j - 1].xLeft > temp.xLeft)
                {
                    AllObjects[j] = AllObjects[j - 1];
                    j--;
                }
                AllObjects[j] = temp;
            }
        }
        #endregion
        #endregion

    }
}
