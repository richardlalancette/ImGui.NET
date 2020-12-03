using System;
using System.Collections.Generic;

namespace ImGui.NET.SampleProgram
{
    class NodeFactory
    {
        private static List<Type> _types = new List<Type>();
        
        public static void RegisterType<T>()
        {
            _types.Add(typeof(T));            
        }
        
        public static IEnumerable<Type> TypeList()
        {
            return _types;
        }
    }
}