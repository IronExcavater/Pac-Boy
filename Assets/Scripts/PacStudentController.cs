using UnityEngine;

public class PacStudentController : Character
{
    private KeyCode lastInput;
    private KeyCode currentInput;

    protected void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) lastInput = KeyCode.W;
        if (Input.GetKeyDown(KeyCode.A)) lastInput = KeyCode.A;
        if (Input.GetKeyDown(KeyCode.S)) lastInput = KeyCode.S;
        if (Input.GetKeyDown(KeyCode.D)) lastInput = KeyCode.D;
        
        if (!AnimationManager.TargetExists(transform)) NextPos();;
        UpdateAnimator();
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
                StartCoroutine(BlinkTransition("Armed", 2));
                break;
        }
        UpdateAnimator();
    }

    protected override void NextPos()
    {
        if (!IsAlive) return;
        if (IsLocked) return;
        CurrentPosition = NextPosition;
        
        var currentTile = GameManager.LevelTilemap().GetTile(Vector3Int.RoundToInt(CurrentPosition));
        if (GameManager.IsFillerTile(currentTile))
        {
            var offset = CurrentPosition.x < 0 ? 1 : 0;
            var teleportPosition = new Vector3(-CurrentPosition.x - offset, CurrentPosition.y);
            transform.position = teleportPosition;
            CurrentPosition = teleportPosition;
        }
        
        var possibleLastPos = InputToPosition(lastInput); // Newest input
        var possibleCurrentPos = InputToPosition(currentInput); // Existing input
        
        var lastDirectionTile = GameManager.LevelTilemap().GetTile(Vector3Int.RoundToInt(possibleLastPos));
        var currentDirectionTile = GameManager.LevelTilemap().GetTile(Vector3Int.RoundToInt(possibleCurrentPos));
        
        if (possibleLastPos != CurrentPosition && GameManager.IsGroundTile(lastDirectionTile)
            || GameManager.IsFillerTile(lastDirectionTile))
        {
            currentInput = lastInput;
            NextPosition = possibleLastPos;
            AddTween();
            DustParticle();
            return;
        }
        if (possibleCurrentPos != CurrentPosition && GameManager.IsGroundTile(currentDirectionTile)
            || GameManager.IsFillerTile(currentDirectionTile))
        {
            NextPosition = possibleCurrentPos;
            AddTween();
            DustParticle();
            return;
        }

        currentInput = KeyCode.None;
        AudioManager.PlaySfxOneShot(AudioManager.Audio.hit);
    }
    
    protected override void AddTween()
    {
        CurrentPosition = transform.position;
        AnimationManager.AddTween(transform, NextPosition,
            Vector3.Distance(CurrentPosition, NextPosition) / GameManager.Game.characterSpeed,
            AnimationManager.Easing.Linear);
    }
    
    private Vector3 InputToPosition(KeyCode input) => input switch
    {
        KeyCode.W => CurrentPosition + Vector3.up,
        KeyCode.A => CurrentPosition + Vector3.left,
        KeyCode.S => CurrentPosition + Vector3.down,
        KeyCode.D => CurrentPosition + Vector3.right,
        _ => CurrentPosition
    };
    
    public override void Death()
    {
        base.Death();
        AudioManager.PlaySfxOneShot(AudioManager.Audio.defeat);
        GameManager.RestartLevel(4);
    }

    public void PlayStep()
    {
        AudioManager.PlaySfxOneShot(AudioManager.Audio.step);
    }
}
