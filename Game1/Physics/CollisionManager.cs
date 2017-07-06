using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameBehaviour
{
    //Based on the Collision manager from SimpleDynamics by Minsi

    public class CollisionManager
    {
        private List<CollisionPair> mCollisionPairs;
        Vector2[] normalsAABB = new Vector2[4] {
                            new Vector2(1,0),
                            new Vector2(-1,0),
                            new Vector2(0,1),
                            new Vector2(0,-1) };

        public CollisionManager()
        {
            mCollisionPairs = new List<CollisionPair>();
        }
        
        //Code from Simple Dynamics framework - Minsi Chen
        public void AddCollisionPair(CollisionPair pair)
        {
            Object A = pair.BodyA;
            Object B = pair.BodyB;

            
            //Check if the pair already exists
            foreach (var p in mCollisionPairs)
            {
                if (
                    (Object.ReferenceEquals(A, p.BodyA) && Object.ReferenceEquals(B, p.BodyB)) ||
                    (Object.ReferenceEquals(A, p.BodyB) && Object.ReferenceEquals(B, p.BodyA))
                   )
                {
                    return;
                }
            }

            mCollisionPairs.Add(pair);
        }


        public void Resolve(CollisionPair data)
        {
            Object a = data.BodyA;
            Object b = data.BodyB;

            if (a.Name == "Button" || b.Name == "Button")
                return;

            if (a == null || b == null)
                return;

            //perform the displacement using the collision depth and normal
            #region Displacement 
            //data.ContactNormal.Normalize();

            Vector2 displacement = (data.ContactNormal * data.CollisionDepth);

            if (!a.IsStatic && b.IsStatic && !b.IsParticle)
                a.Position += displacement * 1f;
            else if (!a.IsStatic && !b.IsParticle)
                a.Position += displacement / 1f;

            if (!b.IsStatic && a.IsStatic && !a.IsParticle)
                b.Position -= displacement * 1f;
            else if (!b.IsStatic && !a.IsParticle)
                b.Position -= displacement /1f;

            (a.HitBox).UpdatePos(a.Position);
            (b.HitBox).UpdatePos(b.Position);
            #endregion

            //perform the impulse response 
            #region Impulse Collision
            Vector2 relativeVelocity = b.RigidBody.LinearVelocity - a.RigidBody.LinearVelocity;
            float rVN = Vector2.Dot(relativeVelocity, data.ContactNormal);

            float elasticity = (a.Bounciness + b.Bounciness) / 2;

            float mass;

            if (a.IsStatic || b.IsParticle)
                mass = b.RigidBody.InvMass;
            else if (b.IsStatic || a.IsParticle)
                mass = a.RigidBody.InvMass;
            else
                mass = (a.RigidBody.InvMass + b.RigidBody.InvMass);

            float impulse = (-1 * (1 + elasticity) * rVN) / mass;

            if (!a.IsStatic && !b.IsParticle)
            {
                data.BodyA.RigidBody.LinearVelocity -= (data.ContactNormal * (impulse / a.RigidBody.Mass));
            }
            if (!b.IsStatic && !a.IsParticle)
            {
                data.BodyB.RigidBody.LinearVelocity += (data.ContactNormal * (impulse / b.RigidBody.Mass));
            }
            //Console.WriteLine("Friction impulse" + frictionImpulse + " Linear velocity" + a.RigidBody.LinearVelocity);
            #endregion

            relativeVelocity = b.RigidBody.LinearVelocity - a.RigidBody.LinearVelocity;
            rVN = Vector2.Dot(relativeVelocity, data.ContactNormal);

            //perform the friction response
            #region Friction Collision
            //Referenced - https://gamedevelopment.tutsplus.com/tutorials/how-to-create-a-custom-2d-physics-engine-friction-scene-and-jump-table--gamedev-7756
            float frictionImpulse;
            float dynamicFriction;
            Vector2 tangent = relativeVelocity - (rVN * data.ContactNormal);
            if (tangent != Vector2.Zero)
                tangent.Normalize();
            else
                tangent = Vector2.Zero;

            float magnitude = -Vector2.Dot(relativeVelocity, tangent);
            magnitude = magnitude / (a.RigidBody.InvMass + b.RigidBody.InvMass);

            float totalFriction = MathHelper.Clamp((float)Math.Sqrt(a.StaticFriction * a.StaticFriction + b.StaticFriction * b.StaticFriction), 0, 1f);

            float impulse2 = (-1 * (1 + elasticity) * Vector2.Dot(relativeVelocity, tangent));

            if (Math.Abs(magnitude) < impulse2 * totalFriction)
                frictionImpulse = (magnitude);
            else
            {
                dynamicFriction = MathHelper.Clamp((float)Math.Sqrt(a.DynamicFriction * a.DynamicFriction + b.DynamicFriction * b.DynamicFriction), 0f, 1f);
                frictionImpulse = (dynamicFriction);
            }

            if (!a.IsStatic && !b.IsParticle)
            {
                data.BodyA.RigidBody.LinearVelocity -= tangent / (1 - frictionImpulse) * impulse2 / a.RigidBody.Mass;
            }
            if (!b.IsStatic && !a.IsParticle)
            {
                data.BodyB.RigidBody.LinearVelocity += tangent / (1 - frictionImpulse) * impulse2 / b.RigidBody.Mass;
            }
            #endregion
            
        }

        //Calculates the potential collisions of all bodies in the world
        //Many types of objects are skipped for performance
        //Objects having the same name do not collide - this is a hack for spring joint grids
        //both AABB boxes and circles have an AABB which is used to find any overlaps
        //any overlaps found are then passed into Shape.Collision() to calculate the collision pair
        public void ComputeBroadCollision(List<Object> bodys)
        {
            foreach (var body in bodys)
            {
                if (body.IsStatic || body.HitBox == null) continue;
                foreach (var body2 in bodys)
                {
                    //Cull objects which do not need detection
                    if (body2.HitBox == null) continue; //Can't collide without a hitbox
                    if (body == body2) continue; //Don't collide with self
                    if (body.IgnoreParticles && body2.IsParticle) continue; //Ignore particles 
                    if (body.IsParticle && body2.IgnoreParticles) continue;
                    if (body.Name == body2.Name) continue;

                    //AABB broad collision detection
                    bool isColliding = true;
                    Collider a = body.HitBox;
                    Collider b = body2.HitBox;

                    float[] distances = new float[4]
                    {
                        b.Max().X - a.Min().X,
                        a.Max().X - b.Min().X,
                        b.Max().Y - a.Min().Y,
                        a.Max().Y - b.Min().Y
                    };

                    for (int i = 0; i < 4; i++)
                    {
                        //http://www.gamedev.net/topic/567310-platform-game-collision-detection/
                        if (distances[i] < 0.0f) isColliding = false;
                    }

                    CollisionPair pair;

                    if (isColliding)
                    {

                        if (b is Box)
                            pair = body.HitBox.Collision((Box)b);
                        else
                            pair = body.HitBox.Collision((Circle)b);


                        if (pair.BodyA == null || pair.BodyB == null) continue;

                        if (pair.BodyA.GetType() == typeof(Player) && pair.ContactNormal == new Vector2(0, -1) && !pair.BodyB.IsParticle) ((Player)pair.BodyA).IsColliding = true;
                        if (pair.BodyB.GetType() == typeof(Player) && pair.ContactNormal == new Vector2(0, 1) && !pair.BodyA.IsParticle) ((Player)pair.BodyB).IsColliding = true;
                        if (pair.BodyA.GetType() == typeof(AI) && pair.ContactNormal == new Vector2(0, -1) && !pair.BodyB.IsParticle) ((AI)pair.BodyA).IsColliding = true;
                        if (pair.BodyB.GetType() == typeof(AI) && pair.ContactNormal == new Vector2(0, 1) && !pair.BodyA.IsParticle) ((AI)pair.BodyB).IsColliding = true;

                        if (pair.BodyB.GetType() == typeof(AI) || pair.BodyB.GetType() == typeof(Player) || pair.BodyA.GetType() == typeof(AI) || pair.BodyA.GetType() == typeof(Player) || pair.BodyA.IsParticle || pair.BodyB.IsParticle)
                        {
                            pair.BodyA.Colliding = pair.BodyB;
                            pair.BodyB.Colliding = pair.BodyA;
                        }


                        AddCollisionPair(pair);
                    }

                    ResolveAllPairs();
                    
                }
            }
        }

        public void ResolveAllPairs()
        {
            foreach (var p in mCollisionPairs)
            {
                Resolve(p);
            }

            mCollisionPairs.Clear();
        }
    }
}

