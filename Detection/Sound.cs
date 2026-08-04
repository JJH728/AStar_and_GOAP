using UnityEngine;
using Systems.Collection.Generic;

namespace Squad
{
    public class Sound
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
            AlertLevel = alertLevel;
            CanCrossDimension = canCrossDimension;
        }
    }

    public static class SoundList
    {
        public static readonly Sound Walking = 
            new Sound("Walking", 5f, SquadBlackboard.AlertLevel.Suspicious, 0f, false);
        public static readonly Sound Running = 
            new Sound("Running", 10f, SquadBlackboard.AlertLevel.Suspicious, 0f, false);
        public static readonly Sound Generator = 
            new Sound("Generator", 15f, SquadBlackboard.AlertLevel.Alerted, 6f, true);
        public static readonly Sound Decoy = 
            new Sound("Decoy", 10f, SquadBlackboard.AlertLevel.Suspicious, 0f, false);
    }
}
