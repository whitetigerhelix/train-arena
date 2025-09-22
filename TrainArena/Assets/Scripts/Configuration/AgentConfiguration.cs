using UnityEngine;

namespace TrainArena.Configuration
{
    //TODO: We need to roll the actual configuration classes into this AgentConfiguration file

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
    /// Configuration for reward calculation coefficients and thresholds
    /// </summary>
    [System.Serializable]
    public static class RewardConfig
    {
        // Reward coefficients
        public const float VelocityRewardCoeff = 0.03f;      // Reward for moving toward target
        public const float UprightRewardCoeff = 0.02f;      // Reward for staying upright
        public const float EnergyPenaltyCoeff = 0.001f;     // Penalty for excessive energy use
        public const float AngularVelocityPenaltyCoeff = 0.001f; // Penalty for excessive rotation
            
        // Reward thresholds
        public const float UprightThreshold = 0.8f;         // Uprightness threshold for balance reward
        public const float UprightBonus = 0.5f;             // Bonus for maintaining uprightness
        public const float TargetSpeed = 1.0f;              // Default target speed for movement
            
        // Episode configuration
        public const float EpisodeGracePeriod = 8.0f;       // Grace period before termination checks
        public const float MaxEpisodeDuration = 30.0f;      // Maximum episode length
        public const float MinUprightness = 0.1f;           // Minimum uprightness before termination
        public const float MinHeight = -0.5f;               // Minimum height before termination
            
        // Heuristic movement configuration
        public const float HeuristicFrequencyDefault = 0f;  // Default heuristic frequency (0 = use config default)
        public const float HeuristicAmplitudeDefault = 1.0f; // Default heuristic amplitude multiplier
    }

    /// <summary>
    /// Configuration for ragdoll T-pose and joint reset behavior
    /// </summary>
    [System.Serializable]
    public static class RagdollTPoseConfig
    {
        // Joint drive settings for natural movement (differentiated by joint type)
        // Default values from reference (natural body movement)
        public const float DefaultSpringForce = 125f;       // Default spring force for joint drives
        public const float DefaultDampingForce = 0.5f;      // Default damping force for joint drives
        
        // Head-specific settings (higher damping to prevent vibration)
        public const float HeadSpringForce = 100f;          // Lower spring force for head joint
        public const float HeadDampingForce = 5.0f;         // Much higher damping to prevent rapid vibration
        
        // Arm-specific settings (softer springs for natural movement)
        public const float ArmSpringForce = 80f;            // Softer spring force for more natural arm movement
        public const float ArmDampingForce = 1.2f;          // Higher damping to prevent excessive flailing
        
        // Leg-specific settings (stronger for locomotion)
        public const float LegSpringForce = 150f;           // Higher spring force for weight-bearing legs
        public const float LegDampingForce = 1.0f;          // Balanced damping for locomotion
        
        // Physics settings
        public const float MinimumRigidbodyMass = 1f;       // Minimum mass for joint rigidbodies
        
        // Episode reset positioning
        public const float SpawnElevationOffset = 0f;       // Elevation offset to prevent ground clipping during spawn
        
        /// <summary>
        /// Get joint-specific spring and damping values for natural movement
        /// </summary>
        public static (float spring, float damping) GetJointDriveSettings(string jointName)
        {
            // Head joints need higher damping to prevent vibration
            if (jointName == RagdollJointNames.Head)
            {
                return (HeadSpringForce, HeadDampingForce);
            }
            // Arm joints need moderate settings for natural extension
            else if (jointName == RagdollJointNames.LeftUpperArm || jointName == RagdollJointNames.RightUpperArm ||
                     jointName == RagdollJointNames.LeftLowerArm || jointName == RagdollJointNames.RightLowerArm)
            {
                return (ArmSpringForce, ArmDampingForce);
            }
            // Leg joints need stronger settings for locomotion
            else if (jointName == RagdollJointNames.LeftUpperLeg || jointName == RagdollJointNames.RightUpperLeg ||
                     jointName == RagdollJointNames.LeftLowerLeg || jointName == RagdollJointNames.RightLowerLeg ||
                     jointName == RagdollJointNames.LeftFoot || jointName == RagdollJointNames.RightFoot)
            {
                return (LegSpringForce, LegDampingForce);
            }
            // Default settings for chest and other joints
            else
            {
                return (DefaultSpringForce, DefaultDampingForce);
            }
        }
        
        /// <summary>
        /// Get T-pose target rotation for a specific joint (adjusted for natural arm extension)
        /// </summary>
        public static Quaternion GetTPoseRotation(string jointName)
        {
            // Upper arms should extend outward horizontally (T-pose) - more conservative approach
            if (jointName == RagdollJointNames.LeftUpperArm)
            {
                return Quaternion.Euler(0, 0, 90); // Left arm horizontal (simple)
            }
            else if (jointName == RagdollJointNames.RightUpperArm)
            {
                return Quaternion.Euler(0, 0, -90); // Right arm horizontal (simple)
            }
            // Lower arms should extend outward to prevent folding
            else if (jointName == RagdollJointNames.LeftLowerArm)
            {
                return Quaternion.Euler(0, 0, -30); // Extend outward from left upper arm
            }
            else if (jointName == RagdollJointNames.RightLowerArm)
            {
                return Quaternion.Euler(0, 0, 30); // Extend outward from right upper arm
            }
            // Legs should be straight down
            else if (jointName == RagdollJointNames.LeftUpperLeg || jointName == RagdollJointNames.RightUpperLeg ||
                     jointName == RagdollJointNames.LeftLowerLeg || jointName == RagdollJointNames.RightLowerLeg)
            {
                return Quaternion.identity; // Straight down
            }
            // Feet should be level
            else if (jointName == RagdollJointNames.LeftFoot || jointName == RagdollJointNames.RightFoot)
            {
                return Quaternion.Euler(-90, 0, 0); // Feet flat/level
            }
            // Head and chest upright
            else if (jointName == RagdollJointNames.Head || jointName == RagdollJointNames.Chest)
            {
                return Quaternion.identity; // Upright
            }
            
            // Default to neutral rotation
            return Quaternion.identity;
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