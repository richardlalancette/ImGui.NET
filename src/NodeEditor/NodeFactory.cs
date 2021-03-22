using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGui.Extensions;

namespace NodeEditor
{
    public class NodeFactory
    {
        public static Node CreateInstance(Type type, Vector2 scenePos, int index)
        {
            Node instance = (Node) Activator.CreateInstance(type, index, "New node", scenePos, new NodeData(), 2, 2);
            return instance;
        }
        
        public static IEnumerable<Type> TypeList()
        {
            IEnumerable<Type> nodeTypes = typeof(Node)
                .Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Node)) && !t.IsAbstract)
                .Select(t => t);
                
            return nodeTypes;
        }
    }
}