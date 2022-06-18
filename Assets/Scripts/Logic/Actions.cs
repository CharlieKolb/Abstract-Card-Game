using System.Collections.Generic;
using System.Linq;

public class Diff<Content> {
    public static Diff<Content> Empty = new Diff<Content>();

    public List<Content> added = new List<Content>();
    public List<Content> removed = new List<Content>();
}

class Differ<Content> {
    public static Diff<Content> FromAdded(params Content[] args) {
        return new Diff<Content> {
            added = args.ToList(),
            removed = new List<Content>(),
        };
    }

    public static Diff<Content> FromRemoved(params Content[] args) {
        return new Diff<Content> {
            added = new List<Content>(),
            removed = args.ToList(),
        };
    }
}


public interface IPayloadBase {}

public interface IKeyBase {}

public abstract class GameAction<P> where P : IPayloadBase {
    public string key;
    public P payload;

    public GameAction(string key, P payload) {
        this.key = key;
        this.payload = payload;
    }
}

public class PlayerActionKey : IKeyBase {
    public static string DAMAGED = "DAMAGED";
    public static string DIES = "DIES";
}

public class CardActionKey : IKeyBase {
    public static string DRAW = "DRAW";
    public static string USE = "USE";
    public static string DISCARD = "DISCARD";
}

public class CardActionPayload : IPayloadBase {
    public Card card;

    public CardActionPayload(Card card) {
        this.card = card;
    }
}

public class CollectionActionKey : IKeyBase {
    public static string COUNT_CHANGED = "COUNT_CHANGED";
}

public class EntityActionKey : IKeyBase {

}

public class BoardEntityActionKey : EntityActionKey {
    public static string PLAY = "ON_PLAY"; // any time a board entity enters the board
    public static string EXIT = "ON_EXIT"; // any time a creature leaves the board
}

public class CreatureEntityActionKey : BoardEntityActionKey {
    public static string DAMAGED = "DAMAGED";
    public static string SUMMON = "ON_SUMMON"; // any time a creature is summoned
    public static string ATTACK = "ON_ATTACK";
    public static string DEATH = "ON_DEATH"; // any time a creature is killed

}

public class BoardAreaActionKey : CollectionActionKey {}

public class CreatureAreaActionKey : BoardAreaActionKey {}


public class CardCollectionActionKey : CollectionActionKey {
}


public class HandActionKey : CardCollectionActionKey {
    public static string DRAW = "DRAW";
    public static string DISCARD = "DISCARD";
}

public class DeckActionKey : CardCollectionActionKey  {
    public static string MILL = "MILL";
    public static string SHUFFLED = "SHUFFLE";
}

public class GraveyardActionKey : CardCollectionActionKey  {
}


public class EnergyActionKey : IKeyBase {
    public static string PAY = "PAY";
}

public class PlayerPayload {
    public Player target;

    public PlayerPayload(Player target) {
        this.target = target;
    }
}

public class CollectionPayload<C, E> : IPayloadBase where E : Entity where C : Collection<E> {
    public C collection;
    public Diff<E> diff;

    public CollectionPayload(C collection, Diff<E> diff = null) {
        this.collection = collection;
        this.diff = diff == null ? Diff<E>.Empty : diff;
    }
}
public class CardCollectionPayload : CollectionPayload<CardCollection, Card> { public CardCollectionPayload(CardCollection c, Diff<Card> diff = null) : base(c, diff) {} }
public class DeckPayload : CollectionPayload<Deck, Card> { public DeckPayload(Deck d, Diff<Card> diff = null) : base(d, diff) {} }
public class HandPayload : CollectionPayload<Hand, Card> { public HandPayload(Hand h, Diff<Card> diff = null) : base(h, diff) {} }
public class GraveyardPayload : CollectionPayload<Graveyard, Card> { public GraveyardPayload(Graveyard g, Diff<Card> diff = null) : base(g, diff) {} }

public class BoardAreaPayload<E> : CollectionPayload<BoardCollection<E>, E> where E : BoardEntity { public BoardAreaPayload(BoardCollection<E> b, Diff<E> diff = null) : base(b, diff) {} }
public class CreatureAreaPayload : BoardAreaPayload<CreatureEntity> { public CreatureAreaPayload(CreatureCollection c, Diff<CreatureEntity> diff = null) : base(c, diff) {} }


public class EntityPayload<E> : IPayloadBase where E : Entity{
    public Entity entity;

    public EntityPayload(E entity) {
        this.entity = entity;
    }
}
public class EntityPayload : EntityPayload<Entity> { public EntityPayload(Entity e) : base(e) {} }
public class BoardEntityPayload<E> : EntityPayload<E> where E : BoardEntity { public BoardEntityPayload(E e) : base(e) {} }
public class BoardEntityPayload : EntityPayload<BoardEntity> { public BoardEntityPayload(BoardEntity e) : base(e) {} }
public class CreaturePayload : BoardEntityPayload<CreatureEntity> { public CreaturePayload(CreatureEntity e) : base(e) {} }


public class EnergyPayload : IPayloadBase {
    public Energy resources;
    public Entity source;

    public EnergyPayload(Energy resources, Entity source = null) {
        this.resources = resources;
        this.source = source;
    }
}