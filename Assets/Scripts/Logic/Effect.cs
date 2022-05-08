using System.Linq;


public abstract class Effect
{
    public Effect()
    {}

    public EffectContext getContext() {
        return new EffectContext();
    }

    public virtual bool canApply(GameState gameState) {
        return true;
    }

    protected abstract void doApply(GameState gameState);

    public void apply(GameState gameState) {
        doApply(gameState);
    }
}

public class SelfDrawEffect : Effect
{
    protected override void doApply(GameState gameState)
    {
        gameState.activePlayer.drawCard();
    }
}

public class SpawnEffect : Effect
{
    Stats stats;

    public SpawnEffect(Stats stats) {
        this.stats = stats;
    }

    public override bool canApply(GameState gameState) {
        var board = gameState.activePlayer.side.creatures;
        return board.getExisting().Count() < board.Count;
    }

    protected override void doApply(GameState gameState)
    {
        var board = gameState.activePlayer.side.creatures;
        var i = 0;
        while (i < board.Count && !board.tryPlay(new CreatureEntity(stats), i, getContext())) i++;
    }
}