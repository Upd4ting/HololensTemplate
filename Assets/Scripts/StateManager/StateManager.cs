using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.StateManager
{
    internal class StateManager : Singleton<StateManager>
    {
        public enum RunningState
        {
            Running,
            Stopped,
            Paused
        }

        [SerializeField] private readonly bool _autoStart = true;

        [SerializeField] private readonly List<IState> _list = new List<IState>();

        private int _index;

        private RunningState _running;

        private void OnStart()
        {
            if (this._autoStart)
                Start();
        }

        private void OnUpdate()
        {
            if (this._running == RunningState.Running)
            {
                IState state = this._list[this._index];
                state.OnUpdate();
                if (state.IsFinished())
                    Next(state.GetParams());
            }
        }

        private void OnFixedUpdate()
        {
            if (this._running == RunningState.Running)
                this._list[this._index].OnFixedUpdate();
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        public void Start(int index)
        {
            index = this._index;
            this._list[this._index].OnStart();
            this._running = RunningState.Running;
        }

        public void Start()
        {
            Start(0);
        }

        private void Next(params object[] args)
        {
            this._list[this._index++].OnStop();
            this._list[this._index].OnStart(args);
        }

        public void Pause()
        {
            if (this._running == RunningState.Running)
                this._running = RunningState.Paused;
        }

        public void Resume()
        {
            if (this._running == RunningState.Paused)
                this._running = RunningState.Running;
        }

        public void Stop()
        {
            if (this._running == RunningState.Running)
            {
                this._list[this._index].OnCancel();
                this._running = RunningState.Stopped;
            }
        }

        public void Reset()
        {
            Stop();
        }

        public void Clear()
        {
            Stop();
            this._list.Clear();
        }
    }
}