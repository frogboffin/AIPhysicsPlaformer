using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace GameBehaviour
{
    public class AIManager
    {
        public List<Node> Nodes = new List<Node>();
        public List<NodeConnection> Conns;
        public List<NodeConnection> path;
        public AI bot;
        private Player player;
        private World world;
        private Node targetNode;
        private Node moveNode;
        private Node botNode;

        public AIManager(AI _bot, Player _player, World _world)
        {
            //Nodes
            Conns = new List<NodeConnection>();
            path = new List<NodeConnection>();
            bot = _bot;
            player = _player;
            world = _world;
        }

        //Populates the nodes of the world
        //Does not create nodes for non pathable/no hitbox or particles 
        //depending on the width of the object a different number of nodes are created 
        //Jump nodes are set as the edge of each object
        public void PopulateNodes(List<Object> objs)
        {
            foreach (Object obj in objs)
            {
                List<Node> tempNodes = new List<Node>();

                if (!obj.Pathable) continue;
                if (obj.IsParticle) continue;
                if (obj.HitBox == null) continue;

                //populate nodes
                float width = Math.Abs(obj.HitBox.Max().X - obj.HitBox.Min().X);
                Vector2 topLeft = new Vector2(obj.HitBox.Min().X + 0, obj.HitBox.Min().Y - 15);
                Vector2 topRight = new Vector2(obj.HitBox.Min().X + width - 0, obj.HitBox.Min().Y - 15);

                Vector2 leftSide = Vector2.Zero;
                Vector2 rightSide = Vector2.Zero;

                Vector2 middleLeft = Vector2.Zero;
                Vector2 middleRight = Vector2.Zero;

                if (width > 40)
                {
                    leftSide = new Vector2(obj.Center().X - width / 3, obj.Position.Y - 15);
                    rightSide = new Vector2(obj.Center().X + width / 3, obj.Position.Y - 15);
                }

                if (width > 600)
                {
                    middleLeft = new Vector2(obj.Center().X - width / 6, obj.Position.Y - 15);
                    middleRight = new Vector2(obj.Center().X + width / 6, obj.Position.Y - 15);
                }

                if ( width > 100)
                    tempNodes.Add(new Node(topLeft, true, obj)); //Jump nodes

                if (leftSide != Vector2.Zero)
                    tempNodes.Add(new Node(leftSide,false, obj)); //Edge nodes
                if (middleLeft != Vector2.Zero)
                    tempNodes.Add(new Node(middleLeft, false, obj));

                if (obj.Name == "Spring")
                    tempNodes.Add(new Node(new Vector2(obj.Center().X, obj.Position.Y - 5), true, obj));
                else
                    tempNodes.Add(new Node(new Vector2(obj.Center().X, obj.Position.Y - 5), false, obj));

                if (obj == bot)
                    botNode = tempNodes.Last();

                if (middleRight != Vector2.Zero)
                    tempNodes.Add(new Node(middleRight, false, obj));
                if (rightSide != Vector2.Zero)
                    tempNodes.Add(new Node(rightSide, false, obj)); //Edge nodes

                if ( width > 100)
                    tempNodes.Add(new Node(topRight, true, obj)); //Jump nodes

                Nodes.AddRange(tempNodes);

                for (int i = 0; i < tempNodes.Count-1; i++)
                {
                    Conns.Add(new NodeConnection(tempNodes[i], tempNodes[i + 1]));
                }
            }
        }


        //Main update for the AI bot 
        //Generates populates the nodes, connectiona and pathfind the bots target
        //Gets the next node in the path and moves the bot towards it, if the path is empty then the bot moves towards the player
        public void Update()
        {
            if (bot == null) return;

            if (path.Count() == 0)
            {
                moveNode = new Node(player.Position, false, player);
            }
            else
                moveNode = path.Last().home;

            if (bot.IsColliding || (bot.Position.X - moveNode.Position.X) < 40)
            {
                Nodes.Clear();
                Conns.Clear();
                path.Clear();

                PopulateNodes(world.mStaticPropList.Concat(world.mDynamicPropList).ToList());
                SelectTarget();
                PreSearchCull();
                PopulateConnections();
                Search();
            }
            PopulateNodes(world.mDynamicPropList);
            SelectTarget();
            Search();

            bot.Move((moveNode.Position - bot.Center()) / (moveNode.Position - bot.Center()).Length(), 400f );
            //Vector2 i = ((moveNode.Position - bot.Center()) / (moveNode.Position - bot.Center()).Length());
            //Console.WriteLine(i);

            if (moveNode.Position.Y < bot.HitBox.Min().Y + 15 && bot.IsColliding)
            {
                bot.Jump(((moveNode.Position - bot.Center()).Length() - 20) / 130f);
                bot.Move((moveNode.Position - bot.Center()) / (moveNode.Position - bot.Center()).Length(), 1000f );
            }

        }

        //Standard a* search between this frames nodes and connections G is the distance from the next node and H is the unobstucted distance from the end target (button)
        public void Search()
        {
            if (targetNode == null || targetNode.Paths.Count() == 0) return;

            Node start = botNode;
            Node end = targetNode;

            Node current = start;
            List<Node> open = new List<Node>();
            List<Node> closed = new List<Node>();

            foreach (NodeConnection c in start.Paths)
            {
                float g = (c.dest.Position - current.Position).Length()/2f;
                float h = (c.dest.Position - end.Position).Length();
                c.dest.fValue = h + g;
                c.dest.routeFrom = start;
                open.Add(c.dest);
            }
            
            while (open.Count() != 0)
            {
                Node smallestNode = open[0];
                foreach (Node n in open.ToList())
                {
                    if (n.fValue < smallestNode.fValue)
                    {
                        smallestNode = n;
                    }
                }
                current = smallestNode;

                open.Remove(current);
                closed.Add(current);

                foreach (NodeConnection n in current.Paths)
                {
                    if (closed.Contains(n.dest)) continue;

                    float g = (n.dest.Position - current.Position).Length()/2f;
                    float h = (n.dest.Position - end.Position).Length();

                    if (!open.Contains(n.dest))
                    {
                        n.dest.fValue = h + g;
                        n.dest.routeFrom = current;
                        open.Add(n.dest);
                    }
                    else
                    {
                        if (n.dest.fValue > h + g)
                        {
                            n.dest.fValue = h + g;
                            n.dest.routeFrom = current;
                        }
                    }
                }
                if (closed.Contains(end))
                    break;
            }

            while (current != start)
            {
                path.Add(new NodeConnection(current, current.routeFrom));
                current = current.routeFrom;
            }

            if (end == botNode) path.Reverse();

            //Console.WriteLine(path.Count());
            //Console.WriteLine((targetNode.Position - botNode.Position).Length());

            //Console.WriteLine("Nodes: " + Nodes.Count());
            //Console.WriteLine("Connections: " + Conns.Count());
        }

        //Uses the current list of nodes and generates connections between them
        //Only nodes within 260 length can be joined and 2 intersection tests are done between each object in the world so that connections cannot be through objects
        public void PopulateConnections()
        {
            foreach (Node n in Nodes)
            {
             foreach (Node n2 in Nodes)
                {
                    bool connection = true;
                    //if (n == n2) continue;
                    if ((n.Position - n2.Position).Length() > 260) continue;

                    //Skip the collision detection so the node cannot be blocked
                    if ((n.Position - n2.Position).Length() < 100 && n.Parent.Name == "Button")
                    {
                        Conns.Add(new NodeConnection(n, n2));
                        n.Paths.Add(Conns.Last());
                        n2.Paths.Add(Conns.Last().Swapped());
                        continue;
                    }
                    //if (Math.Abs(n.Position.X - n2.Position.X) > 200) continue;
                    //if (n.Position.Y == n2.Position.Y) continue;
                    //if (n.JumpNode && n2.JumpNode) continue;

                    bool con = false;
                    foreach(NodeConnection c in Conns)
                    {
                        if (Node.ReferenceEquals(c.home, n) && Node.ReferenceEquals(c.dest, n2) || Node.ReferenceEquals(c.home, n2) && Node.ReferenceEquals(c.dest, n))
                            con = true; 
                    }
                    if (con) continue;

                    foreach (Object o in world.mStaticPropList.Concat(world.mDynamicPropList).ToList())
                    {
                        if (o == bot || o == player || o.IsParticle) continue;
                        

                        for (int i = 0; i < 2; i++)
                        {
                            Vector2 b1;
                            Vector2 b2;
                            if (i == 0)
                            {
                                b2 = new Vector2(o.HitBox.Center().X, o.HitBox.Min().Y);
                                b1 = new Vector2(o.HitBox.Center().X, o.HitBox.Max().Y);
                            }
                            else
                            {
                                b2 = new Vector2(o.HitBox.Min().X, o.HitBox.Center().Y);
                                b1 = new Vector2(o.HitBox.Max().X, o.HitBox.Center().Y);
                            }


                            float denom = ((b2.Y - b1.Y) * (n2.Position.X - n.Position.X)) - ((b2.X - b1.X) * (n2.Position.Y - n.Position.Y));

                            if (denom == 0)
                            {
                                //connection = true;
                                continue;
                            }
                        
                            float ua = ((b2.X - b1.X) * (n.Position.Y - b1.Y)) - ((b2.Y - b1.Y) * (n.Position.X - b1.X));
                            float ub = ((n2.Position.X - n.Position.X) * (n.Position.Y - b1.Y)) - ((n2.Position.Y - n.Position.Y) * (n.Position.X - b1.X));

                            ua = (ua / denom);
                            ub = (ub / denom);

                            if (ua >= 0 && ua <= 1f && ub >= 0 && ub <= 1)
                            {
                                connection = false;
                                break;
                            }
                        }


                    }

                    if (connection)
                    {
                        Conns.Add(new NodeConnection(n, n2));
                        n.Paths.Add(Conns.Last());
                        n2.Paths.Add(Conns.Last().Swapped());
                    }
                }
            }
        }

        //Used to draw the debug lines so the nodes and paths can be seen
        public void DrawNodes(SpriteBatch _sb)
        {
            foreach (Node n in Nodes)
            {
                n.Draw(_sb);
            }
            foreach (NodeConnection c in Conns)
            {
                //c.Draw(_sb);
            }
            foreach (NodeConnection c in path)
            {
                c.Draw(_sb);
                c.home.Draw(_sb);
                c.dest.Draw(_sb);
            }
        }

        //Removes out of bounds nodes and nodes which are too close together
        public void PreSearchCull() 
        {
            foreach (Node n in Nodes.ToList())
            {
                if (n.Position.X > 1280 || n.Position.X < 0 || n.Position.Y > 800 || n.Position.Y < 0)
                    Nodes.Remove(n);
            }

            foreach(Node n in Nodes.ToList())
            {
                foreach(Node n2 in Nodes.ToList())
                {
                    if (n == n2) continue;
                    if ((n.Position - n2.Position).Length() < 40)
                    {
                        if (n.Parent.Name == "Button" && n2.Parent != bot)
                            Nodes.Remove(n2);
                    }
                }
            }

        }

        //Removes the Connections, dynamic nodes and path
        public void PostSearchCull()
        {
            foreach (NodeConnection n in Conns.ToList())
            {
                //if (n.cull)
                {
                    Conns.Remove(n);
                }
            }
            foreach (Node n in Nodes.ToList())
            {
                if (!n.Parent.IsStatic)
                    Nodes.Remove(n);
            }
            path.Clear();
        }

        //used to select a target for the bot
        public void SelectTarget()
        {
            Object target;

            if (world.mActiveButtons.Count != 0)
            {
                //Get the distance between both buttons and player/ai
                float d0 = (botNode.Position - world.mActiveButtons[0].Position).Length();
                float d1 = (botNode.Position - world.mActiveButtons[1].Position).Length();
                float p0 = (player.Position - world.mActiveButtons[0].Position).Length();
                float p1 = (player.Position - world.mActiveButtons[1].Position).Length();

                target = world.mActiveButtons.Last();

                //If the player is closer to one of the nodes then the ai will choose the other node if the ai is closer to both then he will chose the last one
                if (p0 < d0)
                {
                    target = world.mActiveButtons[1];
                }
                else if (p1 < d1)
                {
                    target = world.mActiveButtons[0];
                }
                else if (d0 < d1)
                    target = world.mActiveButtons[0];
                else
                    target = world.mActiveButtons[1];

            }
            else
                target = player; //If no buttons are available follow the player




            foreach (Node n in Nodes)
            {
                if (n.Parent == target)
                {
                    targetNode = n;
                    //Console.WriteLine(target.Name + " " + targetNode.Position);
                    return;
                }
            }
        }

        public bool InList(NodeConnection a, List<Node> list)
        {
            foreach (Node n in list)
            {
                if (a.dest == n)
                    return true;
            }

            return false;
        }

    }
}
