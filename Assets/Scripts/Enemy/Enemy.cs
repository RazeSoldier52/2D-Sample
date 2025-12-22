using UnityEngine;

public class Enemy : MonoBehaviour,IDamageable,IBoundaryBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Properties")]
    [SerializeField] private float health = 10;

    public void TakeDamage(HitInfo hitInfo)
    {
        health -= hitInfo.damage;
        Debug.Log(gameObject.name + " took"+hitInfo.damage+ " damage! Health:"  + health);
        if (health <= 0) Die();
    }
    public void Die()
    {
        Destroy(gameObject);
    }

    public void HandleBoundaryBehaviour()
    {
        Destroy(gameObject);
    }
}
