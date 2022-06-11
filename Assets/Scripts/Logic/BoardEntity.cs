using System.Collections.Generic;

using System;

using CardGameInterface;


public class BoardEntity : Entity
{}


public class CreatureEntity : BoardEntity
{
    Stats stats;

    public CreatureEntity(Stats stats)
    {
        this.stats = stats;
    }
}


