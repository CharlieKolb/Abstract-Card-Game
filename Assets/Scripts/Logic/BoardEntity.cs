using System.Collections.Generic;

using System;

using CardGameInterface;


public class BoardEntity : Entity
{
    public Player owner;
    public string name;

    public BoardEntity(Player owner, string name) {
        this.owner = owner;
        this.name = name;
    }
}


public class CreatureEntity : BoardEntity
{
    public CreatureCardData baseData;
    public Stats stats;
    public bool hasAttacked = false;

    public CreatureEntity(Player owner, CreatureCardData data) : base(owner, data.name)
    {
        this.baseData = data;
        this.stats = new Stats(data.stats);
        GS.ga.phaseActionHandler.after.on(PhaseActionKey.ENTER, p => {
            if (GS.gameStateData.activeController.player == owner && p.phase == Phases.drawPhase) {
                hasAttacked = false;
            }
        });
    }
}


