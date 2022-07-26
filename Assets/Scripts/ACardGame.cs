using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using System.Linq;

using System;

using CardGameInterface;


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


public class Player
{
    public Side side;
    public int lifepoints = 15;

    public Player(DeckBlueprint deckBlueprint)
    {
        side = new Side(deckBlueprint, this);
    }

    public void inflictDamage(int value) {
        lifepoints -= value;
        // if (lifepoints <= 0) triggerLoss();
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
            // triggerLoss();
            return;
        }
        side.hand.add(side.deck.draw());
    }
}

public class GameStateData {
    public GamePhase currentPhase;
    public AbstractCardGameController activeController;
    public AbstractCardGameController passiveController;
}


public class GameActions {
    public GameActionHandler<PlayerPayload> playerActionHandler;
    
    public GameActionHandler<CardActionPayload> cardActionHandler;

    public GameActionHandler<CardCollectionPayload> cardCollectionActionHandler;
    // into
    public GameActionHandler<HandPayload> handActionHandler;
    public GameActionHandler<DeckPayload> deckActionHandler;
    public GameActionHandler<GraveyardPayload> graveyardActionHandler;
    
    
    public GameActionHandler<CreatureAreaPayload> creatureAreaActionHandler;

    public GameActionHandler<EntityPayload> entityActionHandler;
    public GameActionHandler<BoardEntityPayload> boardActionHandler;
    public GameActionHandler<CreaturePayload> creatureActionHandler;


    public GameActionHandler<EnergyPayload> energyActionHandler;

    public GameActionHandler<PhasePayload> phaseActionHandler;

    public GameActionHandler<EffectPayload> effectActionHandler;

    public GameActions(Engine engine) {
        playerActionHandler = new GameActionHandler<PlayerPayload>(engine);
        cardActionHandler = new GameActionHandler<CardActionPayload>(engine);
        cardCollectionActionHandler = new GameActionHandler<CardCollectionPayload>(engine);
        handActionHandler = new GameActionHandler<HandPayload>(engine);
        deckActionHandler = new GameActionHandler<DeckPayload>(engine);
        graveyardActionHandler = new GameActionHandler<GraveyardPayload>(engine);
        creatureAreaActionHandler = new GameActionHandler<CreatureAreaPayload>(engine);
        entityActionHandler = new GameActionHandler<EntityPayload>(engine);
        boardActionHandler = new GameActionHandler<BoardEntityPayload>(engine);
        creatureActionHandler = new GameActionHandler<CreaturePayload>(engine);
        energyActionHandler = new GameActionHandler<EnergyPayload>(engine);
        phaseActionHandler = new GameActionHandler<PhasePayload>(engine);
        effectActionHandler = new GameActionHandler<EffectPayload>(engine);
    }


}


public class GS
{
    // to be initialized
    public static GameStateData gameStateData_global = new GameStateData();
    public static GameActions ga_global;

    public GameStateData gameStateData => gameStateData_global;
    public GameActions ga => ga_global;

    public GS(Engine engine) {
        // ga = new GameActions(engine);
    }


    // end
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
        GS.ga_global.phaseActionHandler.Invoke(PhaseActionKey.ENTER, new PhasePayload(this), () => _onEntry.Invoke(this));
    }
    public void executeExit() {
        GS.ga_global.phaseActionHandler.Invoke(PhaseActionKey.EXIT, new PhasePayload(this), () => _onExit.Invoke(this));
    }
    public bool hasOptions() { return _hasOptions.Invoke(this); }
    public GamePhase nextPhase() { return _nextPhase.Invoke(); }
}

public class PhaseUtil
{
    public static void drawCard(GamePhase p)
    {
        GS.gameStateData_global.activeController.player.drawCard();
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



class MyCardGame
{
    Engine engine;
    public MyCardGame(SideConfig s1, SideConfig s2)
    {
        engine = new Engine(s1, s2);
    }

    public IEnumerator startGame()  {
        engine.startGame();
        yield break;
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
    public GameObject targetSelecterPrefab;

    MyCardGame myCardGame;


    // Start is called before the first frame update
    void Awake()
    {
        var maker = new CardBPMaker(Instantiate, cardPrefab);

        var deckBp1 = new DeckBlueprint(new Dictionary<CardBlueprint, int>{
            { maker.makeCreatureBP("Baseball Kid"), 15 },
            { maker.makeCreatureBP("Ojama Green"), 5 },
        });

        var deckBp2 = new DeckBlueprint(new Dictionary<CardBlueprint, int>{
            { maker.makeCreatureBP("Mystical Elf"), 15 },
            { maker.makeCreatureBP("Ojama Green"), 5 },
        });


        var p1Controller = this.transform.Find("Player1").GetComponent<AbstractCardGameController>();
        var p2Controller = this.transform.Find("Player2").GetComponent<AbstractCardGameController>();


        var s1 = new SideConfig {
            controller = p1Controller,
            deck = deckBp1, 
        };

        var s2 = new SideConfig {
            controller = p2Controller,
            deck = deckBp2, 
        };

        GS.gameStateData_global.activeController = p1Controller;
        GS.gameStateData_global.passiveController = p2Controller;


        myCardGame = new MyCardGame(s1, s2);
    }

    void Start() {
        StartCoroutine(myCardGame.startGame());
    }
}
