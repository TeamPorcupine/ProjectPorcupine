#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

namespace ProjectPorcupine.State
{
    public abstract class State
    {
        protected Character character;

        public State(string name, Character character, State nextState)
        {
            Name = name;
            this.character = character;
            NextState = nextState;
        }

        public string Name { get; protected set; }

        public State NextState { get; protected set; }

        public virtual void Enter()
        {
        }

        public abstract void Update(float deltaTime);

        public virtual void Exit()
        {
        }

        public virtual void Interrupt()
        {
            if (NextState != null)
            {
                NextState.Interrupt();
                NextState = null;
            }
        }

        public override string ToString()
        {
            return string.Format("[{0}State]", Name);
        }

        protected void Finished()
        {
            character.SetState(NextState);
        }

        #region Debug

        [UberLogger.StackTraceIgnore]
        protected void FSMLog(string message, params object[] par)
        {
            string prefixedMessage = string.Format("{0} {1}({2}): {3}", character.GetName(), GetType().Name, GetHashCode(), message);
            Debug.ULogChannel("FSM", prefixedMessage, par);
        }

        #endregion
    }
}