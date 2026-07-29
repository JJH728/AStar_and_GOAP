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

    
}