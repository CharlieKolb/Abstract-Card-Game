using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public abstract class Interaction {
    public bool executed = false;

    public async Task<GS> execute(GS gameState) {
        var start = await startExecute(gameState);

        if (!start) return null;

        return doExecute(gameState);        
    }
    protected virtual Task<bool> startExecute(GS gameState) { return Task.FromResult(true); }
    protected abstract GS doExecute(GS gameState);

}

public class PassPhaseInteraction : Interaction {
    protected override GS doExecute(GS gameState)
    {
        gameState = gameState.gameStateData.currentPhase.executeExit(gameState);

        return gameState;
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

    protected override GS doExecute(GS gameState)
    {
        var ownCreatureCollection = gameState.gameStateData.activeController.player.side.creatures;
        // TODO(TwoPlayer) - replace this with a "target player" logic
        var otherCreatureCollection = gameState.gameStateData.passiveControllers[0].player.side.creatures;

        var idx = ownCreatureCollection.find(creature);
        var opponent = otherCreatureCollection[idx];
        if (opponent == null) {
            // direct attack
            gameState = new DamagePlayerEffect(creature.stats.attack, gameState.gameStateData.passiveControllers[0].player).apply(gameState, owner);
        }
        else {
            gameState = new DamageCreatureEffect(creature.stats.attack, opponent).apply(gameState, owner);
            gameState = new DamageCreatureEffect(opponent.stats.attack, creature).apply(gameState, owner);
        }
        creature.hasAttacked = true;
        return gameState;
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
    public Card card;
    public Hand hand;
    public Player owner;
    public Engine engine;

    public PlayCardInteraction(Card card, Hand hand, Player owner, Engine engine) {
        this.card = card;
        this.hand = hand;
        this.owner = owner;
        this.engine = engine;
    }

    protected async override Task<bool> startExecute(GS gameState) {
        var newGS = gameState;
        foreach (var effect in card.effects) {
            newGS = await engine.resolveEffect(newGS, effect, owner);
            if (newGS == null) return false;
        }

        // note: missing gameState transforms
        return true;
    }

    protected override GS doExecute(GS gameState)
    {
        gameState.ga.energyActionHandler.Invoke(EnergyActionKey.PAY, new EnergyPayload(card.cost, card), () => {
            owner.side.energy = owner.side.energy.Without(card.cost);
            hand.remove(card);
            card.use(gameState, owner);
        });

        return gameState;
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        var oth = (PlayCardInteraction) obj;
        return card == oth.card; // we ignore player as creatures are unique
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        return card.GetHashCode();
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

    protected override GS doExecute(GS gameState)
    {
        gameState.ga.energyActionHandler.Invoke(EnergyActionKey.SAC, new EnergyPayload(target.cost, target), () => {
            hand.remove(target);
            owner.side.maxEnergy = owner.side.maxEnergy.With(target.sac);
            owner.side.energy = owner.side.energy.With(target.sac);
        });
        return gameState;
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
    public EffectContext context;
    EffectTarget target;

    public SelectTargetInteraction(EffectTarget target, EffectContext context) {
        this.context = context;
        this.target = target;
    }

    protected override GS doExecute(GS gameState)
    {
        context.gameState = gameState;
        target.callback(context);
        target.called = true;
        return gameState;
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        var oth = (SelectTargetInteraction) obj;
        return target == oth.target && context == oth.context;
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        return target.GetHashCode() + context.GetHashCode();
    }
}

public class CancelSelectionInteraction : Interaction {

    public CancelSelectionInteraction() {
    }

    
    protected override GS doExecute(GS gameState)
    {
        // todo?
        return gameState;
    }

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        var oth = (CancelSelectionInteraction) obj;
        return true;
    }
    
    // override object.GetHashCode
    public override int GetHashCode()
    {
        return 1252134;
    }
}