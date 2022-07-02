using System.Linq;
using UnityEngine;

public class CardGameAiController : AbstractCardGameController {
    private float delayBetweenActions = 0.2f;
    private float activeDelayLeft;
    
    private bool saccedThisTurn;

    protected override void doInstantiate() {
        activeDelayLeft = delayBetweenActions;
        GS.entityActionHandler.after.onAll((_) => activeDelayLeft = 0.5f);
        GS.cardCollectionActionHandler.after.onAll((_) => activeDelayLeft = 0.5f);
        GS.phaseActionHandler.after.on(PhaseActionKey.ENTER, (x) => {
            if (x.phase == Phases.drawPhase && GS.gameStateData.activeController == this) {
                saccedThisTurn = false;
            }
        });
    }

    // LateUpdate to let GameState tick first
    private void LateUpdate() {
        activeDelayLeft -= Time.deltaTime;
        if (activeDelayLeft > 0) return;

        activeDelayLeft = delayBetweenActions;

        if (this != GS.gameStateData.activeController) return;

        var interactionOptions = im.getInteractions(); 

        if (saccedThisTurn) interactionOptions = interactionOptions.Where(x => !(x is SacCardInteraction)).ToList();

        if (interactionOptions.Count == 0) {
            return;
        }
        var interaction = interactionOptions[Random.Range(0, interactionOptions.Count)];
        if (interaction is SacCardInteraction) saccedThisTurn = true;

        GS.PushInteraction(interaction);
    }
}