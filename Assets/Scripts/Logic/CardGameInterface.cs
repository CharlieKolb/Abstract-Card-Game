using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine;

using System.Linq;

using System;


namespace CardGameInterface {

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