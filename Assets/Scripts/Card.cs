using UnityEngine;

// A logical card, usually a ccollection of effects based off a blueprint
public abstract class Card {
    protected GameState gameState;
    protected CardBlueprint cardBP;

    public string name => cardBP.cardName;

    public Card(GameState gameState, CardBlueprint cardBP) {
        this.cardBP = cardBP;
        this.gameState = gameState;
    }


    public void use() {
        var side = this.gameState.activePlayer.side;
        side.hand.remove(this);

        cardBP.effects.ForEach(x => x.apply(this.gameState));
    }

    public bool canUseFromGraveyard() {
        return false;
    }

    public bool canUseFromHand() {
        return true;
    }

    public bool canUseFromDeck() {
        return false;
    }
}

public class CreatureCard : Card {
    public CreatureCard(GameState gameState, CreatureCardBlueprint cardBP) : base(gameState, cardBP) {}
}