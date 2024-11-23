using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; 

public class Dummy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float invincibilityDuration = 0.5f;
    [SerializeField] private Color hitColor = new Color(1f, 0.5f, 0.5f, 1f);
    [SerializeField] private float colorChangeDuration = 0.2f;

    private float currentHealth;
    public float damageTaken { get; private set; }

    private bool isInvincible;
    private bool isAttacking;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private bool destroyedInConfusion; // 혼란 상태에서 처치 여부

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        InitializeStats();
    }

    private void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"[{gameObject.name}] SpriteRenderer component is missing!");
            enabled = false;
            return;
        }
        originalColor = spriteRenderer.color;
    }

    private void InitializeStats()
    {
        currentHealth = maxHealth;
        isInvincible = false;
        player = GameObject.FindWithTag("Player")?.transform;

        if (player == null)
        {
            Debug.LogError($"[{gameObject.name}] Player not found in scene!");
            enabled = false;
        }

        destroyedInConfusion = false; // 초기화
    }

    private void Update()
    {
        if (player == null || isAttacking) return;

        MoveTowardsPlayer();
        CheckForAttack();
    }

    private void MoveTowardsPlayer()
    {
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, player.position, step);
    }

    private void CheckForAttack()
    {
        if (Vector3.Distance(transform.position, player.position) < attackRange)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(damage);
            Debug.Log($"[{gameObject.name}] Dealt {damage} damage to Player");
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    public void TakeDamage(float incomingDamage)
    {
        if (isInvincible)
        {
            Debug.Log($"[{gameObject.name}] Damage blocked by invincibility");
            return;
        }

        currentHealth -= incomingDamage;
        damageTaken += incomingDamage;

        Debug.Log($"[{gameObject.name}] Took {incomingDamage} damage. Current health: {currentHealth}");

        StartCoroutine(HitRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitRoutine()
    {
        isInvincible = true;
        spriteRenderer.color = hitColor;

        yield return new WaitForSeconds(colorChangeDuration);
        spriteRenderer.color = originalColor;

        yield return new WaitForSeconds(invincibilityDuration - colorChangeDuration);
        isInvincible = false;

        Debug.Log($"[{gameObject.name}] Invincibility ended");
    }

    public void MarkAsDestroyedInConfusion()
    {
        destroyedInConfusion = true;
    }

    private void Die()
    {
        Debug.Log($"[{gameObject.name}] has been destroyed");
        Destroy(gameObject);
    }

    public bool IsDestroyedInConfusion() => destroyedInConfusion;

    public float GetDamageTaken() => damageTaken;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}
