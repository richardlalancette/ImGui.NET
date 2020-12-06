namespace ImGui.NET.SampleProgram
{
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