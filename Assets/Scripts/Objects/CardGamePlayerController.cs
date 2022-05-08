using System;
using UnityEngine;

public class CardGamePlayerController : MonoBehaviour {
    CreatureArea creatureArea;
    HandArea handArea;

    public void Start() {
        handArea = GetComponentInChildren<HandArea>();
        creatureArea = GetComponentInChildren<CreatureArea>();
    }

        
    public void Instantiate(Player player) {
        handArea.SetCollection(player.side.hand);
        creatureArea.SetCollection(player.side.creatures);

        handArea.onCardUse.AddListener((cardObject) => {
            var card = cardObject.card;
            if (card.canUseFromHand()) {
                card.use();
                handArea.collection.remove(card);
            }
        });
    }
}