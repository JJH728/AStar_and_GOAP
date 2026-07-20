using System.Collections.Generic;
using UnityEngine;

namespace Squad
{
    /// <summary>
    /// Shared "squad memory" — the generalization of Half-Life's squad-leader
    /// information relay. Any chaser's perception writes here; every chaser's
    /// GOAP planner reads here. One sighting propagates to the whole squad.
    ///
    /// This is a scene-level singleton (one squad). For multiple independent
    /// squads, make it a regular component and give each chaser a reference
    /// to its own squad's blackboard.
    /// </summary>
    public class SquadBlackboard : MonoBehaviour
    {
        [SerializeField]
        private float AlertedToSuspiciusTime = 12f;
        [SerializeField]
        private float SuspiciousToCalmTime = 25f;
        
        public static SquadBlackboard Instance { get; private set; }

        public enum AlertLevel { Calm, Suspicious, Alerted }

        // --- Shared facts ---
        public AlertLevel Alert { get; private set; } = AlertLevel.Calm;
        public bool PlayerCurrentlyVisible { get; private set; }
        public Vector3 LastKnownPlayerPos { get; private set; }
        public float TimeSinceLastSeen { get; private set; } = Mathf.Infinity;

        // --- Sound facts (for the InvestigateSound goal) ---
        // HasSound is true while there's an un-investigated sound to check out.
        // LastSoundPos is where it came from. A chaser walks there (MoveToSound),
        // searches (SearchSoundArea), then calls ClearSound() so it isn't chased
        // forever. Generator sounds and footsteps both land here; the difference
        // (footsteps stay in one dimension, generator sound crosses) is enforced
        // by WHO calls ReportSound — see the detection system, not here.
        public bool HasSound { get; private set; }
        public Vector3 LastSoundPos { get; private set; }

        // 추격, 매복 등등의 역할을 나누고, 각 추격자들이 일제히 같은 행동을 하지 않도록 조율
        // <(역할), (추격자 번호)> 타입의 Dictionary로 역할을 현재 수행 중인 추격자를 기록
        private readonly Dictionary<string, int> _roleClaims = new();

        private void Awake()
        {
            // 기존에 있던 BlackBoard Instance 제거
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (!PlayerCurrentlyVisible)
                TimeSinceLastSeen += Time.deltaTime;

            // 시간이 지남에 따라 경계 상태를 서서히 품
            // 
            if (Alert == AlertLevel.Alerted && TimeSinceLastSeen > 12f)
                Alert = AlertLevel.Suspicious;
            if (Alert == AlertLevel.Suspicious && TimeSinceLastSeen > 25f)
                Alert = AlertLevel.Calm;
        }

        /// <summary>플레이어가 시야에 들어왔을 때</summary>
        public void ReportSighting(Vector3 playerPos)
        {
            LastKnownPlayerPos = playerPos;
            PlayerCurrentlyVisible = true;
            TimeSinceLastSeen = 0f;
            Alert = AlertLevel.Alerted;
        }

        /// <summary>플레이어가 시야에서 사라졌을 때</summary>
        public void ReportLostSight()
        {
            PlayerCurrentlyVisible = false;
            // LastKnownPlayerPos는 지우지 않고 남겨두어
            // 시야에서 사라져도 잠시동안은 주변을 수색하도록 한다.
        }

        /// <summary>
        /// Report a heard sound (footstep, generator, etc.) — softer than a
        /// sighting. Sets HasSound so an idle chaser will pick up the
        /// InvestigateSound goal and walk over to check it out.
        /// The caller (detection system) decides whether a given chaser can hear
        /// this sound at all — e.g. a generator sound is reported to chasers in
        /// both dimensions, a footstep only to chasers in the player's dimension.
        /// </summary>
        public void ReportSound(Vector3 soundPos)
        {
            if (Alert == AlertLevel.Calm)
                Alert = AlertLevel.Suspicious;
            HasSound = true;
            LastSoundPos = soundPos;
            // 소리가 들렸지만 플레이어는 보이지 않을 때
            // 소리가 들린 위치를 플레이어가 2초 전(조금 전) 있었을 위치라고 추정
            if (!PlayerCurrentlyVisible)
            {
                LastKnownPlayerPos = soundPos;
                TimeSinceLastSeen = Mathf.Min(TimeSinceLastSeen, 2f);
            }
        }

        /// <summary>소리에 대한 조사를 끝냈을 때</summary>
        public void ClearSound() => HasSound = false;

        // --- Role arbitration ---
        // A chaser tries to claim a role; returns true if it got it.
        public bool TryClaimRole(string role, int chaserId)
        {
            // 역할을 선점한 다른 추격자가 이미 존재
            if (_roleClaims.TryGetValue(role, out int owner) && owner != chaserId)
                return false;
            // 이 추격자에게 역할 부여
            _roleClaims[role] = chaserId;
            return true;
        }

        public void ReleaseRole(string role, int chaserId)
        {
            if (_roleClaims.TryGetValue(role, out int owner) && owner == chaserId)
                _roleClaims.Remove(role);
        }
    }
}
