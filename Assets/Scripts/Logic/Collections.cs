using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Element<Content> {
    public Content value { get; set; }
    public int index { get; set; }
}

public abstract class Collection<Content> where Content : Entity
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
}

// BoardEntity Collections
public class CreatureCollection : BoardCollection<CreatureEntity> {
    public CreatureCollection() : base(5) {
        
    }

    protected override void InvokeCountChanged(Diff<CreatureEntity> diff, Action action)
    {
        GS.creatureAreaActionHandler.Invoke(
            BoardAreaActionKey.COUNT_CHANGED,
            new CreatureAreaPayload(this, diff),
            action
        );

    }
}


public abstract class BoardCollection<E> : Collection<E>
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

    protected abstract void InvokeCountChanged(Diff<E> diff, Action action);

    public bool tryPlay(E entity, int index, EffectContext context) {
        if (content[index] != null) return false;

        InvokeCountChanged(Differ<E>.FromAdded(entity), () => content[index] = entity);

        return true;
    }
    

    public List<E> clearEntities(Predicate<Element<E>> condition, EffectContext effectContext) {
        var cleared = new List<E>();
        foreach (var entityCtx in getExisting()) {
            var entity = entityCtx.value;
            
            if (condition.Invoke(entityCtx)) {
                InvokeCountChanged(Differ<E>.FromRemoved(entity), () => {
                    content[entityCtx.index] = null;
                    cleared.Add(entity);
                });
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
public abstract class CardCollection : Collection<Card>
{
    public virtual bool hidden() { return true; }

    public bool isEmpty() {
        return content.Count == 0;
    }

    public void add(Card card) {
        GS.cardCollectionActionHandler.Invoke(
            CardCollectionActionKey.COUNT_CHANGED,
            new CardCollectionPayload(this, Differ<Card>.FromAdded(card)),
            () => content.Add(card)
        );
    }

    public void remove(Card card) {
        GS.cardCollectionActionHandler.Invoke(
            CardCollectionActionKey.COUNT_CHANGED,
            new CardCollectionPayload(this, Differ<Card>.FromRemoved(card)),
            () => content.Remove(card)
        );
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
    public Deck() {
        GS.cardCollectionActionHandler.before.onAll(x => { if (Equals(x.arg.collection)) GS.deckActionHandler.before.Trigger(x.key, makePayload(x.arg)); });
        GS.cardCollectionActionHandler.after.onAll(x => { if (Equals(x.arg.collection)) GS.deckActionHandler.after.Trigger(x.key, makePayload(x.arg)); });
    }
    
    private DeckPayload makePayload(CardCollectionPayload cp) {
        return new DeckPayload(this, cp.diff);
    }

    public override bool hidden()
    {
        return true;
    }

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
{
    public Graveyard() {
        GS.cardCollectionActionHandler.before.onAll(x => { if (Equals(x.arg.collection)) GS.graveyardActionHandler.before.Trigger(x.key, makePayload(x.arg)); });
        GS.cardCollectionActionHandler.after.onAll(x => { if (Equals(x.arg.collection)) GS.graveyardActionHandler.after.Trigger(x.key, makePayload(x.arg)); });
    }

    private GraveyardPayload makePayload(CardCollectionPayload cp) {
        return new GraveyardPayload(this, cp.diff);
    }


    public override bool hidden()
    {
        return false;
    }
}


public class Hand : CardCollection
{
    public Hand() {
        GS.cardCollectionActionHandler.before.onAll(x => { if (Equals(x.arg.collection)) GS.handActionHandler.before.Trigger(x.key, makePayload(x.arg)); });
        GS.cardCollectionActionHandler.after.onAll(x => { if (Equals(x.arg.collection)) GS.handActionHandler.after.Trigger(x.key, makePayload(x.arg)); });
    }

    private HandPayload makePayload(CardCollectionPayload cp) {
        return new HandPayload(this, cp.diff);
    }

    public override bool hidden()
    {
        return true;
    }
}