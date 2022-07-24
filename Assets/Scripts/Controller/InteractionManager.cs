using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

using System.Linq;

public class InteractionManager : MonoBehaviour
{
    AbstractCardGameController controller;
    Side side => controller.player.side;
    GameObject passTurnButton;

    // stack to keep track of which is the active listener
    int stack = 0;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponentInParent<AbstractCardGameController>();   

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
    
    Dictionary<int, List<(EventTrigger.Entry, List<EventTrigger.Entry>)>> eventDict = new Dictionary<int, List<(EventTrigger.Entry, List<EventTrigger.Entry>)>>();
    public void updateInteractions(List<Interaction> interactions, Action<Interaction> callback) {
        int stackId = ++stack;
        eventDict[stackId] = new List<(EventTrigger.Entry, List<EventTrigger.Entry>)>();
        
        foreach (var iact in interactions) {
            SpawnInteraction(iact, callback, stackId);
        }
    }

    GameObject resolveObject(EffectContext ec) {
        var targetable = GameObject.FindGameObjectsWithTag("Targetable")
            .Select(x => (x, x.GetComponent<EntityObject>()))
            .Where(x => x.Item2 != null)
            .Where(x => x.Item2.getEntity().Equals(ec.targetEntity))
            .ToList();

        if (targetable.Count < 1) {
            throw new Exception("Did not find match");
        }

        if (targetable.Count > 1) {
            throw new Exception("Found multiple matches for EffectContext");
        }

        return targetable[0].Item1;
    }

    void SpawnInteraction(Interaction interaction, Action<Interaction> callback, int id) {
        GameObject triggerObj = null;
        PointerEventData.InputButton button = PointerEventData.InputButton.Left;
        if (interaction is PlayCardInteraction) {
            var pci = (PlayCardInteraction) interaction;
            triggerObj = controller.handArea.getObject(pci.card).gameObject;
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
            triggerObj = resolveObject(sti.context);
        }
        else if (interaction is CancelSelectionInteraction) {
            var csi = (CancelSelectionInteraction) interaction;
            triggerObj = passTurnButton;
            // pass
        }

        var entry = new EventTrigger.Entry {
            eventID = EventTriggerType.PointerDown,
        };


        var triggers = triggerObj.GetComponent<EventTrigger>().triggers;
        entry.callback.AddListener((ed) => {
            lock(callback) {
                if (ed.used) return;
                if (id != stack) {
                    Debug.Log("denied due to stack! mine: " + id + ", current: " + stack);
                    return; // swallow events for newer interactions so we don't resolve on their triggers
                }
                if (((PointerEventData) ed).button == button) {
                    ed.Use();
                    entry.callback.RemoveAllListeners();
                    Debug.Log(triggers.Count);
                    eventDict[id].ForEach(x => x.Item2.Remove(x.Item1));
                    Debug.Log(triggers.Count);
                    eventDict.Remove(id);

                    --stack;

                    Debug.Log("mine: " + id + ", current: " + stack);
                    callback(interaction);
                }
            }
        });

        if (!triggers.Contains(entry)) {
            triggers.Add(entry);
            eventDict[id].Add((entry, triggers));
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
