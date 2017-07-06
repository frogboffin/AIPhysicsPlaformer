using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameBehaviour
{
    class SpringJoint
    {
        public Object BodyA { get; set; }
        public Object BodyB { get; set; }
        public float Stiffness { get; set; }
        public float RestLength { get; set; }
        public float Dampen { get; set; }
        public bool IsBridge { get; set; }

        public SpringJoint()
        {
            Stiffness = 100;  //stiffness of the spring
            RestLength = 25;  //The rest length of the spring
            Dampen = 0.2f;      //Spring force dampen factor
            IsBridge = true;
        }

        public SpringJoint(float stiff, float rest, float damp)
        {
            Stiffness = stiff;  //stiffness of the spring
            RestLength = rest;  //The rest length of the spring
            Dampen = damp;      //Spring force dampen factor
        }

        public void ApplyForce()
        {
            //Vector between the two masses attached to the spring
            //Vector2 s_vec = BodyB.Center() - BodyA.Center();
            Vector2 s_vec = BodyB.Center() - BodyA.Center();

            if (s_vec == Vector2.Zero) s_vec = Vector2.One * 0.00001f;
            //Distance between the two masses, i.e. the length of the spring
            float length = s_vec.Length();

            float force = -Stiffness * (length - RestLength);

            BodyA.ForceAdd((s_vec * (force / length)) * -Dampen, 1);// /BodyB.RigidBody.InvMass;
            BodyB.ForceAdd((s_vec * (force / length)) * Dampen, 1);// /BodyB.RigidBody.InvMass;

            if (IsBridge)
            {
                BodyA.RigidBody.LinearVelocity *= Vector2.UnitY;
                BodyB.RigidBody.LinearVelocity *= Vector2.UnitY;
            }


        }

        public void Draw(SpriteBatch sb)
        {
            //DebugDraw.Instance.DrawLine(BodyB.Center(), BodyA.Center(), sb);
        }
    }
}
