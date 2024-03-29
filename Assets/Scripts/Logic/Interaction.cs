using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public abstract class Interaction {
    public bool executed = false;

    public async Task<GS> execute(GS gameState) {
        var start = await startExecute(gameState);

        if (start == null) return gameState;

        return doExecute(gameState);        
    }
    protected virtual Task<GS> startExecute(GS gameState) { return Task.FromResult(gameState); }
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

    protected async override Task<GS> startExecute(GS gameState) {
        foreach (var effect in card.effects) {
            gameState = await engine.resolveEffect(gameState, effect, owner);
            if (gameState == null) return null;
        }

        // note: missing gameState transforms
        return gameState;
    }

    protected override GS doExecute(GS gameState)
    {
        gameState = gameState.ga.actionHandler.Invoke(new Reactions.ENERGY.PAY(gameState, card.cost, owner.side), (pl) => {
            pl.side.energy = pl.side.energy.Without(pl.energyAmount);
            return pl;
        });

        gameState = gameState.ga.actionHandler.Invoke(new Reactions.HAND.REMOVED(gameState, card, hand), (pl) => {
            pl.hand.applyDiff(Differ<Card>.FromRemoved(pl.card));
            pl.hand.Announce(pl);
            return pl;
        });

        gameState = gameState.ga.actionHandler.Invoke(new Reactions.CARD.USED(gameState, card, owner.side), pl => {
            // TODO(Payload)
            pl.gameState = pl.card.use(pl.gameState, pl.side.player);
            pl.card.Announce(pl);
            return pl;
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
        gameState.ga.actionHandler.Invoke(new Reactions.HAND.REMOVED(gameState, target, hand), (pl) => {
            pl.hand.applyDiff(Differ<Card>.FromRemoved(pl.card));
            pl.hand.Announce(pl);

            return pl;
        });

        gameState.ga.actionHandler.Invoke(new Reactions.ENERGY.SAC(gameState, target.cost, owner.side), (pl) => {
            pl.side.maxEnergy = pl.side.maxEnergy.With(target.sac);
            pl.side.energy = pl.side.energy.With(target.sac);
            return pl;
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