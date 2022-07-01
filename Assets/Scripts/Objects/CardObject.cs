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

        {var x = transform.Find("NameObject").gameObject;
        var y = x.GetComponent<TMPro.TextMeshPro>();
        y.text = card.name;}
 

        {var x = transform.Find("CostObject").gameObject;
        var y = x.GetComponent<TMPro.TextMeshPro>();
        y.text = card.cost.ToString();}

        doInstantiate(card);
    }

    protected abstract void doInstantiate(Card card);

    private void OnMouseDown() {
        triggerUse();
    }
}