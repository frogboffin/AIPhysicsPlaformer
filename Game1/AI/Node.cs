using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace GameBehaviour
{
    public class Node
    {
        public Vector2 Position { get; set; }
        public bool JumpNode { get; set; }
        public List<NodeConnection> Paths { get; set; }
        public Object Parent { get; set; }
        public Node routeFrom { get; set; }
        public bool cull = false;
        public float fValue { get; set; }

        public Node(Vector2 pos, bool jump, Object par)
        {
            Position = pos;
            JumpNode = jump;
            Parent = par;
            Paths = new List<NodeConnection>();
        }

        public void Draw(SpriteBatch _sb)
        {
            DebugDraw.Instance.DrawCube(Position, 5, _sb);
        }

    }

    public class NodeConnection
    {
        public Node home, dest;
        public bool cull = true;

        public NodeConnection(Node _a, Node _b)
        {
            home = _a;
            dest = _b;
        }

        public float Distance()
        {
            return (home.Position - dest.Position).Length();
        }

        //returns a connection with a swapped start and end
        public NodeConnection Swapped()
        {
            return new NodeConnection(dest, home);
        }

        public void Draw(SpriteBatch _sb)
        {
            DebugDraw.Instance.DrawLine(home.Position, dest.Position, _sb);
        }
    }

}
