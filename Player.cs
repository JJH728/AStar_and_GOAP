using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float slowDownWhileWalk;
    private float horizontalAxis;
    private float verticalAxis;
    private Vector3 moveVector;
    private Animator animator;
    private bool isWalk;
    private bool isJump;
    void Awake()
    {
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
