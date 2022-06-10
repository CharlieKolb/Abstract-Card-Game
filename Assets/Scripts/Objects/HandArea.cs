using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using CardGameInterface;


public class HandArea : Area<Hand, Card, CardObject> {
    
    public GameObject creatureCardPrefab;
    // public GameObject spellCardPrefab;

    public UnityEvent<CardObject> onCardUse = new UnityEvent<CardObject>();


    public override CardObject resolvePrefab(Card card) {
        if (card is CreatureCard) {
            var obj = Instantiate(creatureCardPrefab).GetComponent<CreatureCardObject>();
            obj.Instantiate(card, () => onCardUse.Invoke(obj));
            return obj;
        }

        throw new System.Exception("Unhandled card type!");
    }

    public override void Start() {
        base.Start();
    }

    public override void Update() {
        base.Update();

        if (collection == null) return;


        moveCardsTowardsTarget(Time.deltaTime * 0.5f);
    }


    public override void initCollection(Hand hand) {
        GS.handActionHandler.after.on(HandActionKey.COUNT_CHANGED, (x) => {
            if (x.collection == this.collection) doRefresh(x.diff);
        });
        
    }

    public override void refresh(Diff<Card> diff) {
        refreshCards();
    }
    
    void refreshCards() {
        targetPositions = new List<Vector3>();
        var cl = cardLayout(collection.Count);
        for(var i = 0; i < collection.Count; ++i) {
            var card = collection[i];
            var gameObj = objectMapper[card].gameObject;
            gameObj.SetActive(true);
            gameObj.transform.parent = this.transform;

            var width = gameObj.transform.GetComponent<BoxCollider>().bounds.size.z;
            targetPositions.Add(new Vector3(width * cl[i], 0, 0));

        }
        moveCardsTowardsTarget(1f);
    }

    public void moveCardsTowardsTarget(float percent) {
        for (var i = 0; i < targetPositions.Count; ++i) {
            var cardTransform = objectMapper[collection[i]].transform;
            var curr = cardTransform.position;
            var target = targetPositions[i] + transform.parent.position;

            if (curr != target) {
                cardTransform.position = Vector3.MoveTowards(curr, target, percent * Vector3.Distance(curr, target));
            }
        }
    }

    float[] cardLayout(int cardCount) {
        // 0: []
        // 1: [0]
        // 2: [-0.5, 0.5]
        // 3: [-1, 0, 1]
        // 4: [-1.5, -0.5, 0.5, 1.5]
        float[] res = new float[cardCount];
        for(var i = 0; i < cardCount; ++i) {
            res[i] = -((cardCount - 1) / 2.0f) + i; 
        }
        return res;
    }
}

