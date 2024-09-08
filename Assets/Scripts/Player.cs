public class Player : Character
{
    public override void ChangeMode(GameManager.Mode mode)
    {
        switch (mode)
        {
            case GameManager.Mode.Normal:
                IsArmed = false;
                break;
            case GameManager.Mode.Scared:
                IsArmed = true;
                break;
            // Temporary modes
            case GameManager.Mode.Direction:
                Facing = (Direction) ((int) Facing + 1);
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
    
    public void PlayStep()
    {
        AudioManager.PlaySfxOneShot(AudioManager.Audio.step);
    }
}
