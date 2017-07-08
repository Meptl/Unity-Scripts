/**
 * Health System
 */
using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour {
    // Should we clamp this?
    public float health = 0f;

    public void hurt(float dmg) {
        health -= dmg;
    }

    public void heal(float dmg) {
        health += dmg;
    }

    public bool isDead() {
        return health < 0;
    }
}
