using UnityEngine;
using Systems.Collection.Generic;

namespace Squad
{
    class Sound
    {
        public string Name {get;}
        public float Radius {get;}
        public float Duration {get;}
        public SquadBlackboard.AlertLevel AlertLevel {get;}
        public bool CanCrossDimension {get;}

        public Sound(string name, float radius, float duration, SqaudBlackboard.AlertLevel alertLevel, bool canCrossDimension)
        {
            Name = name;
            Radius = radius;
            Duration = duration;
            AlertLevel = alertLevel;\
            CanCrossDimension = canCrossDimension;
        }
    }
}
