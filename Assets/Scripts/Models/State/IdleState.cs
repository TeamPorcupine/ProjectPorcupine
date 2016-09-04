using System;
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
            totalIdleTime = Random.Range(4f, 1.5f);
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

