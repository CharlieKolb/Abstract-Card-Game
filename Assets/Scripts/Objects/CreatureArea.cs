using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CardGameInterface;

public class CreatureArea : Area<CreatureCollection, CreatureEntity, CreatureObject> {

    public GameObject creatureObjectPrefab;

    public override CreatureObject resolvePrefab(CreatureEntity creature) {
        var obj = Instantiate(creatureObjectPrefab).GetComponent<CreatureObject>();
        obj.Instantiate(creature, () => onUse.Invoke(obj));
        return obj;
    }

    Vector3[] positions = new Vector3[] {
        new Vector3(-1.5f, 0, 0),
        new Vector3(-0.75f, 0, 0),
        new Vector3(0, 0, 0),
        new Vector3(0.75f, 0, 0),
        new Vector3(1.5f, 0, 0),
    };
    
    public override void Start() {
        base.Start();
    }

    public override void Update() {
        base.Update();

        if (collection == null) return;
    }

    protected override void initCollection() {
        GS.creatureAreaActionHandler.after.on(CreatureAreaActionKey.COUNT_CHANGED, (x) =>  { 
            if(x.collection == collection) doRefresh(x.diff);
        });
    }


    protected override void refresh(Diff<CreatureEntity> context) {
        foreach (var x in collection.getExisting()) {
            objectMapper[x.value].transform.parent = transform;
            objectMapper[x.value].transform.localPosition = positions[x.index];
        }
    }
}