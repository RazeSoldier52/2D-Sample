using UnityEngine;
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
[System.Serializable]
public struct AttackProfile
{
    public string name;
    public float damage;
    public Vector2 offset;
    public Vector2 size;
    public HitType type;
}

