using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using System;

using CardGameInterface;

public class EffectContext {
    public BoardEntity target;

    public Effect effect;
    public Player source;

    public EffectContext WithTarget(BoardEntity target) {
        this.target = target;
        return this;
    }

    public EffectContext WithEffect(Effect effect) {
        this.effect = effect;
        return this;
    }

    public EffectContext WithSource(Player source) {
        this.source = source;
        return this;
    }
}


public class Side
{
    public Player player;
    public Hand hand;
    public CreatureCollection creatures;
    public Deck deck;
    public Graveyard graveyard;
    public HP hp;
    public Energy energy;
    public Energy maxEnergy;

    public Side(DeckBlueprint deckBlueprint, Player player) {
        deck = Deck.FromBlueprint(deckBlueprint);
        this.player = player;

        hand = new Hand();
        creatures = new CreatureCollection();
        graveyard = new Graveyard();
        hp = new HP();
        maxEnergy = Energy.FromRed(5).WithGreen(5).WithBlue(5); 
        
        energy = new Energy(maxEnergy);
    }

    public bool hasOptions()
    {
        foreach (var e in hand.getExisting())
        {
            if (e.value.canUseFromHand(player)) return true;
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

    public CreatureCardBlueprint makeCreatureBP(string name)
    {
        var card = Cards.creatures[name];
        return new CreatureCardBlueprint(card, func, prefab);
    }
}


public abstract class CardBlueprint
{
    public CardData data;
    public List<Effect> effects;

    public delegate T InstantiateOriginal<T>(GameObject original) where T : UnityEngine.Object;

    GameObject prefab;
    F.InstantiateOriginal instantiate;



    public CardBlueprint(CardData data, List<Effect> effects, F.InstantiateOriginal instantiateFunc, GameObject prefab)
    {
        this.data = data;
        this.effects = effects;
        instantiate = instantiateFunc;
        this.prefab = prefab;
    }

    public abstract Card MakeCard();

    public GameObject Instantiate() {
        return instantiate(prefab);
    }
}

public class CreatureCardBlueprint : CardBlueprint {
    public Stats stats;
    public CreatureCardBlueprint(
        CreatureCardData data,
        F.InstantiateOriginal instantiateFunc,
        GameObject prefab
    ) : base(data, data.effects.Concat(new List<Effect>{ new SpawnCreatureEffect(data) }).ToList(), instantiateFunc, prefab) {
        this.stats = data.stats;
    }

    public override Card MakeCard()
    {
        return new CreatureCard(this);
    }
}



public class HP
{
    public int points = 20;
}


public class Player : IPlayer
{
    public Side side;
    public int lifepoints = 15;

    public Player(DeckBlueprint deckBlueprint)
    {
        side = new Side(deckBlueprint, this);
    }

    public void inflictDamage(int value) {
        lifepoints -= value;
        if (lifepoints <= 0) triggerLoss();
    }

    public bool hasOptions()
    {
        if (side.hasOptions()) return true;

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

public class GameStateData {
    public Turn currentTurn;
    public AbstractCardGameController activeController;
    public AbstractCardGameController passiveController;

}

public static class GS
{
    // to be initialized
    public static GameStateData gameStateData = new GameStateData();
    // end
    public static GameActionHandler<PlayerPayload> playerActionHandler = new GameActionHandler<PlayerPayload>();
    
    public static GameActionHandler<CardActionPayload> cardActionHandler = new GameActionHandler<CardActionPayload>();

    public static GameActionHandler<CardCollectionPayload> cardCollectionActionHandler = new GameActionHandler<CardCollectionPayload>();
    // into
    public static GameActionHandler<HandPayload> handActionHandler = new GameActionHandler<HandPayload>();
    public static GameActionHandler<DeckPayload> deckActionHandler = new GameActionHandler<DeckPayload>();
    public static GameActionHandler<GraveyardPayload> graveyardActionHandler = new GameActionHandler<GraveyardPayload>();
    
    
    public static GameActionHandler<CreatureAreaPayload> creatureAreaActionHandler = new GameActionHandler<CreatureAreaPayload>();

    public static GameActionHandler<EntityPayload> entityActionHandler = new GameActionHandler<EntityPayload>();
    public static GameActionHandler<BoardEntityPayload> boardActionHandler = new GameActionHandler<BoardEntityPayload>();
    public static GameActionHandler<CreaturePayload> creatureActionHandler = new GameActionHandler<CreaturePayload>();


    public static GameActionHandler<EnergyPayload> energyActionHandler = new GameActionHandler<EnergyPayload>();

    public static GameActionHandler<PhasePayload> phaseActionHandler = new GameActionHandler<PhasePayload>();

    public static bool debug = true;

    static GS() {
        if (debug) {
            cardActionHandler.before.onAll(x => Debug.Log(x.ToString()));
            cardCollectionActionHandler.before.onAll(x => Debug.Log(x.ToString()));
            handActionHandler.before.onAll(x => Debug.Log(x.ToString()));
            deckActionHandler.before.onAll(x => Debug.Log(x.ToString()));
            graveyardActionHandler.before.onAll(x => Debug.Log(x.ToString()));
            creatureAreaActionHandler.before.onAll(x => Debug.Log(x.ToString()));
        }
    }

    // Need an explicit (action, after) stack for cards to interact with queued casts
    private static List<(Action, Action)> gameStack = new List<(Action, Action)>();

    public static void PushAction(Action before, Action action, Action after) {
        gameStack.Add((action, after));
        before(); // might add further calls to the stack

        // assumptions: no interaction removes cards from the stack.
        //              may instead replace actions with no-ops
        if (action != gameStack[gameStack.Count - 1].Item1) {
            throw new Exception("GameStack invariant broken");
        }
        action();
        if (action != gameStack[gameStack.Count - 1].Item1) {
            throw new Exception("GameStack invariant broken 2");
        }
        gameStack.RemoveAt(gameStack.Count - 1);
        after();
    }

    private static ConcurrentQueue<Interaction> interactionQueue = new ConcurrentQueue<Interaction>();
    public static void EnqueueInteraction(Interaction interaction) {
        interaction.execute();
        // interactionQueue.Enqueue(interaction);
    }
}


// Note that even before the beginning of a phase currentPhase of a Turn will already be changed
// Similarly we are still in the old turn after end of turn
public class GamePhase
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

    public void executeEntry() {
        GS.phaseActionHandler.Invoke(PhaseActionKey.ENTER, new PhasePayload(this), () => _onEntry.Invoke(this));
    }
    public void executeExit() {
        GS.phaseActionHandler.Invoke(PhaseActionKey.EXIT, new PhasePayload(this), () => _onExit.Invoke(this));
    }
    public bool hasOptions() { return _hasOptions.Invoke(this); }
    public GamePhase nextPhase() { return _nextPhase.Invoke(); }
}

public class PhaseUtil
{
    public static void drawCard(GamePhase p)
    {
        GS.gameStateData.activeController.player.drawCard();
    }
}

public static class Phases
{
    public static GamePhase drawPhase;
    public static GamePhase mainPhase1;
    public static GamePhase battlePhase;
    public static GamePhase mainPhase2;
    public static GamePhase endPhase;

    static Phases()
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
    public Turn(TurnContext tc) : base(Phases.drawPhase, tc)
    {}

    public override void startTurn()
    {
        GS.gameStateData.currentTurn = this;
        GS.phaseActionHandler.Invoke(PhaseActionKey.ENTER, new PhasePayload(Phases.drawPhase), () => {
            this.currentPhase = Phases.drawPhase;
        });
    }
}

class MyCardGame : TurnGame<Turn, TurnContext, AbstractCardGameController>
{
    public MyCardGame(AbstractCardGameController p1, DeckBlueprint d1, AbstractCardGameController p2, DeckBlueprint d2)
    {
        Player player1 = new Player(d1);
        Player player2 = new Player(d2);
        p1.Instantiate(player1);
        p2.Instantiate(player2);
        GS.gameStateData.activeController = p1;
        GS.gameStateData.passiveController = p2;
    }

    public override Turn makeTurn() {
        return new Turn(new TurnContext());
    }

    public override IEnumerable<AbstractCardGameController> getNextPlayer()
    {
        while (true)
        {
            yield return GS.gameStateData.activeController;
            var tmp = GS.gameStateData.activeController;
            GS.gameStateData.activeController = GS.gameStateData.passiveController;
            GS.gameStateData.passiveController = tmp;
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
            yield return new WaitForEndOfFrame();
        }

        yield break;
    }


    // Start is called before the first frame update
    void Start()
    {
        var maker = new CardBPMaker(Instantiate, cardPrefab);

        var deckBp1 = new DeckBlueprint(new Dictionary<CardBlueprint, int>{
            { maker.makeCreatureBP("Brute"), 5 },
            { maker.makeCreatureBP("Fisher"), 5 },
            { maker.makeCreatureBP("Guardian"), 5 },
        });

        var deckBp2 = new DeckBlueprint(new Dictionary<CardBlueprint, int>{
            { maker.makeCreatureBP("Brute"), 5 },
            { maker.makeCreatureBP("Fisher"), 5 },
            { maker.makeCreatureBP("Guardian"), 5 },
        });


        var p1Controller = this.transform.Find("Player1").GetComponent<AbstractCardGameController>();
        var p2Controller = this.transform.Find("Player2").GetComponent<AbstractCardGameController>();


        myCardGame = new MyCardGame(p1Controller, deckBp1, p2Controller, deckBp2);

        var c = StartCoroutine(RunGame());

    }


    // Update is called once per frame
    void Update()
    {}
}
