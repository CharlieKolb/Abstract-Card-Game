using UnityEngine;
using System;

// Instance of a boardEntity, usually a creature or spell
public abstract class BoardEntityObject : EntityObject {
    Action triggerUse;

    public BoardEntity entity;

    public virtual void Instantiate(BoardEntity entity, Action triggerUse) {
        this.entity = entity;
        this.triggerUse = triggerUse;

        {var x = transform.Find("NameObject").gameObject;
        var y = x.GetComponent<TMPro.TextMeshPro>();
        y.text = entity.name;}
 

        doInstantiate(entity);
    }

    protected abstract void doInstantiate(BoardEntity entity);

    private void OnMouseDown() {
        triggerUse();
    }

    public override Entity getEntity()
    {
        return entity;
    }
}