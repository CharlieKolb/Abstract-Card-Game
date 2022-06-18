using System.Linq;
using UnityEngine;

public class CardGameAiController : AbstractCardGameController {
    private bool willPassTurn = true;

    private float delayBetweenActions = 1f;
    private float activeDelayLeft;
    
    protected override void doInstantiate() {
        activeDelayLeft = delayBetweenActions;
        GS.entityActionHandler.after.onAll((_) => activeDelayLeft = 0.5f);
        GS.cardCollectionActionHandler.after.onAll((_) => activeDelayLeft = 0.5f);
    }

    public void Update() {
        activeDelayLeft -= Time.deltaTime;
        if (activeDelayLeft > 0) return;

        activeDelayLeft = delayBetweenActions;

        if (this != GS.gameStateData.activeController) return;

        if (willPassTurn) return;

        var side = player.side;
        var hand = side.hand;
        var creatureArea = side.creatures;
        var first = hand.getExisting().FirstOrDefault(e => e.value.canUseFromHand());
        if (first != null) {
            hand.remove(first.value);
            first.value.use();
        }
        willPassTurn = true;
    }

    public override bool passesTurn() {
        if (willPassTurn) {
            willPassTurn = false;
            return true;
        }
        
        return false;
    }
}