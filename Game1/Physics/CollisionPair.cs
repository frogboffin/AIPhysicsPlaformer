using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace GameBehaviour
{
    public class CollisionPair
    {
        public Object BodyA { get; set; }
        public Object BodyB { get; set; }
        public Vector2 ContactNormal { get; set; }
        public Vector2 ContactPoint { get; set; }
        public float CollisionDepth { get; set; }

        public CollisionPair()
        {
            BodyA = null;
            BodyB = null;
        }
    }
}
