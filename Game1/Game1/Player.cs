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
    public class Player : Object
    {
        public int DashCooldown { get; set; }

        public Player(String _name, Vector2 _pos, Texture2D _tex) : base(_name, _pos, _tex)
        {
            Name = _name;
            RigidBody = new RigidBody2D();
            HitBox = new Box(_pos, _tex.Width, _tex.Height, this);

            Position = _pos;
            Texture = _tex;
            IsStatic = true;
            Bounciness = 0.3f;
            StaticFriction = 1f;
            DynamicFriction = 0.7f;
            IsColliding = false;
            DashCooldown = 3;
        }



        public void Move(Vector2 direction, float _force)
        {
            if (direction.Y < 0) return;
            RigidBody.LinearVelocity += direction * _force;

            if (!IsColliding && RigidBody.LinearVelocity.Y < 0)
                RigidBody.MaxSpeed = 450;
            else
                RigidBody.MaxSpeed = 800;
        }

        public void Jump()
        {
            if (IsColliding)
            {
                RigidBody.LinearVelocity += (RigidBody.LinearVelocity * 0.3f) + new Vector2(0, -1) * 3000;
            }
        }

        //Dash is a debug feature only the player can perform at the moment
        //Further options would allow the AI to perform dash when needed to manouvre mid air
        public void Dash(Vector2 dir)
        {
            RigidBody.LinearVelocity += dir * 500f;
        }
    }
}
