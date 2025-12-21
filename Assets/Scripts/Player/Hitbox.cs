using UnityEngine;

public class Hitbox : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private int lightAtttackDamage = 5;
    [SerializeField] private int heavyAttackDamage = 10;
    void Start()
    {
        
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        if(other.TryGetComponent<Enemy>(out Enemy enemy))
        {
            enemy.TakeDamage(lightAtttackDamage);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
