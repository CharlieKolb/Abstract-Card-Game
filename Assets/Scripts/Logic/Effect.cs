using System.Linq;
using UnityEngine;

public abstract class Effect
{
    public Effect()
    {}

    public EffectContext getContext() {
        return new EffectContext();
    }

    public virtual bool canApply() {
        return true;
    }

    protected abstract void doApply();

    public void apply() {
        doApply();
    }
}

public class SelfDrawEffect : Effect
{
    protected override void doApply()
    {
        GS.gameStateData.activeController.player.drawCard();
    }
}

public class SpawnEffect : Effect
{
    Stats stats;

    public SpawnEffect(Stats stats) {
        this.stats = stats;
    }

    public override bool canApply() {
        var board = GS.gameStateData.activeController.player.side.creatures;
        return board.getExisting().Count() < board.Count;
    }

    protected override void doApply()
    {
        var board = GS.gameStateData.activeController.player.side.creatures;
        var i = 0;
        while (i < board.Count && !board.tryPlay(new CreatureEntity(stats), i, getContext())) {
            i++;
            if (i == board.Count) throw new System.Exception("Tried to play card on full board!");
        }
    }
}