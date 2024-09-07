using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [Header("Components:")]
    [SerializeField] private Animator ani;

    private bool _isFacingRight = true; 

    public abstract void ChangeMode(GameManager.Mode mode);
}
