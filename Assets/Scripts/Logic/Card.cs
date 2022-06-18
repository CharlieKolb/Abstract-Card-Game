using UnityEngine;

// A logical card, usually a ccollection of effects based off a blueprint
public abstract class Card : Entity {
    protected CardBlueprint cardBP;

    public Energy costs => cardBP.costs;
    public string name => cardBP.cardName;

    public Card(CardBlueprint cardBP) {
        this.cardBP = cardBP;
    }

    public void use(Player owner) {
        GS.energyActionHandler.Invoke(EnergyActionKey.PAY, new EnergyPayload(cardBP.costs, this), () => {
            owner.side.energy = owner.side.energy.Without(cardBP.costs);
        });
        cardBP.effects.ForEach(x => x.apply(owner));
    }

    public bool canUseFromHand(Player owner) {
        return cardBP.costs.canBePaid(owner.side.energy) && cardBP.effects.TrueForAll(x => x.canApply(owner));
    }
}

public class CreatureCard : Card {
    public Stats stats;
    public CreatureCard(CreatureCardBlueprint cardBP) : base(cardBP) {
        stats = cardBP.stats;
    }
}