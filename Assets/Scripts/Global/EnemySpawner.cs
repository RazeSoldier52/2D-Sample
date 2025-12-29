using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning Options")]
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] bool spawnOnlyOnce = true;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
    public void SpawnEnemy()
    {
        if (enemyPrefab != null) Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        Debug.Log("Enemy Spawned!");
        if (spawnOnlyOnce) gameObject.SetActive(false);
    }
}
