using System.Linq;
using System.Collections.Generic;

using UnityEngine;

public class EffectTargetContext {
    public Player owner { set; get; }
}

public abstract class Effect : Entity
{
    public List<EffectTarget> requests = new List<EffectTarget>();

    public Effect()
    {}

    public EffectContext getContext() {
        return new EffectContext();
    }

    public virtual bool canApply(Player owner) {
        return requests.All(x => x.hasValidTargetCondition(new EffectTargetContext { owner=owner }));
    }

    protected abstract void doApply(Player owner);

    public IEnumerator<EffectTarget> resolveTargets() {
        for (int idx = 0; idx < requests.Count; ++idx) {
            var elem = requests[idx];
            yield return elem;
            while (!elem.called) {
                yield return elem;
            }
        }
    }

    public void apply(Player owner) {
        doApply(owner);
    }

    public abstract Effect Clone();
}

public class SelfDrawEffect : Effect
{
    protected override void doApply(Player owner)
    {
        owner.drawCard();
    }

    public override Effect Clone()
    {
        return new SelfDrawEffect();
    }
}

public class DamagePlayerEffect : Effect {
    int amount;
    Player target;
    public DamagePlayerEffect(int amount, Player target) {
        this.amount = amount;
        this.target = target;
    }

    protected override void doApply(Player owner)
    {
        var board = owner.side.creatures;
        GS.playerActionHandler.Invoke(PlayerActionKey.DAMAGED, new PlayerPayload(target), () => {
            target.lifepoints -= amount;
        });
        if (target.lifepoints <= 0) {
            GS.playerActionHandler.Invoke(PlayerActionKey.DIES, new PlayerPayload(target), () => {
                target.triggerLoss();
            });
        }
    }

    public override Effect Clone()
    {
        return new DamagePlayerEffect(amount, target);
    }
}

public class DamageCreatureEffect : Effect {
    int amount;
    CreatureEntity target;
    public DamageCreatureEffect(int amount, CreatureEntity target) {
        this.amount = amount;
        this.target = target;
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

    public override Effect Clone()
    {
        return new DamageCreatureEffect(amount, target);
    }
}

public class SpawnCreatureEffect : Effect
{
    CreatureCardData data;
    int index;

    public SpawnCreatureEffect(CreatureCardData data) {
        this.data = data;
        requests.Add(EffectTargets.targetEmptyFriendlyField(idx => {
            this.index = idx;
        }));
    }

    public override bool canApply(Player owner) {
        if (!base.canApply(owner)) return false;

        var board = owner.side.creatures;
        return board.getExisting().Count() < board.Count;
    }

    protected override void doApply(Player owner)
    {
        var board = owner.side.creatures;
        if(!board.tryPlay(new CreatureEntity(owner, data), index, getContext())) {
            throw new System.Exception("Unable to play card!");
        }
    }

    public override Effect Clone()
    {
        return new SpawnCreatureEffect(data);
    }
}