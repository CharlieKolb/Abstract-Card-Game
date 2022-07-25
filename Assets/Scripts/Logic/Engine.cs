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

public class Engine {
    GS gameState;

    List<IEnumerator<bool>> interactionQueue = new List<IEnumerator<bool>>();

    SideConfig s1;
    SideConfig s2;
    SideConfig active;

    List<EffectTarget> resolveTargetStack;

    public Engine(SideConfig s1, SideConfig s2) {
        this.s1 = s1;
        this.s2 = s2;
        GS.ga = new GameActions(this);
    }

    public async void startGame() {
        var gs = new GS();

        s1.Instantiate();
        s2.Instantiate();

        GS.gameStateData.currentPhase = Phases.drawPhase;
        GS.gameStateData.currentPhase.executeEntry();

        GS.ga.phaseActionHandler.after.on(PhaseActionKey.EXIT, (p) => {
            if (p.phase == Phases.endPhase)
            {
                GS.gameStateData.passiveController = active.controller;
                active = (s1.Equals(active)) ? s2 : s1;
                GS.gameStateData.activeController = active.controller;
            }
        });

        for (int i = 0; i < 5; ++i)
        {
            s1.controller.player.drawCard();
            s2.controller.player.drawCard();
        }
        active = s1;


        while (true) {
            var interaction = await active.controller.selectInteraction(getNormalInteractions(gs));
            var newGS = await interaction.execute(gs);
            if (newGS != null) gs = newGS;
        }
    }


    public async Task<GS> resolveEffect(GS gameState, Effect effect, Player owner) {
        var tasks = new List<Interaction>();
        foreach (var x in effect.requests) {
            var res = await active.controller.selectInteraction(getTargetCandidates(owner.side, x));
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

        res.AddRange(getInteractions(s1));
        res.AddRange(getInteractions(s2));


        return res;
    }

    // Do not consume more than one interaction per call
    public List<Interaction> getInteractions(SideConfig sideConfig) {
        var res = new List<Interaction>();

        if (sideConfig.controller != GS.gameStateData.activeController) {
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
            .Where(c => c.value.canUseFromHand(owner))
            .Select(x => new PlayCardInteraction(x.value, hand, owner, this))
            .ToList<Interaction>();

        var saccable = all.Select(x => new SacCardInteraction(x.value, hand, owner)).ToList<Interaction>();

        return playable.Concat(saccable).ToList();
    }

    private List<Interaction> getInteractions(Player owner, CreatureCollection creatures) {
        if (GS.gameStateData.activeController.player != owner || GS.gameStateData.currentPhase != Phases.battlePhase) {
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
                    new EffectContext()
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
                    new EffectContext()
                        .WithOwner(side.player)
                        .WithEntity(e.value)
            )
        );


        res.AddRange(
            side.hand.getAll()
                .Select(e => 
                    new EffectContext()
                        .WithOwner(side.player)
                        .WithEntity(e.value)
            )
        );

        foreach (var x in new List<Entity>{ side.deck, side.hand, side.graveyard }) {
            res.Add(
                new EffectContext()
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

