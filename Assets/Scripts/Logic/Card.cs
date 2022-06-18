using UnityEngine;

// A logical card, usually a ccollection of effects based off a blueprint
public abstract class Card : Entity {
    protected CardBlueprint cardBP;

    public string name => cardBP.cardName;

    public Card(CardBlueprint cardBP) {
        this.cardBP = cardBP;
    }

    public void use(Player owner) {
        GS.resourcesActionHandler.Invoke(ResourcesActionKey.PAY, new ResourcesPayload(cardBP.costs, this), () => {
            owner.side.resources = owner.side.resources.Without(cardBP.costs);
        });
        cardBP.effects.ForEach(x => x.apply(owner));
    }

    public bool canUseFromHand(Player owner) {
        return cardBP.costs.canBePaid(owner.side.resources) && cardBP.effects.TrueForAll(x => x.canApply(owner));
    }
}

public class CreatureCard : Card {
    public CreatureCard(CreatureCardBlueprint cardBP) : base(cardBP) {}
}