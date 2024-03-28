using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//This handles everything related to player movement
public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb {get; private set;}

    Player player;

    /// <summary>
    /// This is the UNORMALIZED direction that the player is holding
    /// </summary>
    public Vector2 heldDir {get; private set;}

    #region Hit Variables
    public bool isHit { get; private set; } = false;
    public const float hitPower = 30;
    public const float defaultHitTime = 3f;
    public LayerMask wallMask;
    public float hitTime { get; private set; }
    #endregion

    #region Swing Variables
    public const float swingSpeed = 20;
    //This is how long the player can't move
    public const float swingStun = 1;

    public Vector2 lastVelDir {get; private set;} = Vector2.right;
    #endregion

    public Vector2 lastHitDir { get; private set; }

    #region Movement States
    MovementState currentMovementState;
    int currentMovementStateID;

    NormalMovement normalMovement = new NormalMovement();
    HitMovement hitMovement = new HitMovement();
    SwingMovement swingMovement = new SwingMovement();
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        currentMovementState = normalMovement;
        currentMovementState.EnterMove(player, this);
    }

    // This ensures it is only called by the player if we are the owner
    public void OwnerFixedUpdate()
    {
        if(rb.velocity.sqrMagnitude > 0.2) lastVelDir = rb.velocity.normalized;
    }

    // This ensures it is only called by the player if we are the owner
    public void OwnerUpdate()
    {
        heldDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    #region Flip Sprites
    public void CheckFlip(){
        if(heldDir.x != 0){
            player.sprites.FlipSprites(heldDir.x == -1);
        }
    }
    public void SetFlip(bool flip){
        player.sprites.FlipSprites(flip);
    }
    #endregion
    
    public void FixedMoveWithSpeed(float speed){
        //return if hiding, don't want to move when in a spot!
        if(player.localHiding) return;

        currentMovementState.FixedUpdateMove(player, this, speed);
        return;
    }

    
    //These are functions that other objects call to get the player to do things
    #region Do Stuff Functions
    public void ChangeState(int id, bool overrideCurrentState){
        //Only do this if we are transitioning to a new state AND we don't want to
        //(if we want to be hit in another direction while still being hit, we want to change back into the state we are in)
        if(currentMovementStateID == id && !overrideCurrentState) return; 

        //Tell the current State that we are leaving
        currentMovementState.LeaveMove(player, this);

        if(id == 0) {
            currentMovementState = normalMovement;
        }
        else if (id == 1) currentMovementState = hitMovement;
        else if (id == 2) currentMovementState = swingMovement;

        currentMovementState.EnterMove(player, this);
    }

    public void ResetSpeed(){
        //This just sets the speed to zero
        rb.velocity = Vector2.zero;
        player.animator.SetFloat("M_Speed", 0f);
    }


    #endregion

    #region SetData Functions

    public void SetPlayer(Player obj){
        player = obj;
    }
    public void SetRigidbody(Rigidbody2D rigid){
        rb = rigid;
    }
    public void SetHitDir(Vector2 hitDir, float time = defaultHitTime){
        lastHitDir = hitDir.normalized;
        hitTime = time;
        //MAYBE change state to Hit
    }  

    //This is called by the states to confirm that we have entered them
    public void SetCurrentStateID(int id){
        currentMovementStateID = id;
    }

    public int GetCurrentStateID(){return currentMovementStateID;}

    #endregion
}
