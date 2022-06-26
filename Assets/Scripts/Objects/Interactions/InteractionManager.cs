using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class InteractionManager : MonoBehaviour
{
    public GameObject interactionObject;

    public List<Interaction> getInteractions() {
        return new List<Interaction>{ new PassPhaseInteraction() }
                .Concat(getInteractions(side.hand))
                .Concat(getInteractions(side.creatures)).ToList();
    }

    public List<Interaction> getInteractions(Hand hand) {
        if (GS.gameStateData.activeController.player != side.player || ! new List<GamePhase>{ Phases.mainPhase1, Phases.mainPhase2 }.Contains(GS.gameStateData.currentTurn.currentPhase)) {
            return new List<Interaction>();
        }

        return hand.getExisting()
            .Where(c => c.value.canUseFromHand(side.player))
            .Select(x => new PlayCardInteraction(x.value, hand, side.player))
            .ToList<Interaction>();
    }

    public List<Interaction> getInteractions(CreatureCollection creatures) {
        if (GS.gameStateData.activeController.player != side.player || GS.gameStateData.currentTurn.currentPhase != Phases.battlePhase) {
            return new List<Interaction>();
        }

        return creatures.getExisting()
            .Where(c => !c.value.hasAttacked)
            .Select(x => new DeclareAttackInteraction(x.value, side.player))
            .ToList<Interaction>();
    }

    AbstractCardGameController controller;
    Side side => controller.player.side;
    GameObject passTurnButton;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponentInParent<AbstractCardGameController>();   
        GS.phaseActionHandler.after.on(PhaseActionKey.ENTER, (_) => flushInteractions());

        passTurnButton = transform.Find("PassTurnButton").gameObject;
    }

    class Comparer : IEqualityComparer<Interaction> {
        public bool Equals(Interaction i1, Interaction i2) {
            return i1.Equals(i2);
        }

        public int GetHashCode(Interaction i)
        {
            return i.GetHashCode();
        }
    }

    Dictionary<Interaction, InteractionObject> currentInteractions = new Dictionary<Interaction, InteractionObject>(new Comparer());

    void flushInteractions() {
        foreach (var obj in currentInteractions.Values) {
            Destroy(obj.gameObject);
        }
        currentInteractions.Clear();
        updateInteractions();
    }

    void updateInteractions() {
        var interactions = getInteractions();
        var seenKeys = new HashSet<Interaction>();
        foreach (var iact in interactions) {
            if (currentInteractions.ContainsKey(iact)) {
                seenKeys.Add(iact);
                continue;
            }
            currentInteractions.Add(iact, SpawnInteraction(iact));
            seenKeys.Add(iact);
        }

        var toBeRemoved = new HashSet<Interaction>();
        foreach (var key in currentInteractions.Keys) {
            if (!seenKeys.Contains(key)) {
                toBeRemoved.Add(key);
            }
        }

        foreach (var key in toBeRemoved) {
            var obj = currentInteractions[key];
            currentInteractions.Remove(key);
            Destroy(obj.gameObject);
        }
    }

    InteractionObject SpawnInteraction(Interaction interaction) {
        var iobj = Instantiate(interactionObject).GetComponent<InteractionObject>();
        if (interaction is PlayCardInteraction) {
            var pci = (PlayCardInteraction) interaction;
            var obj = controller.handArea.getObject(pci.target);
            iobj.transform.SetParent(obj.transform, false);
        }
        else if (interaction is PassPhaseInteraction) {
            var ppi = (PassPhaseInteraction) interaction;
            iobj.transform.SetParent(passTurnButton.transform, false);
        }
        else if (interaction is DeclareAttackInteraction) {
            var dai = (DeclareAttackInteraction) interaction;
            iobj.transform.SetParent(controller.creatureArea.getObject(dai.creature).transform, false);
        }
        iobj.transform.position = iobj.transform.parent.position;
        iobj.OnClick.AddListener(() => {
            flushInteractions();
            GS.EnqueueInteraction(interaction);
        });

        return iobj;
    }

    // Update is called once per frame
    void Update()
    {
        updateInteractions();
    }
}
