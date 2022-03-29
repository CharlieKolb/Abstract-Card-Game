using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CardGameInterface;

public class BoardEntity : ActionHandler<string, EffectContext>
{}


public class CreatureEntity : BoardEntity
{
    Stats stats;

    public CreatureEntity(Stats stats)
    {
        this.stats = stats;
    }
}