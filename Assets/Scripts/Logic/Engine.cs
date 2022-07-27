using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

using Debug = UnityEngine.Debug;

public interface ITargetable {}

public interface IInteractionHandler {
    Task<Interaction> selectInteraction(List<Interaction> interactions);
}

public struct SideConfig {
    public DeckBlueprint deck;
    public AbstractCardGameController controller;

    public void Instantiate() {
        controller.Instantiate(new Player(deck));
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        
        return controller.Equals (((SideConfig) obj).controller);
    }
    
    public override int GetHashCode()
    {
        return controller.GetHashCode();
    }
}

public enum EngineState {
    Ready,
}

public interface IRuleSet {
    GameActions getActions(Engine engine);
}

public interface IGameConfig {
    List<SideConfig> getSides();
    IRuleSet getRuleSet();
}

public class Engine {
    GS gameState;

    List<IEnumerator<bool>> interactionQueue = new List<IEnumerator<bool>>();

    List<SideConfig> sides;
    SideConfig activeSide;

    List<EffectTarget> resolveTargetStack;

    public Engine(IGameConfig config) {
        this.sides = config.getSides();
        var ruleset = config.getRuleSet();

        GS.ga_global = new GameActions(this, ruleset.getActions(this));
        gameState = new GS(this);

        gameState.gameStateData.activeController = sides[0].controller;
        gameState.gameStateData.passiveControllers = sides.GetRange(1, sides.Count - 1).Select(x => x.controller).ToList();

    }

    public async void startGame() {
        sides.ForEach(x => x.Instantiate());

        gameState.gameStateData.currentPhase = Phases.drawPhase;
        gameState = gameState.gameStateData.currentPhase.executeEntry(gameState);


        for (int i = 0; i < 5; ++i)
        {
            sides.ForEach(s => s.controller.player.drawCard());
        }


        while (true) {
            var interaction = await gameState.gameStateData.activeController.selectInteraction(getNormalInteractions(gameState));
            var newGS = await interaction.execute(gameState);
            if (newGS != null) gameState = newGS;
        }
    }


    public async Task<GS> resolveEffect(GS gameState, Effect effect, Player owner) {
        var tasks = new List<Interaction>();
        foreach (var x in effect.requests) {
            var res = await gameState.gameStateData.activeController.selectInteraction(getTargetCandidates(owner.side, x));
            if (res is CancelSelectionInteraction) return null;
            tasks.Add(res);
        }

        foreach (var task in tasks) {
            gameState = await task.execute(gameState);            
        }

        return gameState;
    }


    public List<Interaction> getNormalInteractions(GS gameState) {
        var res = new List<Interaction>();

        sides.ForEach(x => res.AddRange(getInteractions(x)));

        return res;
    }

    // Do not consume more than one interaction per call
    public List<Interaction> getInteractions(SideConfig sideConfig) {
        var res = new List<Interaction>();

        if (sideConfig.controller != gameState.gameStateData.activeController) {
            return res;
        }

        res.Add(new PassPhaseInteraction());

        var player = sideConfig.controller.player;
        var side = sideConfig.controller.player.side;

        return res
                .Concat(getInteractions(player, side.hand))
                .Concat(getInteractions(player, side.creatures)).ToList();
    }


    private List<Interaction> getInteractions(Player owner, Hand hand) {
        var all = hand.getExisting();
        var playable = all
            .Where(c => c.value.canUseFromHand(gameState, owner))
            .Select(x => new PlayCardInteraction(x.value, hand, owner, this))
            .ToList<Interaction>();

        var saccable = all.Select(x => new SacCardInteraction(x.value, hand, owner)).ToList<Interaction>();

        return playable.Concat(saccable).ToList();
    }

    private List<Interaction> getInteractions(Player owner, CreatureCollection creatures) {
        if (gameState.gameStateData.activeController.player != owner || gameState.gameStateData.currentPhase != Phases.battlePhase) {
            return new List<Interaction>();
        }

        return creatures.getExisting()
            .Where(c => !c.value.hasAttacked)
            .Select(x => new DeclareAttackInteraction(x.value, owner))
            .ToList<Interaction>();
    }

    private List<Interaction> getTargetCandidates(Side side, EffectTarget target) {
        var res = new List<EffectContext>();

        // CreatureFields
        res.AddRange(
            side.creatures.getAll()
                .Select(e => 
                    new EffectContext(gameState)
                        .WithOwner(side.player)
                        .WithEntity(new CreatureCollectionIndex {
                            index = e.index,
                            collection = side.creatures
                        })
                )
        );

        // Existing creatures
        res.AddRange(
            side.creatures.getExisting()
                .Select(e => 
                    new EffectContext(gameState)
                        .WithOwner(side.player)
                        .WithEntity(e.value)
            )
        );


        res.AddRange(
            side.hand.getAll()
                .Select(e => 
                    new EffectContext(gameState)
                        .WithOwner(side.player)
                        .WithEntity(e.value)
            )
        );

        foreach (var x in new List<Entity>{ side.deck, side.hand, side.graveyard }) {
            res.Add(
                new EffectContext(gameState)
                    .WithOwner(side.player)
                    .WithEntity(x)
            );
        }

        return res
            .Where(ec => target.isValidTargetCondition.Invoke(ec))
            .Select<EffectContext, Interaction>(ec => new SelectTargetInteraction(target, ec))
            .Concat(new List<Interaction>{ new CancelSelectionInteraction() })
            .ToList<Interaction>();
    }
}

