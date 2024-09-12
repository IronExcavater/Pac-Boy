public class Player : Character
{
    protected override void Start()
    {
        IsArmed = false;
        base.Start();
    }
    
    public override void ChangeMode(GameManager.Mode mode)
    {
        switch (mode)
        {
            case GameManager.Mode.Chase or GameManager.Mode.Scatter:
                IsArmed = false;
                break;
            case GameManager.Mode.Scared:
                IsArmed = true;
                break;
        }
    }
    
    public void PlayStep()
    {
        AudioManager.PlaySfxOneShot(AudioManager.Audio.step);
    }
}
