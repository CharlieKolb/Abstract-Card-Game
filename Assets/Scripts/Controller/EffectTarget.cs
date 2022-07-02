using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class EffectTarget {
    public Predicate<EffectTargetContext> hasValidTargetCondition { set; get; }
    public Predicate<(GameObject, EffectTargetContext)> isValidTargetCondition;
    public Action<(GameObject, EffectTargetContext)> callback { set; get; }
    public bool called = false;
}

static class EffectTargets {
    public static Func<Action<int>, EffectTarget> targetEmptyFriendlyField = (cb) => new EffectTarget {
        hasValidTargetCondition = (x) => new List<int>{ 0, 1, 2, 3, 4 }.Any(i => x.owner.side.creatures[i] == null),
        isValidTargetCondition = (x) => {
            if (x.Item2.owner != GS.gameStateData.activeController.player) return false;
            var cf = x.Item1.GetComponent<CreatureField>();
            return cf != null && x.Item2.owner.side.creatures[cf.index] == null;
        },
        callback = (x) => cb((int) x.Item1.GetComponent<CreatureField>().index), 
    };
}