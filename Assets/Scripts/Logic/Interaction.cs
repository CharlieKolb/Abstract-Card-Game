public abstract class Interaction {
    public delegate void OnExecute();
    public event OnExecute onExecute;
    public bool executed = false;

    public void execute() {
        onExecute?.Invoke();
        doExecute();        
        executed = true;
    }

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
            new DamageCreatureEffect(opponent.stats.attack, creature).apply(opponent.owner);
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
