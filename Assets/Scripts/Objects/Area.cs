using UnityEngine;
using UnityEngine.Events;

using CardGameInterface;

using System.Collections.Generic;
using System.Linq;

public abstract class Area<Collection, Content, ContentObject> : MonoBehaviour
    where Content : Entity
    where Collection : Collection<Content>
    where ContentObject : MonoBehaviour
{
    public AbstractCardGameController controller => GetComponentInParent<AbstractCardGameController>();

    public Collection collection { get; private set; }

    public UnityEvent<ContentObject> onUse = new UnityEvent<ContentObject>();

    protected Dictionary<Content, ContentObject> objectMapper;

    public ContentObject getObject(Content key) {
        return objectMapper[key];
    }  

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

    protected abstract void initCollection();
    protected abstract void refresh(Diff<Content> context);
    public abstract ContentObject resolvePrefab(Content content);

    protected void doRefresh(Diff<Content> context) {
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

    public void Init(Collection collection) {
        this.collection = collection;
        initCollection();
        doRefresh(Differ<Content>.FromAdded(collection.getExisting().Select(x => x.value).ToArray()));
    }


    


    
}
 