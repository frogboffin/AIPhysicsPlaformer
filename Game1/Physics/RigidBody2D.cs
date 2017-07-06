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
    public class RigidBody2D
    {
        private Vector2 linVel;
        public float MaxSpeed { get; set; }
        public float Mass { get; set; }
        public float InvMass { get; set; }
        //public Vector2 Position { get; set; }
        public Vector2 LinearVelocity
        {
            get
            {
                return linVel;
            }
            set
            {
                //linVel = value;
                
                linVel = Vector2.Clamp(value, Vector2.One * -MaxSpeed, Vector2.One * MaxSpeed);
            }
        }
        public Vector2 Force { get; set; }
        //public Collider HitBox{ get; set; }
        //public Texture2D Texture { get; set; }
        
        public RigidBody2D()
        {
            MaxSpeed = 700;
            Mass = 100.0f;
            InvMass = 1.0f / Mass;
            LinearVelocity = new Vector2();
            Force = new Vector2();
        }

        public void SetMass(float mass)
        {
            Mass = mass;
            InvMass = 1.0f / mass;
        }
    }
}