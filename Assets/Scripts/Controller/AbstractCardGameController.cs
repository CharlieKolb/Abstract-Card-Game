using UnityEngine;

public abstract class AbstractCardGameController : MonoBehaviour {
    public CreatureArea creatureArea;
    public HandArea handArea;
    public Player player;

    public bool opposing = false;

    protected abstract void doInstantiate();
    public void Instantiate(Player player) {
        this.player = player;

        // Might as well instantiate prefabs here instead...
        handArea = GetComponentInChildren<HandArea>();
        creatureArea = GetComponentInChildren<CreatureArea>();

        handArea.Init(this, player.side.hand);
        creatureArea.Init(this, player.side.creatures);


        doInstantiate();
    }

    public void tryPassPhase() {
        if (GS.gameStateData.activeController != this) return;

        // Note that this is current spammable, may wish to check whether there's already a passPhase object in the queue
        GS.EnqueueInteraction(new PassPhaseInteraction());
    }
}