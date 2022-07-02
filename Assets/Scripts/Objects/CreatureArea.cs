using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CardGameInterface;

public class CreatureArea : Area<CreatureCollection, CreatureEntity, CreatureObject> {

    public GameObject creatureObjectPrefab;
    public GameObject creatureFieldPrefab;

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

        for (int i = 0; i < positions.Length; ++i) {
            var obj = Instantiate(creatureFieldPrefab);
            obj.GetComponent<CreatureField>().index = i;
            obj.transform.parent = transform;
            obj.transform.localPosition = (this.controller.opposing ? -1 : 1) * positions[i];
        }

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
            // Override rotation to "face" the camera
            // there's probably a better way
            var obj = objectMapper[x.value];
            obj.transform.rotation = Quaternion.Euler(obj.transform.eulerAngles.x, 0, obj.transform.eulerAngles.z);

            obj.transform.parent = transform;
            obj.transform.localPosition = (this.controller.opposing ? -1 : 1) * positions[x.index];
        }
    }
}