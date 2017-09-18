using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.Int32;

namespace RegProj
{
    public class Command
    {
        public string name;
        public string regex;
    }

    public static partial class CommandsCompiler
    {
        private static Regex parseInput = new Regex(
            @"^(?<base>\((.+?)\)|\\?.)(?<reps>(\{.+\}))?(?<extra>[\?\+\*]+)?"
            , RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        /// <summary>
        /// Verifies if the regex can be used by CommandsCompiler.
        /// Must be a valid regex and not contain the following regex symbols: $ ^ 
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return false;

            try
            {
                Regex.Match("", pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return
                !pattern.Contains('$')
                && !pattern.Contains('^')
                ;
        }

        public static bool ValidatesCommands(List<Command> commands)
        {
            foreach (var command in commands)
            {
                if (!IsValidRegex(command.regex))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// higher priority for those inserted first
        /// </summary>
        /// <param name="commands"></param>
        /// <returns></returns>
        private static InputTreeNode BuildInputTree(List<Command> commands)
        {
            string[] groupNames = parseInput.GetGroupNames();

            InputTreeNode root = new InputTreeNode();
            InputTreeNode
                preNode = new InputTreeNode(); //always without children, used to compare stuff before adding a new one
            InputTreeNode iterNode;
            InputTreeNode childNode;
            InputTreeNode newNode;
            string[] temp;
            Match match;
            bool last; //is this the last iteration of command[i]?
            string regex;

            foreach (Command command in commands)
            {
                iterNode = root;
                regex = command.regex;
                do //if match remove content from command and create a node
                {
                    match = parseInput.Match(regex);
                    GroupCollection groupCollection = match.Groups;
                    preNode.SetMinMax(1, 1);
                    preNode.Base = "";
                    preNode.Extra = "";
                    foreach (var groupName in groupNames) //for (int j = 1; j < groupCollection.Count; ++j)
                    {
                        string capture = groupCollection[groupName].Value;
                        if (!groupCollection[groupName].Success) continue;
                        switch (groupName)
                        {
                            case "base":
                                preNode.Base = capture;
                                break;
                            case "reps":
                                capture = capture.Remove(capture.Length - 1);
                                capture = capture.Remove(0, 1);
                                if (capture.Contains(','))
                                {
                                    temp = capture.Split(',');
                                    preNode.Min = Parse(temp[0]);
                                    preNode.Max = Parse(temp[1]);
                                }
                                else
                                {
                                    preNode.Min = Parse(capture);
                                    preNode.Max = Parse(capture);
                                }
                                break;
                            case "extra":
                                preNode.Extra = capture;
                                break;
                        }
                    }
                    regex = regex.Remove(0, match.Length);
                    last = regex.Length == 0;
                    if (last)
                    {
                        if (preNode.Base[0] == '(')
                            preNode.Base = preNode.Base.Replace("(", "(?<" + command.name + ">");
                        else preNode.Base = "(?<" + command.name + ">" + preNode.Base + ")";
                    }
                    if ((childNode = iterNode.HasChild(preNode)) == null)
                    {
                        newNode = new InputTreeNode(preNode);
                        iterNode.AddChild(newNode);
                        iterNode = newNode;
                    }
                    else iterNode = childNode;
                    if (last) break; //leave faster but use match anyway just in case some unespected input appears
                } while (match.Success);
                if (!last) throw new Exception("Unexpected input < " + regex + " > on command " + command.regex);
            }
            return root;
        }

        private static void SimplifyTree(InputTreeNode rootNode)
        {
            rootNode.BFS_MAP(SimplifyRepetitions);
        }

        public static string CompileCommands(List<Command> commands)
        {
            //generate tree already making some improvements
            InputTreeNode tree = CommandsCompiler.BuildInputTree(commands);
            SimplifyTree(tree); //simplify the tree further
            return tree.GenerateRegexFromInputTree();
        }

        internal class InputTreeNode
        {
            public string Base;
            public string Extra = "";
            public int Min, Max;
            public List<InputTreeNode> Children = null;

            public InputTreeNode()
            {
            }

            public InputTreeNode(InputTreeNode preNode)
            {
                this.Base = preNode.Base;
                this.Extra = preNode.Extra;
                this.Min = preNode.Min;
                this.Max = preNode.Max;

                if (Min != 1 || Max != 1 || !Extra.Equals("?")) return;
                Min = 0;
                Extra = "";
            }

            public void AddChild(InputTreeNode child)
            {
                if (Children == null) Children = new List<InputTreeNode>();
                Children.Add(child);
            }

            public InputTreeNode HasChild(InputTreeNode preNode)
            {
                return Children?.FirstOrDefault(child =>
                    child.Base.Equals(preNode.Base) && child.Extra.Equals(preNode.Extra) && child.Min == preNode.Min &&
                    child.Max == preNode.Max);
            }

            public void SetMinMax(int min, int max)
            {
                Min = min;
                Max = max;
            }

            public string GenerateRegexFromInputTree()
            {
                var builder = new StringBuilder();
                if (Children == null) return builder.ToString();
                if (Children.Count == 1) Children[0].GenerateRegexFromInputTree(builder);
                else
                {
                    builder.Append('(');
                    foreach (var child in Children)
                    {
                        child.GenerateRegexFromInputTree(builder);
                        builder.Append('|');
                    }
                    builder.Remove(builder.Length - 1, 1);
                    builder.Append(')');
                }
                return builder.ToString();
            }

            private void GenerateRegexFromInputTree(StringBuilder builder)
            {
                builder.Append(Base);
                if (Min == Max)
                {
                    if (Min != 1) builder.Append("{" + Min.ToString() + "}");
                }
                else
                {
                    if (Min == 0 && Max == 1 && Extra.Equals("")) builder.Append("?");
                    else builder.Append("{" + Min.ToString() + "," + Max.ToString() + "}");
                }
                builder.Append(Extra);
                if (Children == null) return;
                if (Children.Count == 1) Children[0].GenerateRegexFromInputTree(builder);
                else
                {
                    builder.Append('(');
                    foreach (var child in Children)
                    {
                        child.GenerateRegexFromInputTree(builder);
                        builder.Append('|');
                    }
                    builder.Remove(builder.Length - 1, 1);
                    builder.Append(')');
                }
            }

            public delegate void Transform(InputTreeNode node);

            public void BFS_MAP(Transform transform)
            {
                var transverse = new Queue<InputTreeNode>();
                var auxQueue = new Queue<InputTreeNode>();
                auxQueue.Enqueue(this);

                while (auxQueue.Count > 0)
                {
                    var node = auxQueue.Dequeue();
                    transverse.Enqueue(node);
                    if (node.Children != null)
                        foreach (var child in node.Children)
                            auxQueue.Enqueue(child);
                }

                while (transverse.Count > 0)
                    transform(transverse.Dequeue());
            }

            public enum InputTreeNodeCollision
            {
                /// <summary>
                /// no collision occurs
                /// </summary>
                No = 0,
                /// <summary>
                /// the nodes have the same base, min, max. Extras must be within certain values but do not need to match.
                /// </summary>
                Equal,
                /// <summary>
                /// node is contained inside of the reference node
                /// </summary>
                Inside,
                /// <summary>
                /// inside in reverse priority
                /// </summary>
                Contains,
                /// <summary>
                /// starts outside the reference node and colides later
                ///  </summary>
                Crosses,
                /// <summary>
                /// starts inside the reference node and stops coliding later (crosses in reverse priority)
                ///  </summary>
                IsCrossed,
                OutAfter,
                OutBefore
            }

            public InputTreeNodeCollision CollidesWith(InputTreeNode node)
            {
                //TODO maybe change this condition?
                if (!(
                    Base.Equals(node.Base) &&
                    (Extra.Equals("") || Extra.Equals("+") && Min != Max)
                    &&
                    (node.Extra.Equals("") || node.Extra.Equals("+") && node.Min != node.Max)
                    )) return InputTreeNodeCollision.No;

                if (Min >= node.Max) return InputTreeNodeCollision.OutAfter;

                if (node.Min >= Max) return InputTreeNodeCollision.OutBefore;

                if (node.Min == Min && node.Max == Max) return InputTreeNodeCollision.Equal;

                if (node.Min <= Min && node.Max >= Max) return InputTreeNodeCollision.Inside;

                if (node.Min >= Min && node.Max <= Max) return InputTreeNodeCollision.Contains;

                if (node.Min < Max && node.Min > Min) return InputTreeNodeCollision.Crosses;
                //jic
                if (node.Max > Min && node.Min < Min) return InputTreeNodeCollision.IsCrossed;

                throw new Exception("Bad Implementation - case missing");
            }

        }
    }
}