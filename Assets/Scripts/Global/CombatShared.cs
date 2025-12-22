using UnityEngine;
using System;
public enum HitType
{
    Physical,
    Fire
}
public struct HitInfo
{
    public HitType hitType;
    public float damage;
    public Vector2 hitPoint;
    public Vector2 hitDirection;
}
public interface IDamageable
{
    void TakeDamage(HitInfo hitInfo);
}
public interface IBoundaryBehaviour
{
    void HandleBoundaryBehaviour();
}
public enum VerticalState
{
    Grounded,
    Airborne
}
[Flags]
public enum MovementState
{
    NotMoving=0,
    Running=1<<0,
    Sprinting=1<<1,
    Dashing=1<<2
}

[Flags]
public enum Attack
{
    NotAttacking=0,
    Attacking=1<<0,
    LightAttack=1<<1,
    HeavyAttack=1<<2
}

[Serializable]
public struct PlayerStateProfile
{
    public VerticalState vertical;
    public bool canAttack;
    public Attack attack;
    public MovementState movementState;
    public bool canMove; 
}

