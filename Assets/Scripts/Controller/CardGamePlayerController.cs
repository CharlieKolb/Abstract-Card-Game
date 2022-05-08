using System;
using UnityEngine;

public class CardGamePlayerController : AbstractCardGameController {
        
    protected override void doInstantiate() {
        handArea.onCardUse.AddListener((cardObject) => {
            var card = cardObject.card;
            if (card.canUseFromHand()) {
                card.use();
                handArea.collection.remove(card);
            }
        });
    }

    bool keyPressed = false;
    void Update() {
        if (!keyPressed)
            keyPressed = Input.GetKeyDown(KeyCode.A);
    }

    public override bool passesTurn() {
        var didPass = keyPressed;
        keyPressed = false;
        return didPass;
    }

}