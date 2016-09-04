using System;
using System.Linq;

namespace ProjectPorcupine.State
{
    public abstract class State
    {
        public State NextState { get; protected set; }

        public string Name { get; protected set; }

        protected Character character;

        public State(string name, Character character, State nextState)
        {
            Name = name;
            this.character = character;
            NextState = nextState;
        }

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

        protected void Finished()
        {
            character.SetState(NextState);
        }

        public override string ToString()
        {
            return string.Format("[{0}State]", Name);
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

