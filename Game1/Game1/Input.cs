using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameBehaviour
{
    public sealed class Input
    {
        private static readonly Input instance = new Input();
        private Input()
        {

        }

        public static Input Instance
        {
            get
            {
                return instance;
            }
        }

        

        public Vector2 GetAxis()
        {

            float xDir = 0, yDir = 0;
            KeyboardState kb = Keyboard.GetState();

            for (int i = 0; i < kb.GetPressedKeys().Length; i++)
            {
                switch (kb.GetPressedKeys()[i])
                {
                    case Keys.W:
                        yDir -= 1;
                        break;
                    case Keys.S:
                        yDir += 1;
                        break;
                    case Keys.A:
                        xDir -= 1;
                        break;
                    case Keys.D:
                        xDir += 1;
                        break;
                    default:
                        //xDir = 0;
                        //yDir = 0;
                        break;
                }
            }

            return new Vector2(MathHelper.Clamp(xDir,-1,1), MathHelper.Clamp(yDir,-1,1));
        }
    }
}
