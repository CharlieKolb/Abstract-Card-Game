using System;
using UnityEngine;

// Instance of a card, usually in hand or preview
public class CreatureCardObject : CardObject {
    TMPro.TextMeshPro statsText;
    CreatureCard creatureCard;



    protected override void doInstantiate(Card c) {
        creatureCard = (CreatureCard) c; 
        statsText = transform.Find("StatsObject").gameObject.GetComponent<TMPro.TextMeshPro>();
        
        var mat = Resources.Load<Material>(Cards.creatures[c.name].pathToMaterial);
        transform.Find("PicturePlane").GetComponent<MeshRenderer>().material = mat;
    }

    public void Update() {
        if (statsText != null) statsText.text = creatureCard.stats.ToString();
    }
}