using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PacStudentController : Character
{
    private KeyCode lastInput;
    private KeyCode currentInput;
    private KeyCode positionInput;

    private KeyCode[] movementKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
    
    public bool CanPhase { get; set; }

    protected void Update()
    {
        foreach (var key in movementKeys)
        {
            if (!Input.GetKeyDown(key)) continue;
            lastInput = key;
            positionInput = key;
            break;
        }

        if (Input.GetKeyDown(KeyCode.Space)) StartCoroutine(PhaseAbility());
        if (NextPosition != CurrentPosition && !AnimationManager.TargetExists(transform))
            positionInput = KeyCode.None;
        
        if (!AnimationManager.TargetExists(transform)) NextPos();
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
        if (!IsAlive || IsLocked) return;
        CurrentPosition = NextPosition;
        
        var currentTile = GameManager.LevelTilemap().GetTile(Vector3Int.RoundToInt(CurrentPosition));
        if (GameManager.IsFillerTile(currentTile))
        {
            var offset = CurrentPosition.x < 0 ? 1 : 0;
            var teleportPosition = new Vector3(-CurrentPosition.x - offset, CurrentPosition.y);
            transform.position = CurrentPosition = teleportPosition;
        }
        
        var possibleLastPos = CurrentPosition + InputToPosition(lastInput); // Newest input
        var possibleCurrentPos = CurrentPosition + InputToPosition(currentInput); // Existing input
        
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

        if (currentInput == KeyCode.None) return;
        currentInput = KeyCode.None;
        AudioManager.PlaySfxOneShot(AudioManager.Audio.hit);
        WallParticle();
    }

    private IEnumerator PhaseAbility()
    {
        if (SceneManager.GetActiveScene().name != "InnovationScene") yield break;
        if (!IsAlive || IsLocked || positionInput == KeyCode.None || !CanPhase) yield break;
        var direction = InputToPosition(positionInput);
        var map = GameManager.LevelTilemap();
        for (var i = 2; i < 5; i++)
        {
            var wallPosition = CurrentPosition + direction * i;
            if (!GameManager.IsWallTile(map.GetTile(Vector3Int.RoundToInt(wallPosition)))) continue;
            for (var j = i; j < 5; j++)
            {
                var phasePosition = CurrentPosition + direction * j;
                if (!GameManager.IsGroundTile(map.GetTile(Vector3Int.RoundToInt(phasePosition)))) continue;
                AnimationManager.RemoveTween(transform);
                BashParticle();
                IsLocked = true;
                AudioManager.PlaySfxOneShot(AudioManager.Audio.potion);
                yield return new WaitForSeconds(0.2f);
                transform.position = CurrentPosition = NextPosition = phasePosition;
                IsLocked = false;
                CanPhase = false;
                break;
            }
            break;
        }
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
        KeyCode.W => Vector3.up,
        KeyCode.A => Vector3.left,
        KeyCode.S => Vector3.down,
        KeyCode.D => Vector3.right,
        _ => Vector3.zero
    };
    
    protected void WallParticle() { EmitParticle(dustMaterial, "Particles", 2, new Vector3(0, 0, DustDirection(Facing) - 180), 3);}

    public override void Reset()
    {
        base.Reset();
        lastInput = currentInput = KeyCode.None;
    }
    
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
