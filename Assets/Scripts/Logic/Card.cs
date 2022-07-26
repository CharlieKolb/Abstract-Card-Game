using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// A logical card, usually a collection of effects based of a blueprint
public abstract class Card : Entity {
    protected CardBlueprint cardBP;

    public Energy cost => cardBP.data.cost;
    public Energy sac => cardBP.data.sac;
    public string name => cardBP.data.name;

    public List<Effect> effects;

    public Card(CardBlueprint cardBP) {
        this.cardBP = cardBP;
        this.effects = cardBP.effects.Select(x => x.Clone()).ToList();
    }

    public void use(Player owner) {
        effects.ForEach(x => x.apply(owner));
    }

    public bool canUseFromHand(Player owner) {
        if (GS.gameStateData_global.activeController.player != owner.side.player ||
        !new List<GamePhase>{ Phases.mainPhase1, Phases.mainPhase2 }.Contains(GS.gameStateData_global.currentPhase)) {
            return false;
        }
        return cost.canBePaid(owner.side.energy) && cardBP.effects.TrueForAll(x => x.canApply(owner));
    }
}

public class CreatureCard : Card {
    public Stats stats;
    public CreatureCard(CreatureCardBlueprint cardBP) : base(cardBP) {
        stats = new Stats(cardBP.stats);
    }
}