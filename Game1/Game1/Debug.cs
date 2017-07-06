using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameBehaviour
{
    public sealed class DebugDraw
    {
        private Texture2D t;
        private static readonly DebugDraw instance = new DebugDraw();
        public bool debugMode = false;

        private DebugDraw()
        {

        }

        public static DebugDraw Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        private class Nested
        {
            static Nested()
            {

            }

            internal static readonly DebugDraw instance = new DebugDraw();
        }

        public void SetTexture(Texture2D _t)
        {
            t = _t;
        }

        public void DrawLine(Vector2 start, Vector2 end, SpriteBatch sb)
        {
            if (!debugMode) return;

            Vector2 edge = end - start;
            // calculate angle to rotate line
            float angle =
                (float)Math.Atan2(edge.Y, edge.X);

            sb.Draw(t,
                new Rectangle(// rectangle defines shape of line and position of start of line
                    (int)start.X,
                    (int)start.Y,
                    (int)edge.Length(), //sb will strech the texture to fill this rectangle
                    1), //width of line, change this to make thicker line
                null,
                Color.White, //colour of line
                angle,     //angle of line (calulated above)
                new Vector2(0, 0), // point in line about which to rotate
                SpriteEffects.None,
                0);
        }

        public void DrawCube(Vector2 pos, int size, SpriteBatch sb)
        {
            if (!debugMode) return;

            sb.Draw(t,
                new Rectangle(// rectangle defines shape of line and position of start of line
                    (int)pos.X - size/2,
                    (int)pos.Y - size/2,
                    size, //sb will strech the texture to fill this rectangle
                    size), //width of line, change this to make thicker line
                null,
                Color.White, //colour of line
                0,     //angle of line (calulated above)
                new Vector2(0, 0), // point in line about which to rotate
                SpriteEffects.None,
                0);
        }
    }
}
