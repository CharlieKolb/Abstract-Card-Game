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
    public List<AbstractCardGameController> passiveControllers = new List<AbstractCardGameController>();
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

    public GameActions(Engine engine, GameActions parent = null) {
        playerActionHandler = new GameActionHandler<PlayerPayload>(engine, parent?.playerActionHandler);
        cardActionHandler = new GameActionHandler<CardActionPayload>(engine, parent?.cardActionHandler);
        cardCollectionActionHandler = new GameActionHandler<CardCollectionPayload>(engine, parent?.cardCollectionActionHandler);
        handActionHandler = new GameActionHandler<HandPayload>(engine, parent?.handActionHandler);
        deckActionHandler = new GameActionHandler<DeckPayload>(engine, parent?.deckActionHandler);
        graveyardActionHandler = new GameActionHandler<GraveyardPayload>(engine, parent?.graveyardActionHandler);
        creatureAreaActionHandler = new GameActionHandler<CreatureAreaPayload>(engine, parent?.creatureAreaActionHandler);
        entityActionHandler = new GameActionHandler<EntityPayload>(engine, parent?.entityActionHandler);
        boardActionHandler = new GameActionHandler<BoardEntityPayload>(engine, parent?.boardActionHandler);
        creatureActionHandler = new GameActionHandler<CreaturePayload>(engine, parent?.creatureActionHandler);
        energyActionHandler = new GameActionHandler<EnergyPayload>(engine, parent?.energyActionHandler);
        phaseActionHandler = new GameActionHandler<PhasePayload>(engine, parent?.phaseActionHandler);
        effectActionHandler = new GameActionHandler<EffectPayload>(engine, parent?.effectActionHandler);
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
public class GamePhase : Entity
{
    public string phaseName;
    Func<GamePhase> _nextPhase;
    Action<GS, GamePhase> _onEntry;
    Action<GS, GamePhase> _onExit;
    public GamePhase(string name, Func<GamePhase> nextPhase, Action<GS, GamePhase> onEntry, Action<GS, GamePhase> onExit)
    {
        phaseName = name;
        _nextPhase = nextPhase;
        _onEntry = onEntry;
        _onExit = onExit;
    }

    public GS executeEntry(GS gameState) {
        var pl = new PhasePayload(this, gameState);
        gameState.ga.phaseActionHandler.Invoke(PhaseActionKey.ENTER, pl, () => _onEntry.Invoke(gameState, this));
        Announce(PhaseActionKey.ENTER, pl);
        return gameState;
    }
    public GS executeExit(GS gameState) {
        var pl = new PhasePayload(this, gameState);
        gameState.ga.phaseActionHandler.Invoke(PhaseActionKey.EXIT, pl, () => _onExit.Invoke(gameState, this));
        Announce(PhaseActionKey.EXIT, pl);
        return gameState;
    }
    public GamePhase nextPhase() { return _nextPhase.Invoke(); }
}

public class PhaseUtil
{
    public static void drawCard(GS gameState, GamePhase p)
    {
        // TODO(GameConfig)
        gameState.gameStateData.activeController.player.drawCard();
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
        Action<GS, GamePhase> defaultEntry = (GS gs, GamePhase p) => { };
        Action<GS, GamePhase> defaultExit = (GS gs, GamePhase p) => { };

        endPhase = new GamePhase("EndPhase", () => null, defaultEntry, defaultExit);
        mainPhase2 = new GamePhase("Main Phase 2", () => endPhase, defaultEntry, defaultExit);
        battlePhase = new GamePhase("Battle Phase", () => mainPhase2, defaultEntry, defaultExit);
        mainPhase1 = new GamePhase("Main Phase 1", () => battlePhase, defaultEntry, defaultExit);
        drawPhase = new GamePhase("Draw Phase", () => mainPhase1, defaultEntry, (gs, p) => PhaseUtil.drawCard(gs, p));
    }
}



class MyCardGame
{
    Engine engine;
    public MyCardGame(GameConfig config)
    {
        engine = new Engine(config);
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
        var p1Controller = this.transform.Find("Player1").GetComponent<AbstractCardGameController>();
        var p2Controller = this.transform.Find("Player2").GetComponent<AbstractCardGameController>();

        var deckBp1 = new DeckBlueprint(new Dictionary<CardBlueprint, int>{
            { maker.makeCreatureBP("Baseball Kid"), 15 },
            { maker.makeCreatureBP("Ojama Green"), 5 },
        });

        var deckBp2 = new DeckBlueprint(new Dictionary<CardBlueprint, int>{
            { maker.makeCreatureBP("Mystical Elf"), 15 },
            { maker.makeCreatureBP("Ojama Green"), 5 },
        });


        var s1 = new SideConfig {
            controller = p1Controller,
            deck = deckBp1, 
        };

        var s2 = new SideConfig {
            controller = p2Controller,
            deck = deckBp2, 
        };



        myCardGame = new MyCardGame(
            new GameConfig(
                new List<SideConfig>{ s1, s2 }
            )
        );
    }

    void Start() {
        StartCoroutine(myCardGame.startGame());
    }
}

class RuleSet : IRuleSet {
    public GameActions getActions(Engine engine) {
        var actions = new GameActions(engine);

        actions.phaseActionHandler.before.on(PhaseActionKey.ENTER, (x) => {
            if (x.phase == Phases.drawPhase) {
                x.activePlayer.side.energy = new Energy(x.activePlayer.side.maxEnergy);
                
                
                foreach (var item in x.activePlayer.side.creatures
                    .getExisting()
                    .Select(x => x.value))
                {
                    item.hasAttacked = false;   
                }   
            }
        });

        actions.phaseActionHandler.after.on(PhaseActionKey.EXIT, (x) => {
            var gsd = x.gameState.gameStateData;
            gsd.currentPhase = gsd.currentPhase.nextPhase();
            if (gsd.currentPhase == null) {
                gsd.currentPhase = Phases.drawPhase;
                gsd.passiveControllers.Add(gsd.activeController);
                gsd.activeController = gsd.passiveControllers[0];
                gsd.passiveControllers.RemoveAt(0);
            }
        });


        return actions;
    }
}

class GameConfig : IGameConfig {
    List<SideConfig> configs;
    public GameConfig(List<SideConfig> sideConfigs) {
        configs = sideConfigs;
    }


    public List<SideConfig> getSides() {
        return configs;
    }

    public IRuleSet getRuleSet() {
        return new RuleSet();
    }
}