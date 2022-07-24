using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public class CardGameAiController : AbstractCardGameController {
    private bool saccedThisTurn;

    protected override void doInstantiate() {
        GS.ga.phaseActionHandler.after.on(PhaseActionKey.ENTER, (x) => {
            if (x.phase == Phases.drawPhase && GS.gameStateData.activeController == this) {
                saccedThisTurn = false;
            }
        });
    }

    protected override Task<Interaction> doSelectInteraction(List<Interaction> interactions) {
        if (saccedThisTurn) interactions = interactions.Where(x => !(x is SacCardInteraction)).ToList();
        var selected = interactions[Random.Range(0, interactions.Count)];
        if (selected is SacCardInteraction) saccedThisTurn = true;


        return Task.FromResult(selected);
    }
}