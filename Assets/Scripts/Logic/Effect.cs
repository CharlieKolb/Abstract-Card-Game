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

public class DamagePlayerEffect : Effect {
    int amount;
    Player target;
    public DamagePlayerEffect(int amount, Player target) {
        this.amount = amount;
        this.target = target;
    }

    public override bool canApply(Player owner) {
        return true;
    }

    protected override void doApply(Player owner)
    {
        var board = owner.side.creatures;
        GS.playerActionHandler.Invoke(PlayerActionKey.DAMAGED, new PlayerPayload(target), () => {
            target.lifepoints -= amount;
        });
        Debug.Log(target.lifepoints);
        if (target.lifepoints <= 0) {
            GS.playerActionHandler.Invoke(PlayerActionKey.DIES, new PlayerPayload(target), () => {
                target.triggerLoss();
            });
        }
    }
}

public class DamageCreatureEffect : Effect {
    int amount;
    CreatureEntity target;
    public DamageCreatureEffect(int amount, CreatureEntity target) {
        this.amount = amount;
        this.target = target;
    }

    public override bool canApply(Player owner) {
        return true;
    }

    protected override void doApply(Player owner)
    {
        GS.creatureActionHandler.Invoke(CreatureEntityActionKey.DAMAGED, new CreaturePayload(target), () => {
            target.stats.health -= amount;
        });

        if (target.stats.health <= 0) {
            GS.creatureActionHandler.Invoke(
                CreatureEntityActionKey.DEATH,
                new CreaturePayload(target),
                () => {
                    var x = getContext().WithEffect(this).WithSource(owner).WithTarget(target);
                    // Just clear it on both rather than looking, should probably have a function for this
                    GS.gameStateData.activeController.player.side.creatures.clearEntity(target, x);
                    GS.gameStateData.passiveController.player.side.creatures.clearEntity(target, x);
                }
            );
        }
    }
}

public class SpawnCreatureEffect : Effect
{
    CreatureCardData data;

    public SpawnCreatureEffect(CreatureCardData data) {
        this.data = data;
    }

    public override bool canApply(Player owner) {
        var board = owner.side.creatures;
        return board.getExisting().Count() < board.Count;
    }

    protected override void doApply(Player owner)
    {
        var board = owner.side.creatures;
        var i = 0;
        while (i < board.Count && !board.tryPlay(new CreatureEntity(owner, data), i, getContext())) {
            i++;
            if (i == board.Count) throw new System.Exception("Tried to play card on full board!");
        }
    }
}