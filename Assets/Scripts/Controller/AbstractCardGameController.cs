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

    public abstract bool passesTurn();
}