using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalMovement : MovementState
{
    #region Movement Variables

    Vector2 vel;
    float accel = 45f;
    //This is essenally the decelleration that doesn't change the players speed
    float handling = 40f;
    float stillDeccel = 30f;
    //This is the decceleration added on top when the player holds the opposite direction
    float skidDeccel = -30f;

    #endregion

    public override void EnterMove(Player player, PlayerMovement movement)
    {
        movement.SetCurrentStateID(0);

        ResetMovementVariables();
    }

    public override void LeaveMove(Player player, PlayerMovement movement)
    {
        
    }
    public override void FixedUpdateMove(Player player, PlayerMovement movement, float maxSpeed)
    {
        //Handle player movement. NOT GOOD! NOT NORMALIZED! Will need to be a few more lines to make normalized movement work. You'll need to store the X and Y velocity
        //and mutiply it by a normalizeed vector
        //movement.rb.velocity = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f).normalized * speed;

        #region NewMovement

        vel = movement.rb.velocity;

        //We want to record the current direction being held
        Vector2 heldDir = movement.heldDir.normalized;
        if(!Application.isFocused) heldDir = Vector2.zero;
        
        //We want to determine the dot of that compaired to our current direction
        float dirDot = 0f;
        if(heldDir != Vector2.zero) dirDot = Vector3.Dot(heldDir, vel);

        //We want to calculate our change in speed and direction to that mutiplied by the dot (opsite directions will be slower)
        Vector2 velAcceleration;
        if(heldDir != Vector2.zero) velAcceleration = heldDir * ((accel + handling) * Time.fixedDeltaTime);
        else velAcceleration = Vector2.zero;

        //We want to calculate our deceleration
        Vector2 velDecceleration;

        if(heldDir != Vector2.zero){
            //The player is holding down (The deceleration helps with turning)
            if(dirDot < -0.9f){
                //This means that the player is basically holding the other direction
                velDecceleration = -vel.normalized * ((skidDeccel - handling) * Time.fixedDeltaTime);
            }else{
                //This is just normal decleration. The more decelleration the easier it is to turn (we complesate for the speed loss by adding it to player speed).
                velDecceleration = -vel.normalized * (handling * Time.fixedDeltaTime);
            }
        }else{
            //The player is not holding down
            if(vel.sqrMagnitude > 0.1f) velDecceleration = -vel.normalized * (stillDeccel * Time.fixedDeltaTime);
            else velDecceleration = -vel;
        }

        Vector2 velChange = velAcceleration + velDecceleration;

        //We change our vel by that amount
        vel += velChange;

        //We cap our speed so we don't go too fast
        if(vel.sqrMagnitude > maxSpeed * maxSpeed){
            vel = vel.normalized * maxSpeed;
        }
        
        
        #endregion

        movement.rb.velocity = vel;

        //Set the players animation just after the movement because it depends on it
        player.animator.SetFloat("M_Speed", movement.rb.velocity.sqrMagnitude);

        movement.CheckFlip();
    }

    void ResetMovementVariables(){
        vel = Vector2.zero;
    }
}
