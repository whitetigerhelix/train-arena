using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;
using TrainArena.Core;

namespace TrainArena.Core
{
    /// <summary>
    /// Base class for all TrainArena agents providing shared functionality
    /// Implements common activity management, debug support, and unified behavior
    /// </summary>
    public abstract class BaseTrainArenaAgent : Agent, ITrainArenaAgent
    {
        [Header("Agent Activity Control")]
        [SerializeField] private AgentActivity agentActivity = AgentActivity.Active;
        
        /// <summary>
        /// Agent's current activity state - controls whether agent responds to actions
        /// </summary>
        public virtual AgentActivity AgentActivity 
        { 
            get => agentActivity; 
            set => agentActivity = value; 
        }
        
        /// <summary>
        /// The main Transform representing this agent
        /// </summary>
        public abstract Transform MainTransform { get; }
        
        /// <summary>
        /// The main Rigidbody for physics and velocity visualization
        /// </summary>
        public abstract Rigidbody MainRigidbody { get; }
        
        /// <summary>
        /// Display name for UI and logging
        /// </summary>
        public virtual string DisplayName => name;
        
        /// <summary>
        /// Agent type identifier for UI display
        /// </summary>
        public abstract string AgentTypeIcon { get; }
        
        /// <summary>
        /// ML-Agents BehaviorParameters component
        /// </summary>
        public BehaviorParameters BehaviorParameters => GetComponent<BehaviorParameters>();
        
        /// <summary>
        /// Unity ML-Agents Agent component
        /// </summary>
        public Agent Agent => this;
        
        /// <summary>
        /// Whether the agent should respond to heuristic actions when active
        /// </summary>
        public virtual bool ShouldUseHeuristic()
        {
            return AgentActivity == AgentActivity.Active && 
                   BehaviorParameters != null && 
                   BehaviorParameters.BehaviorType == BehaviorType.HeuristicOnly;
        }
        
        /// <summary>
        /// Get all raycast sensors for visualization
        /// </summary>
        public virtual RayPerceptionSensorComponent3D[] GetRaycastSensors()
        {
            return GetComponentsInChildren<RayPerceptionSensorComponent3D>();
        }
        
        /// <summary>
        /// Get observation range for visualization
        /// </summary>
        public virtual float GetObservationRange()
        {
            return 2f; // Default observation range
        }
        
        /// <summary>
        /// Base implementation of OnActionReceived with activity check
        /// Derived classes should override HandleActiveActions instead of this method
        /// </summary>
        public override sealed void OnActionReceived(ActionBuffers actions)
        {
            // Debug logging for activity state issues
            if (Time.fixedTime % 5f < Time.fixedDeltaTime) // Log every 5 seconds
            {
                TrainArenaDebugManager.Log($"🤖 {name}: AgentActivity={AgentActivity}, OnActionReceived called", 
                    TrainArenaDebugManager.DebugLogLevel.Verbose);
            }
            
            // Check if agent is inactive - if so, don't respond to actions (for demos)
            if (AgentActivity == AgentActivity.Inactive)
            {
                TrainArenaDebugManager.Log($"⚫ {name}: Actions skipped - AgentActivity is Inactive", 
                    TrainArenaDebugManager.DebugLogLevel.Verbose);
                HandleInactiveState(); // Allow derived classes to handle inactive state
                return; // Skip all actions but keep agent in scene
            }
            
            // Call derived class implementation
            HandleActiveActions(actions);
        }
        
        /// <summary>
        /// Base implementation of Heuristic with activity check
        /// Derived classes should override HandleActiveHeuristic instead of this method
        /// </summary>
        public override sealed void Heuristic(in ActionBuffers actionsOut)
        {
            // Debug logging for heuristic state issues
            if (Time.fixedTime % 5f < Time.fixedDeltaTime) // Log every 5 seconds
            {
                TrainArenaDebugManager.Log($"🧠 {name}: AgentActivity={AgentActivity}, Heuristic called", 
                    TrainArenaDebugManager.DebugLogLevel.Verbose);
            }
            
            // Check if agent is inactive - if so, don't provide heuristic actions
            if (AgentActivity == AgentActivity.Inactive)
            {
                TrainArenaDebugManager.Log($"⚫ {name}: Heuristic skipped - AgentActivity is Inactive", 
                    TrainArenaDebugManager.DebugLogLevel.Verbose);
                // Zero out all actions when inactive
                var ca = actionsOut.ContinuousActions;
                for (int i = 0; i < ca.Length; i++)
                    ca[i] = 0f;
                return;
            }
            
            // Call derived class implementation
            HandleActiveHeuristic(actionsOut);
        }
        
        /// <summary>
        /// Handle actions when agent is active - implement in derived classes
        /// </summary>
        protected abstract void HandleActiveActions(ActionBuffers actions);
        
        /// <summary>
        /// Handle heuristic actions when agent is active - implement in derived classes
        /// </summary>
        protected abstract void HandleActiveHeuristic(in ActionBuffers actionsOut);
        
        /// <summary>
        /// Handle state when agent is inactive - override in derived classes if needed
        /// </summary>
        protected virtual void HandleInactiveState()
        {
            // Default implementation does nothing - derived classes can override
        }
        
        /// <summary>
        /// Logging utility for consistent agent messaging
        /// </summary>
        protected void LogAgent(string message, TrainArenaDebugManager.DebugLogLevel level = TrainArenaDebugManager.DebugLogLevel.Important)
        {
            TrainArenaDebugManager.Log($"{AgentTypeIcon} [{DisplayName}] {message}", level, this);
        }
    }
}