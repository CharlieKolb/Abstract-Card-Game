using UnityEngine;
using System;
using CardGameInterface;

// Instance of a card, usually in hand or preview
public abstract class CardObject : MonoBehaviour {
    Action triggerUse;

    public Card card; // effects and name

    public void Instantiate(Card card, Action triggerUse) {
        this.card = card;
        this.triggerUse = triggerUse;
    }

    public override string ToString() {
        return this.card.name;
    }

    private void OnMouseDown() {
        triggerUse();
    }
}