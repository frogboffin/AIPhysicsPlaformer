using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace GameBehaviour
{
    public interface Collider
    {

        Vector2 Center();
        void UpdatePos(Vector2 v);
        Vector2 Min();
        Vector2 Max();
        CollisionPair Collision(Box b);
        CollisionPair Collision(Circle c);
    }

    public class Box : Collider
    {
        public Object parent { get; set; }
        public Vector2 TopLeft { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }

        public Box(Vector2 topLeft, float height, float width, Object par)
        {
            TopLeft = topLeft;
            Height = height;
            Width = width;
            parent = par;
        }

        public Vector2 Center()
        {
            return new Vector2(TopLeft.X + Width/2, TopLeft.Y + Height/2);
        }

        public bool IsColliding(Circle c)
        {
            return false;
        }

        public bool IsColliding(Box b)
        {
            return false;
        }

        public Vector2 Min()
        {
            //Console.WriteLine("MIN");
            return TopLeft;
        }

        public Vector2 Max()
        {
            return new Vector2(TopLeft.X + Width, TopLeft.Y + Height);
        }

        public void UpdatePos(Vector2 v)
        {
            TopLeft = v;
        }

        //AABB Box to AABB box collision
        //Creates a collision pair with appropriate info
        public CollisionPair Collision(Box b)
        {
            Box a = this;
            Vector2 collisionNormal = Vector2.Zero;
            float collisionDepth = 1000;

            Vector2[] normalsAABB = new Vector2[4] 
                        {
                            new Vector2(1,0),
                            new Vector2(-1,0),
                            new Vector2(0,1),
                            new Vector2(0,-1)
                        };

            float[] distances = new float[4]
                    {
                        b.Max().X - a.Min().X,
                        a.Max().X - b.Min().X,
                        b.Max().Y - a.Min().Y,
                        a.Max().Y - b.Min().Y
                    };

            for (int i = 0; i < 4; i++)
            {
                if (i == 0 || (distances[i] < collisionDepth))
                {
                    collisionNormal = normalsAABB[i];
                    collisionDepth = distances[i];
                }
            }
            CollisionPair p = new CollisionPair();
            p.BodyA = a.parent;
            p.BodyB = b.parent;
            p.CollisionDepth = collisionDepth;
            p.ContactNormal = collisionNormal;
            p.ContactPoint = (a.Center() + (collisionNormal * (1 / collisionDepth)));

            return p;
        }

        //AABB - Cirlce collision
        //has an issue with the normal on this collision
        //circles like to get stuck on the corner of the box
        public CollisionPair Collision(Circle c)
        {
            Box a = this;
            Vector2 collisionNormal = Vector2.Zero;
            float collisionDepth = 1000;

            Vector2[] normalsAABB = new Vector2[4]
                        {
                            new Vector2(-1,0),
                            new Vector2(1,0),
                            new Vector2(0,-1),
                            new Vector2(0,1)
                        };

            float[] distances = new float[4]
                    {
                        c.Max().X - a.Min().X,
                        a.Max().X - c.Min().X,
                        c.Max().Y - a.Min().Y,
                        a.Max().Y - c.Min().Y
                    };

            for (int i = 0; i < 4; i++)
            {
                if (i == 0 || (distances[i] < collisionDepth))
                {
                    collisionNormal = normalsAABB[i];
                    collisionDepth = distances[i];
                }
            }
            CollisionPair p = new CollisionPair();
            p.BodyA = a.parent;
            p.BodyB = c.parent;

            p.ContactNormal = collisionNormal;

            Vector2 d = c.Center() - a.Center();
            d = Vector2.Clamp(d, new Vector2(-Width/2, -Height/2), new Vector2(Width / 2, Height / 2));
            p.ContactPoint = a.Center() + d;
            float n = d.Length();
            p.CollisionDepth = c.Radius - (p.ContactPoint - c.Center()).Length();
            Vector2 normal = p.ContactPoint - c.Center() + (Vector2.UnitY * 0.0001f);
            normal.Normalize();
            p.ContactNormal = normal;
            return p;
        }
    }

    public class Circle : Collider
    {
        public Circle(Vector2 _center, float radius, Object par)
        {
            _Center = _center;
            Radius = radius;
            parent = par;
        }

        public Vector2 TopLeft { get; set; }
        public Object parent { get; set; }
        public Vector2 _Center { get; set; }
        public float Radius { get; set; }
        
        public Vector2 Min()
        {
            return Center() - new Vector2(Radius, Radius);
        }

        public Vector2 Max()
        {
            return Center() + new Vector2(Radius, Radius);
        }

        public bool IsColliding(Circle c)
        {
            return false;
        }

        public bool IsColliding(Box b)
        {
            return false;
        }
        public Vector2 Center()
        {
            return _Center;
        }

        public void UpdatePos(Vector2 v)
        {
            TopLeft = v;
            _Center = new Vector2(v.X + Radius, v.Y + Radius);
        }

        //Calls the Boxes method for Cirlce-Box collision as they are the same but the objects are reversed
        public CollisionPair Collision(Box a)
        {
            return a.Collision(this);
        } 

        //Circle - cirlce collisions
        public CollisionPair Collision(Circle c)
        {
            Circle a = this;
            float xRel = a.Center().X - c.Center().X;
            float yRel = a.Center().Y - c.Center().Y;

            float collisionDistance = (float)Math.Sqrt((xRel * xRel) + (yRel * yRel));

            if (collisionDistance > a.Radius + c.Radius)
            {
                return new CollisionPair(); //null collision pair = no collision
            }
            //collision detected
            CollisionPair p = new CollisionPair();
            p.BodyA = a.parent;
            p.BodyB = c.parent;
            p.ContactPoint = new Vector2(((a.Center().X * c.Radius) + (c.Center().X * a.Radius)) / (a.Radius + c.Radius), ((a.Center().Y * c.Radius) + (c.Center().Y * a.Radius)) / (a.Radius + c.Radius));

            p.CollisionDepth = (a.Radius + c.Radius - collisionDistance);
            Vector2 normal = (a.Center() - c.Center());
            normal.Normalize();
            p.ContactNormal = normal;

            return p;
        } 
    }
}
