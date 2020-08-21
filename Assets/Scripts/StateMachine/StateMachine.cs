using System;
using System.Collections.Generic;

public class StateMachine
{
    private IState _currentState;
    private readonly Dictionary<Type, List<Transition>> _transitionMap;
    private readonly List<Transition> _anyTransitions;

    public StateMachine()
    {
        _transitionMap = new Dictionary<Type, List<Transition>>();
        _anyTransitions = new List<Transition>();
    }

    public void AddTransition(IState from, IState to, Func<bool> predicate)
    {
        if (!_transitionMap.TryGetValue(from.GetType(), out List<Transition> _))
            _transitionMap[@from.GetType()] = new List<Transition>();

        _transitionMap[from.GetType()].Add(new Transition(to, predicate));
    }

    public void AddAnyTransition(IState to, Func<bool> predicate)
    {
        _anyTransitions.Add(new Transition(to, predicate));
    }

    public void Tick()
    {
        Transition? transition = GetTransition();
        if (transition != null) ChangeState(transition.Value.To);

        _currentState?.Tick();
    }

    public void ChangeState(IState to)
    {
        if (_currentState == to) return;

        _currentState.OnExit();
        _currentState = to;
        _currentState.OnEnter();
    }


    private Transition? GetTransition()
    {
        foreach (Transition transition in _anyTransitions)
        {
            if (transition.Predicate()) return transition;
        }

        foreach (Transition transition in _transitionMap[_currentState.GetType()])
        {
            if (transition.Predicate()) return transition;
        }

        return null;
    }

    private struct Transition
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

public interface IState
{
    void OnEnter();
    void Tick();
    void OnExit();
}