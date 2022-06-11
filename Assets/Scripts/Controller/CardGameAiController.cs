using System.Linq;
using UnityEngine;

public class CardGameAiController : AbstractCardGameController {
    private bool willPassTurn = true;
    
    protected override void doInstantiate() {
    }

    public void Update() {
        if (this != GS.activeController) return;

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