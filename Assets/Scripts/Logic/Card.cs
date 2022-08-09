using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// A logical card, usually a collection of effects based of a blueprint
public abstract class Card : Entity {
    protected CardData data;

    public Energy cost => data.cost;
    public Energy sac => data.sac;
    public string name => data.name;

    public List<Effect> effects => data.effects.Concat(childEffects()).ToList();

    public Card(CardData data) {
        this.data = data;
    }

    public GS use(GS gameState, Player owner) {
        gameState = doUse(gameState, owner);
        effects.ForEach(x => {
            gameState = x.apply(gameState, owner);
        });
        return gameState;
    }

    protected virtual GS doUse(GS gameState, Player owner) { return gameState; }

    public bool canUseFromHand(GS gameState, Player owner) {
        if (gameState.gameStateData.activeController.player != owner.side.player) {
            return false;
        }

        return childCanUseFromHand(gameState, owner) && cost.canBePaid(owner.side.energy) && effects.TrueForAll(x => x.canApply(gameState, owner));
    }

    protected abstract bool childCanUseFromHand(GS gameSTate, Player owner);
    protected virtual List<Effect> childEffects() {
        return new List<Effect>();
    }
}

public class CreatureCard : Card {
    public CreatureCardData creatureData;
    public Effect spawnEffect;

    public CreatureCard(CreatureCardData data) : base(data) {
        creatureData = data;
        spawnEffect = new SpawnCreatureEffect(data);
    }

    protected override bool childCanUseFromHand(GS gameState, Player owner) {
        if (!new List<GamePhase>{ Phases.mainPhase1, Phases.mainPhase2 }.Contains(gameState.gameStateData.currentPhase)) {
            return false;
        }
        return true;        
    }

    protected override List<Effect> childEffects() {
        return new List<Effect>{spawnEffect};
    }

    public override bool isColor(EffectHandle.ColorPattern color) {
        return creatureData.cost.getValue(color) > 0;
    }
}