using System;
using System.Collections.Generic;

public class OnAllContext<Key, ArgType>
{
    public Key key { get; set; }
    public ArgType arg { get; set; }

    public override string ToString()
    {
        return "(" + key.ToString() + ", " + arg.ToString() + ")";
    }
}

public class ActionHandler<Key, ArgType>
{
    ActionHandler<Key, ArgType> parent; // Can be null. Child events also invoke parent

    Dictionary<Key, List<Action<ArgType>>> actions;
    List<Action<OnAllContext<Key, ArgType>>> actionsOnAll;

    public ActionHandler(ActionHandler<Key, ArgType> parent = null) {
        this.parent = parent;
        actions = new Dictionary<Key, List<Action<ArgType>>>();
        actionsOnAll = new List<Action<OnAllContext<Key, ArgType>>>();
    }

    private void initKey(Key key)
    {
        if (!actions.ContainsKey(key))
        {
            actions[key] = new List<Action<ArgType>>();
        }
    }

    public void on(Key key, Action<ArgType> action)
    {
        initKey(key);

        actions[key].Add(action);
    }

    public void onAll(Action<OnAllContext<Key, ArgType>> action)
    {
        actionsOnAll.Add(action);
    }

    public void offAll(Action<OnAllContext<Key, ArgType>> action)
    {
        actionsOnAll.Remove(action);
    }

    public void off(Key key, Action<ArgType> action)
    {
        actions[key].Remove(action);
    }

    public void Trigger(Key key, ArgType arg)
    {
        if (parent != null) parent.Trigger(key, arg);

        if (actions.ContainsKey(key)) actions[key].ForEach(x => x.Invoke(arg));
       
        actionsOnAll.ForEach(x => x.Invoke(new OnAllContext<Key, ArgType>
        {
            key = key,
            arg = arg,
        }));
    }
}

public class GameActionHandler<ArgType>
{
    public ActionHandler<string, ArgType> before;
    public ActionHandler<string, ArgType> after;

    Engine engine;

    public GameActionHandler(Engine engine, GameActionHandler<ArgType> parent = null) {
        before = new ActionHandler<string, ArgType>(parent?.before);
        after = new ActionHandler<string, ArgType>(parent?.after);
        this.engine = engine;
    }

    // should be moved to the engine, with before and after being part of the state
    // before and after will probably take and return gamestates
    public void Invoke(string key, ArgType argType, Action action)
    {
        GS.PushAction(
            () => before.Trigger(key, argType),
            action,
            () => after.Trigger(key, argType)
        );
    }
}