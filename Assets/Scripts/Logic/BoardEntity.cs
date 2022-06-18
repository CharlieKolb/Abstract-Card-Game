using System.Collections.Generic;

using System;

using CardGameInterface;


public class BoardEntity : Entity
{
    public Player owner;

    public BoardEntity(Player owner) {
        this.owner = owner;
    }
}


public class CreatureEntity : BoardEntity
{
    Stats stats;

    public CreatureEntity(Player owner, Stats stats) : base(owner)
    {
        this.stats = stats;
    }
}


