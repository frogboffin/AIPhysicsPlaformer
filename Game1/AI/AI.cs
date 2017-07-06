using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameBehaviour
{
    public class AI : Object
    {
        public int DashCooldown { get; set; }

        public AI(String _name, Vector2 _pos, Texture2D _tex) : base(_name, _pos, _tex)
        {
            Name = _name;
            RigidBody = new RigidBody2D();
            HitBox = new Box(_pos, _tex.Width, _tex.Height, this);

            Position = _pos;
            Texture = _tex;
            IsStatic = true;
            Bounciness = 0.3f;
            StaticFriction = 1f;
            DynamicFriction = 0.3f;
            IsColliding = false;
            DashCooldown = 3;
        }

        public void Move(Vector2 direction, float _force)
        {
            direction *= Vector2.UnitX;

            //if (IsColliding)
            //    RigidBody.Force += direction * _force;
            //else
            //    RigidBody.Force += direction * _force * 0.65f;

            RigidBody.LinearVelocity += direction * _force/100;

            if (!IsColliding && RigidBody.LinearVelocity.Y < 0)
                RigidBody.MaxSpeed = 550;
            else
                RigidBody.MaxSpeed = 800;

        }

        public void Jump(float height)
        {
            if (IsColliding)
            {
                RigidBody.LinearVelocity = (RigidBody.LinearVelocity * 0.3f) + new Vector2(0, -1) * 500 * height;
            }
        }


        //Dash is a debug feature only the player can perform at the moment
        //Further options would allow the AI to perform dash when needed to manouvre mid air
        public void Dash(Vector2 dir)
        {
            RigidBody.LinearVelocity += dir * 80f;
        }
    }
}

