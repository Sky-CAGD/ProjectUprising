using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public int Shield { get; }
    public int Health { get; }

    void Damage(int damage);
    void Heal(int healAmt);
}
