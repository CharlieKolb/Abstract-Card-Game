using System;
using UnityEngine;

public class CardGameAiController : AbstractCardGameController {
    protected override void doInstantiate() {
    }

    public override bool passesTurn() {
        return true;
    }
}