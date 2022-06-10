using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using System;

using CardGameInterface;

public class EffectContext {
    public BoardEntity target;

    public Effect effect;
    public Player player;
}




public class Side
{
    public Player player;
    public Hand hand;
    public CreatureCollection creatures;
    public Deck deck;
    public Graveyard graveyard;
    public HP hp;
    public Resources resources;
    public Resources maxResources;

    public Side(DeckBlueprint deckBlueprint, Player player) {
        deck = Deck.FromBlueprint(deckBlueprint);
        this.player = player;

        hand = new Hand();
        creatures = new CreatureCollection();
        graveyard = new Graveyard();
        hp = new HP();
        maxResources = new Resources(5);
        resources = new Resources(maxResources.cost);
    }

    public bool hasOptions()
    {
        foreach (var e in hand.getExisting())
        {
            if (e.value.canUseFromHand()) return true;
        }

        return false;
    }
}

// Symmetric 
public class StandardBattlefield
{
    Side player1;
    Side player2;
}

public class F
{
    public delegate GameObject InstantiateOriginal(GameObject original);
}

public class CardBPMaker
{
    F.InstantiateOriginal func;
    GameObject prefab;

    public CardBPMaker(F.InstantiateOriginal instantiateFunc, GameObject prefab)
    {
        func = instantiateFunc;
        this.prefab = prefab;
    }

    public CardBlueprint makeCreatureBP(string name, Stats stats, List<Effect> effects = null)
    {
        return new CreatureCardBlueprint(name, stats, effects != null ? effects : new List<Effect>(), func, prefab);
    }
}


public abstract class CardBlueprint
{
    public string cardName;
    public List<Effect> effects;

    public delegate T InstantiateOriginal<T>(GameObject original) where T : UnityEngine.Object;

    GameObject prefab;
    F.InstantiateOriginal instantiate;



    public CardBlueprint(string name, List<Effect> effects, F.InstantiateOriginal instantiateFunc, GameObject prefab)
    {
        cardName = name;
        instantiate = instantiateFunc;
        this.effects = effects;
        this.prefab = prefab;
    }

    public abstract Card MakeCard();

    public GameObject Instantiate() {
        return instantiate(prefab);
    }
}

public class CreatureCardBlueprint : CardBlueprint {
    public CreatureCardBlueprint(
        string name,
        Stats stats,
        List<Effect> effects,
        F.InstantiateOriginal instantiateFunc,
        GameObject prefab
    ) : base(name, effects.Concat(new List<Effect>{ new SpawnEffect(stats) }).ToList(), instantiateFunc, prefab) {
    }

    public override Card MakeCard()
    {
        return new CreatureCard(this);
    }
}

public class Deck : CardCollection
{
    public Deck() {
        GS.cardCollectionActionHandler.before.onAll(x => { if (Equals(x)) GS.deckActionHandler.before.Trigger(x.key, makePayload(x.arg)); });
        GS.cardCollectionActionHandler.after.onAll(x => { if (Equals(x)) GS.deckActionHandler.after.Trigger(x.key, makePayload(x.arg)); });
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

    // public override void postAdd(Card c)
    // {
    //     base.postAdd(c);
    //     Shuffle();
    // }

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
        return result;
    }

}

public class Graveyard : CardCollection
{
    public override bool hidden()
    {
        return false;
    }
}


public class Hand : CardCollection
{
    public Hand() {
        GS.cardCollectionActionHandler.before.onAll(x => { if (Equals(x)) GS.handActionHandler.before.Trigger(x.key, makePayload(x.arg)); });
        GS.cardCollectionActionHandler.after.onAll(x => { if (Equals(x)) GS.handActionHandler.after.Trigger(x.key, makePayload(x.arg)); });
    }

    private HandPayload makePayload(CardCollectionPayload cp) {
        return new HandPayload(this, cp.diff);
    }

    public override bool hidden()
    {
        return true;
    }
}

public class HP
{
    public int points = 20;
}


public class Player : IPlayer
{
    public Side side;

    public Player(DeckBlueprint deckBlueprint)
    {
        side = new Side(deckBlueprint, this);
    }

    public bool hasOptions()
    {
        if (side.hasOptions()) return true;

        // todo: skip turn button event here?
        return false;
    }
    

    public void drawCard()
    {
        if (side.deck.isEmpty())
        {
            triggerLoss();
            return;
        }
        side.hand.add(side.deck.draw());
    }
}

public interface IBase {}


public abstract class GameAction<P> where P : IBase {
    public string key;
    public P payload;

    public GameAction(string key, P payload) {
        this.key = key;
        this.payload = payload;
    }
}

public enum CardActionKey {
    DRAW,
    USE,
    DISCARD,
}

public class CardActionPayload : IBase {
    public Card card;

    public CardActionPayload(Card card) {
        this.card = card;
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

public class CollectionActionKey {
    public static string COUNT_CHANGED = "COUNT_CHANGED";
}

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

public class CardEvent : GameAction<CardActionPayload> {
    public CardEvent(string key, CardActionPayload payload) : base(key, payload) {}
}

public class GameActionHandler<ArgType> {
    public ActionHandler<string, ArgType> before = new ActionHandler<string, ArgType>();
    public ActionHandler<string, ArgType> after = new ActionHandler<string, ArgType>();

    public void Invoke(string key, ArgType argType, Action action) {
        before.Trigger(key, argType);
        action();
        after.Trigger(key, argType);
    }
}

public class CollectionPayload<C, E> : IBase where E : Entity where C : Collection<E> {
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

public class BoardAreaPayload : CollectionPayload<BoardCollection, BoardEntity> { public BoardAreaPayload(BoardCollection b, Diff<BoardEntity> diff = null) : base(b, diff) {} }
public class CreatureAreaPayload : CollectionPayload<CreatureCollection, CreatureEntity> { public CreatureAreaPayload(CreatureCollection c, Diff<CreatureEntity> diff = null) : base(c, diff) {} }

public static class GS
{
    // to be initialized
    public static Phases phases;
    public static Turn currentTurn;
    public static AbstractCardGameController activeController;
    public static AbstractCardGameController passiveController;
    // end
    
    // todo: finish complete tree and add listeners to cascade calls (see Hand)
    public static GameActionHandler<CardActionPayload> cardActionHandler = new GameActionHandler<CardActionPayload>();

    public static GameActionHandler<CardCollectionPayload> cardCollectionActionHandler = new GameActionHandler<CardCollectionPayload>();
    // into
    public static GameActionHandler<HandPayload> handActionHandler = new GameActionHandler<HandPayload>();
    public static GameActionHandler<DeckPayload> deckActionHandler = new GameActionHandler<DeckPayload>();
    
    
    public static GameActionHandler<BoardAreaPayload> boardAreaActionHandler = new GameActionHandler<BoardAreaPayload>();
    // into
    public static GameActionHandler<CreatureAreaPayload> creatureAreaActionHandler = new GameActionHandler<CreatureAreaPayload>();


}

public class GamePhase : Phase
{
    public string phaseName;
    Func<GamePhase> _nextPhase;
    Action<GamePhase> _onEntry;
    Action<GamePhase> _onExit;
    Func<GamePhase, bool> _hasOptions;
    public GamePhase(string name, Func<GamePhase> nextPhase, Action<GamePhase> onEntry, Action<GamePhase> onExit, Func<GamePhase, bool> hasOptions)
    {
        phaseName = name;
        _nextPhase = nextPhase;
        _onEntry = onEntry;
        _onExit = onExit;
        _hasOptions = hasOptions;
    }

    public override void onEntry() { Debug.Log("OnEntry: " + phaseName); _onEntry.Invoke(this); }
    public override void onExit() { Debug.Log("OnExit: " + phaseName); _onExit.Invoke(this); }

    public override bool hasOptions() { return _hasOptions.Invoke(this); }

    public override Phase nextPhase() { return _nextPhase.Invoke(); }
}

public class PhaseUtil
{
    public static void drawCard(GamePhase p)
    {
        GS.activeController.player.drawCard();
    }
}

public class Phases
{
    public GamePhase drawPhase;
    public GamePhase mainPhase1;
    public GamePhase battlePhase;
    public GamePhase mainPhase2;
    public GamePhase endPhase;

    public Phases()
    {
        Action<GamePhase> defaultEntry = (GamePhase p) => { };
        Action<GamePhase> defaultExit = (GamePhase p) => { };
        Func<GamePhase, bool> defaultHasOptions = (GamePhase p) => false;

        endPhase = new GamePhase("EndPhase", () => null, defaultEntry, defaultExit, defaultHasOptions);
        mainPhase2 = new GamePhase("Main Phase 2", () => endPhase, defaultEntry, defaultExit, defaultHasOptions);
        battlePhase = new GamePhase("Battle Phase", () => mainPhase2, defaultEntry, defaultExit, defaultHasOptions);
        mainPhase1 = new GamePhase("Main Phase 1", () => battlePhase, defaultEntry, defaultExit, defaultHasOptions);
        drawPhase = new GamePhase("Draw Phase", () => mainPhase1, defaultEntry, (p) => PhaseUtil.drawCard(p), defaultHasOptions);
    }
}

public class TurnContext : ITurnContext {
    Side side { get; set; }
}

public class Turn : ITurn<TurnContext>
{
    public Turn(TurnContext tc) : base(GS.phases.drawPhase, tc)
    {}

    public override void startTurn()
    {
        this.currentPhase = GS.phases.drawPhase;
        GS.currentTurn = this;
    }
}

class MyCardGame : TurnGame<Turn, TurnContext, AbstractCardGameController>
{
    public Phases phases;

    public MyCardGame(AbstractCardGameController p1, DeckBlueprint d1, AbstractCardGameController p2, DeckBlueprint d2)
    {
        Player player1 = new Player(d1);
        Player player2 = new Player(d2);
        p1.Instantiate(player1);
        p2.Instantiate(player2);
        GS.activeController = p1;
        GS.passiveController = p2;

        GS.phases = new Phases();
    }

    public override Turn makeTurn() {
        return new Turn(new TurnContext());
    }

    public override IEnumerable<AbstractCardGameController> getNextPlayer()
    {
        while (true)
        {
            yield return GS.activeController;
            var tmp = GS.activeController;
            GS.activeController = GS.passiveController;
            GS.passiveController = tmp;
        }
    }
}


public class DeckBlueprint
{
    public Dictionary<CardBlueprint, int> cards;
    public DeckBlueprint(Dictionary<CardBlueprint, int> cards)
    {
        this.cards = cards;
    }
}


public class ACardGame : MonoBehaviour
{
    public GameObject cardPrefab;

    MyCardGame myCardGame;


    IEnumerator RunGame()
    {
        var advancer = myCardGame.advance();
        while (advancer.MoveNext())
        {
            var controller = GS.activeController;
            while (!controller.passesTurn())
            {
                yield return new WaitForEndOfFrame();
            }
            if (advancer.Current) break;
        }

        yield break;
    }


    // Start is called before the first frame update
    void Start()
    {
        var maker = new CardBPMaker(Instantiate, cardPrefab);

        var deckBp1 = new DeckBlueprint(new Dictionary<CardBlueprint, int>{
            { maker.makeCreatureBP("cardA", new Stats(3,3)), 5 },
            { maker.makeCreatureBP("cardB", new Stats(2,1)), 5 },
            { maker.makeCreatureBP("cardC", new Stats(3,4)), 5 },
            { maker.makeCreatureBP("cardD", new Stats(3,2)), 5 },
        });

        var deckBp2 = new DeckBlueprint(new Dictionary<CardBlueprint, int>{
            { maker.makeCreatureBP("cardA", new Stats(3,3)), 5 },
            { maker.makeCreatureBP("cardB", new Stats(2,1)), 5 },
            { maker.makeCreatureBP("cardC", new Stats(3,4)), 5 },
            { maker.makeCreatureBP("cardD", new Stats(3,2)), 5 },
        });


        var playerController = GetComponentInChildren<CardGamePlayerController>();
        var aiController = GetComponentInChildren<CardGameAiController>();


        myCardGame = new MyCardGame(playerController, deckBp1, aiController, deckBp2);

        var c = StartCoroutine(RunGame());

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
