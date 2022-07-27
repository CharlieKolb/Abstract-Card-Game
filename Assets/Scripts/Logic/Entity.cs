using System;

using Debug = UnityEngine.Debug;

public class Observable {
    ActionHandler<string, object> handler = new ActionHandler<string, object>();

    protected void Announce(string key, object data) {
        Debug.Log("B");

        handler.Trigger(key, data);
    }

    public void Subscribe(string key, Action<object> callback) {
        Debug.Log("1");

        handler.on(key, callback);
    }

    public void Unsubscribe(string key, Action<object> callback) {
        handler.off(key, callback);
    }
}

public class Entity : Observable, ITargetable {
}