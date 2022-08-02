using System.Collections.Generic;
using System.Linq;

public class Invokable<P> : PayloadBase where P : PayloadBase {
    public string key;
    public P payload;

    public static Invokable<P> From(GS gameState, string key, P payload) {
        var self = new Invokable<P>();
        self.gameState = gameState;
        self.key = key;
        self.payload = payload;
        return self;
    }
}

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

public class PLFab {
    
}



public interface IKeyBase {}

public class PhaseActionKey : IKeyBase {
    public static string ENTER = "ENTER";
    public static string EXIT = "EXIT";
}

public class PlayerActionKey : IKeyBase {
    public static string DAMAGED = "DAMAGED";
    public static string DIES = "DIES";
    public static string DRAWS = "DRAWS";
}

public class EffectActionKey : IKeyBase {
    public static string TRIGGERED = "TRIGGERED";
}

public class CardActionKey : IKeyBase {
    public static string IS_DRAW = "IS_DRAW";
    public static string IS_USED = "IS_USED";
    public static string IS_DISCARDED = "IS_DISCARDED";
}




public abstract class PayloadBase {
    public GS gameState;

    public PayloadBase() {
        gameState = null;
    }

    public PayloadBase(GS gs) {
        gameState = gs;
    }
}


public class CardActionPayload : PayloadBase {
    public Card card;

    public CardActionPayload(GS gameState, Card card) : base(gameState) {
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
    public static string SACCED = "SACCED";
    public static string USED = "SACCED";
}

public class DeckActionKey : CardCollectionActionKey  {
    public static string MILL = "MILL";
    public static string SHUFFLED = "SHUFFLE";
}

public class GraveyardActionKey : CardCollectionActionKey  {
}


public class EnergyActionKey : IKeyBase {
    public static string PAY = "PAY";
    public static string SAC = "SAC";
    public static string RECHARGE = "RECHARGE"; // regain energy at start of turn
}

public class PlayerPayload : PayloadBase {
    public Player target;

    public PlayerPayload(GS gameState, Player target) : base(gameState) {
        this.target = target;
    }
}

public class EffectPayload : PayloadBase {
    public Effect effect;

    public EffectPayload(GS gameState, Effect effect) : base(gameState) {
        this.effect = effect;
    }
}

public class PhasePayload : PayloadBase {
    public GamePhase phase;

    public PhasePayload(GS gameState, GamePhase phase) : base(gameState) {
        this.phase = phase;
    }
}

public class CollectionPayload<C, E> : PayloadBase where E : Entity where C : Collection<E> {
    public C collection;
    public Diff<E> diff;

    public CollectionPayload(GS gameState, C collection, Diff<E> diff = null) : base(gameState) {
        this.collection = collection;
        this.diff = diff == null ? Diff<E>.Empty : diff;
    }
}
public class CardCollectionPayload : CollectionPayload<CardCollection, Card> { public CardCollectionPayload(GS gameState, CardCollection c, Diff<Card> diff = null) : base(gameState, c, diff) {} }
public class DeckPayload : CollectionPayload<Deck, Card> { public DeckPayload(GS gameState, Deck d, Diff<Card> diff = null) : base(gameState, d, diff) {} }
public class HandPayload : CollectionPayload<Hand, Card> { public HandPayload(GS gameState, Hand h, Diff<Card> diff = null) : base(gameState, h, diff) {} }
public class GraveyardPayload : CollectionPayload<Graveyard, Card> { public GraveyardPayload(GS gameState, Graveyard g, Diff<Card> diff = null) : base(gameState, g, diff) {} }

public class BoardAreaPayload<E> : CollectionPayload<BoardCollection<E>, E> where E : BoardEntity { public BoardAreaPayload(GS gameState, BoardCollection<E> b, Diff<E> diff = null) : base(gameState, b, diff) {} }
public class CreatureAreaPayload : BoardAreaPayload<CreatureEntity> { public CreatureAreaPayload(GS gameState, CreatureCollection c, Diff<CreatureEntity> diff = null) : base(gameState, c, diff) {} }


public class EntityPayload<E> : PayloadBase where E : Entity{
    public E entity;

    public EntityPayload(GS gameState, E entity) : base(gameState) {
        this.entity = entity;
    }
}
public class EntityPayload : EntityPayload<Entity> { public EntityPayload(GS gameState, Entity e) : base(gameState, e) {} }
public class BoardEntityPayload<E> : EntityPayload<E> where E : BoardEntity { public BoardEntityPayload(GS gs, E e) : base(gs, e) {} }
public class BoardEntityPayload : EntityPayload<BoardEntity> { public BoardEntityPayload(GS gs, BoardEntity e) : base(gs, e) {} }
public class CreaturePayload : BoardEntityPayload<CreatureEntity> { public CreaturePayload(GS gs, CreatureEntity e) : base(gs, e) {} }


public class EnergyPayload : PayloadBase {
    public Energy resources;
    public Entity source;

    public EnergyPayload(GS gameState, Energy resources, Entity source = null) : base(gameState) {
        this.resources = resources;
        this.source = source;
    }
}