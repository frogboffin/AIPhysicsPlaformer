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
    public class Object
    {
        public String Name { get; set; }
        public Collider HitBox { get; set; }
        public RigidBody2D RigidBody { get; set; }
        private Vector2 pos;
        public Vector2 Position
        {
            get
            {
                //Moves the object if it is a button to visualise a button press, a bit hacked but it works
                if (Colliding.Name == "Player" && Name == "Button" || Colliding.Name == "Bot" && Name == "Button")
                {
                    return pos + Vector2.UnitY * 4;
                }
                return (pos);
            }
            set
            {
                pos = value;
            }
        }
        public Texture2D Texture { get; set; }
        public bool IsStatic { get; set; }
        public float GravityMultiplier { get; set; }
        public float Bounciness { get; set; }
        public float StaticFriction { get; set; }
        public float DynamicFriction { get; set; }
        public bool IsColliding { get; set; }
        public bool Renderable { get; set; }
        public float Alpha { get; set; }
        public bool IgnoreParticles { get; set; }
        public bool IsParticle { get; set; }
        public Object Colliding { get; set; }
        public Vector2 Offset { get; set; }
        public bool Pathable { get; set; }

        public Object(String _name, Vector2 _pos, Texture2D _tex)
        {
            Name = _name;
            HitBox = new Box(_pos, _tex.Width, _tex.Height, this);
            RigidBody = new RigidBody2D();
            Position = _pos;
            Offset = Vector2.Zero;
            Texture = _tex;
            IsStatic = true;
            Bounciness = 0.5f;
            StaticFriction = 0.7f;
            DynamicFriction = 0.5f;
            IsColliding = false;
            Renderable = true;
            Alpha = 1f;
            IgnoreParticles = false;
            IsParticle = false;
            GravityMultiplier = 1.0f;
            Colliding = this;
            if (IsStatic)
                Pathable = true;
            else
                Pathable = false;
        }

        public Object Copy()
        {
            return this;
        }

        public void Step(float dt, World world)
        {
            Vector2 acceleration = (RigidBody.Force) * (1+RigidBody.InvMass);
            RigidBody.LinearVelocity += (acceleration + (world.Gravity * GravityMultiplier)) * dt;

            //simulation of air resistance
            RigidBody.LinearVelocity *= 0.99f;

            Position += RigidBody.LinearVelocity * dt;

            if (HitBox != null)
            {
                (HitBox).UpdatePos(Position);
            }

            IsColliding = false;
            //remove the force after is has been applied
            ForceAdd(-RigidBody.Force,1);
        }

        public void ForceAdd(Vector2 direction, float _force)
        {
            RigidBody.Force += direction * _force;
        }

        public void SetFriction(float staticFriction, float dynamicFriction)
        {
            StaticFriction = staticFriction;
            DynamicFriction = dynamicFriction;
        }

        public Vector2 Center()
        {
            if (HitBox == null)
            {
                return (Position + new Vector2(Texture.Width / 2, Texture.Height / 2));
            }
            else
                return (HitBox).Center();
        }

    }
}
