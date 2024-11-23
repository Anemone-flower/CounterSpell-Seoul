using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public Canvas confusionCanvas; // 혼란 상태를 표시할 캔버스
    public float fadeSpeed = 2f;   // 페이드 인/아웃 속도
    private Image canvasPanel;     // 캔버스 내부의 패널

    public float moveSpeed = 5f; // 이동 속도
    public float jumpForce = 5f; // 점프 힘
    public int jumpCount = 0; // 점프 횟수 (초기값 0)
    public Transform groundCheck; // 땅에 닿았는지 체크할 위치
    public LayerMask groundLayer; // 땅 레이어

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded = false; // 땅에 닿았는지 체크하는 변수
    private float groundCheckRadius = 0.3f;
    private float moveInput;

    public GameObject dummyPrefab;  // 더미 프리팹
    public float confusionDuration = 10f;  // 정신착란 지속 시간
    private float confusionTimer = 15f;  // 10초마다 정신착란 시작
    private bool isConfused = false;
    private float confusionTimeRemaining = 0f;
    private GameObject spawnedDummy;  // 생성된 더미
    private float damageFromDummy = 0f;  // 더미가 주는 피해
    public float attackDamage = 25f;  // 플레이어 공격력
    private bool isDefending = false;  // 방어 상태
    private bool canAttack = true;  // 공격 가능 여부
    public float attackRangeX = 2f; // 공격 범위의 X 크기
    public float attackRangeY = 2f; // 공격 범위의 Y 크기
    public float attackOffsetX = 1f; // 공격 범위의 시작 X 위치
    public float attackOffsetY = -0.5f; // 공격 범위의 시작 Y 위치
    public LayerMask enemyLayer;  // 적 레이어
    public float guardReduction = 0.6f; // 가드 시 받는 피해 감소율 (60%)
    public float guardDuration = 5f;   // 최대 가드 지속 시간
    public float guardCooldown = 3f;   // 가드 후 쿨타임

    private float currentGuardTime = 0f; // 현재 가드 사용 시간
    private bool guardOnCooldown = false; // 가드 쿨타임 상태

    private float health = 100f;  // 플레이어 체력
    private float attackCooldown = 1f; // 공격 쿨타임

    private List<Collider2D> alreadyAttackedDummies = new List<Collider2D>();  // 이미 공격한 더미 추적

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        canvasPanel = confusionCanvas.GetComponentInChildren<Image>();

        // 혼란 캔버스 초기 상태 설정
        confusionCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 2, groundLayer);
        moveInput = Input.GetAxis("Horizontal");

         if (isDefending)
            {
                moveInput = 0;  // 방어 중이면 이동하지 않도록 설정
            }   
        HandleAnimations();
        HandleJump();

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
        if (Input.GetKeyDown(KeyCode.J) && spawnedDummy != null && canAttack)
        {
            Invoke("Attack", 1);
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

        // 플레이어 스프라이트 방향 전환
        if (moveInput > 0) // 오른쪽 이동
        {
            Flip(false);
        }
        else if (moveInput < 0) // 왼쪽 이동
        {
            Flip(true);
        }
    }

    void FixedUpdate()
    {
        // 이동 처리
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    void HandleJump()
    {
        if (isGrounded)
        {
            jumpCount = 0; // 땅에 닿으면 점프 횟수 초기화
        }

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < 2) // 점프 키 입력과 점프 제한 확인
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); // 위쪽으로 힘을 줌
            jumpCount++; // 점프 횟수 증가
            animator.SetTrigger("Jump"); // 점프 애니메이션
        }
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

    private IEnumerator GuardDurationRoutine()
    {
        currentGuardTime = guardDuration;

        while (currentGuardTime > 0)
        {
            currentGuardTime -= Time.deltaTime;
            yield return null;
        }

        Debug.Log("가드 지속 시간이 만료됨");
        StopDefending();

        // 가드 쿨타임 시작
        guardOnCooldown = true;
        yield return new WaitForSeconds(guardCooldown);
        guardOnCooldown = false;
        Debug.Log("가드 쿨타임 종료");
    }

    private void StartConfusion()
    {
        Debug.Log("정신 착란 상태 시작");
        isConfused = true;
        confusionTimeRemaining = confusionDuration;

        // 캔버스를 페이드 인으로 표시
        StartCoroutine(FadeCanvasRoutine(true));

        // 혼란 상태 관련 로직 (더미 생성 등)
        spawnedDummy = Instantiate(dummyPrefab, transform.position + Vector3.right * 2, Quaternion.identity);
    }

    private void EndConfusion()
    {
        Debug.Log("혼란 상태 종료");
        isConfused = false;

        // 현재 존재하는 모든 더미를 찾는다
        var remainingDummies = FindObjectsOfType<Dummy>();

        foreach (var dummy in remainingDummies)
        {
            if (!dummy.IsDestroyedInConfusion())
            {
                float damageToPlayer = dummy.GetDamageTaken() * 0.5f;
                TakeDamage(damageToPlayer);

                Debug.Log($"혼란 종료: 더미로부터 {damageToPlayer}의 피해를 받음");
            }
        }

        // 혼란 상태 종료 후 페이드 아웃
        StartCoroutine(FadeCanvasRoutine(false));

        if (spawnedDummy != null)
        {
            Destroy(spawnedDummy); // 더미 삭제
        }
    }

    private IEnumerator FadeCanvasRoutine(bool fadeIn)
    {
        float targetAlpha = fadeIn ? 0.5f : 0f; // 페이드 인/아웃 목표 알파 값
        float currentAlpha = canvasPanel.color.a;

        confusionCanvas.gameObject.SetActive(true);
        Color panelColor = canvasPanel.color;

        while (Mathf.Abs(currentAlpha - targetAlpha) > 0.01f)
        {
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed); // 알파 값 부드럽게 변화
            panelColor.a = currentAlpha;
            canvasPanel.color = panelColor;
            yield return new WaitForSeconds(0.01f); // 부드럽게 프레임 간 대기
        }

        panelColor.a = targetAlpha;
        canvasPanel.color = panelColor;

        if (!fadeIn)
        {
            confusionCanvas.gameObject.SetActive(false); // 페이드 아웃 후 캔버스를 비활성화
        }
    }

    private void Attack()
    {
        if (!canAttack) return;  // 공격 불가능하면 리턴

        // 공격 쿨타임 처리
        StartCoroutine(AttackCooldown());

        // 공격 범위 설정
        Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 attackPosition = (Vector2)transform.position + new Vector2(attackOffsetX * attackDirection.x, attackOffsetY);
        Vector2 attackSize = new Vector2(attackRangeX, attackRangeY);

        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPosition, attackSize, 0f, enemyLayer);

        foreach (var enemy in hitEnemies)
        {
            // 적 피해 처리
            Debug.Log($"적 {enemy.name} 공격 중");
        }
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void TakeDamage(float damage)
    {
        if (isDefending)
        {
            damage *= (1 - guardReduction); // 방어 중인 경우 감소된 피해 적용
        }

        health -= damage;

        if (health <= 0)
        {
            Debug.Log("플레이어 사망!");
            // 사망 처리 로직
        }
    }

    private void StartDefending()
    {
        if (guardOnCooldown) return;

        if (!isDefending)
        {
            isDefending = true;
            Debug.Log("방어 시작");

            StartCoroutine(GuardDurationRoutine());
        }
    }

    private void StopDefending()
    {
        if (isDefending)
        {
            isDefending = false;
            Debug.Log("방어 종료");
        }
    }

    private void Flip(bool faceLeft)
    {
        transform.localScale = new Vector3(faceLeft ? -1 : 1, 1, 1);
    }
}
