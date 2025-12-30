using System;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private int lightAttackDamage = 5;
    public static event Action<HitInfo> OnAnyHit;
    private void Start()
    {
        if (transform.parent != null)
        {
            transform.tag = transform.parent.tag;
        }
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag(transform.tag)) return;
        if(other.TryGetComponent<IDamageable>(out IDamageable hitRecipient) )
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
