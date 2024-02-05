using System;
using System.Threading.Tasks;

namespace Riateu.Components;

/// <summary>
/// A component that use to define the state behavior from an enum as a state.
/// </summary>
/// <typeparam name="S">An enum contains the state name</typeparam>
public class StateMachine<S> : Component 
where S: unmanaged, Enum
{
    private unsafe static int SToIdx(S state) => *(int*)(&state);
    private unsafe static S IdxToS(int idx) => *(S*)(&idx);
    private Action[] ready;
    private Func<S>[] updates;
    private Action[] exit;
    private Func<Task>[] coroutines;
    private S state;
    private S previousState;

    /// <summary>
    /// A current active state which can also be set.
    /// </summary>
    public S State 
    {
        get => state;
        set 
        {
            previousState = state;
            state = value;
            var pidx = SToIdx(previousState);
            var sIdx = SToIdx(state);
            if (pidx == sIdx)
                return;
            if (pidx != -1) 
            {
                exit[pidx]?.Invoke();
            }
            ready[sIdx]?.Invoke();
            var corou = coroutines[sIdx];
            if (corou != null) 
            {
                coroutine.Run(corou);
            }
        }
    }

    private Coroutine coroutine;

    /// <summary>
    /// An initialization for this component.
    /// </summary>
    public unsafe StateMachine() 
    {
        var count = Enum.GetValues<S>().Length;
        ready = new Action[count];
        updates = new Func<S>[count];
        exit = new Action[count];
        coroutines = new Func<Task>[count];

        coroutine = new Coroutine();
        state = IdxToS(-1);
    }

    /// <summary>
    /// Add a state from a state index.
    /// </summary>
    /// <param name="state">An enum state to be defined as index</param>
    /// <param name="update">A required callback that called on a game loop, and possibly update a state</param>
    /// <param name="ready">A callback that called when this state has set</param>
    /// <param name="exit">A callback that called when the current state has been replaced</param>
    /// <param name="coroutine">A callback that called after it has started</param>
    public void AddState(S state, Func<S> update, Action ready = null, Action exit = null, Func<Task> coroutine = null) 
    {
        int index = SToIdx(state);
        this.ready[index] = ready;
        this.updates[index] = update;
        this.exit[index] = exit;
        this.coroutines[index] = coroutine;
    }

    /// <inheritdoc/>
    public override unsafe void Update(double delta)
    {
        var update = updates[SToIdx(state)];
        if (update != null) 
        {
            State = update();
        }
        if (coroutine.Active) 
        {
            coroutine.Update(delta);
        }
    }
}