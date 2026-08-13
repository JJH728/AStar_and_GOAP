using UnityEngine;
using System.Collections.Generic;

namespace Squad
{
    public class VisionCensor : MonoBehaviour
    {
        /// 시야 감지에 필요한 조건
        /// 1. 플레이어가 감지 거리 안에 있는가?
        /// 2. 플레이어가 적의 시야각 안에 있는가?
        /// 3. 적과 플레이어 사이에 벽이나 장애물이 없는가?
        /// 4. 플레이어가 죽은 상태는 아닌가?
        /// 4가지 조건을 모두 만족해야 적이 플레이어를 "봤다"고 판단
        /// 
        [Header("Vision")]
        // 적이 볼 수 있는 최대 거리
        [SerializeField] private float viewDistance = 18f;
        // 적의 시야각
        [SerializeField] private float viewAngle = 120f;
        // Raycast를 쏠 눈 위치 높이
        [SerializeField] private float eyeHeight = 1.6f;
        // 감지 대상 Layer
        [SerializeField] private LayerMask targetLayer;
        // 장애물의 Layer
        [SerializeField] private LayerMask obstacleLayer;
        // 플레이어
        [SerializeField] private Transform player;
        [SerializeField] private float playerHeight = 1.2f;
        [Tooltip("detection을 얼마 정도의 주기로 하는가")]
        [SerializeField] private float detectInterval = 0.4f;

        private float _detectTimer;

        private void Update()
        {
            if (player == null)
                return;
            
            _detectTimer -= Time.deltaTime;

            if (_detectTimer <= 0)
            {
                DetectVision(player);
                _detectTimer = detectInterval;
            }
        }

        // DetectVision을 한번 수행한 후, 해당 결과를 다음 수행까지 기억하는 역할
        private bool visibleJustBefore;

        // 1. 플레이어가 감지 거리 안에 있는가?
        private bool IsInViewDistance(Transform target)
        {
            Vector3 flatSelf = transform.position;
            Vector3 flatTarget = target.position;
            flatSelf.y = flatTarget.y = 0f;

            return Vector3.Distance(flatSelf, flatTarget) <= viewDistance;
        }

        // 2. 플레이어가 적의 시야각 안에 있는가?
        private bool IsInViewAngle(Transform target)
        {
            // 적 위치 기준 플레이어의 위치를 나타내는 벡터
            Vector3 directionToTarget = target.position - transform.position;

            // 높이 차이가 무시할 만한 경우
            directionToTarget.y = 0f;

            if (directionToTarget.sqrMagnitude < 0.001f)
                return true;

            directionToTarget.Normalize();
            
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            return angle <= viewAngle * 0.5f;
        }

        // 3. 적과 플레이어 사이에 벽이나 장애물이 없는가?
        private bool HasLineOfSight(Transform target)
        {
            Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
            Vector3 targetPoint = target.position + Vector3.up * playerHeight;

            /// 주의 : 플레이어가 벽 너머에서 점프하여 보이게 되면 적이 인식할 수 있는 상태
            /// 인식하지 못하도록 하고 싶다면
            /// 
            /// // 플레이어의 실제 y 대신, 지면 기준 높이를 쓰기
            /// Vector3 flatTarget = target.position;
            /// flatTarget.y = transform.position.y;   // 추격자와 같은 지면 높이로
            /// Vector3 targetPoint = flatTarget + Vector3.up * bodyHeight;
            /// 
            /// targetPoint 산출 코드를 위와 같이 변경하면 됨

            Vector3 direction = targetPoint - eyePosition;
            float distance = direction.magnitude;

            if (distance <= 0.01f)
                return true;

            if (Physics.Raycast(
                eyePosition,
                direction.normalized,
                distance,
                obstacleLayer,
                QueryTriggerInteraction.Ignore
            ))
                return false;

            return true;
        }

        // 1 ~ 3 조건 검사를 한 흐름으로
        private void DetectVision(Transform target)
        {
            bool canSee = IsInViewDistance(target)
            && IsInViewAngle(target)
            && HasLineOfSight(target);

            if (canSee)
                // 플레이어가 시야에 있으면, 매번 호출하여 그 위치를 업데이트
                SquadBlackboard.Instance.ReportSighting(target.position);
            else

                if (visibleJustBefore)
                    // 플레이어가 시야에서 사라지면, 단 한번만 호출하여 위치 제거
                    SquadBlackboard.Instance.ReportLostSight();

            // 이번 실행의 결과를 기억
            visibleJustBefore = canSee;
        }
    }
}
