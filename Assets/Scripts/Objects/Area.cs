using UnityEngine;

using CardGameInterface;

using System.Collections.Generic;
using System.Linq;

public abstract class Area<Collection, Trigger, Content, ContentObject> : MonoBehaviour
    where Trigger: CollectionTrigger, new()
    where Collection : Collection<Trigger, Content>
    where ContentObject : MonoBehaviour
{
    public Collection collection { get; private set; }

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
        foreach (var x in context.added) {
            var comp = resolvePrefab(x);
            objectMapper[x] = comp; 
        }

        foreach (var x in context.removed) {
            Destroy(objectMapper[x].gameObject);
            objectMapper.Remove(x);
        }

        refresh(context);
    }

    public void SetCollection(Collection collection) {
        this.collection = collection;
        doRefresh(CollectionContextFactory<Content>.FromAdded(collection.getExisting().Select(x => x.value).ToArray()));

        collection.on(collection.triggers.ON_COUNT_CHANGE, (x) => {
            Debug.Log("Z");
            doRefresh(x);
        });
    }


    


    
}
 