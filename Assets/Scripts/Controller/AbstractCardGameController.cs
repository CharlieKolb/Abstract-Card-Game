using UnityEngine;

public abstract class AbstractCardGameController : MonoBehaviour {
    public CreatureArea creatureArea;
    public HandArea handArea;
    public Player player;

    public InteractionManager im;

    public bool opposing = false;

    protected abstract void doInstantiate();
    public void Instantiate(Player player) {
        this.player = player;

        // Might as well instantiate prefabs here instead...
        handArea = GetComponentInChildren<HandArea>();
        creatureArea = GetComponentInChildren<CreatureArea>();
        im = GetComponentInChildren<InteractionManager>();

        handArea.Init(this, player.side.hand);
        creatureArea.Init(this, player.side.creatures);


        doInstantiate();
    }
}