using UnityEngine;
using System.Collections.Generic;

namespace TrainArena.Configuration
{
    /// <summary>
    /// Centralized configuration for ML-Agent types and their specifications
    /// </summary>
    [System.Serializable]
    public static class AgentConfiguration
    {
        /// <summary>
        /// Configuration for CubeAgent specifications
        /// </summary>
        public static class CubeAgent
        {
            public const int ActionCount = 2; // moveX, moveZ
            public const string BehaviorName = "CubeAgent";
            
            // Action indices for clarity
            public const int MoveXActionIndex = 0;
            public const int MoveZActionIndex = 1;
        }
        
        /// <summary>
        /// Configuration for RagdollAgent specifications  
        /// </summary>
        public static class RagdollAgent
        {
            public const string BehaviorName = "RagdollAgent";
            
            // Joint categories for locomotion control
            public static readonly string[] LocomotionJointNames = 
            {
                "LeftUpperLeg", "RightUpperLeg",   // Hip joints
                "LeftLowerLeg", "RightLowerLeg",   // Knee joints  
                "LeftFoot", "RightFoot"            // Ankle joints
            };
            
            /// <summary>
            /// Expected number of locomotion joints (hips + knees + ankles)
            /// </summary>
            public const int ExpectedLocomotionJointCount = 6;
            
            /// <summary>
            /// Check if a joint name is part of the locomotion system
            /// </summary>
            public static bool IsLocomotionJoint(string jointName)
            {
                foreach (var name in LocomotionJointNames)
                {
                    if (jointName.Contains(name))
                        return true;
                }
                return false;
            }
        }
    }
    
    /// <summary>
    /// Standardized joint naming configuration for ragdoll creation
    /// </summary>
    [System.Serializable]
    public static class RagdollJointNames
    {
        // Core body parts
        public const string Pelvis = "Pelvis";
        public const string Chest = "Chest";
        public const string Head = "Head";
        
        // Arms (left/right)
        public const string LeftUpperArm = "LeftUpperArm";
        public const string RightUpperArm = "RightUpperArm";
        public const string LeftLowerArm = "LeftLowerArm"; 
        public const string RightLowerArm = "RightLowerArm";
        
        // Legs (left/right) - these are the locomotion joints
        public const string LeftUpperLeg = "LeftUpperLeg";
        public const string RightUpperLeg = "RightUpperLeg";
        public const string LeftLowerLeg = "LeftLowerLeg";
        public const string RightLowerLeg = "RightLowerLeg";
        public const string LeftFoot = "LeftFoot";
        public const string RightFoot = "RightFoot";
        
        /// <summary>
        /// All joint names in creation order
        /// </summary>
        public static readonly string[] AllJoints = 
        {
            Pelvis, Chest, Head,
            LeftUpperArm, RightUpperArm, LeftLowerArm, RightLowerArm,
            LeftUpperLeg, RightUpperLeg, LeftLowerLeg, RightLowerLeg, LeftFoot, RightFoot
        };
        
        /// <summary>
        /// Only the locomotion joint names (legs)
        /// </summary>
        public static readonly string[] LocomotionJoints = 
        {
            LeftUpperLeg, RightUpperLeg, LeftLowerLeg, RightLowerLeg, LeftFoot, RightFoot
        };
        
        /// <summary>
        /// Check if a joint name matches any of the locomotion joints
        /// </summary>
        public static bool IsLocomotionJoint(string jointName)
        {
            foreach (var legJoint in LocomotionJoints)
            {
                if (jointName == legJoint)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get the expected PDJointController configuration for a joint
        /// </summary>
        public static (float minAngle, float maxAngle, float kp, float kd) GetJointControllerConfig(string jointName)
        {
            // Use updated values that match our improved PD controller settings
            switch (jointName)
            {
                case LeftUpperLeg:
                case RightUpperLeg:
                    return (-90f, 90f, 200f, 20f);  // Hip joints - increased range and gains
                    
                case LeftLowerLeg:
                case RightLowerLeg:
                    return (-130f, 10f, 180f, 18f);  // Knee joints - natural flexion range
                    
                case LeftFoot:
                case RightFoot:
                    return (-60f, 30f, 150f, 15f);  // Ankle joints - dorsiflexion/plantarflexion
                    
                case LeftUpperArm:
                case RightUpperArm:
                    return (-90f, 90f, 120f, 12f);  // Shoulder joints
                    
                case LeftLowerArm:
                case RightLowerArm:
                    return (-135f, 0f, 100f, 10f);  // Elbow joints - natural flexion
                    
                case Head:
                    return (-45f, 45f, 80f, 8f);   // Neck joint
                    
                case Chest:
                    return (-30f, 30f, 100f, 10f); // Spine joint
                    
                default:
                    return (-90f, 90f, 200f, 20f); // Default to hip-like values for unknown joints
            }
        }
        
        /// <summary>
        /// Check if a joint name contains any of the specified patterns
        /// </summary>
        public static bool JointNameContains(string jointName, params string[] patterns)
        {
            foreach (var pattern in patterns)
            {
                if (jointName.Contains(pattern))
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Configuration for ragdoll heuristic behavior patterns
    /// </summary>
    [System.Serializable]
    public static class RagdollHeuristicConfig
    {
        // Base timing configuration
        public const float BaseFrequency = 0.8f;      // Base movement frequency for natural timing
        public const float DebugLogInterval = 2f;      // How often to log heuristic debug info
        
        // Joint-specific movement amplitudes and timing
        public static class HipJoints
        {
            public const float Amplitude = 0.8f;
            public const float FrequencyMultiplier = 1.0f;
            public const float PhaseOffset = 0f; // Left hip base phase
            public const float OppositePhaseOffset = Mathf.PI; // Right hip phase
        }
        
        public static class KneeJoints  
        {
            public const float Amplitude = 0.6f;
            public const float FrequencyMultiplier = 2.0f;
            public const float PhaseOffset = Mathf.PI / 4f;
        }
        
        public static class AnkleJoints
        {
            public const float Amplitude = 0.4f;
            public const float FrequencyMultiplier = 1.5f;
            public const float PhaseOffset = Mathf.PI / 2f;
        }
        
        public static class OtherJoints
        {
            public const float Amplitude = 0.3f;
            public const float FrequencyMultiplier = 0.5f;
            public const float PhaseMultiplier = 0.5f; // Phase spread per joint index
        }
        
        /// <summary>
        /// Get heuristic movement parameters for a specific joint
        /// </summary>
        public static (float amplitude, float frequencyMult, float basePhase) GetJointHeuristicConfig(string jointName, int jointIndex)
        {
            // Check for hip/upper leg joints
            if (RagdollJointNames.JointNameContains(jointName, "UpperLeg", "Hip"))
            {
                float phase = RagdollJointNames.JointNameContains(jointName, "Left") ? 
                    HipJoints.PhaseOffset : HipJoints.OppositePhaseOffset;
                return (HipJoints.Amplitude, HipJoints.FrequencyMultiplier, phase);
            }
            
            // Check for knee/lower leg joints
            if (RagdollJointNames.JointNameContains(jointName, "LowerLeg", "Knee"))
            {
                float phase = RagdollJointNames.JointNameContains(jointName, "Left") ? 
                    KneeJoints.PhaseOffset : (KneeJoints.PhaseOffset + Mathf.PI);
                return (KneeJoints.Amplitude, KneeJoints.FrequencyMultiplier, phase);
            }
            
            // Check for ankle/foot joints
            if (RagdollJointNames.JointNameContains(jointName, "Foot", "Ankle"))
            {
                float phase = RagdollJointNames.JointNameContains(jointName, "Left") ? 
                    AnkleJoints.PhaseOffset : (AnkleJoints.PhaseOffset + Mathf.PI);
                return (AnkleJoints.Amplitude, AnkleJoints.FrequencyMultiplier, phase);
            }
            
            // Default for other joints (arms, torso, etc.)
            return (OtherJoints.Amplitude, OtherJoints.FrequencyMultiplier, jointIndex * OtherJoints.PhaseMultiplier);
        }
    }

    /// <summary>
    /// Configuration for agent visual components and features
    /// </summary>
    [System.Serializable]
    public static class AgentVisuals
    {
        /// <summary>
        /// Eye positioning and scale configuration for different agent types
        /// </summary>
        public static class EyeConfiguration
        {
            /// <summary>
            /// Eye configuration for CubeAgent
            /// </summary>
            public static class Cube
            {
                public static readonly Vector3 LeftEyePosition = new Vector3(-0.2f, 0.2f, 0.51f);
                public static readonly Vector3 RightEyePosition = new Vector3(0.2f, 0.2f, 0.51f);
                public static readonly Vector3 EyeScale = Vector3.one * 0.15f;
            }
            
            /// <summary>
            /// Eye configuration for RagdollAgent (positioned on head visual)
            /// </summary>
            public static class Ragdoll
            {
                public static readonly Vector3 LeftEyePosition = new Vector3(-0.15f, 0.1f, 0.45f);
                public static readonly Vector3 RightEyePosition = new Vector3(0.15f, 0.1f, 0.45f);
                public static readonly Vector3 EyeScale = Vector3.one * 0.12f;
            }
        }
        
        /// <summary>
        /// Ragdoll structure navigation paths
        /// </summary>
        public static class RagdollStructure
        {
            /// <summary>
            /// Path to head visual object from pelvis: Pelvis -> Chest -> Head -> Visual
            /// </summary>
            public static readonly string[] HeadVisualPath = { RagdollJointNames.Chest, RagdollJointNames.Head, "Visual" };
            
            /// <summary>
            /// Get head visual object from pelvis using configured path
            /// </summary>
            public static GameObject FindHeadVisual(Transform pelvis)
            {
                Transform current = pelvis;
                
                foreach (string pathComponent in HeadVisualPath)
                {
                    current = current.Find(pathComponent);
                    if (current == null)
                    {
                        TrainArenaDebugManager.LogWarning($"⚠️ Could not find '{pathComponent}' in ragdoll structure path");
                        return null;
                    }
                }
                
                return current.gameObject;
            }
        }
    }
}