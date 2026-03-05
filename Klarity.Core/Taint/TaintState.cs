using Klarity.Core.DFA;

namespace Klarity.Core.Taint;

public class TaintState : IAnalysisState<TaintState>
{
    public HashSet<string> TaintedVariables { get; }

    public TaintState()
    {
        TaintedVariables = new HashSet<string>();
    }

    public TaintState(IEnumerable<string> tainted)
    {
        TaintedVariables = new HashSet<string>(tainted);
    }

    public bool Equals(TaintState other)
    {
        return TaintedVariables.SetEquals(other.TaintedVariables);
    }

    public static TaintState Merge(TaintState s1, TaintState s2)
    {
        var merged = new HashSet<string>(s1.TaintedVariables);
        merged.UnionWith(s2.TaintedVariables);
        return new TaintState(merged);
    }
}
