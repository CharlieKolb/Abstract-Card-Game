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
            handArea.collection.remove(card);
            card.use(player);
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
                Debug.Log("FGH");
                new DamagePlayerEffect(creature.stats.attack, GS.gameStateData.passiveController.player).apply(player);
            }
            else {
                new DamageCreatureEffect(creature.stats.attack, opponent).apply(player);
                new DamageCreatureEffect(opponent.stats.attack, creature).apply(opponent.owner);
            }
            
        }
    }

    public abstract bool passesTurn();
}