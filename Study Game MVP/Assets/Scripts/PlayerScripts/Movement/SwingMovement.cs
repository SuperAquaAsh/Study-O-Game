using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingMovement : MovementState
{
    Vector2 vel;


    //This will come from movement.lastVelDir;
    Vector2 swingDir;
    float swingSpeed;

    float timer;
    float speedDecayTime;
    float totalTime;

    bool hasHit;

    public override void EnterMove(Player player, PlayerMovement movement){
        movement.SetCurrentStateID(2);

        

        if(movement.heldDir == Vector2.zero) swingDir = movement.lastVelDir;
        else swingDir = movement.heldDir.normalized;

        timer = PlayerMovement.swingStun;
        totalTime = PlayerMovement.swingStun;
        
        swingSpeed = PlayerMovement.swingSpeed;

        vel = swingDir * swingSpeed;

        speedDecayTime = 0.1f;

        hasHit = false;

        player.animator.SetFloat("M_Speed", 0f);
        player.animator.SetBool("isSwing", true);
        movement.CheckFlip();

        //We need this because we can't add a Vector2 and a Vector3
        Vector3 addSwingDir = new Vector3(swingDir.x, swingDir.y, 0);
        Vector3 playerCenter = player.transform.position + (Vector3.up * 0.75f);
        Quaternion rot = Quaternion.LookRotation(-addSwingDir, -Vector3.forward);
        Debug.Log((rot * Vector3.forward) + " with: " + addSwingDir);

        EffectsManager.instance.SpwanObjectsNetworked(0, playerCenter + (addSwingDir * 1.75f), rot);
        //EffectsManager.SpwanObjectFromPool(0, playerCenter + (addSwingDir * 1.75f), rot);
    }

    public override void FixedUpdateMove(Player player, PlayerMovement movement, float speed){
        timer -= Time.fixedDeltaTime;

        if(timer <= 0){
            movement.ChangeState(0, false);

            //We then test to hit things if we haven't yet (This shouldn't happen, but is a backfall just in case)
            if(!hasHit) {
                player.CheckThingsToHit();
                hasHit = true;
            }
        }

        //This code is stolen from the older HitMovement Script
        if(speedDecayTime > (totalTime - timer))
        {
            //This is if we are still decaying
            swingSpeed = Mathf.Lerp(PlayerMovement.swingSpeed, 0, Mathf.Clamp((totalTime - timer) / speedDecayTime, 0, 1));
            vel = swingDir * swingSpeed;
        }
        else{
            //We don't move
            vel = Vector2.zero;

            //We then test to hit things if we haven't yet
            if(!hasHit) {
                player.CheckThingsToHit();
                hasHit = true;
            }
        }

        //Lets do a raycast to check for stuff (Also stolen from HitMovement)
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int hitNum = player.playerCollider.Cast(vel.normalized, hits, vel.magnitude * Time.fixedDeltaTime);

        if(hitNum > 0){
            Debug.Log("We have hit a wall! (while swinging)");
            //If we hit something, that means that we were going to hit it anyway, so set the transform to that point
            player.transform.position = hits[0].centroid;

            //MAYBE add a freeze effect

            vel -= vel * Vector2.Dot(hits[0].normal, vel.normalized);

        }

        movement.rb.velocity = vel;
    }

    public override void LeaveMove(Player player, PlayerMovement movement){

        player.animator.SetBool("isSwing", false);
    }
}
