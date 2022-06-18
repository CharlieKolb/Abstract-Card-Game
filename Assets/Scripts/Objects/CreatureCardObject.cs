using System;

// Instance of a card, usually in hand or preview
public class CreatureCardObject : CardObject {
    TMPro.TextMeshPro statsText;
    CreatureCard creatureCard;
    protected override void doInstantiate(Card c) {
        creatureCard = (CreatureCard) c; 
        statsText = transform.Find("StatsObject").gameObject.GetComponent<TMPro.TextMeshPro>();
    }

    public void Update() {
        if (statsText != null) statsText.text = creatureCard.stats.ToString();
    }
}