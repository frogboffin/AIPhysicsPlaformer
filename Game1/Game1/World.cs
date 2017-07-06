using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameBehaviour
{
    public class World
    {
        public List<Object> mDynamicPropList = new List<Object>();
        public List<Object> mStaticPropList = new List<Object>();
        private List<Object> mParticles = new List<Object>();
        private List<Object> mPermanentParticles = new List<Object>();
        private List<SpringJoint> mSpringJointList = new List<SpringJoint>();
        private List<Object> mButtonList = new List<Object>();
        public List<Object> mActiveButtons = new List<Object>();
        private List<Texture2D> texList = new List<Texture2D>();
        public AIManager ai;

        public Vector2 Gravity { get; set; }
        public Vector2 Wind { get; set; }
        public float MaxSpeed { get; set; }
        public CollisionManager collisionManager = new CollisionManager();
        public List<CollisionPair> collisionPairs = new List<CollisionPair>();
        GraphicsDevice graphics;
        private float rainTimer, inputTimer, buttonTimer;
        private const int maxParticles = 450;
        private int currentParticles = 150;
        float timeScale = 1f;
        Random rand = new Random((int)DateTime.Now.Ticks * 100);
        float waterSize = 2;
        Object lid;

        public World(GraphicsDevice _graphics, List<Texture2D> textures)
        {
            Gravity = new Vector2(0.0f, 400f);
            Wind = new Vector2(15.0f, 0.0f);
            graphics = _graphics;
            MaxSpeed = 1;
            rainTimer = 0;
            inputTimer = 0;
            buttonTimer = 0;
            texList = textures;
            BucketWater();


            #region buttons
            mButtonList.Add(AddBox("Button", 125, 515, 30, 10, Color.Black * 0.7f, 0, 1, true, true, true, true));
            mButtonList.Add(AddBox("Button", 1100, 515, 30, 10, Color.Black * 0.7f, 0, 1, true, true, true, true));
            mButtonList.Add(AddBox("Button", 1020, 675, 30, 10, Color.Black * 0.7f, 0, 1, true, true, true, true));
            mButtonList.Add(AddBox("Button", 210, 675, 30, 10, Color.Black * 0.7f, 0, 1, true, true, true, true));
            mButtonList.Add(AddBox("Button", 620, 395, 30, 10, Color.Black * 0.7f, 0, 1, true, true, true, true));
            #endregion
        }

        //Looks for the prop named top and sets it to the lid
        public void Initialise()
        {
            foreach (var prop in mStaticPropList)
            {
                if (prop.Name == "Top")
                {
                    lid = prop;
                    break;
                }
            }
            RandomButton();
        }

        //Flips gravity for debug purposes only - Could be a potential game mechanic
        public void Flip()
        {
            if (inputTimer < 1) return;

            inputTimer = 0;
            Gravity = -Gravity;
            
        }

        //returns a list of both static and dynamic objects
        public List<Object> AllObjects()
        {
            return mDynamicPropList.Concat(mStaticPropList).ToList();
        }

        //Main update loop
        //Timers are updated
        //Spring forces are applied
        //Water bucket is maintained
        //spawns rain and checks for button presses
        //Begins broad collision
        public void Update(GameTime _dt)
        {
            if (waterSize >= 100)
            {
                Console.WriteLine("You Win!");
                return;
            }

            float dt = (float)_dt.ElapsedGameTime.TotalSeconds * timeScale;

            rainTimer += dt;
            inputTimer += dt;
            buttonTimer += dt;

            foreach (var spring in mSpringJointList)
            {
                spring.ApplyForce();
            }

            foreach (var prop in mDynamicPropList.ToList())
            {
                if (prop.IsParticle && prop.Colliding.Name == "WaterInBucket")
                {
                    waterSize += 0.6f;
                    prop.IsParticle = false;
                    BucketWater();
                }

                prop.Step(dt, this);
            }

            int i = 0;
            foreach(var prop in mStaticPropList.ToList())
            {
                if (prop.Name == "Button" && prop.Colliding.Name == "Player" || prop.Name == "Button" && prop.Colliding.Name == "Bot")
                {
                    ai.SelectTarget();
                    i++;
                    //Console.WriteLine("YOU TOUCHED DA BUTTON");
                    prop.Colliding = prop;
                }
            }

            if (i >= 2)
            {
                //Move lid
                lid.IgnoreParticles = true;
                lid.Renderable = false;
            }
            else
            {
                lid.IgnoreParticles = false;
                lid.Renderable = true;
            }


            //Console.WriteLine("SpringJoints - " + mSpringJointList.Count());
            //Console.WriteLine("Dynamic Props - " + mDynamicPropList.Count());
            
            var all = mDynamicPropList.Concat(mStaticPropList);
            collisionManager.ComputeBroadCollision(all.ToList<Object>());

            if (buttonTimer > 10)
            {
                RandomButton();
            }

            if (rainTimer > 0.01f)
            {
                if (rand.Next(0,10000) > 9900)
                {
                    Wind *= -1;
                }
                Rain();
                rainTimer = 0;
            }
        }

        //Main draw loop
        //Draws all renderable objects
        public void Draw(GameTime _dt, SpriteBatch _sb)
        {
            float dt = (float)_dt.ElapsedGameTime.TotalSeconds * timeScale;

            _sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            foreach (var prop in mDynamicPropList)
            {
                if (prop.Renderable)
                {
                    _sb.Draw(prop.Texture, prop.Position, Color.White * prop.Alpha);
                    if (prop.IsParticle)
                    {
                        DebugDraw.Instance.DrawLine(prop.Position, prop.HitBox.Max(), _sb);

                        DebugDraw.Instance.DrawLine(prop.Center(), prop.Center() + prop.RigidBody.LinearVelocity * 10f / prop.RigidBody.LinearVelocity.Length(), _sb);
                    }
                }
            }
            foreach(Object prop in mStaticPropList)
            {
                if (prop.Renderable)
                    _sb.Draw(prop.Texture, prop.Position + prop.Offset,null, null, null, 0, null, Color.White * prop.Alpha,SpriteEffects.None, 1);
            }
            foreach(var prop in mSpringJointList)
            {
                prop.Draw(_sb);
            }
            _sb.End();
        }

        //method for creating a textured box
        //Not used much as drawing boxes with pixels is easier
        public Object AddBox(String _name, float xPos, float yPos, Texture2D tex, float bounciness, float mass,  bool _isStatic, bool renderable, bool ignore)
        {
            Object obj = new Object(_name, new Vector2(xPos, yPos), tex);
            RigidBody2D body = new RigidBody2D();
            obj.HitBox = new Box(obj.Position, tex.Height, tex.Width, obj);
            obj.IsStatic = _isStatic;
            obj.Bounciness = bounciness;
            obj.Renderable = renderable;
            obj.IgnoreParticles = ignore;
            obj.RigidBody.SetMass(mass);

            if (_isStatic)
                mStaticPropList.Add(obj);
            else
                mDynamicPropList.Add(obj);

            return obj;
        }

        //creates a texture of one colour of width*height and then creates the box 
        //If the box is named WaterInBucket then a random alpha colour is given to make it look a little translucent 
        public Object AddBox(String _name, float xPos, float yPos, int width, int height, Color color, float bounciness, float mass, bool _isStatic, bool renderable, bool ignore, bool pathable = true, float friction = 0.8f)
        {
            Random r = new Random((int)DateTime.Now.Ticks * 100);
            Texture2D tex = new Texture2D(graphics, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = color;
                if (_name == "WaterInBucket")
                    data[i] *= ((float)r.NextDouble() / 0.2f);
            }
            tex.SetData(data);

            Object obj = new Object(_name, new Vector2(xPos, yPos), tex);
            Vector2 size = new Vector2(width, height);
            obj.HitBox = new Box(new Vector2(xPos, yPos), height, width, obj);
            obj.IsStatic = _isStatic;
            obj.Bounciness = bounciness;
            obj.RigidBody.SetMass(mass);
            obj.Renderable = renderable;
            obj.IgnoreParticles = ignore;
            obj.Pathable = pathable;
            
            if (_isStatic)
                mStaticPropList.Add(obj);
            else
                mDynamicPropList.Add(obj);

            return obj;
        }

        //Method for creating circles
        //can only be done with textures
        //Not used in the final game due to a small annoying bug with circle - box collisions
        public Object AddCircle(String _name, float xPos, float yPos, Texture2D tex, float bounciness, float mass, bool _isStatic, bool renderable, bool ignore)
        {
            Object obj = new Object(_name, new Vector2(xPos, yPos), tex);
            RigidBody2D body = new RigidBody2D();
            obj.HitBox = new Circle(new Vector2(xPos, yPos), tex.Width/2, obj);
            obj.IsStatic = _isStatic;
            obj.Bounciness = bounciness;
            obj.Renderable = renderable;
            obj.IgnoreParticles = ignore;
            obj.RigidBody.SetMass(mass);
            obj.SetFriction(0.3f, 0.01f);

            if (_isStatic)
                mStaticPropList.Add(obj);
            else
                mDynamicPropList.Add(obj);

            return obj;
        }

        //Creates a player object 
        public Player CreatePlayer(float xPos, float yPos, Texture2D tex)
        {
            Player obj = new Player("Player", new Vector2(xPos, yPos), tex);
            Vector2 size = new Vector2(tex.Width,tex.Height);
            obj.RigidBody.SetMass(100f);
            obj.IsStatic = false;
            obj.Pathable = true;
            //obj.IgnoreParticles = true;
            mDynamicPropList.Add(obj);

            return obj;
        }

        //Creates an AI object
        public AI CreateAI(float xPos, float yPos, Texture2D tex)
        {
            AI obj = new AI("Bot", new Vector2(xPos, yPos), tex);
            Vector2 size = new Vector2(tex.Width, tex.Height);
            obj.RigidBody.SetMass(100f);
            obj.IsStatic = false;
            obj.Pathable = true;
            //obj.IgnoreParticles = true;
            mDynamicPropList.Add(obj);

            return obj;
        }

        //Adds a single spring joint
        //Not used
        public void AddSpringJoint(float x, float y, int size)
        {
            Object[] objs = new Object[3];
            SpringJoint spring = new SpringJoint();
            SpringJoint spring2 = new SpringJoint();
            Random ran = new Random((int)DateTime.Now.Ticks * 100);
            objs[0] = AddBox("SpringA" + mSpringJointList.Count + 1, x, y, size, size, Color.SeaGreen, 0.3f, 15f, true, true, true);
            objs[1] = AddBox("SpringB" + mSpringJointList.Count + 1, x, y + spring.RestLength + 10f, size, size, Color.SeaGreen, 0.3f, 10, false, true, true);
            objs[2] = AddBox("SpringB" + mSpringJointList.Count + 1, x, y + spring.RestLength * 2f + 20f, size, size, Color.SeaGreen, 0.3f, 10, false, true, true);

            //foreach (var body in objs)
            //{
            //    float angle = (float)(Math.PI * ran.NextDouble());
            //    float speed = 0.1f * MaxSpeed * ((float)(ran.NextDouble()));

            //    Vector2 rb_velocity = new Vector2();
            //    rb_velocity.X = speed * (float)(Math.Cos(angle));
            //    rb_velocity.Y = -speed * (float)(Math.Sin(angle));

            //    body.RigidBody.LinearVelocity = rb_velocity;
            //}


            objs[0].HitBox = null;
            spring.BodyA = objs[0];
            spring.BodyB = objs[1];
            mSpringJointList.Add(spring);

            spring2.BodyA = objs[1];
            spring2.BodyB = objs[2];
            mSpringJointList.Add(spring2);
        }

        //Adds a mesh of spring joints in one of 2 configurations
        //Both softbody style objects, bridges and ropes can be made using this method
        //very demanding on the cpu
        public void AddSpringJointGrid(Vector2 pos, int size, int _x, int _y, bool staticTopRow, Texture2D tex, bool isCircle, bool allToAll, float stiff, float rest, float damp, String name = "Spring")
        {
            Object[,] objs = new Object[_x, _y];
            //SpringJoint[] springs = new SpringJoint[2 * size * (size - 1)];
            SpringJoint spring = new SpringJoint();
            bool isStatic = staticTopRow;
            Random ran = new Random((int)DateTime.Now.Ticks * 100);
            bool bridge;
            if (_y == 1)
                bridge = true;
            else
                bridge = false;

            for (int y = 0; y < _y; y++)
            {
                //objs[0, y] = AddBox("Spring" + mSpringJointList.Count + 1, (pos.X + (spring.RestLength + 1f) * x), (pos.Y + (spring.RestLength + 1f) * y), size, size, Color.SeaGreen, 0.3f, 15f,true, true, true);
                for (int x = 0; x < _x; x++)
                {
                    if (tex != null)
                    {
                        objs[x, y] = AddBox(name, (pos.X + (tex.Height) * x), (pos.Y + (tex.Height) * y), tex, 0.01f, 3f, isStatic, !isStatic, true);
                        if (isCircle)
                            objs[x, y].HitBox = new Circle(new Vector2(pos.X, pos.Y), tex.Width / 2, objs[x, y]);
                        if (isStatic)
                            objs[x, y].HitBox = null;
                        
                    }
                    else
                    {
                        if (x == 0 && staticTopRow || x == _x - 1 && staticTopRow)
                            isStatic = true;
                        else
                            isStatic = false;
                        objs[x, y] = AddBox(name, (pos.X + (size) * x), (pos.Y + (size) * y), size-1, size-1, Color.Green * 0.4f, 0.0f, 30f, isStatic, !false, true, false);
                        objs[x, y].SetFriction(0, 0);
                        if (isCircle)
                            objs[x, y].HitBox = new Circle(new Vector2(pos.X, pos.Y), size / 2, objs[x, y]);

                        if (x == (int)(_x / 2) || x == (int)(1) || x == (int)(_x - 2))
                            if (y == 0)
                                objs[x, y].Pathable = true;
                    }

                    //if (allToAll) objs[x, y].Renderable = false;
                    //objs[x, y] = AddBox(name, (pos.X + (spring.RestLength + 0f) * x), (pos.Y + (spring.RestLength + 0f) * y),texList[2], 0.5f, 10, isStatic, false, true);

                }
               // isStatic = false;
                
            }

            int count = 0;

            if (_x == 1 || _y == 1 || !allToAll)
            {
                #region crosshatch joints
                for (int x = 0; x < _x; x++)
                {
                    for (int y = 0; y < _y; y++)
                    {
                        if (x != _x - 1)
                        {
                            spring = new SpringJoint(stiff, (objs[x, y].Position - objs[x + 1, y].Position).Length() + rest, damp);
                            spring.BodyA = objs[x, y];
                            spring.BodyB = objs[x + 1, y];
                            spring.IsBridge = bridge;
                            mSpringJointList.Add(spring);
                            count++;
                        }

                        if (y != _y - 1)
                        {
                            spring = new SpringJoint(20, (objs[x, y].Position - objs[x, y + 1].Position).Length() + rest, damp);
                            spring.BodyA = objs[x, y];
                            spring.BodyB = objs[x, y + 1];
                            spring.IsBridge = bridge;
                            mSpringJointList.Add(spring);
                            count++;
                        }

                        if (y < _y - 1 && x < _x - 1)
                        {
                            spring = new SpringJoint(20, (objs[x, y].Position - objs[x + 1, y + 1].Position).Length() + rest, damp);
                            spring.BodyA = objs[x, y];
                            spring.BodyB = objs[x + 1, y + 1];
                            spring.IsBridge = bridge;
                            mSpringJointList.Add(spring);
                            count++;
                        }

                        if (y > 0 && x < _x - 1)
                        {
                            spring = new SpringJoint(20, (objs[x, y].Position - objs[x + 1, y - 1].Position).Length() + rest, damp);
                            spring.BodyA = objs[x, y];
                            spring.BodyB = objs[x + 1, y - 1];
                            spring.IsBridge = bridge;
                            mSpringJointList.Add(spring);
                            count++;
                        }
                    }
                }
                #endregion
            }
            else
            {
                #region all to all joints
                foreach (Object body in objs)
                {
                    foreach (Object body2 in objs)
                    {
                        if (body == body2) continue;

                        spring = new SpringJoint(stiff, (body.Position - body2.Position).Length(), damp);
                        spring.IsBridge = bridge;
                        if ((body.Position - body2.Position) * Vector2.UnitX == Vector2.Zero) // 
                        { 
                            spring.RestLength = (body.Position - body2.Position).Length() + rest;
                            spring.Stiffness = stiff / 10;

                        }
                        else
                        {
                            spring.Dampen = damp * 50;
                        }
                        spring.BodyA = body;
                        spring.BodyB = body2;
                        mSpringJointList.Add(spring);
                        count++;
                    }
                }

                #endregion
            }


        }

        //gets 2 random buttons and adds them to the active button list
        public void RandomButton()
        {
            if (inputTimer < 1) return;

            inputTimer = 0;

            buttonTimer = 0;
            foreach (Object obj in mStaticPropList.ToList())
            {
                if (obj.Name == "Button")
                {
                    mStaticPropList.Remove(obj);
                    if (mActiveButtons.Count != 0)
                    {
                        mActiveButtons.Remove(obj);
                    }
                }
            }

            Random ran = new Random((int)DateTime.Now.Ticks * 100);
            int a, b;
            a = ran.Next(0, 5);
            do
            {
                b = ran.Next(0, 5);
            } while (a == b);

            mStaticPropList.Insert(0,(mButtonList[a]));
            mStaticPropList.Insert(0,(mButtonList[b]));
            mActiveButtons.Insert(0, mButtonList[a]);
            mActiveButtons.Insert(0, mButtonList[b]);
        }

        //Updates the bucket water object with a larger version
        public void BucketWater()
        {
            foreach (Object p in mStaticPropList.ToList())
            {
                if (p.Name == "WaterInBucket")
                {
                    mStaticPropList.Remove(p);
                }
            }

            Vector2 v = new Vector2(graphics.Viewport.Width/2 -30, graphics.Viewport.Height / 2 * 0.3f + 100 - waterSize +1);
            //AddSpringJointGrid(v , waterSize, 10, 7, false, null, false, true, 100f, waterSize * 10f, 0.004f, "WaterInBucket");
            AddBox("WaterInBucket", v.X, v.Y, 70, (int)waterSize, Color.Blue, 0.3f, 100, true, true, false, false);
        }

        //Creates a rain particle with random properties
        //rain is the only thing effected by wind
        public void Rain()
        {
            if (currentParticles == 0) return;
            if (mParticles.Count > currentParticles)
            { 
                mDynamicPropList.Remove(mParticles[0]);
                mParticles.Remove(mParticles[0]);
            }
            //Spawn rain particles which die after they hit an object
            Random ran = new Random((int)DateTime.Now.Ticks * 100);
            float xPos = ran.Next(10, graphics.Viewport.Width + 10);
            float bounce = ran.Next(0, 7) / 100f;
            Object particle = AddBox("Rain", xPos, -60, 2, 3, Color.Blue, bounce, 1f, false, true, true, false);
            //Object particle = AddCircle("Rain", xPos, -10,texList[3], bounce, 3f, false, true, true);

            Vector2 force = Wind + Vector2.UnitX * ran.Next(-1000, 3000);
            particle.RigidBody.SetMass(ran.Next(3, 100));
            particle.SetFriction(0.01f, 0.01f);
            particle.Alpha = ran.Next(30,60)/100f;
            particle.ForceAdd(force, 1);
            particle.IsParticle = true;
            mParticles.Add(particle);
        }

        //Changes the total number of particles allowed
        public void ToggleRain()
        {
            if (inputTimer < 1) return;

            inputTimer = 0;

            if (currentParticles >= maxParticles)
            {
                currentParticles = 0;
                foreach (Object o in mParticles)
                {
                    mDynamicPropList.Remove(o);
                }

                mParticles.Clear();
            }
            else
                currentParticles += 50;

            Console.WriteLine("Max Particles: " + currentParticles);
        }

       
    }
}
