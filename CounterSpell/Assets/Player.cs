using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // 이동 속도
    public float jumpForce = 5f; // 점프 힘
    public Transform groundCheck; // 땅에 닿았는지 체크할 위치
    public LayerMask groundLayer; // 땅 레이어
    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private float groundCheckRadius = 0.2f;
    private float moveInput;
    public GameObject dummyPrefab;  // 더미 프리팹
    public float confusionDuration = 5f;  // 정신착란 지속 시간
    private float confusionTimer = 10f;  // 10초마다 정신착란 시작
    private bool isConfused = false;
    private float confusionTimeRemaining = 0f;
    private GameObject spawnedDummy;  // 생성된 더미
    private float damageFromDummy = 0f;  // 더미가 주는 피해
    public float attackDamage = 10f;  // 플레이어 공격력
    private bool isDefending = false;  // 방어 상태

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 이동 입력 받기
        moveInput = Input.GetAxis("Horizontal");

        // 땅에 닿았는지 체크
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 애니메이션 설정
        HandleAnimations();

        // 점프 처리
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // 점프
        }

        if (!isConfused)
        {
            confusionTimer -= Time.deltaTime;
            if (confusionTimer <= 0)
            {
                StartConfusion();
            }
        }
        else
        {
            confusionTimeRemaining -= Time.deltaTime;
            if (confusionTimeRemaining <= 0)
            {
                EndConfusion();
            }

            if (spawnedDummy != null)
            {
                damageFromDummy += spawnedDummy.GetComponent<Dummy>().damageTaken;
            }
        }

        // 공격 처리
        if (Input.GetKeyDown(KeyCode.J) && spawnedDummy != null)  // J키로 공격
        {
            Attack();
        }

        // 방어 처리
        if (Input.GetKey(KeyCode.K))  // K키로 방어
        {
            StartDefending();
        }
        else
        {
            StopDefending();
        }
    }

    void FixedUpdate()
    {
        // 이동 처리
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    void HandleAnimations()
    {
        // 걷는 애니메이션
        animator.SetFloat("Speed", Mathf.Abs(moveInput));

        // 점프 애니메이션
        animator.SetBool("IsGrounded", isGrounded);
        if (!isGrounded)
        {
            animator.SetFloat("VerticalSpeed", rb.velocity.y);
        }

        // 공격 애니메이션
        animator.SetBool("IsAttacking", Input.GetKeyDown(KeyCode.J));

        // 방어 애니메이션
        animator.SetBool("IsDefending", isDefending);
    }

    void StartConfusion()
    {
        Debug.Log("정신 착란");
        isConfused = true;
        confusionTimeRemaining = confusionDuration;  // 착란 지속시간 설정
        spawnedDummy = Instantiate(dummyPrefab, transform.position + Vector3.right * 2, Quaternion.identity);  // 더미 생성 위치
    }

    void EndConfusion()
    {
        isConfused = false;
        if (spawnedDummy != null)
        {
            Destroy(spawnedDummy);  // 더미 제거
        }

        // 더미가 받은 피해만큼 플레이어에게 피해 추가
        ApplyDamageFromDummy();
        Debug.Log("플레이어가 더미로부터 받은 피해: " + damageFromDummy);
    }

    void ApplyDamageFromDummy()
    {
        // 더미가 받은 피해만큼 플레이어에게 피해 줌
        Debug.Log("플레이어 체력 감소: " + damageFromDummy);
    }

    // 플레이어의 공격 함수
    void Attack()
    {
        if (spawnedDummy != null)
        {
            spawnedDummy.GetComponent<Dummy>().TakeDamage(attackDamage);
            Debug.Log("더미에게 " + attackDamage + "의 피해를 주었습니다.");
        }
    }

    // 방어 상태 시작
    void StartDefending()
    {
        isDefending = true;
        Debug.Log("방어 시작");
    }

    // 방어 상태 종료
    void StopDefending()
    {
        isDefending = false;
        Debug.Log("방어 종료");
    }
}
