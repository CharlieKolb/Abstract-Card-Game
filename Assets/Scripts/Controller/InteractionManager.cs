using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

using System.Linq;

public class InteractionManager : MonoBehaviour
{
    // Do not consume more than one interaction per call
    public List<Interaction> getInteractions() {
        var res = new List<Interaction>();

        if (GS.target != null) {
            return GameObject.FindGameObjectsWithTag("Targetable")
                .Where(x => GS.target.isValidTargetCondition.Invoke((x, new EffectTargetContext{ owner = GS.gameStateData.activeController.player })))
                .Select(x => new SelectTargetInteraction(GS.target, x, new EffectTargetContext{ owner = GS.gameStateData.activeController.player }))
                .ToList<Interaction>();
        }

        if (controller == GS.gameStateData.activeController) {
            res.Add(new PassPhaseInteraction());
        }

        return res
                .Concat(getInteractions(side.hand))
                .Concat(getInteractions(side.creatures)).ToList();
    }

    private List<Interaction> getInteractions(Hand hand) {
        var all = hand.getExisting();
        var playable = all
            .Where(c => c.value.canUseFromHand(side.player))
            .Select(x => new PlayCardInteraction(x.value, hand, side.player))
            .ToList<Interaction>();

        var saccable = all.Select(x => new SacCardInteraction(x.value, hand, side.player)).ToList<Interaction>();

        return playable.Concat(saccable).ToList();
    }

    private List<Interaction> getInteractions(CreatureCollection creatures) {
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
    
    Dictionary<Interaction, Action> currentInteractions = new Dictionary<Interaction, Action>(new Comparer());

    void flushInteractions() {
        foreach (var act in currentInteractions.Values) {
            act();
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
            var act = currentInteractions[key];
            currentInteractions.Remove(key);
            act();
        }
    }

    Action SpawnInteraction(Interaction interaction) {
        GameObject triggerObj = null;
        PointerEventData.InputButton button = PointerEventData.InputButton.Left;
        bool prepend = false;
        if (interaction is PlayCardInteraction) {
            var pci = (PlayCardInteraction) interaction;
            triggerObj = controller.handArea.getObject(pci.target).gameObject;
        }
        else if (interaction is SacCardInteraction) {
            var pci = (SacCardInteraction) interaction;
            var obj = controller.handArea.getObject(pci.target);
            button = PointerEventData.InputButton.Right;
            triggerObj = controller.handArea.getObject(pci.target).gameObject;
        }
        else if (interaction is PassPhaseInteraction) {
            var ppi = (PassPhaseInteraction) interaction;
            triggerObj = passTurnButton;
        }
        else if (interaction is DeclareAttackInteraction) {
            var dai = (DeclareAttackInteraction) interaction;
            triggerObj = controller.creatureArea.getObject(dai.creature).gameObject;
        }
        else if (interaction is SelectTargetInteraction) {
            var sti = (SelectTargetInteraction) interaction;
            triggerObj = sti.target;
            prepend = true;
        }




        var entry = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerDown,
        };

        entry.callback.AddListener((ed) => {
            if (((PointerEventData) ed).button == button) {
                flushInteractions();
                if (prepend) GS.PrependInteraction(interaction);
                else GS.EnqueueInteraction(interaction);
            }
        });
        triggerObj.GetComponent<EventTrigger>().triggers.Add(entry);
        return () => {
            entry.callback.RemoveAllListeners();
        };
    }

    // Update is called once per frame
    void Update()
    {
        updateInteractions();
    }
}
