using UnityEngine;

// A logical card, usually a ccollection of effects based off a blueprint
public abstract class Card {
    protected CardBlueprint cardBP;
    protected Resources cost;

    public string name => cardBP.cardName;

    public Card(CardBlueprint cardBP) {
        this.cardBP = cardBP;
    }

    public void use() {
        cardBP.effects.ForEach(x => x.apply());
    }

    public bool canUseFromHand() {
        return cardBP.effects.TrueForAll(x => x.canApply());
    }
}

public class CreatureCard : Card {
    public CreatureCard(CreatureCardBlueprint cardBP) : base(cardBP) {}
}