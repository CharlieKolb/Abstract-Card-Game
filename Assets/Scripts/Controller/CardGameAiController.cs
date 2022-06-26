using System.Linq;
using UnityEngine;

public class CardGameAiController : AbstractCardGameController {
    private bool willPassTurn = false;

    private float delayBetweenActions = 0.2f;
    private float activeDelayLeft;
    

    protected override void doInstantiate() {
        activeDelayLeft = delayBetweenActions;
        GS.entityActionHandler.after.onAll((_) => activeDelayLeft = 0.5f);
        GS.cardCollectionActionHandler.after.onAll((_) => activeDelayLeft = 0.5f);
        GS.phaseActionHandler.after.on(PhaseActionKey.ENTER, (x) => {
            if (x.phase == Phases.drawPhase && GS.gameStateData.activeController == this) {
                willPassTurn = false;
            }
        });
    }

    public void Update() {
        activeDelayLeft -= Time.deltaTime;
        if (activeDelayLeft > 0) return;

        activeDelayLeft = delayBetweenActions;

        if (this != GS.gameStateData.activeController) return;

        if (willPassTurn) {
            tryPassPhase();
            return;
        }

        var side = player.side;
        var hand = side.hand;
        var creatureArea = side.creatures;
        var first = hand.getExisting().FirstOrDefault(e => e.value.canUseFromHand(this.player));
        if (first != null) {
            // tryUseCardFromHand(first.value);
            willPassTurn = true;
        }
        tryPassPhase();
    }
}