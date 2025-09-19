using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;

namespace TrainArena.Core
{
    /// <summary>
    /// Agent activity state - controls whether agents respond to actions
    /// This is separate from ML-Agents BehaviorType and controls whether agents respond to actions
    /// </summary>
    public enum AgentActivity
    {
        Active,     // Agent responds normally to all actions
        Inactive    // Agent ignores all actions and remains stationary (for demos)
    }

    /// <summary>
    /// Common interface for all TrainArena agents
    /// Provides unified access to agent properties for debug systems and management
    /// </summary>
    public interface ITrainArenaAgent
    {
        /// <summary>
        /// Agent's current activity state (Active/Inactive)
        /// </summary>
        AgentActivity AgentActivity { get; set; }
        
        /// <summary>
        /// The main Transform representing this agent (for position, movement visualization)
        /// </summary>
        Transform MainTransform { get; }
        
        /// <summary>
        /// The main Rigidbody for physics and velocity visualization
        /// </summary>
        Rigidbody MainRigidbody { get; }
        
        /// <summary>
        /// Display name for UI and logging
        /// </summary>
        string DisplayName { get; }
        
        /// <summary>
        /// Agent type identifier for UI display (🧊 for cube, 🎭 for ragdoll, etc.)
        /// </summary>
        string AgentTypeIcon { get; }
        
        /// <summary>
        /// ML-Agents BehaviorParameters component
        /// </summary>
        BehaviorParameters BehaviorParameters { get; }
        
        /// <summary>
        /// Unity ML-Agents Agent component
        /// </summary>
        Agent Agent { get; }
        
        /// <summary>
        /// Whether the agent should respond to heuristic actions when active
        /// </summary>
        bool ShouldUseHeuristic();
        
        /// <summary>
        /// Get all raycast sensors for visualization
        /// </summary>
        Unity.MLAgents.Sensors.RayPerceptionSensorComponent3D[] GetRaycastSensors();
        
        /// <summary>
        /// Get observation range for visualization
        /// </summary>
        float GetObservationRange();

        /// <summary>
        /// Calculate total observation count based on configuration
        /// </summary>
        int GetTotalObservationCount();
    }
}