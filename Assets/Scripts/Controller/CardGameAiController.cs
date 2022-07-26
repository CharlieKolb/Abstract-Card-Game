using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public class CardGameAiController : AbstractCardGameController {
    private bool saccedThisTurn;

    protected override void doInstantiate() {
        // This shouldn't listen to this directly, we should instead propagate an event as with the interactions
        GS.ga_global.phaseActionHandler.after.on(PhaseActionKey.ENTER, (x) => {
            if (x.phase == Phases.drawPhase && GS.gameStateData_global.activeController == this) {
                saccedThisTurn = false;
            }
        });
    }

    protected override async Task<Interaction> doSelectInteraction(List<Interaction> interactions) {
        if (saccedThisTurn) interactions = interactions.Where(x => !(x is SacCardInteraction)).ToList();
        var selected = interactions[Random.Range(0, interactions.Count)];
        if (selected is SacCardInteraction) saccedThisTurn = true;

        await Task.Delay(500);

        return selected;
    }
}