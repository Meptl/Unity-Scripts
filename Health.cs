/**
 * Health System
 */
using UnityEngine;
using UnityEngine.Networking;

public class Health : NetworkBehaviour {
    // Currently this is static
    public const float maxHealth = 100f;

    [SyncVar]
    public float health = maxHealth;

    /**
     * To hurt apply negative values.
     */
    public void heal(float dmg) {
        if (!isServer) return;

        health += dmg;

        if (health <= 0) {
            RpcDie();
        }
        if (health > maxHealth) {
            health = maxHealth;
        }
    }

    [ClientRpc]
    void RpcDie() {
        if (isLocalPlayer) {
            Destroy(gameObject);
        }
    }
}
