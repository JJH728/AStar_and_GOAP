using UnityEngine;

namespace Squad
{
    /// <summary>
    /// 플레이어가 움직이는 동안 주기적으로 발소리를 방출한다.
    /// 이동 중일 때만, 그리고 일정 간격마다 SoundEmitter.Emit을 호출한다.
    ///
    /// 발소리는 "매 프레임"이 아니라 "발을 디딜 때마다"에 가깝게 내야
    /// 자연스럽다. 그래서 stepInterval마다 한 번씩 낸다(걷기/뛰기 속도에
    /// 따라 간격을 다르게 할 수도 있다).
    ///
    /// 이 스크립트는 "소리를 내는" 책임만 진다. 실제 이동(입력→위치 변경)은
    /// 별도의 플레이어 컨트롤러가 담당하고, 여기서는 "지금 움직이는가"와
    /// "얼마나 빨리 움직이는가"만 참고한다.
    /// </summary>
    public class PlayerStep : MonoBehaviour
    {
        [Header("Detection")]
        [Tooltip("발소리를 들을 수 있는 적의 레이어")]
        [SerializeField] private LayerMask enemyLayer;

        [Header("Footstep timing")]
        [Tooltip("걷는 중 발소리 간격(초)")]
        [SerializeField] private float walkInterval = 0.5f;
        [Tooltip("뛰는 중 발소리 간격(초). 더 짧게 = 더 자주")]
        [SerializeField] private float runInterval = 0.3f;

        [Header("Movement source")]
        [Tooltip("이 속도 이상이면 '움직이는 중'으로 본다")]
        [SerializeField] private float walkThreshold = 0.1f;
        [Tooltip("이 속도 이상이면 '뛰는 중'으로 본다")]
        [SerializeField] private float runThreshold = 4f;

        private Vector3 _lastPosition;
        private float _stepTimer;

        private void Start()
        {
            _lastPosition = transform.position;
        }

        private void Update()
        {
            // 이번 프레임의 이동 속도를 위치 변화로 추정한다.
            // (전용 컨트롤러가 있으면 그쪽의 속도 값을 직접 받아도 된다.)
            Vector3 delta = transform.position - _lastPosition;
            delta.y = 0f;                          // 수평 이동만
            float speed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
            _lastPosition = transform.position;

            // 멈춰 있으면 발소리 없음. 타이머도 리셋해서 다음 걸음이
            // 바로(간격을 새로 채워) 나도록 한다.
            if (speed < walkThreshold)
            {
                _stepTimer = 0f;
                return;
            }

            // 속도에 따라 걷기/뛰기 소리와 간격을 고른다.
            bool running = speed >= runThreshold;
            Sound sound = running ? SoundList.Running : SoundList.Walking;
            float interval = running ? runInterval : walkInterval;

            _stepTimer -= Time.deltaTime;
            if (_stepTimer <= 0f)
            {
                SoundEmitter.Emit(transform.position, sound, enemyLayer);
                _stepTimer = interval;
            }
        }
    }
}