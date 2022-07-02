using UnityEngine;

public abstract class AbstractCardGameController : MonoBehaviour {
    [HideInInspector]
    public CreatureArea creatureArea;
    [HideInInspector]
    public HandArea handArea;
    [HideInInspector]
    public Player player;

    [HideInInspector]
    public InteractionManager im;

    public bool opposing = false;

    protected abstract void doInstantiate();
    public void Instantiate(Player player) {
        this.player = player;

        // Might as well instantiate prefabs here instead...
        handArea = GetComponentInChildren<HandArea>();
        creatureArea = GetComponentInChildren<CreatureArea>();
        im = GetComponentInChildren<InteractionManager>();

        handArea.Init(player.side.hand);
        creatureArea.Init(player.side.creatures);


        doInstantiate();
    }
}