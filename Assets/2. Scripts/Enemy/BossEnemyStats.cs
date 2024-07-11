using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemyStats : EnemyStats
{
    public BossEnemyStats()
    {
        Damage = 20;
        HP = 200;
    }
}
public class NormalEnemyStats : EnemyStats
{
    public NormalEnemyStats()
    {
        Damage = 10;
        HP = 50;
    }
}
public class ChargingEnemyStats : EnemyStats
{
    public ChargingEnemyStats()
    {
        Damage = 15;
        HP = 150;
    }
}