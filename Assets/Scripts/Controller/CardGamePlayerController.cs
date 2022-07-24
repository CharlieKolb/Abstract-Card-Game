using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;


public class CardGamePlayerController : AbstractCardGameController {
        
    protected override void doInstantiate() {
    }

    protected override async Task<Interaction> doSelectInteraction(List<Interaction> interactions) {
        
        var t = new TaskCompletionSource<Interaction>();
        
        bool b = false;
        im.updateInteractions(interactions, (i) => {
            if (b) Debug.Log(i);
            else {
                // b = true;
                t.SetResult(i);
            }
            
        });

        return await t.Task;
    }
}