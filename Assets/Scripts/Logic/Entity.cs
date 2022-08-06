using System;

using Debug = UnityEngine.Debug;

public class Observable {
    GameActionHandler handler = new GameActionHandler();

    public void Announce<I>(I invokable) where I : Invokable {
        handler.Invoke(invokable, (x) => x);
    }

    public Action Subscribe<I>(string key, Action<I> callback) where I : Invokable {
        return handler.after.listen(key, callback);
    }
}

public class Entity : Observable, ITargetable {
}