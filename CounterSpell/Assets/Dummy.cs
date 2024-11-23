using UnityEngine;

public class Dummy : MonoBehaviour
{
    public float health = 50f;  // 더미 체력
    public float damage = 10f;  // 더미의 공격력
    public float damageTaken = 0f;  // 더미가 받은 피해

    private Transform player;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;  // 플레이어 찾기
    }

    void Update()
    {
        if (player != null)
        {
            // 더미가 플레이어를 공격하도록 이동
            MoveTowardsPlayer();
            AttackPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        float step = 2f * Time.deltaTime;  // 더미가 이동할 속도
        transform.position = Vector3.MoveTowards(transform.position, player.position, step);
    }

    void AttackPlayer()
    {
        // 더미가 플레이어에 근접하면 공격
        if (Vector3.Distance(transform.position, player.position) < 1.5f)
        {
            // 플레이어에게 공격
            // 예: 플레이어 체력 감소 (플레이어의 체력 관리 시스템 필요)
            Debug.Log("더미가 플레이어를 공격");
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        damageTaken += amount;  // 받은 피해 기록
        if (health <= 0)
        {
            Destroy(gameObject);  // 더미 처치
        }
    }
}
