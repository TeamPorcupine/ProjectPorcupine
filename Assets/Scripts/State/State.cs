#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software,
// and you are welcome to redistribute it under certain conditions; See
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System.Collections.Generic;

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
            FSMLog(" - Enter");
        }

        public abstract void Update(float deltaTime);

        public virtual void Exit()
        {
            FSMLog(" - Exit");
        }

        public virtual void Interrupt()
        {
            FSMLog(" - Interrupt");

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
            FSMLog(" - Finished");
            character.SetState(NextState);
        }

        #region Debug

        private string StateStack()
        {
            List<string> names = new List<string>{ Name };
            State state = this;
            while (state.NextState != null)
            {
                state = state.NextState;
                names.Insert(0, state.Name);
            }

            return string.Join(".", names.ToArray());
        }

        [UberLogger.StackTraceIgnore]
        protected void FSMLog(string message, params object[] par)
        {
            string prefixedMessage = string.Format("{0} {1}: {2}", character.GetName(), StateStack(), message);
            Debug.ULogChannel("FSM", prefixedMessage, par);
        }

        #endregion
    }
}