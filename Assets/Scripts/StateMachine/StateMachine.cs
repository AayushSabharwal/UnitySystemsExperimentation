using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    public IState CurrentState { get; private set; }
    private readonly Dictionary<Type, List<Transition>> _transitionMap;
    private List<Transition> _currentTransitions;
    private readonly List<Transition> _anyTransitions;
    private readonly List<Transition> _emptyTransitions;

    public StateMachine()
    {
        _transitionMap = new Dictionary<Type, List<Transition>>();
        _anyTransitions = new List<Transition>();
        _currentTransitions = new List<Transition>();
        _emptyTransitions = new List<Transition>(0);
    }

    public void AddTransition(IState from, IState to, Func<bool> predicate)
    {
        if (!_transitionMap.TryGetValue(from.GetType(), out List<Transition> _))
            _transitionMap[from.GetType()] = new List<Transition>(0);

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

        CurrentState?.Tick();
    }

    public void ChangeState(IState to)
    {
        if(to == null) return;
        if (CurrentState == to) return;
        
        CurrentState?.OnExit();
        CurrentState = to;
        if (!_transitionMap.TryGetValue(CurrentState.GetType(), out _currentTransitions))
        {
            _currentTransitions = _emptyTransitions;
        }
        CurrentState?.OnEnter();
    }


    private Transition? GetTransition()
    {
        foreach (Transition transition in _anyTransitions)
        {
            if (transition.Predicate()) return transition;
        }
        
        foreach (Transition transition in _currentTransitions)
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