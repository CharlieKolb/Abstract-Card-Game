using UnityEngine;

public abstract class AbstractCardGameController : MonoBehaviour {
    protected CreatureArea creatureArea;
    protected HandArea handArea;
    public Player player;


    protected abstract void doInstantiate();
    public void Instantiate(Player player) {
        this.player = player;

        // Might as well instantiate prefabs here instead...
        handArea = GetComponentInChildren<HandArea>();
        creatureArea = GetComponentInChildren<CreatureArea>();

        handArea.SetCollection(player.side.hand);
        creatureArea.SetCollection(player.side.creatures);


        doInstantiate();
    }

    protected void tryUseCardFromHand(Card card) {
        if (card.canUseFromHand(player)) {
            GS.EnqueueInteraction(new PlayCardInteraction(card, handArea, player));
        }
    }

    protected void tryUseCreature(CreatureEntity creature) {
        if (GS.gameStateData.currentTurn.currentPhase == Phases.battlePhase) {
            var ownArea = GS.gameStateData.activeController.creatureArea;
            var otherArea = GS.gameStateData.passiveController.creatureArea;

            var idx = ownArea.collection.find(creature);
            var opponent = otherArea.collection[idx];
            if (opponent == null) {
                // direct attack
                new DamagePlayerEffect(creature.stats.attack, GS.gameStateData.passiveController.player).apply(player);
            }
            else {
                new DamageCreatureEffect(creature.stats.attack, opponent).apply(player);
                new DamageCreatureEffect(opponent.stats.attack, creature).apply(opponent.owner);
            }
            
        }
    }

    public void tryPassPhase() {
        if (GS.gameStateData.activeController != this) return;

        // Note that this is current spammable, may wish to check whether there's already a passPhase object in the queue
        GS.EnqueueInteraction(new PassPhaseInteraction());
    }
}