using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Properties")]
    [SerializeField] private int health = 10;

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log(gameObject.name + " took damage! Health: " + health);
        if (health <= 0) Die();
    }
    public void Die()
    {
        Destroy(gameObject);
    }

}
