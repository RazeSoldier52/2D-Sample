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
public enum PrimaryAction
{
    None=0,
    Attacking=1<<0,
    Interacting=1<<1,
    EnvironmentallyBlocked=1<<2,
    MechanicallyBlocked=1<<3
}
[Serializable]
public struct PlayerStateProfile
{
    public VerticalState vertical;
    public MovementState movement;
    public PrimaryAction primaryAction;
    public bool primaryActionFree => primaryAction== PrimaryAction.None;
    public bool canAttack => vertical==VerticalState.Grounded && primaryAction==PrimaryAction.None;
    public bool canJump => vertical == VerticalState.Grounded;
    public bool canMove => !primaryAction.HasFlag(PrimaryAction.EnvironmentallyBlocked) &&
                           !primaryAction.HasFlag(PrimaryAction.MechanicallyBlocked) &&
                           !primaryAction.HasFlag(PrimaryAction.Attacking);

}

