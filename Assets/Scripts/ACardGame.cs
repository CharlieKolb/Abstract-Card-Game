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
}

public class GameStateData {
    public GamePhase currentPhase;

    public AbstractCardGameController activeController;
    public List<AbstractCardGameController> passiveControllers = new List<AbstractCardGameController>();
}


public class GameActions {
    public GameActionHandler actionHandler;

    public GameActions(GameActions parent = null) {
        actionHandler = new GameActionHandler(parent?.actionHandler);
    }
}

public class GS
{
    public GameStateData gameStateData;
    public GameActions ga;

    public GS(GameActions actions) {
        ga = actions;
        gameStateData = new GameStateData();
    }
}


// Note that even before the beginning of a phase currentPhase of a Turn will already be changed
// Similarly we are still in the old turn after end of turn
public class GamePhase : Entity
{
    public string phaseName;
    Func<GamePhase> _nextPhase;
    public GamePhase(string name, Func<GamePhase> nextPhase)
    {
        phaseName = name;
        _nextPhase = nextPhase;
    }

    public GS executeEntry(GS gameState) {
        gameState.ga.actionHandler.Invoke(new Reactions.PHASE.ENTER(gameState, this), (pl) => {
            Announce(pl);
            return pl;
        });
        return gameState;
    }

    public GS executeExit(GS gameState) {
        var pl = new PhasePayload(gameState, this);
        gameState = gameState.ga.actionHandler.Invoke(new Reactions.PHASE.EXIT(gameState, this), (pl) => {
            Announce(pl);
            return pl;
        });
        return gameState;
    }
    public GamePhase nextPhase() { return _nextPhase.Invoke(); }
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
        endPhase = new GamePhase("EndPhase", () => null);
        mainPhase2 = new GamePhase("Main Phase 2", () => endPhase);
        battlePhase = new GamePhase("Battle Phase", () => mainPhase2);
        mainPhase1 = new GamePhase("Main Phase 1", () => battlePhase);
        drawPhase = new GamePhase("Draw Phase", () => mainPhase1);
    }
}



class MyCardGame
{
    public Engine engine { get; private set; }

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

        var config = new GameConfig(
            new List<SideConfig>{ s1, s2 }
        );


        myCardGame = new MyCardGame(
            config
        );

        transform
            .Find("PhaseDisplay")
            .GetComponent<PhaseDisplay>()
            .RegisterEngine(
                myCardGame.engine
            );

    }

    void Start() {
        StartCoroutine(myCardGame.startGame());
    }
}

class RuleSet : IRuleSet {
    public GameActions getActions(Engine engine) {
        var actions = new GameActions();

        // these react on before, but should really be part of the actual invoke, not sure how to do that
        actions.actionHandler.before.on<Reactions.PHASE.ENTER>(Reactions.PHASE.ENTER.Key, (x) => {
            if (x.phase == Phases.drawPhase) {
                var energy = new Energy(x.gameState.gameStateData.activeController.player.side.maxEnergy);
                x.gameState = actions.actionHandler.Invoke(
                    new Reactions.ENERGY.RECHARGE(x.gameState, energy, x.gameState.gameStateData.activeController.player.side),
                    (pl) => {
                        pl.side.energy = pl.energyAmount;
                        return pl;
                    }
                );

                x.gameState = actions.actionHandler.Invoke(
                    new Reactions.DECK.DRAW(
                        x.gameState,
                        x.gameState.gameStateData.activeController.player.side
                    ),
                    GameActionsUtil.handleCardDraw
                );
                
                foreach (var creature in x.gameState.gameStateData.activeController.player.side.creatures
                    .getExisting()
                    .Select(x => x.value))
                {
                    creature.hasAttacked = false;   
                }   
            }

            return x;
        });

        actions.actionHandler.before.on<Reactions.PHASE.EXIT>(Reactions.PHASE.EXIT.Key, (pl) => {
            var gsd = pl.gameState.gameStateData;
            gsd.currentPhase = gsd.currentPhase.nextPhase();
            if (gsd.currentPhase == null) {
                gsd.currentPhase = Phases.drawPhase;
                gsd.passiveControllers.Add(gsd.activeController);
                gsd.activeController = gsd.passiveControllers[0];
                gsd.passiveControllers.RemoveAt(0);
            }
            pl.gameState = gsd.currentPhase.executeEntry(pl.gameState);
            return pl;
        });


        return actions;
    }
}

class GameConfig : IGameConfig {
    List<SideConfig> configs;
    public RuleSet ruleSet;
    public GameConfig(List<SideConfig> sideConfigs) {
        configs = sideConfigs;
        ruleSet = new RuleSet();
    }


    public List<SideConfig> getSides() {
        return configs;
    }

    public IRuleSet getRuleSet() {
        return ruleSet;
    }
}