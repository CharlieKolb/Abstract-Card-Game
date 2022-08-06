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

public class ReactionActionObserver<Key, ArgType> : ReactionActionHandler {

    public ReactionActionObserver(ReactionActionHandler parent = null) : base(parent) {}

}

public class ReactionActionHandler
{
    ReactionActionHandler parent; // Can be null. Child events also invoke parent

    Dictionary<string, List<Func<Invokable, Invokable>>> actions;
    List<Func<OnAllContext<string, Invokable>, OnAllContext<string, Invokable>>> actionsOnAll;

    public ReactionActionHandler(ReactionActionHandler parent = null) {
        this.parent = parent;
        actions = new Dictionary<string, List<Func<Invokable, Invokable>>>();
        actionsOnAll = new List<Func<OnAllContext<string, Invokable>, OnAllContext<string, Invokable>>>();
    }

    private void initKey(string key)
    {
        if (!actions.ContainsKey(key))
        {
            actions[key] = new List<Func<Invokable, Invokable>>();
        }
    }

    public Action on<I>(string key, Func<I, I> action) where I : Invokable
    {
        initKey(key);

        actions[key].Add((i) => action((I) i));

        return () => {
            actions[key].Remove((Func<Invokable, Invokable>) action);
        };
    }

    // public Action onAll(Func<OnAllContext<Key, ArgType>, OnAllContext<Key, ArgType>> action)
    // {
    //     actionsOnAll.Add(action);

    //     return () => {
    //         actionsOnAll.Remove(action);
    //     };
    // }

    public I Trigger<I>(I invokable) where I : Invokable
    {
        if (parent != null) {
            invokable = parent.Trigger(invokable);
        }

        if (actions.ContainsKey(invokable.key)) {
            actions[invokable.key].ForEach(x => {
                invokable = (I) x.Invoke(invokable);
            });
        }
       
        actionsOnAll.ForEach(x => {
            invokable = (I) x.Invoke(
                new OnAllContext<string, Invokable> {
                    key = invokable.key,
                    arg = invokable,
                }
            ).arg;
        });
        return invokable;
    }

    public Action listen<I>(string key, Action<I> callback) where I : Invokable {
        return on<I>(key, (arg) => {
            callback(arg);
            return arg;
        });
    }

    // public Action listenAll(Key key, Action<Key, ArgType> callback) {
    //     return onAll(
    //         (context) => {
    //             callback(context.key, context.arg);
    //             return context;
    //         }
    //     );
    // }

}


public class GameActionObserver : GameActionHandler {
    public GameActionObserver(GameActionHandler parent = null) : base(parent) {
        before = new ReactionActionHandler(parent?.before);
        after = new ReactionActionHandler(parent?.after);
    }
}

public class GameActionHandler
{
    public ReactionActionHandler before;
    public ReactionActionHandler after;


    public GameActionHandler(GameActionHandler parent = null) {
        before = new ReactionActionHandler(parent?.before);
        after = new ReactionActionHandler(parent?.after);
    }

    public GS Invoke<I>(I invokable, Func<I, I> action) where I : Invokable
    {
        invokable = (I) before.Trigger(invokable);
        
        invokable = action(invokable);

        invokable = after.Trigger(invokable);

        return invokable.gameState;
    }
}


public class GameActionsUtil {
    public static Reactions.DECK.DRAW handleCardDraw(Reactions.DECK.DRAW pl) {
            var card = pl.side.deck.draw();
            pl.side.deck.Announce<Reactions.DECK.DRAW>(pl);                        
            pl.gameState = pl.gameState.ga.actionHandler.Invoke(
                new Reactions.HAND.ADDED(pl.gameState, card, pl.side.hand),
                (pl) => {
                    // TODO(PayloadLambdas)
                    pl.hand.add(pl.card);
                    pl.hand.Announce(pl);
                    return pl;
                }
            );

            return pl;
    }
}