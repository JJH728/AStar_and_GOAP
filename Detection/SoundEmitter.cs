using UnityEngine;
using Systems.Collections.Generic;

namespace squad
{
    public static class SoundEmitter
    {
        private const int MaxHits = 16;
        private static readonly Collider[] _hits = new Collider[MaxHits];
        
        // pos에서 sound를 방출한다. sound.Radius 안에 있으면서 enemyLayer 안에 속한 개체에게 이 소리를 보고한다.
        public static void Emit(Vector3 pos, Sound sound, LayerMask listerLayer)
        {
            if (SquadBlackboard.Instance == null)
                return;
            
            int count = Physics.OverlapSphereNonAlloc(
                pos, sound.Radius, _hits, enemyLayer, QueryTriggerInteraction.Ignore);

            if (!count)
                return;

            SquadBlackBoard.Instance.ReportSound(pos, sound);
        }
    }
}
