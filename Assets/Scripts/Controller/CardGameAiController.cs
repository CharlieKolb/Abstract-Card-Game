using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using Action = System.Action;

public class CardGameAiController : AbstractCardGameController {

    public float timeBetweenActions;
    
    private bool saccedThisTurn;

    private void resetTurnState() {
        saccedThisTurn = false;
    }

    protected override void doInstantiate() {
        // This shouldn't listen to this directly, we should instead propagate an event as with the interactions
        Phases.drawPhase.Subscribe(PhaseActionKey.ENTER, (x) => {
            var payload = (PhasePayload) x;
            if (payload.activePlayer == this.player) {
                resetTurnState();
            }
        });
    }

    IEnumerator waitFor(float seconds, Action callback) {
        yield return new WaitForSeconds(seconds);
        callback();
    }

    protected override async Task<Interaction> doSelectInteraction(List<Interaction> interactions) {
        if (saccedThisTurn) interactions = interactions.Where(x => !(x is SacCardInteraction)).ToList();
        var selected = interactions[Random.Range(0, interactions.Count)];
        if (selected is SacCardInteraction) saccedThisTurn = true;

        var t = new TaskCompletionSource<bool>();


        StartCoroutine(waitFor(timeBetweenActions, () => t.SetResult(true)));

        await t.Task;

        return selected;
    }
}