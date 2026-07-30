using UnityEngine;
using System.Collections.Generic;

namespace Squad
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

    // 1. 플레이어가 감지 거리 안에 있는가?
    private Collider[] FindTargetsInViewDistance()
    {
        return Physics.OverlapSphere(
            transform.position,
            viewDistance,
            targetLayer,
            QueryTriggerInteraction.Ignore
        );
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
        Vector3 targetPoint = target.position + Vector3.up * 1.2f;

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

    private void UpdateVision()
    {
        Collider[] candidates = FindTargetsInViewDistance();

        Transform bestTarget = null;
        float bestDistance = float.MaxValue;

        foreach (Collider candidate in candidates)
        {
            Transform target = candidate.transform;

            if (!CanSeeTarget(target))
                continue;

            float distance = Vector3.Distance(transform.position, target.position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = target;
            }
        }

        if (bestTarget != null)
        {
            SetCurrentTarget(bestTarget);
        }
        else
        {
            ClearCurrentTarget();
        }
    }
}
