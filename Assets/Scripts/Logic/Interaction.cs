using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class Interaction {
    public bool executed = false;

    public IEnumerator<bool> execute() {
        var start = startExecute();
        while (start.MoveNext()) {
            yield return start.Current;
            if (start.Current == false) {
                yield break;
            }
        }

        doExecute();        
        executed = true;
    }
    protected virtual IEnumerator<bool> startExecute() { yield return true; }
    protected abstract void doExecute();

}

public class PassPhaseInteraction : Interaction {
    protected override void doExecute()
    {
        GS.gameStateData.currentTurn.advance();
    }

        // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        return true;
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        return 0;
    }

}

public class DeclareAttackInteraction : Interaction {
    public CreatureEntity creature;
    public Player owner;

    public DeclareAttackInteraction(CreatureEntity creature, Player owner) {
        this.creature = creature;
        this.owner = owner;
    }

    protected override void doExecute()
    {
        var ownCreatureCollection = GS.gameStateData.activeController.player.side.creatures;
        var otherCreatureCollection = GS.gameStateData.passiveController.player.side.creatures;

        var idx = ownCreatureCollection.find(creature);
        var opponent = otherCreatureCollection[idx];
        if (opponent == null) {
            // direct attack
            new DamagePlayerEffect(creature.stats.attack, GS.gameStateData.passiveController.player).apply(owner);
        }
        else {
            new DamageCreatureEffect(creature.stats.attack, opponent).apply(owner);
            new DamageCreatureEffect(opponent.stats.attack, creature).apply(owner);
        }
        creature.hasAttacked = true;
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        var oth = (DeclareAttackInteraction) obj;
        return creature == oth.creature; // we ignore player as creatures are unique
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        return creature.GetHashCode();
    }
}

public class PlayCardInteraction : Interaction {
    public Card target;
    public Hand hand;
    public Player owner;

    public PlayCardInteraction(Card target, Hand hand, Player owner) {
        this.target = target;
        this.hand = hand;
        this.owner = owner;
    }

    protected override IEnumerator<bool> startExecute() {
        var results = target.effects.Select(x => GS.ResolveEffectTargets(x, owner)).ToList();

        while (GS.isResolvingEffects) yield return true;

        if (results.Any(x => x != null && x.cancelled)) {
            results.ForEach(x => { if(x != null) x.reset(); });
            yield return false;
        }

        GS.energyActionHandler.Invoke(EnergyActionKey.PAY, new EnergyPayload(target.cost, target), () => {
            owner.side.energy = owner.side.energy.Without(target.cost);
        });
    }

    protected override void doExecute()
    {
        hand.remove(target);
        target.use(owner);
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        var oth = (PlayCardInteraction) obj;
        return target == oth.target; // we ignore player as creatures are unique
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        return target.GetHashCode();
    }

}

public class SacCardInteraction : Interaction {
    public Card target;
    public Hand hand;
    public Player owner;

    public SacCardInteraction(Card target, Hand hand, Player owner) {
        this.target = target;
        this.hand = hand;
        this.owner = owner;
    }

    protected override void doExecute()
    {
        hand.remove(target);
        owner.side.maxEnergy = owner.side.maxEnergy.With(target.sac);
        owner.side.energy = owner.side.energy.With(target.sac);
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        var oth = (SacCardInteraction) obj;
        return target == oth.target; // we ignore player as creatures are unique
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        return target.GetHashCode();
    }

}

public class SelectTargetInteraction : Interaction {
    EffectTarget effect;
    public GameObject target;
    EffectTargetContext context;

    public SelectTargetInteraction(EffectTarget effect, GameObject target, EffectTargetContext context) {
        this.effect = effect;
        this.target = target;
        this.context = context;
    }

    
    protected override void doExecute()
    {
        GS.target = null;
        effect.callback((target, context));
        effect.called = true;
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        var oth = (SelectTargetInteraction) obj;
        return target == oth.target; // we ignore player as creatures are unique
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        return target.GetHashCode();
    }
}

public class CancelSelectionInteraction : Interaction {
    EffectTarget effect;

    public CancelSelectionInteraction(EffectTarget effect) {
        this.effect = effect;
    }

    
    protected override void doExecute()
    {
        GS.CancelSelection();
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        var oth = (CancelSelectionInteraction) obj;
        return effect == oth.effect; // we ignore player as creatures are unique
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        return effect.GetHashCode();
    }

}