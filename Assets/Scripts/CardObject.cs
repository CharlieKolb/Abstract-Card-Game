using UnityEngine;

using CardGameInterface;

// Instance of a card, usually in hand or preview
public abstract class CardObject : MonoBehaviour {
    Card card; // effects and name

    public GameState gameState;

    public void Instantiate(Card card) {
        this.card = card;
    }

    public override string ToString() {
        return this.card.name;
    }

    private void OnMouseDown() {
        if (card.canUseFromHand()) {
            card.use();
            Destroy(this.gameObject);
        }
    }
}