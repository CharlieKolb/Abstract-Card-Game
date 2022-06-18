using System.Linq;
using UnityEngine;

public abstract class Effect : Entity
{
    public Effect()
    {}

    public EffectContext getContext() {
        return new EffectContext();
    }

    public virtual bool canApply(Player owner) {
        return true;
    }

    protected abstract void doApply(Player owner);

    public void apply(Player owner) {
        doApply(owner);
    }
}

public class SelfDrawEffect : Effect
{
    protected override void doApply(Player owner)
    {
        owner.drawCard();
    }
}

public class SpawnEffect : Effect
{
    Stats stats;

    public SpawnEffect(Stats stats) {
        this.stats = stats;
    }

    public override bool canApply(Player owner) {
        var board = owner.side.creatures;
        return board.getExisting().Count() < board.Count;
    }

    protected override void doApply(Player owner)
    {
        var board = owner.side.creatures;
        var i = 0;
        while (i < board.Count && !board.tryPlay(new CreatureEntity(owner, stats), i, getContext())) {
            i++;
            if (i == board.Count) throw new System.Exception("Tried to play card on full board!");
        }
    }
}