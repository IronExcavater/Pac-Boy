public class Ghost : Character
{
    public override void ChangeMode(GameManager.Mode mode)
    {
        switch (mode)
        {
            case GameManager.Mode.Chase or GameManager.Mode.Scatter:
                IsArmed = true;
                break;
            case GameManager.Mode.Scared:
                IsArmed = false;
                break;
        }
    }
}
