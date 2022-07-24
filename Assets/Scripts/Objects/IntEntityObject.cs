class CreatureIndexEntityObject : EntityObject {
    public CreatureCollectionIndex value;

    public override Entity getEntity()
    {
        return value;
    }
}