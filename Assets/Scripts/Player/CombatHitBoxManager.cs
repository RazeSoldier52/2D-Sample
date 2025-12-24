using UnityEngine;
using System.Collections.Generic;
using System;

// 1. The Data Structure
// This defines the properties for a single attack type.
[Serializable]
public struct AttackDefinition
{
    public string attackName; // e.g., "Light1", "HeavyAir"
    [Header("Hitbox Shape")]
    public Vector2 boxSize;
    public Vector2 localOffset;

    [Header("Combat Data")]
    public float damage;
    public HitType damageType;
    public float knockbackForce; // Added based on previous discussions
}

public class CombatHitboxManager : MonoBehaviour
{
    [Header("References")]
    // Assign a child object with a PolygonCollider2D here.
    // We use Polygon because it's easy to reshape into a box dynamically.
    [SerializeField] private PolygonCollider2D activeCollider;

    [Header("Attack Library")]
    // This is where you define all your attacks in the Inspector.
    [SerializeField] private List<AttackDefinition> definedAttacks = new List<AttackDefinition>();

    // Internal state
    private AttackDefinition currentAttackData;
    private Dictionary<string, AttackDefinition> attackLookup;

    // Global event for "Juice" (camera shake, etc.), just like your old script
    public static event Action<HitInfo> OnAnyHit;

    private void Awake()
    {
        // Initialize the lookup dictionary for fast access
        attackLookup = new Dictionary<string, AttackDefinition>();
        foreach (var attack in definedAttacks)
        {
            if (!string.IsNullOrEmpty(attack.attackName))
            {
                attackLookup.TryAdd(attack.attackName, attack);
            }
        }

        // Ensure collider is off by default
        if (activeCollider != null) activeCollider.enabled = false;
    }

    // --- Public API called by PlayerController ---

    public void EnableHitbox(string attackName, int facingDirection)
    {
        if (!attackLookup.TryGetValue(attackName, out AttackDefinition data))
        {
            Debug.LogWarning($"Attack '{attackName}' not found in library!");
            return;
        }

        currentAttackData = data;
        ReshapeCollider(data, facingDirection);
        activeCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        activeCollider.enabled = false;
    }

    // --- Collision Logic ---
    // This replaces the OnTriggerEnter2D from your old Hitbox.cs
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IDamageable>(out IDamageable hitRecipient))
        {
            // Calculate direction for knockback relative to the player
            Vector2 direction = (other.transform.position - transform.parent.position).normalized;

            HitInfo info = new HitInfo
            {
                damage = currentAttackData.damage,
                hitType = currentAttackData.damageType,
                hitPoint = other.ClosestPoint(transform.position),
                hitDirection = direction
                // Note: You could add knockbackForce to HitInfo later if needed
            };

            hitRecipient.TakeDamage(info);
            OnAnyHit?.Invoke(info);
        }
    }

    // --- Helper Methods ---

    private void ReshapeCollider(AttackDefinition data, int facingDirection)
    {
        // Calculate the 4 corners of the box based on size and offset
        Vector2 center = data.localOffset;
        // Flip offset X based on facing direction (assumes 1 is right, -1 is left)
        center.x *= facingDirection;

        Vector2 extents = data.boxSize / 2f;

        Vector2[] points = new Vector2[4];
        points[0] = center + new Vector2(-extents.x, -extents.y); // Bottom Left
        points[1] = center + new Vector2(extents.x, -extents.y);  // Bottom Right
        points[2] = center + new Vector2(extents.x, extents.y);   // Top Right
        points[3] = center + new Vector2(-extents.x, extents.y);  // Top Left

        activeCollider.SetPath(0, points);
    }

    // --- EDITOR VISUALIZATION ---
    // This enables editing visualization in the inspector.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // Draw a wireframe for every defined attack so you can see them all at once
        foreach (var attack in definedAttacks)
        {
            // Account for the parent's position so gizmos move with the player
            Vector3 globalCenter = transform.TransformPoint(attack.localOffset);
            Gizmos.DrawWireCube(globalCenter, attack.boxSize);
        }
    }
}