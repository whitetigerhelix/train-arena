using UnityEngine;

namespace TrainArena.Configuration
{
    /// <summary>
    /// Configuration for scene building and camera positioning
    /// </summary>
    [System.Serializable]
    public static class SceneConfiguration
    {
        /// <summary>
        /// Camera configuration for different scene types
        /// </summary>
        public static class Camera
        {
            // Shared camera settings
            public const float AspectRatio = 16f / 9f;
            public const float DefaultFieldOfView = 60f;
            public const float RagdollFieldOfView = 50f; // Narrower for better detail
            
            /// <summary>
            /// Camera positions for cube agent scenes (4x4 grid, center at 30,0,30)
            /// </summary>
            public static class CubeAgent
            {
                public static readonly Vector3 Position = new Vector3(30, 50, -15);
                public static readonly Vector3 LookAtTarget = new Vector3(30, 0, 30);
            }
            
            /// <summary>
            /// Camera positions for ragdoll agent scenes (2x2 grid, center at 4,0,4)
            /// </summary>
            public static class RagdollAgent
            {
                public static readonly Vector3 Position = new Vector3(4, 12, -8);
                public static readonly Vector3 LookAtTarget = new Vector3(4, 0, 4);
            }
        }
        
        /// <summary>
        /// Lighting configuration for scenes
        /// </summary>
        public static class Lighting
        {
            // Main directional light settings
            public static readonly Vector3 DirectionalLightRotation = new Vector3(50f, -30f, 0f);
            public const float DirectionalLightIntensity = 1.0f;
            public static readonly Color DirectionalLightColor = Color.white;
            
            // Ambient lighting settings  
            public const float AmbientIntensity = 0.3f;
            public static readonly Color AmbientColor = new Color(0.2f, 0.2f, 0.3f);
        }
        
        /// <summary>
        /// Agent prefab naming configuration
        /// </summary>
        public static class AgentPrefabs
        {
            public const string CubeAgentPrefabName = "CubeAgent";
            public const string RagdollAgentPrefabName = "RagdollAgent";
            public const string GoalPrefabName = "Goal";
        }
        
        /// <summary>
        /// Scene layout configuration
        /// </summary>
        public static class Layout
        {
            /// <summary>
            /// Grid dimensions for different scene types
            /// </summary>
            public static class GridDimensions
            {
                // Cube agent grids
                public const int CubeTrainingX = 4;
                public const int CubeTrainingZ = 4;
                public const int CubeTestingX = 2;  
                public const int CubeTestingZ = 2;
                
                // Ragdoll agent grids
                public const int RagdollTrainingX = 2;
                public const int RagdollTrainingZ = 2;
                public const int RagdollTestingX = 1;
                public const int RagdollTestingZ = 1;
            }
        }
    }
}