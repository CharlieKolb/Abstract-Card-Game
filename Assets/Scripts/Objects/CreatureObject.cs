using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureObject : BoardEntityObject
{
    TMPro.TextMeshPro nameText;
    TMPro.TextMeshPro statsText;
    public CreatureEntity creatureEntity;

    protected override void doInstantiate(BoardEntity entity) {
        creatureEntity = (CreatureEntity) entity;
    
        nameText = transform.Find("NameObject").gameObject.GetComponent<TMPro.TextMeshPro>();
        statsText = transform.Find("StatsObject").gameObject.GetComponent<TMPro.TextMeshPro>();
    }

    public void Update() {
        if (statsText != null) statsText.text = creatureEntity.stats.ToString();
    }
}