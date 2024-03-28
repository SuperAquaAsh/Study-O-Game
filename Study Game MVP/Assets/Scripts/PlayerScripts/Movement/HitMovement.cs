using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitMovement : MovementState
{
    Vector2 vel;
    Vector2 hitDir;

    float hitTime;
    float totalHitTime;
    //This is the time it takes to lose all your speed
    float hitDecayTime = 0.5f;
    
    public override void EnterMove(Player player, PlayerMovement movement)
    {
        movement.SetCurrentStateID(1);
        Debug.Log("Just entered Hit state with power of: " + PlayerMovement.hitPower);
        vel = Vector2.zero;
        hitDir = movement.lastHitDir;


        hitTime = movement.hitTime;
        totalHitTime = hitTime;
        hitDecayTime = 0.5f;
        player.animator.SetFloat("M_Speed", 0f);
        player.animator.SetBool("isHit", true);

        player.playerEffects.StartEffectNetworked(0);
    }

    public override void LeaveMove(Player player, PlayerMovement movement)
    {
        player.animator.SetBool("isHit", false);

        player.playerEffects.StopEffectNetworked(0);
    }

    public override void FixedUpdateMove(Player player, PlayerMovement movement, float speed)
    {
        
        //hitTime is basically how much time is left before we can move again
        hitTime -= Time.fixedDeltaTime;

        //if we've run out of time, it's time to stop being hit
        //ALSO, we must not be displaying a wrong answer
        if(hitTime < 0 && !QuestionDisplay.instance.showingWrong){
            movement.rb.velocity = Vector2.zero;
            movement.ChangeState(0, false);
            return;
        }
        //If enough time has passed (hitDecayTime) to lose all our speed, we don't move, otherwise we calcutate (lerp) the speed to how long it would be until we stop
        //if the total time that has passed is more than the time we should be being hit for, then we stay hit
        if(hitDecayTime > (totalHitTime - hitTime))
        {
            //This is if we are still decaying
            vel = hitDir * Mathf.Lerp(PlayerMovement.hitPower, 0, Mathf.Clamp((totalHitTime - hitTime) / hitDecayTime, 0, 1));
        }
        else{
            //We don't move
            vel = Vector2.zero;
        }

        

        
        //Lets send out a ray to check for stuff
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int hitNum = player.playerCollider.Cast(vel.normalized, hits, vel.magnitude * Time.fixedDeltaTime);

        if(hitNum > 0){
            Debug.Log("We have hit a wall! (while being hit)");
            //If we hit something, that means that we were going to hit it anyway, so set the transform to that point
            player.transform.position = hits[0].centroid;

            //MAYBE add a freeze effect

            hitDir = Vector2.Reflect(hitDir, hits[0].normal).normalized;

        }

        movement.rb.velocity = vel;
        


        movement.SetFlip(hitDir.x > 0);
    }
}
