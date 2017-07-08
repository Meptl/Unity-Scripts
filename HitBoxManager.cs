/**
 * HitBoxManager: Contacted by all child hit boxes.
 * HitBox -> HitBoxManager -> Health
 *
 * One limitation of animation events is it only allows one int parameter.
 * We'll deal with this limitation later. (Figure out how to send an int arr)
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBoxManager : MonoBehaviour {
    public HitBox[] hitboxes;
    private Health health;

    void Start() {
        health = GetComponent<Health>(); // Not a required component
    }

    public void StartHitBox(int index) {
        hitboxes[index].setTrigger(true);
    }

    public void StopHitBox(int index) {
        hitboxes[index].setTrigger(false);
    }

    /**
     * Called by child hitboxes.
     */
    public void inform(HitBox child, HitBox other) {
        if (other.manager == this) {
            // Our own hitbox hit our hurtbox
            return;
        }

        bool otherWantsHit = other.manager.canHit(this, child, other);
        bool canHit = this.canHit(this, child, other);

        if (this.health != null && otherWantsHit && canHit) {
            this.health.hurt(other.damage);
        }
    }

    /**
     * Ask other if child
     * May want to include both hit/hurt
     */
    public bool canHit(HitBoxManager manager, HitBox child, HitBox other) {
        if (manager == this) {
            // We are asking ourselves if other and child can interact.
            Debug.Log("we are okay with hit!");
            return true;
        } else {
            Debug.Log("We want to hit other!");
            // other is asking if we want other and child to interact.
            return true;
        }
    }
}
