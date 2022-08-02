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

public class ActionObserver<Key, ArgType> : ActionHandler<Key, ArgType> {

    public ActionObserver(ActionHandler<Key, ArgType> parent = null) : base(parent) {}

}

public class ActionHandler<Key, ArgType>
{
    ActionHandler<Key, ArgType> parent; // Can be null. Child events also invoke parent

    Dictionary<Key, List<Func<ArgType, ArgType>>> actions;
    List<Func<OnAllContext<Key, ArgType>, OnAllContext<Key, ArgType>>> actionsOnAll;

    public ActionHandler(ActionHandler<Key, ArgType> parent = null) {
        this.parent = parent;
        actions = new Dictionary<Key, List<Func<ArgType, ArgType>>>();
        actionsOnAll = new List<Func<OnAllContext<Key, ArgType>, OnAllContext<Key, ArgType>>>();
    }

    private void initKey(Key key)
    {
        if (!actions.ContainsKey(key))
        {
            actions[key] = new List<Func<ArgType, ArgType>>();
        }
    }

    public Action on(Key key, Func<ArgType, ArgType> action)
    {
        initKey(key);

        actions[key].Add(action);

        return () => {
            actions[key].Remove(action);
        };
    }

    public Action onAll(Func<OnAllContext<Key, ArgType>, OnAllContext<Key, ArgType>> action)
    {
        actionsOnAll.Add(action);

        return () => {
            actionsOnAll.Remove(action);
        };
    }

    public ArgType Trigger(Key key, ArgType arg)
    {
        if (parent != null) {
            arg = parent.Trigger(key, arg);
        }

        if (actions.ContainsKey(key)) actions[key].ForEach(x => {
            arg = x.Invoke(arg);
        });
       
        actionsOnAll.ForEach(x => {
            arg = x.Invoke(
                new OnAllContext<Key, ArgType> {
                    key = key,
                    arg = arg,
                }
            ).arg;
        });
        return arg;
    }

    public Action listen(Key key, Action<ArgType> callback) {
        return on(key, (arg) => {
            callback(arg);
            return arg;
        });
    }

    public Action listenAll(Key key, Action<Key, ArgType> callback) {
        return onAll(
            (context) => {
                callback(context.key, context.arg);
                return context;
            }
        );
    }

}

public class GameActionObserver<P> : GameActionHandler<P> where P : PayloadBase {
    public GameActionObserver(Engine engine, GameActionHandler<P> parent = null) : base(engine, parent) {
        before = new ActionObserver<string, P>(parent?.before);
        after = new ActionObserver<string, GS>(parent?.after);
    }
}

public class GameActionHandler<Payload> where Payload : PayloadBase
{
    public ActionHandler<string, Payload> before;
    public ActionHandler<string, GS> after;

    Engine engine;

    public GameActionHandler(Engine engine, GameActionHandler<Payload> parent = null) {
        before = new ActionHandler<string, Payload>(parent?.before);
        after = new ActionHandler<string, GS>(parent?.after);
        this.engine = engine;
    }

    public GS Invoke(string key, Payload payload, Func<Payload, GS> action)
    {
        payload = before.Trigger(key, payload);
        
        var gs = action(payload);

        gs = after.Trigger(key, gs);

        return gs;
    }

    public GS Invoke(Invokable<Payload> invokable, Func<Payload, GS> action)
    {
        invokable.payload = before.Trigger(invokable.key, invokable.payload);
        
        var gs = action(invokable.payload);

        gs = after.Trigger(invokable.key, gs);

        return gs;
    }
}