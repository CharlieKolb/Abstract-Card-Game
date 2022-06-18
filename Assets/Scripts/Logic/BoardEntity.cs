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
    public Stats stats;

    public CreatureEntity(Player owner, string name, Stats stats) : base(owner, name)
    {
        this.stats = stats;
    }
}


