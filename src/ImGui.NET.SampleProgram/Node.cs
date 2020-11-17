using System.Numerics;

namespace ImGui.NET.SampleProgram
{
    class Node
    {
        public readonly int Id;
        public readonly string Name;
        public Vector2 Pos;
        public Vector2 Size;
        public float Value;
        public Vector4 Color;
        public int InputsCount;
        public int OutputsCount;
        public bool Selected;
        public bool Hovered;
        public bool Down;

        public Node(int id, string name, Vector2 pos, float value, Vector4 color, int inputsCount, int outputsCount)
        {
            Id = id;
            Name = name;
            Pos = pos;
            Size = new Vector2();
            Value = value;
            Color = color;
            InputsCount = inputsCount;
            OutputsCount = outputsCount;
            Selected = false;
            Hovered = false;
            Down = false;
        }

        public Vector2 GetInputSlotPos(int slotNo)
        {
            return new Vector2(Pos.X, Pos.Y + Size.Y * ((float) slotNo + 1) / ((float) InputsCount + 1));
        }

        public Vector2 GetOutputSlotPos(int slotNo)
        {
            return new Vector2(Pos.X + Size.X, Pos.Y + Size.Y * ((float) slotNo + 1) / ((float) OutputsCount + 1));
        }
    }

    class NodeLink
    {
        public int InputIdx;
        public int InputSlot;
        public int OutputIdx;
        public int OutputSlot;

        public NodeLink(int inputIdx, int inputSlot, int outputIdx, int outputSlot)
        {
            InputIdx = inputIdx;
            InputSlot = inputSlot;
            OutputIdx = outputIdx;
            OutputSlot = outputSlot;
        }
    }
}