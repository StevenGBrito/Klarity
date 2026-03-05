namespace Klarity.Core.CFG;

public class ControlFlowGraph
{
    public List<BasicBlock> Blocks { get; } = new();
    public BasicBlock EntryBlock { get; private set; }
    public BasicBlock ExitBlock { get; private set; }

    public ControlFlowGraph(BasicBlock entryBlock, BasicBlock exitBlock)
    {
        EntryBlock = entryBlock;
        ExitBlock = exitBlock;
        Blocks.Add(entryBlock);
        if (exitBlock != entryBlock)
        {
            Blocks.Add(exitBlock);
        }
    }

    public void AddBlock(BasicBlock block)
    {
        if (!Blocks.Contains(block))
        {
            Blocks.Add(block);
        }
    }
}
