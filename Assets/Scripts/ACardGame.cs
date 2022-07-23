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

    private static List<IEnumerator<bool>> interactionQueue = new List<IEnumerator<bool>>();
    
    public static void PushInteraction(Interaction interaction) {
        interactionQueue.Insert(0, interaction.execute());
    }

    static List<IEnumerator<EffectTarget>> targetQueue = new List<IEnumerator<EffectTarget>>();
    public static bool isResolvingEffects => targetQueue.Count > 0;

    // Return false if cancelled
    public static EffectTarget ResolveEffectTargets(Effect effect, Player owner) {
        var effectTargets = effect.resolveTargets();
        if (!effectTargets.MoveNext()) return null;
    
        var cur = effectTargets.Current;
        targetQueue.Insert(0, effectTargets);
        return cur;
    }

    public static void CancelSelection() {
        while (targetQueue.Count > 0) {
            var front = targetQueue[0];
            if (front.Current != null) front.Current.cancelled = true;
            targetQueue.RemoveAt(0);
        }
        gameStateData.activeController.im.target = null;
    }

    public static void Tick() {
        while (targetQueue.Count > 0) {
            var front = targetQueue[0];
            if (!front.MoveNext()) targetQueue.RemoveAt(0);
            else {
                gameStateData.activeController.im.target = front.Current;
                break;
            }
        }

        while (interactionQueue.Count > 0) {
            IEnumerator<bool> front = interactionQueue[0];
            if (!front.MoveNext() || front.Current == false) {
                interactionQueue.RemoveAt(0);
            }
            else {
                return;
            }
        }
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
        GS.ga.phaseActionHandler.Invoke(PhaseActionKey.ENTER, new PhasePayload(Phases.drawPhase), () => {
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
        for (int i = 0; i < 5; ++i) {
            player1.drawCard();
            player2.drawCard();
        }
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
    public GameObject targetSelecterPrefab;

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
            { maker.makeCreatureBP("Baseball Kid"), 15 },
            { maker.makeCreatureBP("Ojama Green"), 5 },
        });

        var deckBp2 = new DeckBlueprint(new Dictionary<CardBlueprint, int>{
            { maker.makeCreatureBP("Mystical Elf"), 15 },
            { maker.makeCreatureBP("Ojama Green"), 5 },
        });


        var p1Controller = this.transform.Find("Player1").GetComponent<AbstractCardGameController>();
        var p2Controller = this.transform.Find("Player2").GetComponent<AbstractCardGameController>();


        myCardGame = new MyCardGame(p1Controller, deckBp1, p2Controller, deckBp2);

        var c = StartCoroutine(RunGame());

    }


    void Update()
    {
        GS.Tick();
    }
}
