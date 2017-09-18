using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegProj
{
    public static partial class CommandsCompiler
    {
        private static readonly InputTreeNode.Transform simplifyRepetitions = node =>
        {
            if (node.Children == null) return;
            //Console.WriteLine("Doing simplification stuff on node");
            var children = node.Children;
            var processed = new List<InputTreeNode>();
            for (int i = children.Count - 1; i >= 0; i--)//start iteration form less prioritized to high prioritized
            {
                var result = children[i];//if no collision is found is added has is
                for (int j = processed.Count - 1; j >= 0; j--) //from higher priority to lowest priority
                {
                    //TODO
                    InputTreeNode.InputTreeNodeCollision collision = result.CollidesWith(processed[j]);
                    if (collision == InputTreeNode.InputTreeNodeCollision.No) continue;
                    else
                    {
                        if (collision == InputTreeNode.InputTreeNodeCollision.Equal)
                            result = MergeEqualNodes(result, processed[j]);
                        else if (collision == InputTreeNode.InputTreeNodeCollision.OutAfter)
                            result = MergeNodesOutter(result,processed[j],true);
                        else if (collision == InputTreeNode.InputTreeNodeCollision.OutBefore)
                            result = MergeNodesOutter(result, processed[j], false);
                        else if (collision == InputTreeNode.InputTreeNodeCollision.Inside)
                            result = MergeNodesIfInside(result, processed[j]);
                        else if (collision == InputTreeNode.InputTreeNodeCollision.Contains)
                            result = MergeNodesIfContains(result, processed[j]);
                        else if (collision == InputTreeNode.InputTreeNodeCollision.Crosses)
                            throw new NotImplementedException();
                        else if (collision == InputTreeNode.InputTreeNodeCollision.Iscrossed)
                            throw new NotImplementedException();
                        processed.Remove(processed[j]);
                    }
                }
                processed.Add(result);
            }
            //TODO maybe this reverse step can be avoided and some more efficent variation can be used?
            processed.Reverse();//make high priority nodes come first
            node.Children = processed;
        };

        internal static InputTreeNode.Transform SimplifyRepetitions => simplifyRepetitions;

        private static InputTreeNode MergeEqualNodes(InputTreeNode prioritized, InputTreeNode other)
        {
            var newNode = new InputTreeNode(prioritized);
            foreach (var child in prioritized.Children) newNode.AddChild(child);
            foreach (var child in other.Children) newNode.AddChild(child);
            return newNode;
        }

        private static InputTreeNode MergeNodesIfInside(InputTreeNode prioritized, InputTreeNode other)
        {
            return MergeNodeAuxiliar1(prioritized, other, false);
        }

        private static InputTreeNode MergeNodesIfContains(InputTreeNode prioritized, InputTreeNode other)
        {
            return MergeNodeAuxiliar1(prioritized, other, true);
        }

        private static InputTreeNode MergeNodeAuxiliar1(InputTreeNode prioritized, InputTreeNode other, bool reversePriority)
        {
            if (reversePriority)
            {
                var aux = other;
                other = prioritized;
                prioritized = aux;
            }
            var newNode = new InputTreeNode
            {
                Base = prioritized.Base,
                Min = other.Min,
            };
            //greedy longest match
            var newNode2 = new InputTreeNode { Base = prioritized.Base };

            //note that min and max values can't be both the  same

            if (prioritized.Min == other.Min)
            {
                newNode.Max = prioritized.Max;
                if (newNode.Min != newNode.Max) newNode.Extra = "+";

                newNode2.SetMinMax(0, other.Max - prioritized.Max);
                foreach (var child in other.Children) newNode2.AddChild(child);

                if (!reversePriority) foreach (var child in prioritized.Children) newNode.AddChild(child);
                newNode.AddChild(newNode2);
                if (reversePriority) foreach (var child in prioritized.Children) newNode.AddChild(child);

                return newNode;
            }

            if (prioritized.Max == other.Max)
            {
                newNode.Max = prioritized.Min - 1;
                if (newNode.Min != newNode.Max) newNode.Extra = "+";

                newNode2.SetMinMax(1, other.Max - prioritized.Min + 1);

                if (!reversePriority) foreach (var child in prioritized.Children) newNode2.AddChild(child);
                foreach (var child in other.Children) newNode2.AddChild(child);
                if (reversePriority) foreach (var child in prioritized.Children) newNode2.AddChild(child);

                newNode.AddChild(newNode2);
                foreach (var child in other.Children) newNode.AddChild(child);

                return newNode;
            }

            //no equal borders

            newNode.Max = prioritized.Min - 1;
            if (newNode.Min != newNode.Max) newNode.Extra = "+";

            newNode2.SetMinMax(1, prioritized.Max - prioritized.Min+1);
            if(newNode2.Min!=newNode2.Max) newNode2.Extra = "+";//greedy longest match
            newNode.AddChild(newNode2);
            foreach (var child in other.Children)  newNode.AddChild(child);

            var newNode3 = new InputTreeNode
            {
                Base = prioritized.Base,
                Min = 0,
                Max = other.Max - prioritized.Max
            };
            foreach (var child in other.Children) newNode3.AddChild(child);

            if (!reversePriority) foreach (var child in prioritized.Children) newNode2.AddChild(child);
            newNode2.AddChild(newNode3);
            if (reversePriority) foreach (var child in prioritized.Children) newNode2.AddChild(child);

            return newNode;
        }

        private static InputTreeNode MergeNodeAuxiliar2(InputTreeNode prioritized, InputTreeNode other,
            bool reversePriority)
        {
            if (reversePriority)
            {
                var aux = other;
                other = prioritized;
                prioritized = aux;
            }

            var newNode = new InputTreeNode { Base = prioritized.Base };

            /*TODO
             //IFCROSSES
              newNode.SetMinMax(prioritized.Min,other.Min);
             newNode.Extra = "+";//greedy longest match

             var newNode2 = new InputTreeNode { Base = prioritized.Base };
             newNode2.SetMinMax(0,prioritized.Max-other.Min);
             newNode2
                */
            return newNode;
        }

        private static InputTreeNode MergeNodesOutter(
            InputTreeNode prioritized, InputTreeNode other,
            bool prioritizeComesAfter
            )
        {
            if (prioritizeComesAfter)
            {
                var aux = other;
                other = prioritized;
                prioritized = aux;
            }

            var newNode = new InputTreeNode { Base = prioritized.Base, Min = prioritized.Min, Max = prioritized.Max };
            if (newNode.Min != newNode.Max) newNode.Extra = "+";
            var newNode2 = new InputTreeNode { Base = prioritized.Base, Min = other.Min - prioritized.Max, Max = other.Max - other.Min};
            if (newNode2.Min != newNode2.Max) newNode.Extra = "+";
            foreach (var child in other.Children) newNode2.AddChild(child);

            if (!prioritizeComesAfter) foreach (var child in prioritized.Children) newNode.AddChild(child);
            newNode.AddChild(newNode2);
            if (prioritizeComesAfter) foreach (var child in prioritized.Children) newNode.AddChild(child);

            return newNode;
        }
    }
}
