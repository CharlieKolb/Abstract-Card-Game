using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CardGameInterface;

public class CreatureArea : Area<CreatureCollection, EntityTrigger, CreatureEntity, CreatureObject> {

    public GameObject creatureObjectPrefab;

    public override CreatureObject resolvePrefab(CreatureEntity card) {
        return Instantiate(creatureObjectPrefab).GetComponent<CreatureObject>();
    }



    Vector3[] positions = new Vector3[] {
        new Vector3(-2, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(2, 0, 0),
    };
    
    public override void Start() {
        base.Start();
    }

    public override void Update() {
        base.Update();

        if (collection == null) return;
    }

    public override void refresh(CollectionContext<CreatureEntity> context) {
        foreach (var x in collection.getExisting()) {
            objectMapper[x.value].transform.position = positions[x.index];
        }
    }
}