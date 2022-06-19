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

public class ITurnContext {}

public abstract class ITurn<TC> where TC : ITurnContext {
    public GamePhase currentPhase;
    public TC turnContext;

    public delegate void EndTurn();
    public event EndTurn endTurn;


    protected ITurn(GamePhase startPhase, TC turnContext) {
        this.turnContext = turnContext;
        currentPhase = startPhase;
    }

    public bool hasOptions() {
        return currentPhase.hasOptions();
    }

    // return true if turn can be advanced again
    public bool advance() {
        var nextPhase = currentPhase.nextPhase();
        currentPhase.executeExit();
        if (nextPhase == null) {
            endTurn.Invoke();
        }

        if (nextPhase != null) {
            currentPhase = nextPhase;
            currentPhase.executeEntry();
            return true;
        }
        return false;
    }

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

    public bool gameEnded = false;

    // Call to pass
    public IEnumerator<bool> advance() {
        foreach (var controller in getNextPlayer()) {
            var turn = makeTurn();
            bool turnRunning = true;
            turn.endTurn += () => {
                turnRunning = false;
            };
            turn.startTurn();
            do {
                if (gameEnded) yield break;
                yield return true;
            } while (turnRunning);
        }
        yield break;
    }
}
}