using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;


public class CardGamePlayerController : AbstractCardGameController {
        
    protected override void doInstantiate() {
    }

    protected override async Task<Interaction> doSelectInteraction(List<Interaction> interactions) {
        
        var t = new TaskCompletionSource<Interaction>();
        im.updateInteractions(interactions, (i) => t.SetResult(i));

        return await t.Task;
    }
}