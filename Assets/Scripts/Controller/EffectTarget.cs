using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class EffectTarget {
    public Predicate<EffectContext> hasValidTargetCondition { set; get; }
    public Predicate<EffectContext> isValidTargetCondition;
    public Action<EffectContext> callback { set; get; }
    public bool called = false;
    public bool cancelled = false;

    public void reset() {
        called = false;
        cancelled = false;
    }
}

static class EffectTargets {
    public static Func<Action<int>, EffectTarget> targetEmptyFriendlyField = (cb) => new EffectTarget {
        hasValidTargetCondition = (x) => new List<int>{ 0, 1, 2, 3, 4 }.Any(i => x.owner.side.creatures[i] == null),
        isValidTargetCondition = (x) => {
            if (x.owner != GS.gameStateData.activeController.player) return false;
            return x.targetIndex.HasValue && x.owner.side.creatures[x.targetIndex.Value] == null;
        },
        callback = (x) => cb(x.targetIndex.Value), 
    };
}