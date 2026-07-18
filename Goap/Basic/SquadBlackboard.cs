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

        // Role claims: which chaser claimed which role this planning cycle.
        // Lets one chaser "direct chase" while another "claims an ambush spot",
        // preventing everyone from doing the same thing.
        private readonly Dictionary<string, int> _roleClaims = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            if (!PlayerCurrentlyVisible)
                TimeSinceLastSeen += Time.deltaTime;

            // Decay alert over time once the player is lost.
            if (Alert == AlertLevel.Alerted && TimeSinceLastSeen > 12f)
                Alert = AlertLevel.Suspicious;
            if (Alert == AlertLevel.Suspicious && TimeSinceLastSeen > 25f)
                Alert = AlertLevel.Calm;
        }

        /// <summary>Called by any chaser's perception when it sees the player.</summary>
        public void ReportSighting(Vector3 playerPos)
        {
            LastKnownPlayerPos = playerPos;
            PlayerCurrentlyVisible = true;
            TimeSinceLastSeen = 0f;
            Alert = AlertLevel.Alerted;
        }

        /// <summary>Called when a chaser that was seeing the player loses sight.</summary>
        public void ReportLostSight()
        {
            PlayerCurrentlyVisible = false;
            // LastKnownPlayerPos is kept — this is what enables "search the last
            // place we saw them" behavior across the whole squad.
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
            if (Alert == AlertLevel.Calm) Alert = AlertLevel.Suspicious;
            HasSound = true;
            LastSoundPos = soundPos;
            if (!PlayerCurrentlyVisible)
            {
                LastKnownPlayerPos = soundPos;
                TimeSinceLastSeen = Mathf.Min(TimeSinceLastSeen, 2f);
            }
        }

        /// <summary>Called by SearchSoundArea once a sound has been investigated.</summary>
        public void ClearSound() => HasSound = false;

        // --- Role arbitration ---
        // A chaser tries to claim a role; returns true if it got it.
        public bool TryClaimRole(string role, int chaserId)
        {
            if (_roleClaims.TryGetValue(role, out int owner) && owner != chaserId)
                return false;
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
