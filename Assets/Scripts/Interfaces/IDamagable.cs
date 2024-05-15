/*
 * Author: Kilan Sky Larsen
 * Last Updated: 5/14/2024
 * Description: An interface to define the necessary aspects of damagable things in the game
 */

public interface IDamagable
{
    public int Shield { get; }
    public int Health { get; }

    void Damage(int damage);
    void Heal(int healAmt);
}
