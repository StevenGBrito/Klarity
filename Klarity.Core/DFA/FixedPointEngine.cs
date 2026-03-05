using Klarity.Core.CFG;

namespace Klarity.Core.DFA;

public class FixedPointEngine<T> where T : IAnalysisState<T>
{
    private readonly ControlFlowGraph _cfg;
    private readonly IDataFlowAnalysis<T> _analysis;

    public FixedPointEngine(ControlFlowGraph cfg, IDataFlowAnalysis<T> analysis)
    {
        _cfg = cfg;
        _analysis = analysis;
    }

    public Dictionary<BasicBlock, T> Run()
    {
        var inputStates = new Dictionary<BasicBlock, T>();
        var outputStates = new Dictionary<BasicBlock, T>();

        // Init states
        foreach (var block in _cfg.Blocks)
        {
            inputStates[block] = _analysis.InitialState;
            outputStates[block] = _analysis.InitialState;
        }

        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (var block in _cfg.Blocks)
            {
                // Merge predecessors
                T input = _analysis.InitialState;
                if (block.Predecessors.Any())
                {
                    input = block.Predecessors.Select(p => outputStates[p]).Aggregate(_analysis.Merge);
                }
                
                if (!input.Equals(inputStates[block]))
                {
                    inputStates[block] = input;
                    changed = true;
                }

                // Transfer
                T output = input;
                foreach (var stmt in block.Statements)
                {
                    output = _analysis.Transfer(output, stmt);
                }

                if (!output.Equals(outputStates[block]))
                {
                    outputStates[block] = output;
                    changed = true;
                }
            }
        }

        return inputStates;
    }
}
