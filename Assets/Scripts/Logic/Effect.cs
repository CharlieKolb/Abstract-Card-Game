using System.Linq;
using System.Collections.Generic;
using System;

using Debug = UnityEngine.Debug;

public class EffectContext {
    public Entity targetEntity;

    public Effect effect;
    public Player owner;

    public GS gameState;

    public EffectContext(GS gameState) {
        this.gameState = gameState;
    }

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

    public virtual bool canApply(GS gameState, Player owner) {
        return requests.All(x => x.hasValidTargetCondition(new EffectContext(gameState).WithOwner(owner)));
    }

    protected abstract GS doApply(GS gameState, Player owner);

    public GS apply(GS gameState, Player owner) {
        // engine.InvokeEffect(myEffect)
        // ..
        // gs = myEffect.apply(gs, owner)
        // TODO(Engine)
        GS.ga_global.effectActionHandler.Invoke(EffectActionKey.TRIGGERED, new EffectPayload(this), () => doApply(gameState, owner));
        return gameState;
    }

    public abstract Effect Clone();
}

public class SelfDrawEffect : Effect
{
    protected override GS doApply(GS gameState, Player owner)
    {
        owner.drawCard();
        return gameState;
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

    protected override GS doApply(GS gameState, Player owner)
    {
        damageEffect.setAmount(formula(damageEffect.getAmount()));
        return gameState;
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

    protected override GS doApply(GS gameState, Player owner)
    {
        var board = owner.side.creatures;
        gameState.ga.playerActionHandler.Invoke(PlayerActionKey.DAMAGED, new PlayerPayload(target), () => {
            target.lifepoints -= amount;
        });
        if (target.lifepoints <= 0) {
            gameState.ga.playerActionHandler.Invoke(PlayerActionKey.DIES, new PlayerPayload(target), () => {
                // todo: handle loss
            });
        }
        return gameState;
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

    protected override GS doApply(GS gameState, Player owner)
    {
        gameState.ga.creatureActionHandler.Invoke(CreatureEntityActionKey.DAMAGED, new CreaturePayload(target), () => {
            target.stats.health -= amount;
        });

        if (target.stats.health <= 0) {
            gameState.ga.creatureActionHandler.Invoke(
                CreatureEntityActionKey.DEATH,
                new CreaturePayload(target),
                () => {
                    var x = new EffectContext(gameState).WithEffect(this).WithOwner(owner).WithEntity(target);
                    // Just clear it on both rather than looking, should probably have a function for this
                    gameState.gameStateData.activeController.player.side.creatures.clearEntity(gameState, target, x);
                    gameState.gameStateData.passiveController.player.side.creatures.clearEntity(gameState, target, x);
                }
            );
        }
        return gameState;
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

    public override bool canApply(GS gameState, Player owner) {
        if (!base.canApply(gameState, owner)) return false;

        var board = owner.side.creatures;
        return board.getExisting().Count() < board.Count;
    }

    protected override GS doApply(GS gameState, Player owner)
    {
        var board = owner.side.creatures;
        if(!board.tryPlay(gameState, new CreatureEntity(owner, data), index, new EffectContext(gameState).WithEffect(this).WithOwner(owner))) {
            throw new System.Exception("Unable to play card!");
        }
        return gameState;
    }

    public override Effect Clone()
    {
        return new SpawnCreatureEffect(data);
    }
}