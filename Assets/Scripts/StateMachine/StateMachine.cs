using System;
using System.Collections.Generic;
using Utility.Assertions;

public class StateMachine
{
    public IState CurrentState { get; private set; }
    private readonly Dictionary<Type, List<Transition>> _transitionMap;
    private List<Transition> _currentTransitions;
    private readonly List<Transition> _emptyTransitions;

    public StateMachine()
    {
        _transitionMap = new Dictionary<Type, List<Transition>>();
        _emptyTransitions = new List<Transition>(0);
        _currentTransitions = _emptyTransitions;
    }

    public void AddTransition(IState from, IState to, Func<bool> predicate)
    {
        if (!_transitionMap.TryGetValue(from.GetType(), out List<Transition> _))
            _transitionMap[from.GetType()] = new List<Transition>(0);

        _transitionMap[from.GetType()].Add(new Transition(to, predicate));
    }

    public void Tick()
    {
        Transition? transition = GetTransition();
        if (transition != null) ChangeState(transition.Value.To);

        CurrentState?.Tick();
    }

    public void ChangeState(IState to)
    {
        Assert.IsNotNull(to, "StateMachine/ChangeState: to state cannot be null");

        if (CurrentState == to) return;

        // Debug.Log($"from {CurrentState} to {to}");

        CurrentState?.OnExit();
        CurrentState = to;
        CurrentState.OnEnter();

        if (!_transitionMap.TryGetValue(CurrentState.GetType(), out _currentTransitions))
            _currentTransitions = _emptyTransitions;
    }


    private Transition? GetTransition()
    {
        foreach (Transition transition in _currentTransitions)
        {
            if (transition.Predicate())
                return transition;
        }

        return null;
    }

    private readonly struct Transition
    {
        public readonly IState To;
        public readonly Func<bool> Predicate;

        public Transition(IState to, Func<bool> predicate)
        {
            To = to;
            Predicate = predicate;
        }
    }
}