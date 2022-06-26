using System;
using UnityEngine;

public class CardGamePlayerController : AbstractCardGameController {
        
    protected override void doInstantiate() {
        handArea.onUse.AddListener((cardObject) => {
            // tryUseCardFromHand(cardObject.card);
        });

        creatureArea.onUse.AddListener((creatureObject) => {
            // tryUseCreature(creatureObject.creatureEntity);
        });
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.A)) tryPassPhase();
    }

}