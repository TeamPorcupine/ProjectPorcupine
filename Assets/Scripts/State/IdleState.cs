#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using Random = UnityEngine.Random;

namespace ProjectPorcupine.State
{
    [System.Diagnostics.DebuggerDisplay("Idle: ")]
    public class IdleState : State
    {
        private float totalIdleTime;
        private float timeSpentIdle;

        public IdleState(Character character, State nextState = null)
            : base("Idle", character, nextState)
        {
            timeSpentIdle = 0f;
            totalIdleTime = Random.Range(0.2f, 2.0f);
        }

        public override void Update(float deltaTime)
        {
            timeSpentIdle += deltaTime;
            if (timeSpentIdle >= totalIdleTime)
            {
                // We are done. Lets look for work.
                character.SetState(null);
            }
        }
    }
}