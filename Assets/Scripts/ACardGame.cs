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
    public GameActionHandler<PlayerPayload> playerActionHandler = new GameActionHandler<PlayerPayload>();
    
    public GameActionHandler<CardActionPayload> cardActionHandler = new GameActionHandler<CardActionPayload>();

    public GameActionHandler<CardCollectionPayload> cardCollectionActionHandler = new GameActionHandler<CardCollectionPayload>();
    // into
    public GameActionHandler<HandPayload> handActionHandler = new GameActionHandler<HandPayload>();
    public GameActionHandler<DeckPayload> deckActionHandler = new GameActionHandler<DeckPayload>();
    public GameActionHandler<GraveyardPayload> graveyardActionHandler = new GameActionHandler<GraveyardPayload>();
    
    
    public GameActionHandler<CreatureAreaPayload> creatureAreaActionHandler = new GameActionHandler<CreatureAreaPayload>();

    public GameActionHandler<EntityPayload> entityActionHandler = new GameActionHandler<EntityPayload>();
    public GameActionHandler<BoardEntityPayload> boardActionHandler = new GameActionHandler<BoardEntityPayload>();
    public GameActionHandler<CreaturePayload> creatureActionHandler = new GameActionHandler<CreaturePayload>();


    public GameActionHandler<EnergyPayload> energyActionHandler = new GameActionHandler<EnergyPayload>();

    public GameActionHandler<PhasePayload> phaseActionHandler = new GameActionHandler<PhasePayload>();

    public GameActionHandler<EffectPayload> effectActionHandler = new GameActionHandler<EffectPayload>();


}


public class GS
{
    // to be initialized
    public static GameStateData gameStateData = new GameStateData();
    public static GameActions ga = new GameActions();


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
        GS.ga.phaseActionHandler.Invoke(PhaseActionKey.ENTER, new PhasePayload(this), () => _onEntry.Invoke(this));
    }
    public void executeExit() {
        GS.ga.phaseActionHandler.Invoke(PhaseActionKey.EXIT, new PhasePayload(this), () => _onExit.Invoke(this));
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
    void Start()
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


        Player player1 = new Player(deckBp1);
        Player player2 = new Player(deckBp2);
        p1Controller.Instantiate(player1);
        p2Controller.Instantiate(player2);
        GS.gameStateData.activeController = p1Controller;
        GS.gameStateData.passiveController = p2Controller;

        var s1 = new SideConfig {
            controller = p1Controller,
            deck = deckBp1, 
        };

        var s2 = new SideConfig {
            controller = p2Controller,
            deck = deckBp2, 
        };

        myCardGame = new MyCardGame(s1, s2);
        StartCoroutine(myCardGame.startGame());

    }
}
