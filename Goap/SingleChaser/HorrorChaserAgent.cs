using System.Collections.Generic;
using UnityEngine;

namespace Squad
{
    /// <summary>
    /// A horror-game chaser driven by GOAP with exactly three goals:
    ///   CatchPlayer  (priority 100) — reach the player when detected
    ///   InvestigateSound (50)       — walk to a heard sound and search it
    ///   Wander (1)                  — fallback when there's nothing to chase
    ///
    /// The enemy cannot be fought, so there is no survive/retreat goal. Goal
    /// selection each cycle picks the highest-priority goal whose desired fact
    /// isn't already satisfied; the planner then chains actions to reach it.
    ///
    /// This reuses the exact same GoapPlanner (A* over world states) as the
    /// squad demo — only the goal set and action list differ.
    /// 
    /// 이 스크립트를 오브젝트에 추가하면
    /// ChaserLocomotion도 자동으로 추가된다
    /// ChaserLocomotion이 있어야 작동하는 HorrorChaserAgent에 있어서
    /// 에러 방지 역할을 함.
    /// </summary>
    [RequireComponent(typeof(ChaserLocomotion))]
    public class HorrorChaserAgent : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;

        [Header("판정 거리")]
        [Tooltip("플레이어에게 어느 정도로 접근하면 게임 오버 판정인가")]
        public float CatchRadius = 1.2f;
        [Tooltip("소리/배회 target에 어느 정도로 접근하면 행동을 시작하는가")]
        public float ArriveRadius = 0.6f;

        [Header("Replanning")]
        [Tooltip("replan을 얼마 정도의 주기로 하는가")]
        [SerializeField] private float replanInterval = 0.4f;

        public ChaserLocomotion Locomotion { get; private set; }

        // Exposed for on-screen debug visualization (see the gizmo section).
        public string CurrentGoalName { get; private set; } = "-";
        public string CurrentActionName => _currentAction?.Name ?? "-";

        private List<GoapAction> _actions;
        private List<Goal> _goals;
        private Queue<GoapAction> _plan;
        private GoapAction _currentAction;
        private ChaserContext _ctx;
        private float _replanTimer;

        private void Awake()
        {
            Locomotion = GetComponent<ChaserLocomotion>();
        }

        private void Start()
        {
            _ctx = new ChaserContext
            {
                Agent = null, // MultipleChaser일 경우에만 채워넣는 필드임.
                Blackboard = SquadBlackboard.Instance,
                Self = transform,
                Player = player,
                Locomotion = Locomotion,
                CatchRadius = CatchRadius,
                ArriveRadius = ArriveRadius,
            };
            // Action과 Goal 불러오기
            _actions = ChaserActions.Build();
            _goals = ChaserGoals.Build();
        }

        private void Update()
        {
            _replanTimer -= Time.deltaTime;

            // Replan when the timer elapses, or when we've run out of plan.
            if (_replanTimer <= 0f || (_plan == null || _plan.Count == 0) && _currentAction == null)
            {
                Replan();
                _replanTimer = replanInterval;
            }

            // Advance the current action.
            if (_currentAction != null)
            {
                bool done = _currentAction.Perform(_ctx);
                if (done)
                    _currentAction = (_plan != null && _plan.Count > 0) ? _plan.Dequeue() : null;
            }
        }

        private void Replan()
        {
            WorldState state = BuildWorldState();
            Goal goal = SelectGoal(state);
            CurrentGoalName = goal?.Name ?? "-";
            if (goal == null) { _plan = null; _currentAction = null; return; }

            _plan = GoapPlanner.Plan(_actions, state, goal, _ctx);
            _currentAction = (_plan != null && _plan.Count > 0) ? _plan.Dequeue() : null;
        }

        /// <summary>
        /// Build the current facts. Detection facts come from the shared
        /// blackboard, so if the detection system flips playerVisible mid-chase
        /// (e.g. the player crossed into the other dimension), the very next
        /// replan will drop CatchPlayer and fall to InvestigateSound or Wander.
        /// </summary>
        private WorldState BuildWorldState()
        {
            var bb = _ctx.Blackboard;
            var s = new WorldState();

            // Catch-goal facts.
            s.Facts["playerVisible"] = bb.PlayerCurrentlyVisible;
            s.Facts["playerCaught"] = false;

            // Investigate-goal facts.
            s.Facts["heardSound"] = bb.HasSound;
            s.Facts["atSoundLocation"] = false;
            s.Facts["soundInvestigated"] = false;

            // Wander is always achievable; its fact starts false so the goal is
            // never "already satisfied" and thus always available as a floor.
            s.Facts["wandering"] = false;
            return s;
        }

        /// <summary>Highest-priority goal whose desired state isn't already met.</summary>
        private Goal SelectGoal(WorldState state)
        {
            Goal best = null;
            foreach (Goal g in _goals)
            {
                if (state.Matches(g.Desired)) continue;   // already satisfied
                // Only consider a goal if the planner could plausibly reach it:
                // cheap pre-filter avoids planning for InvestigateSound when no
                // sound exists, etc. (The planner would fail anyway, but this
                // skips the wasted search.)
                if (!GoalIsRelevant(g, state)) continue;
                if (best == null || g.Priority > best.Priority) best = g;
            }
            return best;
        }

        /// <summary>
        /// Quick "is this goal even worth planning for" check based on the
        /// entry-condition facts. Keeps the chaser from trying to chase with no
        /// player or investigate with no sound.
        /// </summary>
        private bool GoalIsRelevant(Goal g, WorldState s)
        {
            switch (g.Name)
            {
                case "CatchPlayer":
                    return Fact(s, "playerVisible");
                case "InvestigateSound":
                    return Fact(s, "heardSound");
                case "Wander":
                    return true;   // always relevant
                default:
                    return true;
            }
        }

        private static bool Fact(WorldState s, string key)
            => s.Facts.TryGetValue(key, out bool v) && v;

        // ---- Debug visualization ------------------------------------------

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            // Draw current goal/action as labels would need Handles (editor-only);
            // here we just draw reach radii and a line to the current target so
            // you can SEE what the chaser is doing during play.
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, CatchRadius);

            if (_ctx != null && _ctx.Blackboard != null && _ctx.Blackboard.HasSound)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_ctx.Blackboard.LastSoundPos, 0.4f);
                Gizmos.DrawLine(transform.position, _ctx.Blackboard.LastSoundPos);
            }
        }
    }
}
