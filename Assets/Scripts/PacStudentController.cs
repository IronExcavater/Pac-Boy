using UnityEngine;

public class PacStudentController : Character
{
    private KeyCode lastInput;
    private KeyCode currentInput;
    
    protected override void Start()
    {
        base.Start();
        IsArmed = false;
    }

    protected void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) lastInput = KeyCode.W;
        if (Input.GetKeyDown(KeyCode.A)) lastInput = KeyCode.A;
        if (Input.GetKeyDown(KeyCode.S)) lastInput = KeyCode.S;
        if (Input.GetKeyDown(KeyCode.D)) lastInput = KeyCode.D;

        if (AnimationManager.TargetExists(transform)) return;
        NextPos();
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
        CurrentPosition = NextPosition;
        var possibleLastPos = InputToPosition(lastInput); // Newest input
        var possibleCurrentPos = InputToPosition(currentInput); // Existing input
        
        var lastTile = GameManager.LevelTilemap().GetTile(Vector3Int.RoundToInt(possibleLastPos));
        var currentTile = GameManager.LevelTilemap().GetTile(Vector3Int.RoundToInt(possibleCurrentPos));
        
        if (possibleLastPos != CurrentPosition && lastTile.Equals(GameManager.GroundTile()))
        {
            currentInput = lastInput;
            NextPosition = possibleLastPos;
            AnimationManager.AddTween(transform, NextPosition, 1 / GameManager.CharacterSpeed(),
                AnimationManager.Easing.Linear);
            UpdateAnimator();
            DustParticle();
            return;
        }
        if (possibleCurrentPos != CurrentPosition && currentTile.Equals(GameManager.GroundTile()))
        {
            NextPosition = possibleCurrentPos;
            AnimationManager.AddTween(transform, NextPosition, 1 / GameManager.CharacterSpeed(),
                AnimationManager.Easing.Linear);
            UpdateAnimator();
            DustParticle();
            return;
        }

        currentInput = KeyCode.None;
        AudioManager.PlaySfxOneShot(AudioManager.Audio.hit);

        UpdateAnimator();
    }
    
    private Vector3 InputToPosition(KeyCode input) => input switch
    {
        KeyCode.W => CurrentPosition + Vector3.up,
        KeyCode.A => CurrentPosition + Vector3.left,
        KeyCode.S => CurrentPosition + Vector3.down,
        KeyCode.D => CurrentPosition + Vector3.right,
        _ => CurrentPosition
    };

    public void PlayStep()
    {
        AudioManager.PlaySfxOneShot(AudioManager.Audio.step);
    }
}
