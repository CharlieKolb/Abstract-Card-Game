using System;

using Debug = UnityEngine.Debug;

public class Observable {
    ActionObserver<string, object> handler = new ActionObserver<string, object>();

    public void Announce(string key, object data) {
        handler.Trigger(key, data);
    }

    public Action Subscribe(string key, Action<object> callback) {
        return handler.listen(key, callback);
    }
}

public class Entity : Observable, ITargetable {
}