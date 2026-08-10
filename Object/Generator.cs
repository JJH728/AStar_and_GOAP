using UnityEngine;

namespace Squad
{
    /// <summary>
    /// 발전기. 작동 중일 때 주기적으로 발전기 소리를 방출한다.
    ///
    /// 발전기 소리는 지속적이라, 켜져 있는 동안 emitInterval마다 계속
    /// SoundEmitter.Emit을 호출한다. (발소리 같은 일회성 소리와 달리
    /// "계속 나는" 소리이므로, 켜진 동안 반복해서 방출한다.)
    ///
    /// 게임 흐름상 플레이어가 이 발전기를 작동시키면(Activate) 소리가
    /// 나기 시작하고, 그 소리가 반경 안 적들을 끌어들인다. "발전기를 켜야
    /// 진행되지만, 켜면 노출된다"는 긴장을 만든다.
    /// </summary>
    public class Generator : MonoBehaviour
    {
        [Header("Detection")]
        [Tooltip("발전기 소리를 들을 수 있는 적의 레이어")]
        [SerializeField] private LayerMask enemyLayer;

        [Header("Emit timing")]
        [Tooltip("작동 중 소리 방출 간격(초)")]
        [SerializeField] private float emitInterval = 0.5f;

        [Header("State")]
        [Tooltip("시작하자마자 작동 상태로 둘지")]
        [SerializeField] private bool startsActive = false;

        // 작동 중인가. 외부(플레이어 상호작용)에서 켜고 끌 수 있다.
        public bool IsActive { get; private set; }

        private float _emitTimer;

        private void Start()
        {
            IsActive = startsActive;
        }

        private void Update()
        {
            if (!IsActive)
                return;

            _emitTimer -= Time.deltaTime;
            if (_emitTimer <= 0f)
            {
                SoundEmitter.Emit(transform.position, SoundList.Generator, enemyLayer);
                _emitTimer = emitInterval;
            }
        }

        /// <summary>발전기를 켠다(플레이어가 작동시킬 때 호출).</summary>
        public void Activate()
        {
            IsActive = true;
            _emitTimer = 0f;   // 켜자마자 첫 소리가 바로 나도록
        }

        /// <summary>발전기를 끈다.</summary>
        public void Deactivate()
        {
            IsActive = false;
        }
    }
}