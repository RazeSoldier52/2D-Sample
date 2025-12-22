using System;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private int lightAttackDamage = 5;
    [SerializeField] private int heavyAttackDamage = 10;
    public static event Action<HitInfo> OnAnyHit;

    public void OnTriggerEnter2D(Collider2D other)
    {
        if(other.TryGetComponent<IDamageable>(out IDamageable hitRecipient))
        {
            HitInfo info = new HitInfo
            {
                damage = lightAttackDamage,
                hitType = HitType.Physical,
                hitPoint = other.ClosestPoint(transform.position),
                hitDirection = (other.transform.position - transform.position).normalized
            };
            hitRecipient.TakeDamage(info);
            OnAnyHit?.Invoke(info);
        }
    }

}
