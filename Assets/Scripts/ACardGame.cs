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
    public GameState gameState;
}




public class Side
{
    public Player player;
    public Hand hand;
    public CreatureCollection creatures;
    public Deck deck;
    public Graveyard graveyard;
    public HP hp;

    public Side(GameState gameState, DeckBlueprint deckBlueprint, Player player) {
        deck = Deck.FromBlueprint(deckBlueprint, gameState);
        hand = new Hand();
        creatures = new CreatureCollection();
        graveyard = new Graveyard();
        hp = new HP();
        this.player = player;
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

    public abstract Card MakeCard(GameState gameState);

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

    public override Card MakeCard(GameState gameState)
    {
        return new CreatureCard(gameState, this);
    }
}

public class Deck : CardCollection
{
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

    public override void postAdd(Card c)
    {
        base.postAdd(c);
        Shuffle();
    }

    public static Deck FromBlueprint(DeckBlueprint deck, GameState gameState)
    {
        var result = new Deck();
        foreach (var key in deck.cards.Keys)
        {
            var val = deck.cards[key];
            for (var i = 0; i < val; ++i)
            {
                result.add(key.MakeCard(gameState));
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

public class HandTrigger : CollectionTrigger
{
    public static string ON_DISCARD = "ON_DISCARD";
}

public class Hand : CardCollection<HandTrigger>
{
    public override bool hidden()
    {
        return true;
    }

    public override void postRemove(Card c)
    {
        
    }
}

public class HP
{
    public int points = 20;
}

public class PlayerController
{
    public PlayerController(Player player)
    {

    }
}

public class Player : IPlayer
{
    GameState gameState;
    public Side side;

    public Player(GameState gameState, DeckBlueprint deckBlueprint)
    {
        side = new Side(gameState, deckBlueprint, this);
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

public class GameState
{
    public Phases phases;
    public Turn currentTurn;
    public Player activePlayer;
    public Player opponentPlayer;

    public override string ToString()
    {
        return string.Format(
            "\n\tactivePlayer:{0},\n\toppopnentPlayer:{1}\n", activePlayer.ToString(), opponentPlayer.ToString()
        );
    }
}

public class GamePhase : Phase
{
    public string phaseName;
    public GameState gameState;
    Func<GamePhase> _nextPhase;
    Action<GamePhase> _onEntry;
    Action<GamePhase> _onExit;
    Func<GamePhase, bool> _hasOptions;
    public GamePhase(string name, GameState gs, Func<GamePhase> nextPhase, Action<GamePhase> onEntry, Action<GamePhase> onExit, Func<GamePhase, bool> hasOptions)
    {
        phaseName = name;
        gameState = gs;
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
        p.gameState.activePlayer.drawCard();
    }
}

public class Phases
{
    public GamePhase drawPhase;
    public GamePhase mainPhase1;
    public GamePhase battlePhase;
    public GamePhase mainPhase2;
    public GamePhase endPhase;

    public Phases(GameState gameState)
    {
        Action<GamePhase> defaultEntry = (GamePhase p) => { };
        Action<GamePhase> defaultExit = (GamePhase p) => { };
        Func<GamePhase, bool> defaultHasOptions = (GamePhase p) => false;

        endPhase = new GamePhase("EndPhase", gameState, () => null, defaultEntry, defaultExit, defaultHasOptions);
        mainPhase2 = new GamePhase("Main Phase 2", gameState, () => endPhase, defaultEntry, defaultExit, defaultHasOptions);
        battlePhase = new GamePhase("Battle Phase", gameState, () => mainPhase2, defaultEntry, defaultExit, defaultHasOptions);
        mainPhase1 = new GamePhase("Main Phase 1", gameState, () => battlePhase, defaultEntry, defaultExit, defaultHasOptions);
        drawPhase = new GamePhase("Draw Phase", gameState, () => mainPhase1, defaultEntry, (p) => PhaseUtil.drawCard(p), defaultHasOptions);
    }
}

public class TurnContext : ITurnContext {
    Side side { get; set; }
}

public class Turn : ITurn<TurnContext>
{
    GameState gameState;
    public Turn(GameState gameState, TurnContext tc) : base(gameState.phases.drawPhase, tc)
    {
        this.gameState = gameState;
    }

    public override void startTurn()
    {
        this.currentPhase = gameState.phases.drawPhase;
        gameState.currentTurn = this;
    }
}

class MyCardGame : TurnGame<Turn, TurnContext, Player>
{
    public GameState gameState;
    public Phases phases;

    public MyCardGame(DeckBlueprint playerDeck, DeckBlueprint opponentDeck)
    {
        gameState = new GameState();
        gameState.activePlayer = new Player(gameState, playerDeck);
        gameState.opponentPlayer = new Player(gameState, opponentDeck);

        gameState.phases = new Phases(gameState);
    }

    public override Turn makeTurn() {
        return new Turn(gameState, new TurnContext());
    }

    public override IEnumerable<Player> getNextPlayer()
    {
        while (true)
        {
            yield return gameState.activePlayer;
            var tmp = gameState.activePlayer;
            gameState.activePlayer = gameState.opponentPlayer;
            gameState.opponentPlayer = tmp;
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
        Debug.Log(1);
        var advancer = myCardGame.advance();

        while (advancer.MoveNext())
        {
            while (keyPressed == false)
            {
                yield return new WaitForEndOfFrame(); // yield frame until A is pressed
            }
            keyPressed = false;
            if (advancer.Current) break;
        }

        Debug.Log(2);
        Debug.Log(myCardGame.gameState.activePlayer.victoryState);
        Debug.Log(myCardGame.gameState.opponentPlayer.victoryState);
        yield break;
    }


    // Start is called before the first frame update
    void Start()
    {

        var maker = new CardBPMaker(Instantiate, cardPrefab);

        var deckBp = new DeckBlueprint(new Dictionary<CardBlueprint, int>{
            { maker.makeCreatureBP("cardA", new Stats(3,3)), 5 },
            { maker.makeCreatureBP("cardB", new Stats(2,1)), 5 },
            { maker.makeCreatureBP("cardC", new Stats(3,4)), 5 },
            { maker.makeCreatureBP("cardD", new Stats(3,2)), 5 },
        });

        myCardGame = new MyCardGame(deckBp, deckBp); // Need to clone here!!!

        var playerController = GetComponentInChildren<CardGamePlayerController>();
        playerController.Instantiate(myCardGame.gameState.activePlayer);

        var c = StartCoroutine(RunGame());

    }

    bool keyPressed = false;

    // Update is called once per frame
    void Update()
    {
        if (!keyPressed)
            keyPressed = Input.GetKeyDown(KeyCode.A);
    }
}
