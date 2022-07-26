using System.Linq;
using System.Collections.Generic;
using System;

using Debug = UnityEngine.Debug;

public class EffectContext {
    public Entity targetEntity;

    public Effect effect;
    public Player owner;

    public EffectContext WithEntity(Entity target) {
        this.targetEntity = target;
        return this;
    }

    public EffectContext WithEffect(Effect effect) {
        this.effect = effect;
        return this;
    }

    public EffectContext WithOwner(Player owner) {
        this.owner = owner;
        return this;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        var oth = (EffectContext) obj;
        return this.targetEntity == oth.targetEntity &&
                this.effect == oth.effect &&
                this.owner == oth.owner;
    }
    
    public override int GetHashCode()
    {
        return (effect == null ? 5 : effect.GetHashCode()) +
            10 * owner.GetHashCode() +
            (targetEntity == null ? 25 : targetEntity.GetHashCode());
    }
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
        return requests.All(x => x.hasValidTargetCondition(new EffectContext().WithOwner(owner)));
    }

    protected abstract void doApply(Player owner);

    public void apply(Player owner) {
        GS.ga_global.effectActionHandler.Invoke(EffectActionKey.TRIGGERED, new EffectPayload(this), () => doApply(owner));
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

public class PersistentConditionalEffect {
    // todo
}


public interface IDamageEffect { 
    public int getAmount();
    public void setAmount(int x);
}

public class MutateDamageEffect : Effect {
    Func<int, int> formula;
    IDamageEffect damageEffect;
    public MutateDamageEffect(Func<int, int> formula, IDamageEffect damageEffect) {
        this.formula = formula;
        this.damageEffect = damageEffect;
    }

    protected override void doApply(Player owner)
    {
        damageEffect.setAmount(formula(damageEffect.getAmount()));
    }

    public override Effect Clone()
    {
        return new MutateDamageEffect(formula, damageEffect);
    }
}

public class DamagePlayerEffect : Effect, IDamageEffect {
    int amount;
    Player target;
    public DamagePlayerEffect(int amount, Player target) {
        this.amount = amount;
        this.target = target;
    }

    protected override void doApply(Player owner)
    {
        var board = owner.side.creatures;
        GS.ga_global.playerActionHandler.Invoke(PlayerActionKey.DAMAGED, new PlayerPayload(target), () => {
            target.lifepoints -= amount;
        });
        if (target.lifepoints <= 0) {
            GS.ga_global.playerActionHandler.Invoke(PlayerActionKey.DIES, new PlayerPayload(target), () => {
                // todo: handle loss
            });
        }
    }

    public int getAmount() {
        return amount;
    }

    public void setAmount(int x) {
        amount = x;
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
        GS.ga_global.creatureActionHandler.Invoke(CreatureEntityActionKey.DAMAGED, new CreaturePayload(target), () => {
            target.stats.health -= amount;
        });

        if (target.stats.health <= 0) {
            GS.ga_global.creatureActionHandler.Invoke(
                CreatureEntityActionKey.DEATH,
                new CreaturePayload(target),
                () => {
                    var x = getContext().WithEffect(this).WithOwner(owner).WithEntity(target);
                    // Just clear it on both rather than looking, should probably have a function for this
                    GS.gameStateData_global.activeController.player.side.creatures.clearEntity(target, x);
                    GS.gameStateData_global.passiveController.player.side.creatures.clearEntity(target, x);
                }
            );
        }
    }

    public int getAmount() {
        return amount;
    }

    public void setAmount(int x) {
        amount = x;
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