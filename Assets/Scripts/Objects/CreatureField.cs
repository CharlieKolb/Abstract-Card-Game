using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureField : EntityObject
{
    public int index;
    public CreatureCollection collection;

    public override Entity getEntity()
    {
        return new CreatureCollectionIndex {
            index = index,
            collection = collection
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
