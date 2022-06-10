using System.Collections.Generic;

using System;

using CardGameInterface;

// public class EntityTrigger : CollectionTrigger
// {
//     public string ON_PLAY = "ON_PLAY"; // any time a creature enters the board
//     // public string ON_SUMMON = "ON_SUMMON"; // any time a creature is summoned
//     // public string ON_ATTACK = "ON_ATTACK";
//     // public string ON_DEATH = "ON_DEATH"; // any time a creature is killed
//     public string ON_EXIT = "ON_EXIT"; // any time a creature leaves the board
// }

public class BoardEntity : Entity
{}


public class CreatureEntity : BoardEntity
{
    Stats stats;

    public CreatureEntity(Stats stats)
    {
        this.stats = stats;
    }
}


public class CreatureCollection : BoardCollection<CreatureEntity> {
    public CreatureCollection() : base(5) {}
}


public class BoardCollection<E> : Collection<E>
    where E: BoardEntity
{
    public BoardCollection(int width) : base(new List<E>(new E[width])) {}

    // public bool trySummonCreature(Creature creature, int index, EffectContext context) {
    //     var res = trySummonCreature(creature, index, context);

    //     if (res) {
    //         actions.Trigger(triggers.ON_SUMMON, this);
    //     }

    //     return res;
    // }

    public bool tryPlay(E entity, int index, EffectContext context) {
        if (content[index] != null) return false;
        content[index] = entity;

        // Trigger(triggers.ON_COUNT_CHANGE, CollectionContextFactory<E>.FromAdded(entity));

        return true;
    }

    public List<E> clearEntities(Predicate<Element<E>> condition, EffectContext effectContext) {
        var cleared = new List<E>();
        foreach (var entityCtx in getExisting()) {
            var entity = entityCtx.value;

            if (condition.Invoke(entityCtx)) { 
            };
        }

        // if (cleared.Count > 0) Trigger(triggers.ON_COUNT_CHANGE, CollectionContextFactory<E>.FromRemoved(cleared.ToArray()));

        return cleared;
    }

    public E clearEntity(int index, EffectContext effectContext) {
        var list = clearEntities(x => x.index == index, effectContext);
        if (list.Count == 0) return null;

        return list[0];
    }
}
public class BoardCollection : BoardCollection<BoardEntity> {
    public BoardCollection(int width) : base(width) {}
}