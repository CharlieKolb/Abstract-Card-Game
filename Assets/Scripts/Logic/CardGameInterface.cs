using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine;

using System.Linq;

using System;


namespace CardGameInterface {

public class OnAllContext<Key, ArgType> {
    public Key key { get; set; }
    public ArgType arg { get; set; }
}

public class ActionHandler<Key, ArgType> {
    Dictionary<Key, List<Action<ArgType>>> actions;
    List<Action<OnAllContext<Key, ArgType>>> actionsOnAll;

    public ActionHandler() {
        actions = new Dictionary<Key, List<Action<ArgType>>>();
        actionsOnAll = new List<Action<OnAllContext<Key, ArgType>>>();
    }

    private void initKey(Key key) {
        if (!actions.ContainsKey(key)) {
            actions[key] = new List<Action<ArgType>>();
        }
    }

    public void on(Key key, Action<ArgType> action) {
        initKey(key);

        actions[key].Add(action);
    }

    public void onAll(Action<OnAllContext<Key, ArgType>> action) {
        actionsOnAll.Add(action);
    }

    public void offAll(Action<OnAllContext<Key, ArgType>> action) {
        actionsOnAll.Remove(action);
    }

    public void off(Key key, Action<ArgType> action) {
        actions[key].Remove(action);
    }

    public void Trigger(Key key, ArgType arg) {
        if (actions.ContainsKey(key)) actions[key].ForEach(x => x.Invoke(arg));
        actionsOnAll.ForEach(x => x.Invoke(new OnAllContext<Key, ArgType>{
            key = key,
            arg = arg,
        }));
    }
}


public class Element<Content> {
    public Content value { get; set; }
    public int index { get; set; }

}

public abstract class Collection<Content> where Content : Entity
{
    protected List<Content> content;

    public Collection() {
        this.content = new List<Content>();
    }
    public Collection(List<Content> content) {
        this.content = content;
    }

    public int Count => content.Count;
    public Content this[int i]
    {
        get { return content[i]; }
    }

    public IEnumerable<Element<Content>> getExisting() {
        for(var i = 0; i < content.Count; ++i) {
            var e = content[i];
            if (e == null) continue;
            yield return new Element<Content> {
                value = e,
                index = i,
            };
        }
    }
}


public abstract class CardCollection : Collection<Card>
{
    public virtual bool hidden() { return true; }

    public bool isEmpty() {
        return content.Count == 0;
    }

    public void add(Card card) {
        GS.cardCollectionActionHandler.before.Trigger(CardCollectionActionKey.COUNT_CHANGED, new CardCollectionPayload(this, Differ<Card>.FromAdded(card)));
        content.Add(card);
        GS.cardCollectionActionHandler.after.Trigger(CardCollectionActionKey.COUNT_CHANGED, new CardCollectionPayload(this, Differ<Card>.FromAdded(card)));
    }

    public void remove(Card card) {
        GS.cardCollectionActionHandler.before.Trigger(CardCollectionActionKey.COUNT_CHANGED, new CardCollectionPayload(this, Differ<Card>.FromRemoved(card)));
        content.Remove(card);
        GS.cardCollectionActionHandler.after.Trigger(CardCollectionActionKey.COUNT_CHANGED, new CardCollectionPayload(this, Differ<Card>.FromRemoved(card)));
    }

    public void Shuffle() {
        var count = content.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = content[i];
            content[i] = content[r];
            content[r] = tmp;
        }
    }

    public override string ToString()
    {
        return string.Join(",", content.AsEnumerable().Select((c) => c.ToString()));
    }
}

public class Config {
    public bool autoAdvanceTurns = false;
}

public abstract class Phase {
    public virtual void onEntry() {}
    public virtual void onExit() {}

    public virtual bool hasOptions() { return false; }
 
    public virtual Phase nextPhase() { return null; }
}

public class ITurnContext {}

public abstract class ITurn<TC> where TC : ITurnContext {
    public Phase currentPhase;
    public TC turnContext;

    protected ITurn(Phase startPhase, TC turnContext) {
        this.turnContext = turnContext;
        currentPhase = startPhase;
        startPhase.onEntry();
    }

    public bool hasOptions() {
        return currentPhase.hasOptions();
    }

    // return true if turn can be advanced again
    public bool advance() {
        var nextPhase = currentPhase.nextPhase();
        currentPhase.onExit();
        onChange(currentPhase, nextPhase);
        if (nextPhase != null) {
            currentPhase = nextPhase;
            currentPhase.onEntry();
            return true;
        }
        return false;
    }

    public void onChange(Phase current, Phase next) {}

    public abstract void startTurn();
}


public enum VictoryState {
    Playing,
    Won,
    Lost,
}

public abstract class IPlayer {
    public bool canWin => true;
    public bool canLose => true;


    public VictoryState victoryState = VictoryState.Playing;
    public System.Action onTrigger;
    public void triggerLoss() {
        if (!canLose) return;

        victoryState = VictoryState.Lost;
        onTrigger.Invoke();
    }
    public void triggerWin() {
        if (!canWin) return;
        victoryState = VictoryState.Won;
        onTrigger.Invoke();
    }
}


public abstract class TurnGame<Turn, TC, Player>
    where TC : ITurnContext
    where Turn : ITurn<TC>
    where Player : AbstractCardGameController
{

    public Config getConfig() { return new Config(); }
    public abstract IEnumerable<Player> getNextPlayer(); // return the next player, in a 2 person game with infinite turns this would be an infinite iterator with the first player being returned first and from there alternating with the other player

    public abstract Turn makeTurn();

    // Call to pass
    public IEnumerator<bool> advance() {
        foreach (var controller in getNextPlayer()) {
            bool gameEnded = false;
            controller.player.onTrigger = () => gameEnded = true;

            var turn = makeTurn();
            turn.startTurn();
            do {
                if (gameEnded) yield return true;
                yield return false;
            } while (turn.advance());

        }
        yield break;
    }
}
}