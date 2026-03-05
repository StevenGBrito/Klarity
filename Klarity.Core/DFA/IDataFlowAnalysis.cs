using Microsoft.CodeAnalysis;

namespace Klarity.Core.DFA;

public interface IAnalysisState<T>
{
    bool Equals(T other);
}

public interface IDataFlowAnalysis<T> where T : IAnalysisState<T>
{
    T InitialState { get; }
    T Merge(T state1, T state2);
    T Transfer(T inputState, SyntaxNode node);
}
