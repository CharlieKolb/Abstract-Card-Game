using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

using Debug = UnityEngine.Debug;

public class Element<Content> {
    public Content value { get; set; }
    public int index { get; set; }
}

public abstract class Collection<Content> : Entity where Content : Entity
{
    protected List<Content> content;

    public Collection() {
        this.content = new List<Content>();
    }
    public Collection(List<Content> content) {
        this.content = content;
    }

    public int Count => content.Count;
    public Content this[int i]
    {
        get { return content[i]; }
    }

    public int find(Content c) {
        foreach (var x in getExisting()) {
            if (x.value == c) return x.index;
        }
        return -1;
    }

    public IEnumerable<Element<Content>> getExisting() {
        for(var i = 0; i < content.Count; ++i) {
            var e = content[i];
            if (e == null) continue;
            yield return new Element<Content> {
                value = e,
                index = i,
            };
        }
    }

    public IEnumerable<Element<Content>> getAll() {
        for(var i = 0; i < content.Count; ++i) {
            var e = content[i];
            yield return new Element<Content> {
                value = e,
                index = i,
            };
        }
    }

}

public class CreatureCollectionIndex : Entity {
    public int index;
    public CreatureCollection collection;

    public CreatureEntity at() {
        return collection[index];
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }
        var oth = (CreatureCollectionIndex)obj;
        return index == oth.index && collection == oth.collection;
    }

    // override object.GetHashCode
    public override int GetHashCode()
    {
        // TODO: write your implementation of GetHashCode() here
        return index.GetHashCode() + collection.GetHashCode();
    }
}

// BoardEntity Collections
public class CreatureCollection : BoardCollection<CreatureEntity> {
    public CreatureCollection() : base(5) {
        
    }

        // Announce(BoardAreaActionKey.COUNT_CHANGED, cap);
}


public abstract class BoardCollection<E> : Collection<E>, ITargetable
    where E: BoardEntity
{
    public BoardCollection(int width) : base(new List<E>(new E[width])) {

    }

    // public bool trySummonCreature(Creature creature, int index, EffectContext context) {
    //     var res = trySummonCreature(creature, index, context);

    //     if (res) {
    //         actions.Trigger(triggers.ON_SUMMON, this);
    //     }

    //     return res;
    // }

    public Diff<E> tryPlay(GS gameState, E entity, int index, EffectContext context) {
        if (content[index] != null) return null;

        content[index] = entity;

        return Differ<E>.FromAdded(entity);
    }
    

    public List<E> clearEntities(Predicate<Element<E>> condition, EffectContext effectContext) {
        var cleared = new List<E>();
        foreach (var entityCtx in getExisting()) {
            var entity = entityCtx.value;
            
            if (condition.Invoke(entityCtx)) {
                content[entityCtx.index] = null;
                cleared.Add(entity);
            };
        }

        return cleared;
    }

    public E clearEntity(int index, EffectContext effectContext) {
        var list = clearEntities(x => x.index == index, effectContext);
        if (list.Count == 0) return null;

        return list[0];
    }

    public E clearEntity(E entity, EffectContext effectContext) {
        var list = clearEntities(x => x.value == entity, effectContext);
        if (list.Count == 0) return null;

        return list[0];
    }
}

// Card Collections
public abstract class CardCollection : Collection<Card>, ITargetable
{
    public bool isEmpty() {
        return content.Count == 0;
    }

    public void applyDiff(Diff<Card> diff) {
        diff.added.ForEach(x => add(x));
        diff.removed.ForEach(x => remove(x));
    }

    public void add(Card card) {
        content.Add(card);
    }

    public void remove(Card card) {
        content.Remove(card);

    }

    public void Shuffle() {
        var count = content.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = content[i];
            content[i] = content[r];
            content[r] = tmp;
        }
    }

    public override string ToString()
    {
        return string.Join(",", content.AsEnumerable().Select((c) => c.ToString()));
    }
}


public class Deck : CardCollection
{
    public Card draw()
    {
        Card card = this.content[0];
        this.remove(card);

        return card;
    }

    public static Deck FromBlueprint(DeckBlueprint deck)
    {
        var result = new Deck();
        foreach (var key in deck.cards.Keys)
        {
            var val = deck.cards[key];
            for (var i = 0; i < val; ++i)
            {
                result.add(key.MakeCard());
            }
        }
        result.Shuffle();
        return result;
    }

}

public class Graveyard : CardCollection
{}

public class Hand : CardCollection
{
    public Hand() {}
}