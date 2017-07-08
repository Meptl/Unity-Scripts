/**
 * Hitbox: Screams when touched.
 * Though the underlying shape could anything.
 * This class covers both hit and hurt boxes with layer assignments distinguishing the two.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour {
    public HitBoxManager manager;
    public float damage = 0f;

    private int otherLayer;
    private bool isHit;
    private bool canTrigger;

    void Start() {
        manager = GetComponentInParent<HitBoxManager>();
        if (manager == null) {
            Debug.LogError("No manager for hitbox.");
        }

        // Hit/Hurt box determination is dictated by what layer we are on
        if (gameObject.layer == LayerMask.NameToLayer("HitBox")) {
            isHit = true;
            this.canTrigger = false; // Hitboxes off by default
        } else if (gameObject.layer == LayerMask.NameToLayer("HurtBox")) {
            isHit = false;
        } else {
            Debug.LogError("HitBox not assigned a layer of: HitBox or HurtBox.");
        }

        otherLayer = LayerMask.NameToLayer(isHit ? "HurtBox" : "HitBox");
    }

    /**
     * Communication stream: HurtBox -> Manager -> HitBoxManager
     * Currently HitBoxes don't do anything.
     */
    void OnTriggerEnter(Collider other) {
        if (this.canTrigger && other.gameObject.layer == otherLayer) {
            if (!isHit) {
                HitBox hitBox = other.GetComponent<HitBox>();
                if (hitBox == null) {
                    Debug.LogError(other.name + " on HitBox layer, but no HitBox Component.");
                }

                this.manager.inform(this, hitBox);
            } else {
                // HitBox response to hitting a hurtbox.
            }
        }
    }

    public void setTrigger(bool val) {
        this.canTrigger = val;
    }
}
