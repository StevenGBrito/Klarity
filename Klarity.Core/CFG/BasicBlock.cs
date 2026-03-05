using Microsoft.CodeAnalysis;

namespace Klarity.Core.CFG;

public class BasicBlock
{
    public int Id { get; }
    public List<SyntaxNode> Statements { get; } = new();
    public List<BasicBlock> Predecessors { get; } = new();
    public List<BasicBlock> Successors { get; } = new();

    public BasicBlock(int id)
    {
        Id = id;
    }

    public void AddStatement(SyntaxNode statement)
    {
        Statements.Add(statement);
    }

    public void AddSuccessor(BasicBlock block)
    {
        if (!Successors.Contains(block))
        {
            Successors.Add(block);
            block.Predecessors.Add(this);
        }
    }
}
