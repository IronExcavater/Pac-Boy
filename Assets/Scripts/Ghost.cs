public class Ghost : Character
{
    public override void ChangeMode(GameManager.Mode mode)
    {
        switch (mode)
        {
            case GameManager.Mode.Normal:
                IsArmed = true;
                break;
            case GameManager.Mode.Scared:
                IsArmed = false;
                break;
            // Temporary modes
            case GameManager.Mode.Flip:
                IsFacingRight = !IsFacingRight;
                break;
            case GameManager.Mode.Move:
                ani.SetBool(GetAnimatorHash("Moving"), !ani.GetBool(GetAnimatorHash("Moving")));
                break;
            case GameManager.Mode.Attack:
                ani.SetTrigger(GetAnimatorHash("Attack"));
                break;
            case GameManager.Mode.Defeat:
                ani.SetTrigger(GetAnimatorHash("Defeat"));
                break;
        }
    }
}
