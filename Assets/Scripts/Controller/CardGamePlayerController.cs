using System;
using UnityEngine;

public class CardGamePlayerController : AbstractCardGameController {
        
    protected override void doInstantiate() {
        handArea.onUse.AddListener((cardObject) => {
            tryUseCardFromHand(cardObject.card);
        });

        creatureArea.onUse.AddListener((creatureObject) => {
            tryUseCreature(creatureObject.creatureEntity);
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