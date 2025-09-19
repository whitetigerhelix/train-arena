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
            switch (jointName)
            {
                case LeftUpperLeg:
                case RightUpperLeg:
                    return (-45f, 45f, 80f, 8f);  // Hip joints
                    
                case LeftLowerLeg:
                case RightLowerLeg:
                    return (-90f, 90f, 80f, 8f);  // Knee joints
                    
                case LeftFoot:
                case RightFoot:
                    return (-30f, 30f, 80f, 8f);  // Ankle joints
                    
                default:
                    return (0f, 0f, 80f, 8f);     // Default values
            }
        }
    }
}