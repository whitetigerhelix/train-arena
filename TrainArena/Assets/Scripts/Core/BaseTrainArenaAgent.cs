using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace TrainArena.Core
{
    /// <summary>
    /// Base class for all TrainArena agents providing shared functionality
    /// Implements common activity management, debug support, arena integration, and episode management
    /// </summary>
    public abstract class BaseTrainArenaAgent : Agent, ITrainArenaAgent
    {
        [Header("Agent Activity Control")]
        [SerializeField] private AgentActivity agentActivity = AgentActivity.Active;
        
        [Header("Scene References")]
        public Transform goal;
        public LayerMask obstacleMask = -1; // Default to everything for tag-based detection
        
        [Header("Observation Space Configuration")]
        public int raycastDirections = 8; // Default raycast directions for observation space
        
        [Header("Episode Management")]
        public int maxEpisodeSteps = 500;
        public float episodeTimeLimit = 30f;
        
        // Episode tracking - shared across all agent types
        protected int episodeStepCount;
        protected float episodeStartTime;
        protected int totalEpisodesCompleted;
        protected int totalActionsReceived;
        
        // Arena integration - shared positioning system
        protected ArenaHelper arenaHelper;
        protected Vector3 arenaCenter;
        
        // Memory management - shared across all agents
        protected static int globalEpisodeCount = 0;
        
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
        /// Calculate total observation count based on configuration
        /// Must be implemented by derived classes to match their observation space
        /// </summary>
        public abstract int GetTotalObservationCount();

        /// <summary>
        /// Shared initialization logic for all agents
        /// </summary>
        public override void Initialize()
        {
            TrainArenaDebugManager.Log($"{gameObject.name} initializing", context: transform);
            
            // Find EnvManager component using multiple fallback strategies
            EnvInitializer envManager = FindEnvManagerComponent();
            
            if (envManager != null)
            {
                arenaHelper = envManager.ArenaHelper;
                arenaCenter = transform.parent ? transform.parent.position : Vector3.zero;
                TrainArenaDebugManager.Log($"🔗 {gameObject.name} connected to ArenaHelper: {arenaHelper.GetDebugInfo()}", 
                                         TrainArenaDebugManager.DebugLogLevel.Important);
            }
            else
            {
                TrainArenaDebugManager.LogError($"❌ {gameObject.name} could not find EnvInitializer! Arena positioning will not work correctly.");
            }
            
            // Call agent-specific initialization
            InitializeAgent();
        }
        
        /// <summary>
        /// Finds the EnvManager component using multiple fallback strategies for reliability
        /// </summary>
        private EnvInitializer FindEnvManagerComponent()
        {
            // Strategy 1: Find by type in the scene (most reliable)
            EnvInitializer envInitializer = FindFirstObjectByType<EnvInitializer>();
            if (envInitializer != null)
            {
                TrainArenaDebugManager.Log($"✅ Found EnvInitializer via FindFirstObjectByType", TrainArenaDebugManager.DebugLogLevel.Verbose);
                return envInitializer;
            }
            
            // Strategy 2: Look for EnvManager as a sibling (same parent)
            if (transform.parent != null)
            {
                Transform envManager = transform.parent.Find("EnvManager");
                if (envManager != null)
                {
                    envInitializer = envManager.GetComponent<EnvInitializer>();
                    if (envInitializer != null)
                    {
                        TrainArenaDebugManager.Log($"✅ Found EnvManager as sibling", TrainArenaDebugManager.DebugLogLevel.Verbose);
                        return envInitializer;
                    }
                }
            }
            
            // Strategy 3: Search all siblings manually
            if (transform.parent != null)
            {
                foreach (Transform sibling in transform.parent)
                {
                    if (sibling != transform && sibling.name.Contains("EnvManager"))
                    {
                        envInitializer = sibling.GetComponent<EnvInitializer>();
                        if (envInitializer != null)
                        {
                            TrainArenaDebugManager.Log($"✅ Found EnvManager via sibling search", TrainArenaDebugManager.DebugLogLevel.Verbose);
                            return envInitializer;
                        }
                    }
                }
            }
            
            // Strategy 4: Last resort - GameObject.Find (less efficient but comprehensive)
            GameObject envManagerGO = GameObject.Find("EnvManager");
            if (envManagerGO != null)
            {
                envInitializer = envManagerGO.GetComponent<EnvInitializer>();
                if (envInitializer != null)
                {
                    TrainArenaDebugManager.Log($"✅ Found EnvManager via GameObject.Find", TrainArenaDebugManager.DebugLogLevel.Important);
                    return envInitializer;
                }
            }
            
            TrainArenaDebugManager.LogError($"❌ Could not find EnvManager with EnvInitializer component using any strategy!");
            return null;
        }
        
        /// <summary>
        /// Agent-specific initialization - override in derived classes
        /// </summary>
        protected virtual void InitializeAgent()
        {
            // Default implementation does nothing - override in derived classes
        }

        /// <summary>
        /// Shared episode begin logic for all agents
        /// </summary>
        public override void OnEpisodeBegin()
        {
            // Reset episode tracking
            episodeStepCount = 0;
            episodeStartTime = Time.time;
            totalEpisodesCompleted++;
            totalActionsReceived = 0;
            globalEpisodeCount++;
            
            // Periodic garbage collection to prevent memory buildup
            if (globalEpisodeCount % 50 == 0)
            {
                System.GC.Collect();
                TrainArenaDebugManager.Log($"🧹 Performed garbage collection after {globalEpisodeCount} episodes", 
                                         TrainArenaDebugManager.DebugLogLevel.Important);
            }
            
            // Reset physics for main rigidbody
            if (MainRigidbody != null)
            {
                MainRigidbody.linearVelocity = Vector3.zero;
                MainRigidbody.angularVelocity = Vector3.zero;
            }
            
            // Use ArenaHelper for consistent position generation if available
            if (arenaHelper != null)
            {
                ResetPositionWithArenaHelper();
            }
            else
            {
                // Fallback positioning without ArenaHelper
                ResetPositionFallback();
            }
            
            // Call agent-specific episode begin logic
            OnAgentEpisodeBegin();
        }
        
        /// <summary>
        /// Reset agent and goal positions using ArenaHelper
        /// </summary>
        protected virtual void ResetPositionWithArenaHelper()
        {
            // Generate new agent position using ArenaHelper
            Vector3 newAgentPos = arenaHelper.GetRandomAgentPosition(arenaCenter);
            MainTransform.position = newAgentPos;
            MainTransform.rotation = Quaternion.Euler(0f, Random.Range(0, 360f), 0f);

            // Generate new goal position using ArenaHelper (ensures minimum distance from agent)
            if (goal != null)
            {
                Vector3 newGoalPos = arenaHelper.GetRandomGoalPosition(arenaCenter, newAgentPos);
                goal.position = newGoalPos;
                
                // Debug logging with distance validation
                float distance = Vector3.Distance(newAgentPos, newGoalPos);
                bool withinBounds = arenaHelper.IsWithinArenaBounds(newAgentPos, arenaCenter) && 
                                   arenaHelper.IsWithinArenaBounds(newGoalPos, arenaCenter);
                TrainArenaDebugManager.Log($"Episode Reset: Agent {gameObject.name} → {newAgentPos}, Goal → {newGoalPos} | Distance: {distance:F2} | InBounds: {withinBounds}", 
                                         TrainArenaDebugManager.DebugLogLevel.Verbose);
            }
        }
        
        /// <summary>
        /// Fallback positioning when ArenaHelper is not available
        /// </summary>
        protected virtual void ResetPositionFallback()
        {
            TrainArenaDebugManager.LogWarning($"⚠️ {gameObject.name}: ArenaHelper not available, using fallback positioning");
            var arena = transform.parent;
            Vector3 center = arena ? arena.position : Vector3.zero;
            MainTransform.position = center + new Vector3(Random.Range(-8f, 8f), 0.5f, Random.Range(-8f, 8f));
            MainTransform.rotation = Quaternion.Euler(0f, Random.Range(0, 360f), 0f);
            
            if (goal != null)
            {
                goal.position = center + new Vector3(Random.Range(-8f, 8f), 1.0f, Random.Range(-8f, 8f));
            }
        }
        
        /// <summary>
        /// Agent-specific episode begin logic - override in derived classes
        /// </summary>
        protected virtual void OnAgentEpisodeBegin()
        {
            // Default implementation does nothing - override in derived classes
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
            
            // Count non-zero actions to detect if agent is receiving proper actions
            bool hasNonZeroAction = false;
            for (int i = 0; i < actions.ContinuousActions.Length; i++)
            {
                if (Mathf.Abs(actions.ContinuousActions[i]) > 0.01f)
                {
                    hasNonZeroAction = true;
                    break;
                }
            }
            
            if (hasNonZeroAction)
            {
                totalActionsReceived++;
            }
            
            // Increment episode step counter
            episodeStepCount++;
            
            // Check episode termination conditions (shared logic)
            if (CheckSharedEpisodeTermination())
            {
                return; // Episode ended, don't process actions
            }
            
            // Call derived class implementation
            HandleActiveActions(actions);
        }
        
        /// <summary>
        /// Check shared episode termination conditions
        /// Returns true if episode should end
        /// </summary>
        protected virtual bool CheckSharedEpisodeTermination()
        {
            // Fall detection (if agent has fallen below ground level)
            if (MainTransform.position.y < -1f)
            {
                AddReward(-0.5f); // Penalty for falling
                TrainArenaDebugManager.Log($"💥 {gameObject.name} FELL! Steps={episodeStepCount} | Time={Time.time - episodeStartTime:F1}s", 
                                         TrainArenaDebugManager.DebugLogLevel.Important);
                EndEpisode();
                return true;
            }
            
            // Step limit check
            if (episodeStepCount >= maxEpisodeSteps)
            {
                AddReward(-0.5f); // Penalty for timeout
                TrainArenaDebugManager.Log($"⏰ {gameObject.name} STEP TIMEOUT! Steps={episodeStepCount} | Episodes={totalEpisodesCompleted}", 
                                         TrainArenaDebugManager.DebugLogLevel.Important);
                EndEpisode();
                return true;
            }
            
            // Time limit check
            if (Time.time - episodeStartTime > episodeTimeLimit)
            {
                AddReward(-0.5f); // Penalty for timeout
                TrainArenaDebugManager.Log($"⏰ {gameObject.name} TIME TIMEOUT! Time={Time.time - episodeStartTime:F1}s | Episodes={totalEpisodesCompleted}", 
                                         TrainArenaDebugManager.DebugLogLevel.Important);
                EndEpisode();
                return true;
            }
            
            return false; // Episode should continue
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
        
        /// <summary>
        /// Get current episode progress for debugging and UI
        /// </summary>
        public virtual string GetEpisodeProgressInfo()
        {
            float episodeTime = Time.time - episodeStartTime;
            return $"Episode {totalEpisodesCompleted} | Step {episodeStepCount}/{maxEpisodeSteps} | Time {episodeTime:F1}s/{episodeTimeLimit:F0}s | Actions {totalActionsReceived}";
        }
        
        /// <summary>
        /// Get agent status for debugging and monitoring
        /// </summary>
        public virtual string GetAgentStatusInfo()
        {
            var behaviorParams = GetComponent<BehaviorParameters>();
            string behaviorType = behaviorParams ? behaviorParams.BehaviorType.ToString() : "Unknown";
            string modelName = behaviorParams?.Model?.name ?? "NO_MODEL";
            bool hasModel = behaviorParams?.Model != null;
            
            Vector3 velocity = MainRigidbody ? MainRigidbody.linearVelocity : Vector3.zero;
            string status = velocity.magnitude > 0.02f ? "MOVING" : "STATIONARY";
            
            return $"{status} | {behaviorType} | Model:{modelName}({hasModel}) | Vel={velocity.magnitude:F2}";
        }
    }
}