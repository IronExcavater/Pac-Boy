using UnityEngine;

public class Player : Character
{
    protected override void Start()
    {
        base.Start();
        IsArmed = false;
    }
    
    public override void TriggerMode()
    {
        switch (GameManager.GameMode)
        {
            case GameManager.Mode.Chase or GameManager.Mode.Scatter:
                IsArmed = false;
                break;
            case GameManager.Mode.Scared:
                IsArmed = true;
                break;
        }
    }

    protected override void NextPos()
    {
        throw new System.NotImplementedException();
    }

    public void PlayStep()
    {
        AudioManager.PlaySfxOneShot(AudioManager.Audio.step);
    }
}
