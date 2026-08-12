using UnityEngine;

/// <summary>
/// 플레이어 이동. CharacterController를 써서 벽에 막히고, 경사·계단을
/// 오르고, 중력과 점프를 처리한다.
///
/// CharacterController는 물리 시뮬레이션에 참여하지 않으므로 중력이
/// 자동으로 적용되지 않는다. 그래서 Y_velocity(수직 속도)를
/// 직접 관리한다:
///   - 매 프레임 중력만큼 아래로 가속
///   - 바닥에 닿으면 0 근처로 리셋(계속 아래로 쌓이지 않게)
///   - 점프하면 위쪽 속도를 한 번 넣어 줌
/// 실제 물리의 "가속도 → 속도 → 위치" 흐름을 손으로 구현하는 셈이다.
/// </summary>

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float moveSpeed;
    [Tooltip("달리는 속도에 대한 걷는 속도의 비율")]
    [SerializeField] private float slowDownWhileWalk;
    [Tooltip("중력 가속도(음수)")]
    [SerializeField] private float gravity = -20f;

    [Header("점프")]
    [Tooltip("점프 가능한 높이(m)")]
    [SerializeField] private float jumpHeight = 1.2f;

    private CharacterController controller;
    private Animator animator;

    // x와 z축은 키보드의 입력을 받아 결정되는 입력값
    // y축은 수학적으로 계산되는 속도의 값
    private float X_Axis;
    private float Y_velocity;
    private float Z_Axis;
    private Vector3 horizontal;
    private bool isWalk;
    private bool isJump;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        GetInput();
        Move();
        Turn();
    }

    // 이동에 필요한 키를 입력받는다
    void GetInput()
    {
        X_Axis = Input.GetAxisRaw("Horizontal");
        Z_Axis = Input.GetAxisRaw("Vertical");
        isWalk = Input.GetButton("Walk"); // 왼쪽 shift 키

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
            isJump = true;
    }

    /// 1. 입력받은 x와 z, Walk키에 따라 수평 속도를 결정한다
    /// 2. Jump를 입력받았다면 y 속도를 점프 초기 속도로 초기화한다
    /// 3. 최종 결정된 속도로 캐릭터를 한 프레임 이동시킨다
    /// 4. 이동 후 캐릭터가 공중에 떠있다면 y 속도를 감소시킨다
    /// 5. 애니메이션을 뛰는지 걷는지에 따라 변경한다
    void Move()
    {
        horizontal = new Vector3(X_Axis, 0, Z_Axis).normalized;
        horizontal *= moveSpeed * (isWalk ? slowDownWhileWalk : 1f);

        if (isJump)
        {
            // 점프 초기 속도는 v^2 = 2gh에 의해 결정
            Y_velocity = Mathf.Sqrt(-2f * jumpHeight * gravity);
            isJump = false;
        }

        Vector3 velocity = horizontal + Vector3.up * Y_velocity;
        controller.Move(velocity * Time.deltaTime);
        
        if (!controller.isGrounded)
            Y_velocity += gravity * Time.deltaTime;
        
        animator.SetBool("isRun", horizontal != Vector3.zero);
        animator.SetBool("isWalk", isWalk);
        animator.SetBool("isInTheAir", !controller.isGrounded);
    }

    // 움직이고 있는 방향을 바라본다
    void Turn()
    {
        // 움직이지 않았다면 Turn을 실행하지 않는다
        if (horizontal == Vector3.zero)
            return;

        transform.LookAt(transform.position + horizontal);
    }
}
