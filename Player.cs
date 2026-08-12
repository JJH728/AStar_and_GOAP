using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 이동. CharacterController를 써서 벽에 막히고, 경사·계단을
/// 오르고, 중력과 점프를 처리한다.
///
/// CharacterController는 물리 시뮬레이션에 참여하지 않으므로 중력이
/// 자동으로 적용되지 않는다. 그래서 verticalVelocity(수직 속도)를
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

    [Header("점프")]
    [Tooltip("점프 가능한 높이(m)")]
    [SerializeField] private float jumpHeight = 1.2f;

    private float horizontalAxis;
    private float verticalAxis;
    private Vector3 moveVector;
    private CharacterController controller;
    private Animator animator;
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
        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");
        isWalk = Input.GetButton("Walk"); // 왼쪽 shift 키
    }

    // 플레이어를 움직인다
    // 뛰는지 걷는지에 따라 속력을 달리하고,
    // 애니메이션을 뛰는지 걷는지에 따라 변경한다
    void Move()
    {
        moveVector = new Vector3(horizontalAxis, 0, verticalAxis).normalized;
        transform.position += moveVector * moveSpeed *
            (isWalk ? slowDownWhileWalk : 1f) * Time.deltaTime;

        animator.SetBool("isRun", moveVector != Vector3.zero);
        animator.SetBool("isWalk", isWalk);
    }

    // 움직이고 있는 방향을 바라본다
    void Turn()
    {
        // 움직이지 않았다면 Turn을 실행하지 않는다
        if (moveVector == Vector3.zero)
            return;

        transform.LookAt(transform.position + moveVector);
    }

    void Jump()
    {
        
    }
}
