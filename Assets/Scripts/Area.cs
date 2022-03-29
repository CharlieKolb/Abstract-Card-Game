using UnityEngine;

using CardGameInterface;

using System.Collections.Generic;

public abstract class Area<Collection, Trigger, Content, ContentObject> : MonoBehaviour
    where Trigger: CollectionTrigger, new()
    where Collection : Collection<Trigger, Content>
{
    protected Collection collection;

    protected Dictionary<Content, ContentObject> objectMapper;

    protected List<Vector3> targetPositions; // Relative target positions for each card

    // Start is called before the first frame update
    public virtual void Start()
    {
        objectMapper = new Dictionary<Content, ContentObject>();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (collection == null) return;
    }

    public abstract void refresh(CollectionContext<Content> context);
    public abstract ContentObject resolvePrefab(Content content);

    private void doRefresh(CollectionContext<Content> context) {
        Debug.Log("A");
        foreach (var x in context.added) {
            Debug.Log("B");
            var comp = resolvePrefab(x);
            Debug.Log(comp);
            objectMapper[x] = comp; 
        }

        foreach (var x in context.removed) {
            Debug.Log("C");

            objectMapper.Remove(x);
        }

        refresh(context);
    }

    public void SetCollection(Collection collection) {
        this.collection = collection;
        doRefresh(CollectionContextFactory<Content>.FromAdded(collection.content.ToArray()));

        collection.on(collection.triggers.ON_COUNT_CHANGE, (x) => {
            doRefresh(x);
        });
    }


    


    
}
 