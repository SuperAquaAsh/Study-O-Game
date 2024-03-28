public abstract class MovementState
{
    public abstract void EnterMove(Player player, PlayerMovement movement);
    public abstract void FixedUpdateMove(Player player, PlayerMovement movement, float speed);
    public abstract void LeaveMove(Player player, PlayerMovement movement);
}
