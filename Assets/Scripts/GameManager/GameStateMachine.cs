using UnityEngine;
using Fusion;
using GNW2.Events;
using System.Collections.Generic;

namespace GNW2.GameManager
{
    public class GameStateMachine : NetworkBehaviour
    {
        [Networked] public GameState CurrentState { get; set; }
        [Networked] public int CurrentRound { get; set; }
        [Networked] public int PlayersReady { get; set; }

        private Dictionary<GameState, IGameState> states = new Dictionary<GameState, IGameState>();

        public void Initialize()
        {
            // Register states
            states[GameState.WaitingForPlayers] = new WaitingForPlayersState(this);
            states[GameState.RoundStarting] = new RoundStartingState(this);
            states[GameState.WaitingForSelections] = new WaitingForSelectionsState(this);
            states[GameState.Evaluating] = new EvaluatingState(this);
            states[GameState.ShowingResults] = new ShowingResultsState(this);
            states[GameState.RoundEnding] = new RoundEndingState(this);

            // Start in waiting state
            if (Object.HasStateAuthority)
            {
                TransitionToState(GameState.WaitingForPlayers);
            }
        }

        public void TransitionToState(GameState newState)
        {
            if (!Object.HasStateAuthority) return;

            Debug.Log($"State Transition: {CurrentState} -> {newState}");

            // Exit current state
            if (states.ContainsKey(CurrentState))
            {
                states[CurrentState].Exit();
            }

            // Update networked state
            CurrentState = newState;

            // Enter new state
            if (states.ContainsKey(newState))
            {
                states[newState].Enter();
            }

            // Broadcast state change
            RPC_BroadcastStateChange(newState);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_BroadcastStateChange(GameState newState)
        {
            // Non-authority clients can react to state changes here
            if (!Object.HasStateAuthority && states.ContainsKey(newState))
            {
                states[newState]?.Enter();
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority && states.ContainsKey(CurrentState))
            {
                states[CurrentState]?.Update();
            }
        }
    }

    // ===== State Implementations =====

    public class WaitingForPlayersState : IGameState
    {
        private GameStateMachine _fsm;

        public WaitingForPlayersState(GameStateMachine fsm)
        {
            _fsm = fsm;
        }

        public void Enter()
        {
            Debug.Log("Waiting for players...");
        }

        public void Update()
        {
            if (GameManager.Instance.activePlayers.Count >= 2)
            {
                // Publish game started event
                EventBus.Publish(new GameStartedEvent
                {
                    PlayerCount = GameManager.Instance.activePlayers.Count
                });

                _fsm.TransitionToState(GameState.RoundStarting);
            }
        }

        public void Exit()
        {
        }
    }

    public class RoundStartingState : IGameState
    {
        private GameStateMachine _fsm;

        public RoundStartingState(GameStateMachine fsm)
        {
            _fsm = fsm;
        }

        public void Enter()
        {
            _fsm.CurrentRound++;
            Debug.Log($"Round {_fsm.CurrentRound} starting!");

            EventBus.Publish(new RoundStartedEvent { RoundNumber = _fsm.CurrentRound });

            // Move to waiting for selections immediately
            _fsm.TransitionToState(GameState.WaitingForSelections);
        }

        public void Update()
        {
        }

        public void Exit()
        {
        }
    }

    public class WaitingForSelectionsState : IGameState
    {
        private GameStateMachine _fsm;

        public WaitingForSelectionsState(GameStateMachine fsm)
        {
            _fsm = fsm;
        }

        public void Enter()
        {
            Debug.Log("Waiting for player selections...");
            _fsm.PlayersReady = 0;
            EventBus.Publish(new ShowSelectionUIEvent());
        }

        public void Update()
        {
            // Transition when both players have made selections
            if (_fsm.PlayersReady >= 2)
            {
                _fsm.TransitionToState(GameState.Evaluating);
            }
        }

        public void Exit()
        {
            EventBus.Publish(new HideSelectionUIEvent());
        }
    }

    public class EvaluatingState : IGameState
    {
        private GameStateMachine _fsm;

        public EvaluatingState(GameStateMachine fsm)
        {
            _fsm = fsm;
        }

        public void Enter()
        {
            Debug.Log("Evaluating round results...");
            // GameHandler will handle the evaluation logic
            // and transition to ShowingResults when done
        }

        public void Update()
        {
        }

        public void Exit()
        {
        }
    }

    public class ShowingResultsState : IGameState
    {
        private GameStateMachine _fsm;
        private float _stateTime;
        private const float RESULT_DISPLAY_TIME = 3f;

        public ShowingResultsState(GameStateMachine fsm)
        {
            _fsm = fsm;
        }

        public void Enter()
        {
            Debug.Log("Showing results...");
            _stateTime = 0f;
        }

        public void Update()
        {
            _stateTime += _fsm.Runner.DeltaTime;

            if (_stateTime >= RESULT_DISPLAY_TIME)
            {
                _fsm.TransitionToState(GameState.RoundEnding);
            }
        }

        public void Exit()
        {
            EventBus.Publish(new HideResultUIEvent());
        }
    }

    public class RoundEndingState : IGameState
    {
        private GameStateMachine _fsm;

        public RoundEndingState(GameStateMachine fsm)
        {
            _fsm = fsm;
        }

        public void Enter()
        {
            Debug.Log("Round ending...");
            // Start next round
            _fsm.TransitionToState(GameState.RoundStarting);
        }

        public void Update()
        {
        }

        public void Exit()
        {
        }
    }
}
