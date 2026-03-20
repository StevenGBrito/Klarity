using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Klarity.Core.CFG;

public class CfgBuilder : CSharpSyntaxWalker
{
    private ControlFlowGraph _graph;
    private BasicBlock _currentBlock;
    private int _blockCounter = 0;

    public ControlFlowGraph Build(SyntaxNode root)
    {
        _blockCounter = 0;
        var entry = CreateBlock();
        var exit = CreateBlock(); // Placeholder exit
        _graph = new ControlFlowGraph(entry, exit);
        _currentBlock = entry;

        Visit(root);

        // Connect last block to exit if not already connected (and not a return/break flow)
        if (_currentBlock != null && !_currentBlock.Successors.Any())
        {
            _currentBlock.AddSuccessor(_graph.ExitBlock);
        }

        return _graph;
    }

    private BasicBlock CreateBlock()
    {
        var block = new BasicBlock(++_blockCounter);
        if (_graph != null) _graph.AddBlock(block);
        return block;
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        // Start a new graph for the method body if we were building per-method graphs.
        // For this simple version, we assume we visit a method body directly or just flow through.
        base.VisitMethodDeclaration(node);
    }

    public override void VisitBlock(BlockSyntax node)
    {
        foreach (var statement in node.Statements)
        {
            Visit(statement);
        }
    }

    // Handle simple statements
    public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node) => AddStatement(node);
    public override void VisitExpressionStatement(ExpressionStatementSyntax node) => AddStatement(node);
    
    private void AddStatement(SyntaxNode node)
    {
        _currentBlock.AddStatement(node);
    }

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        var conditionBlock = _currentBlock;
        conditionBlock.AddStatement(node.Condition);

        var thenBlock = CreateBlock();
        var elseBlock = node.Else != null ? CreateBlock() : null;
        var mergeBlock = CreateBlock();

        // Connect condition to branches
        conditionBlock.AddSuccessor(thenBlock);
        if (elseBlock != null)
            conditionBlock.AddSuccessor(elseBlock);
        else
            conditionBlock.AddSuccessor(mergeBlock);

        // Visit Then
        _currentBlock = thenBlock;
        Visit(node.Statement);
        if (_currentBlock != null) _currentBlock.AddSuccessor(mergeBlock);

        // Visit Else
        if (elseBlock != null)
        {
            _currentBlock = elseBlock;
            Visit(node.Else.Statement);
            if (_currentBlock != null) _currentBlock.AddSuccessor(mergeBlock);
        }

        _currentBlock = mergeBlock;
    }

    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        var conditionBlock = CreateBlock();
        var bodyBlock = CreateBlock();
        var exitBlock = CreateBlock();

        // Connect current to condition (loop start)
        _currentBlock.AddSuccessor(conditionBlock);

        // Handle Condition
        _currentBlock = conditionBlock;
        _currentBlock.AddStatement(node.Condition);
        _currentBlock.AddSuccessor(bodyBlock); // True
        _currentBlock.AddSuccessor(exitBlock); // False

        // Handle Body
        _currentBlock = bodyBlock;
        Visit(node.Statement);
        if (_currentBlock != null) _currentBlock.AddSuccessor(conditionBlock); // Loop back

        // Continue after loop
        _currentBlock = exitBlock;
    }

    public override void VisitForEachStatement(ForEachStatementSyntax node)
    {
        var conditionBlock = CreateBlock();
        var bodyBlock = CreateBlock();
        var exitBlock = CreateBlock();

        _currentBlock.AddSuccessor(conditionBlock);
        _currentBlock = conditionBlock;
        _currentBlock.AddStatement(node.Expression); 
        _currentBlock.AddSuccessor(bodyBlock);
        _currentBlock.AddSuccessor(exitBlock);

        _currentBlock = bodyBlock;
        Visit(node.Statement);
        if (_currentBlock != null) _currentBlock.AddSuccessor(conditionBlock);

        _currentBlock = exitBlock;
    }

    public override void VisitForStatement(ForStatementSyntax node)
    {
        if (node.Declaration != null) AddStatement(node.Declaration);
        foreach (var init in node.Initializers) AddStatement(init);

        var conditionBlock = CreateBlock();
        var bodyBlock = CreateBlock();
        var incrementBlock = CreateBlock();
        var exitBlock = CreateBlock();

        _currentBlock.AddSuccessor(conditionBlock);
        _currentBlock = conditionBlock;
        if (node.Condition != null) _currentBlock.AddStatement(node.Condition);
        _currentBlock.AddSuccessor(bodyBlock);
        _currentBlock.AddSuccessor(exitBlock);

        _currentBlock = bodyBlock;
        Visit(node.Statement);
        if (_currentBlock != null) _currentBlock.AddSuccessor(incrementBlock);

        _currentBlock = incrementBlock;
        foreach (var inc in node.Incrementors) AddStatement(inc);
        _currentBlock.AddSuccessor(conditionBlock);

        _currentBlock = exitBlock;
    }

    public override void VisitSwitchStatement(SwitchStatementSyntax node)
    {
        var conditionBlock = _currentBlock;
        conditionBlock.AddStatement(node.Expression);
        var exitBlock = CreateBlock();

        foreach (var section in node.Sections)
        {
            var sectionBlock = CreateBlock();
            conditionBlock.AddSuccessor(sectionBlock);
            _currentBlock = sectionBlock;
            foreach (var label in section.Labels) _currentBlock.AddStatement(label);
            foreach (var stmt in section.Statements) Visit(stmt);
            if (_currentBlock != null) _currentBlock.AddSuccessor(exitBlock);
        }

        _currentBlock = exitBlock;
    }

    public override void VisitReturnStatement(ReturnStatementSyntax node)
    {
        AddStatement(node);
        _currentBlock.AddSuccessor(_graph.ExitBlock);
        _currentBlock = null; // Following code is unreachable
    }
}
