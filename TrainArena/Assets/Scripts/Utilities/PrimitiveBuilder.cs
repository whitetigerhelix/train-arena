using System.Linq;
using Unity.MLAgents.Policies;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Centralized primitive and material creation system for TrainArena.
/// Ensures consistent URP materials and visual quality across all objects.
/// Eliminates code duplication between SceneBuilder, EnvInitializer, and other builders.
/// </summary>
public static class PrimitiveBuilder
{
    /// <summary>
    /// LEGACY - Old ragdoll configuration class (use ReferenceRagdollConfiguration instead)
    /// </summary>
    [System.Serializable]
    public class RagdollConfiguration
    {
        // Legacy properties for backward compatibility
        public Vector3 pelvisScale = new Vector3(0.5f, 0.4f, 0.3f);
        public Vector3 thighScale = new Vector3(0.2f, 0.6f, 0.2f);
        public Vector3 shinScale = new Vector3(0.15f, 0.5f, 0.15f);
        public Vector3 footScale = new Vector3(0.3f, 0.15f, 0.5f);
        
        public float pelvisMass = 24.4f;
        public float thighMass = 11.0f;
        public float shinMass = 4.4f;
        public float footMass = 1.1f;
        
        public float dynamicFriction = 0.5f;
        public float staticFriction = 0.5f;
        public float bounciness = 0.2f;
        
        // Physics settings
        public float linearDamping = 0.1f;
        public float angularDamping = 0.1f;
        public RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate;
        
        // Joint properties
        public float jointBounciness = 0f;
        public float jointSpring = 125f;
        public float jointDamper = 0.5f;
        public float jointMaxForce = 3.4e+38f;
        
        // PD Controller properties
        public float pdKp = 400f;
        public float pdKd = 30f;
        
        /// <summary>
        /// Configuration for realistic human proportions
        /// </summary>
        public static RagdollConfiguration RealisticHuman()
        {
            var config = new RagdollConfiguration();
            // Use the base functional proportions - they're already realistic
            // Just fine-tune slightly
            config.pelvisScale = new Vector3(0.45f, 0.28f, 0.28f);         // Slightly smaller pelvis
            config.thighScale = new Vector3(0.20f, 0.75f, 0.20f);          // Proportional thigh
            config.shinScale = new Vector3(0.16f, 0.65f, 0.16f);           // Proportional shin
            config.footScale = new Vector3(0.22f, 0.08f, 0.4f);            // Realistic flat foot
            return config;
        }
    }

    /// <summary>
    /// Configurable parameters for ragdoll creation and AI training
    /// </summary>
    [System.Serializable]
    public class ReferenceRagdollConfiguration
    {
        // EXACT MASS DISTRIBUTION FROM REFERENCE (85kg total)
        public float headMass = 6.6f;           // Head: 6.6kg
        public float chestMass = 18.1f;         // Chest: 18.1kg  
        public float pelvisMass = 24.4f;        // Pelvis: 24.4kg
        public float upperArmMass = 3.3f;       // Upper arms: 3.3kg each
        public float lowerArmMass = 2.2f;       // Lower arms: 2.2kg each
        public float upperLegMass = 11.0f;      // Upper legs: 11.0kg each
        public float lowerLegMass = 4.4f;       // Lower legs: 4.4kg each
        public float footMass = 1.1f;           // Feet: 1.1kg each

        // EXACT PHYSICS SETTINGS FROM REFERENCE
        public float jointSpring = 125f;        // Spring: 125
        public float jointDamper = 0.5f;        // Damper: 0.5
        public float jointMaxForce = 3.4e+38f;  // Max Force: 3.4e+38
        public float jointBounciness = 0f;      // No bounciness for stability

        // EXACT BODY PART SCALES FROM REFERENCE - Matching the proportions in the reference images
        public Vector3 headScale = new Vector3(0.25f, 0.25f, 0.25f);           // Small head
        public Vector3 chestScale = new Vector3(0.4f, 0.5f, 0.25f);            // Medium chest  
        public Vector3 pelvisScale = new Vector3(0.35f, 0.3f, 0.2f);           // Smaller pelvis - was too big
        public Vector3 upperArmScale = new Vector3(0.1f, 0.8f, 0.1f);          // Long thin arms
        public Vector3 lowerArmScale = new Vector3(0.08f, 0.7f, 0.08f);        // Long thin forearms
        public Vector3 upperLegScale = new Vector3(0.15f, 0.9f, 0.15f);        // Long thighs
        public Vector3 lowerLegScale = new Vector3(0.12f, 0.85f, 0.12f);       // Long shins
        public Vector3 footScale = new Vector3(0.2f, 0.15f, 0.35f);

        // Physics settings
        public float linearDamping = 0.1f;
        public RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate;
        public float angularDamping = 0.1f;
        
        // Physics material properties
        public float friction = 0.5f;
        public float bounciness = 0.2f;
        
        // Joint limits for legacy methods
        public float hipJointLimit = 90f;
        public float kneeJointLimit = 120f;
        public float ankleJointLimit = 45f;
        
        // PD Controller settings for legacy methods
        public float pdKp = 400f;
        public float pdKd = 30f;
        
        // Anatomical connection settings for legacy methods
        public float hipSocketDepth = 0.08f;
        public float kneeConnectionHeight = 0.15f;
        public float ankleConnectionHeight = 0.12f;
        
        // Legacy property aliases for backward compatibility
        public float dynamicFriction => friction;
        public float staticFriction => friction;
        public float thighMass => upperLegMass;
        public float shinMass => lowerLegMass;
        public Vector3 thighScale => upperLegScale;
        public Vector3 shinScale => lowerLegScale;

        /// <summary>
        /// Creates exact reference configuration matching the reference images
        /// </summary>
        public static ReferenceRagdollConfiguration ReferenceExact()
        {
            return new ReferenceRagdollConfiguration();
        }
        
        /// <summary>
        /// Configuration optimized for AI training
        /// </summary>
        public static ReferenceRagdollConfiguration AITraining()
        {
            return new ReferenceRagdollConfiguration();
        }
        
        /// <summary>
        /// Configuration optimized for stable walking
        /// </summary>
        public static ReferenceRagdollConfiguration FunctionalWalker()
        {
            var config = new ReferenceRagdollConfiguration();
            config.pelvisScale = new Vector3(0.5f, 0.3f, 0.3f);
            config.upperLegScale = new Vector3(0.25f, 0.7f, 0.25f);
            config.lowerLegScale = new Vector3(0.2f, 0.65f, 0.2f);
            config.footScale = new Vector3(0.3f, 0.08f, 0.5f);
            return config;
        }
        
        /// <summary>
        /// Configuration matching inspiration images
        /// </summary>
        public static ReferenceRagdollConfiguration InspirationMatch()
        {
            var config = new ReferenceRagdollConfiguration();
            config.pelvisScale = new Vector3(0.4f, 0.25f, 0.25f);
            config.upperLegScale = new Vector3(0.18f, 0.9f, 0.18f);
            config.lowerLegScale = new Vector3(0.15f, 0.8f, 0.15f);
            config.footScale = new Vector3(0.2f, 0.06f, 0.35f);
            return config;
        }
        
        /// <summary>
        /// Configuration for athletic performance
        /// </summary>
        public static ReferenceRagdollConfiguration Athletic()
        {
            return new ReferenceRagdollConfiguration();
        }
        
        /// <summary>
        /// Configuration for realistic human proportions
        /// </summary>
        public static ReferenceRagdollConfiguration RealisticHuman()
        {
            var config = new ReferenceRagdollConfiguration();
            config.pelvisScale = new Vector3(0.45f, 0.28f, 0.28f);
            config.upperLegScale = new Vector3(0.20f, 0.75f, 0.20f);
            config.lowerLegScale = new Vector3(0.16f, 0.65f, 0.16f);
            config.footScale = new Vector3(0.22f, 0.08f, 0.4f);
            config.hipJointLimit = 65f;
            config.kneeJointLimit = 110f;
            config.ankleJointLimit = 25f;
            config.pdKp = 350f;
            config.pdKd = 25f;
            return config;
        }
        
        /// <summary>
        /// Anatomically correct human proportions
        /// </summary>
        public static ReferenceRagdollConfiguration AnatomicalHuman()
        {
            var config = new ReferenceRagdollConfiguration();
            config.pelvisScale = new Vector3(0.35f, 0.25f, 0.25f);
            config.upperLegScale = new Vector3(0.15f, 0.45f, 0.15f);
            config.lowerLegScale = new Vector3(0.12f, 0.40f, 0.12f);
            config.footScale = new Vector3(0.26f, 0.06f, 0.10f);
            config.pdKd = 35f;
            return config;
        }
    }

    /// <summary>
    /// Current active configuration for ragdoll creation
    /// </summary>
    //public static ReferenceRagdollConfiguration CurrentConfig { get; set; }

    /// <summary>
    /// Creates ragdoll using the stable BlockmanRagdollBuilder system
    /// This is the main entry point for ragdoll creation - delegates to BlockmanRagdollBuilder
    /// </summary>
    public static GameObject CreateRagdoll(string name = "ReferenceRagdoll", Vector3 position = default)
    {
        TrainArenaDebugManager.Log($"🎯 Creating ragdoll using BlockmanRagdollBuilder system", TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Use BlockmanRagdollBuilder for consistent, stable ragdoll creation
        var ragdoll = BlockmanRagdollBuilder.Build(position, new BlockmanRagdollBuilder.Cfg());
        ragdoll.name = name;
        
        // Add ML-Agents components if not already present
        AddMLAgentsToBlockman(ragdoll);
        
        TrainArenaDebugManager.Log($"✅ Ragdoll created using BlockmanRagdollBuilder: {name}", TrainArenaDebugManager.DebugLogLevel.Important);
        return ragdoll;
    }
    
    /// <summary>
    /// Adds ML-Agents components to a blockman ragdoll if they're not already present
    /// </summary>
    private static void AddMLAgentsToBlockman(GameObject ragdoll)
    {
        var pelvis = ragdoll.transform.Find("Pelvis")?.gameObject;
        if (pelvis == null)
        {
            TrainArenaDebugManager.LogError("No Pelvis found in blockman ragdoll - ML-Agents components not added");
            return;
        }
        
        // Add RagdollAgent if not present
        if (pelvis.GetComponent<RagdollAgent>() == null)
        {
            var ragdollAgent = pelvis.AddComponent<RagdollAgent>();
            ragdollAgent.pelvis = pelvis.transform;
            
            // Find leg joints only for ML-Agents control (6 joints for locomotion)
            var joints = new List<PDJointController>();
            var configJoints = ragdoll.GetComponentsInChildren<ConfigurableJoint>();
            
            foreach (var configJoint in configJoints)
            {
                var partName = configJoint.gameObject.name.ToLower();
                
                // Only add PDJointController to leg joints for locomotion
                bool isLegJoint = partName.Contains("upperleg") || partName.Contains("lowerleg") || partName.Contains("foot");
                
                if (isLegJoint)
                {
                    // Add PDJointController if not present
                    var pdController = configJoint.GetComponent<PDJointController>();
                    if (pdController == null)
                    {
                        pdController = configJoint.gameObject.AddComponent<PDJointController>();
                        pdController.joint = configJoint;
                        pdController.kp = 80f;
                        pdController.kd = 8f;
                        
                        // Set reasonable angle limits based on body part
                        if (partName.Contains("upperleg"))
                        {
                            pdController.minAngle = -45f;
                            pdController.maxAngle = 45f;
                        }
                        else if (partName.Contains("lowerleg"))
                        {
                            pdController.minAngle = -90f;
                            pdController.maxAngle = 90f;
                        }
                        else if (partName.Contains("foot"))
                        {
                            pdController.minAngle = -30f;
                            pdController.maxAngle = 30f;
                        }
                    }
                    
                    joints.Add(pdController);
                }
            }
            
            ragdollAgent.joints = joints;
            TrainArenaDebugManager.Log($"Added {joints.Count} PDJointControllers to RagdollAgent", TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Add BehaviorParameters if not present
        if (pelvis.GetComponent<BehaviorParameters>() == null)
        {
            var behaviorParameters = pelvis.AddComponent<BehaviorParameters>();
            behaviorParameters.BehaviorName = "RagdollAgent";
            
            // Use 6 continuous actions for leg joints (hips, knees, ankles)
            behaviorParameters.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(6);
            
            TrainArenaDebugManager.Log($"Added BehaviorParameters with {jointCount} continuous actions", TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Add AutoBehaviorSwitcher if not present
        if (pelvis.GetComponent<AutoBehaviorSwitcher>() == null)
        {
            pelvis.AddComponent<AutoBehaviorSwitcher>();
        }
        
        // Add blinking eyes to head if not already present
        var head = ragdoll.transform.Find("Head")?.gameObject;
        if (head != null && head.GetComponent<EyeBlinkAnimator>() == null)
        {
            AddBlinkingEyes(head);
        }
    }

    /// <summary>
    /// Creates a simple body part with material and physics, no complex joints
    /// </summary>
    private static GameObject CreateSimpleBodyPart(string name, Vector3 localPosition, Vector3 size, PrimitiveType primitiveType, Transform parent, float mass, Color color)
    {
        var bodyPart = GameObject.CreatePrimitive(primitiveType);
        bodyPart.name = name;
        bodyPart.transform.parent = parent;
        bodyPart.transform.localPosition = localPosition;
        bodyPart.transform.localScale = size;
        
        // Apply material like CubeAgent
        var mr = bodyPart.GetComponent<Renderer>();
        Material mat = CreateURPMaterial(smoothness: 0.6f, metallic: 0.1f);
        mat.color = color;
        mat.name = $"{name}Material";
        mr.sharedMaterial = mat;
        
        // Add simple physics
        var rb = bodyPart.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.1f;
        
        return bodyPart;
    }

    /// <summary>
    /// Adds a simple ConfigurableJoint with PDJointController for ML-Agents control
    /// </summary>
    private static void AddSimpleJoint(GameObject child, GameObject parent, string jointName)
    {
        // Add ConfigurableJoint
        var joint = child.AddComponent<ConfigurableJoint>();
        joint.connectedBody = parent.GetComponent<Rigidbody>();
        
        // Simple joint configuration - limit to angular motion only
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked; 
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;
        
        // Set reasonable joint limits (60 degrees in each direction)
        joint.lowAngularXLimit = new SoftJointLimit { limit = -60f };
        joint.highAngularXLimit = new SoftJointLimit { limit = 60f };
        joint.angularYLimit = new SoftJointLimit { limit = 60f };
        joint.angularZLimit = new SoftJointLimit { limit = 60f };
        
        // Add PDJointController for ML-Agents
        var pdController = child.AddComponent<PDJointController>();
        pdController.joint = joint;
        pdController.kp = 80f; // Moderate stiffness
        pdController.kd = 8f;  // Good damping
        
        TrainArenaDebugManager.Log($"🔗 Added joint: {jointName} ({child.name} → {parent.name})", TrainArenaDebugManager.DebugLogLevel.Verbose);
    }

    /// <summary>
    /// LEGACY - DEPRECATED - Use BlockmanRagdollBuilder.Build() instead
    /// This method has been replaced by the more stable BlockmanRagdollBuilder system
    /// </summary>
    [System.Obsolete("Use BlockmanRagdollBuilder.Build() instead", true)]
    public static GameObject CreateSimpleRagdoll(string name, Vector3 position, ReferenceRagdollConfiguration config)
    {
        TrainArenaDebugManager.Log($"🤖 Creating simple stable ragdoll: {name}", TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Create root object
        var root = new GameObject(name);
        root.transform.position = position;
        
        // Create a simple humanoid that looks like your reference
        // Pelvis (foundation)
        var pelvis = CreateSimpleBodyPart("Pelvis", Vector3.zero, config.pelvisScale, PrimitiveType.Cube, root.transform, config.pelvisMass, new Color(0.9f, 0.7f, 0.5f));
        
        // Chest above pelvis
        var chestPos = new Vector3(0, config.pelvisScale.y * 0.5f + config.chestScale.y * 0.5f, 0);
        var chest = CreateSimpleBodyPart("Chest", chestPos, config.chestScale, PrimitiveType.Cube, pelvis.transform, config.chestMass, new Color(0.9f, 0.7f, 0.5f));
        
        // Head on chest with eyes
        var headPos = new Vector3(0, config.chestScale.y * 0.5f + config.headScale.y * 0.5f, 0);
        var head = CreateSimpleBodyPart("Head", headPos, config.headScale, PrimitiveType.Sphere, chest.transform, config.headMass, new Color(1.0f, 0.8f, 0.6f));
        AddBlinkingEyes(head);
        
        // Left arm - horizontal from chest
        var leftShoulderPos = new Vector3(-config.chestScale.x * 0.5f - config.upperArmScale.y * 0.5f, config.chestScale.y * 0.25f, 0);
        var leftUpperArm = CreateSimpleBodyPart("LeftUpperArm", leftShoulderPos, config.upperArmScale, PrimitiveType.Capsule, chest.transform, config.upperArmMass, new Color(0.8f, 0.6f, 0.4f));
        leftUpperArm.transform.localRotation = Quaternion.Euler(0, 0, 90);
        
        var leftElbowPos = new Vector3(-config.upperArmScale.y * 0.5f - config.lowerArmScale.y * 0.5f, 0, 0);
        var leftLowerArm = CreateSimpleBodyPart("LeftLowerArm", leftElbowPos, config.lowerArmScale, PrimitiveType.Capsule, leftUpperArm.transform, config.lowerArmMass, new Color(0.8f, 0.6f, 0.4f));
        
        // Right arm - horizontal from chest
        var rightShoulderPos = new Vector3(config.chestScale.x * 0.5f + config.upperArmScale.y * 0.5f, config.chestScale.y * 0.25f, 0);
        var rightUpperArm = CreateSimpleBodyPart("RightUpperArm", rightShoulderPos, config.upperArmScale, PrimitiveType.Capsule, chest.transform, config.upperArmMass, new Color(0.8f, 0.6f, 0.4f));
        rightUpperArm.transform.localRotation = Quaternion.Euler(0, 0, -90);
        
        var rightElbowPos = new Vector3(config.upperArmScale.y * 0.5f + config.lowerArmScale.y * 0.5f, 0, 0);
        var rightLowerArm = CreateSimpleBodyPart("RightLowerArm", rightElbowPos, config.lowerArmScale, PrimitiveType.Capsule, rightUpperArm.transform, config.lowerArmMass, new Color(0.8f, 0.6f, 0.4f));
        
        // Left leg - vertical from pelvis
        var leftHipPos = new Vector3(-config.pelvisScale.x * 0.25f, -config.pelvisScale.y * 0.5f - config.upperLegScale.y * 0.5f, 0);
        var leftUpperLeg = CreateSimpleBodyPart("LeftUpperLeg", leftHipPos, config.upperLegScale, PrimitiveType.Capsule, pelvis.transform, config.upperLegMass, new Color(0.8f, 0.6f, 0.4f));
        
        var leftKneePos = new Vector3(0, -config.upperLegScale.y * 0.5f - config.lowerLegScale.y * 0.5f, 0);
        var leftLowerLeg = CreateSimpleBodyPart("LeftLowerLeg", leftKneePos, config.lowerLegScale, PrimitiveType.Capsule, leftUpperLeg.transform, config.lowerLegMass, new Color(0.8f, 0.6f, 0.4f));
        
        var leftAnklePos = new Vector3(0, -config.lowerLegScale.y * 0.5f - config.footScale.y * 0.5f, config.footScale.z * 0.25f);
        var leftFoot = CreateSimpleBodyPart("LeftFoot", leftAnklePos, config.footScale, PrimitiveType.Sphere, leftLowerLeg.transform, config.footMass, new Color(0.7f, 0.5f, 0.3f));
        
        // Right leg - vertical from pelvis  
        var rightHipPos = new Vector3(config.pelvisScale.x * 0.25f, -config.pelvisScale.y * 0.5f - config.upperLegScale.y * 0.5f, 0);
        var rightUpperLeg = CreateSimpleBodyPart("RightUpperLeg", rightHipPos, config.upperLegScale, PrimitiveType.Capsule, pelvis.transform, config.upperLegMass, new Color(0.8f, 0.6f, 0.4f));
        
        var rightKneePos = new Vector3(0, -config.upperLegScale.y * 0.5f - config.lowerLegScale.y * 0.5f, 0);
        var rightLowerLeg = CreateSimpleBodyPart("RightLowerLeg", rightKneePos, config.lowerLegScale, PrimitiveType.Capsule, rightUpperLeg.transform, config.lowerLegMass, new Color(0.8f, 0.6f, 0.4f));
        
        var rightAnklePos = new Vector3(0, -config.lowerLegScale.y * 0.5f - config.footScale.y * 0.5f, config.footScale.z * 0.25f);
        var rightFoot = CreateSimpleBodyPart("RightFoot", rightAnklePos, config.footScale, PrimitiveType.Sphere, rightLowerLeg.transform, config.footMass, new Color(0.7f, 0.5f, 0.3f));
        
        // Add ConfigurableJoints for ML-Agents control (6 joints: hips, knees, ankles)
        AddSimpleJoint(leftUpperLeg, pelvis, "LeftHip");
        AddSimpleJoint(rightUpperLeg, pelvis, "RightHip"); 
        AddSimpleJoint(leftLowerLeg, leftUpperLeg, "LeftKnee");
        AddSimpleJoint(rightLowerLeg, rightUpperLeg, "RightKnee");
        AddSimpleJoint(leftFoot, leftLowerLeg, "LeftAnkle");
        AddSimpleJoint(rightFoot, rightLowerLeg, "RightAnkle");
        
        // Add ML-Agents components on the pelvis (root body part)
        var ragdollAgent = pelvis.AddComponent<RagdollAgent>();
        var behaviorParameters = pelvis.AddComponent<BehaviorParameters>();
        behaviorParameters.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(6); // 6 joints
        behaviorParameters.BehaviorName = "RagdollAgent";
        
        // Configure RagdollAgent with joint references
        ragdollAgent.pelvis = pelvis.transform;
        ragdollAgent.joints = new List<PDJointController>
        {
            leftUpperLeg.GetComponent<PDJointController>(),
            rightUpperLeg.GetComponent<PDJointController>(),
            leftLowerLeg.GetComponent<PDJointController>(),
            rightLowerLeg.GetComponent<PDJointController>(),
            leftFoot.GetComponent<PDJointController>(),
            rightFoot.GetComponent<PDJointController>()
        };
        
        // Add AutoBehaviorSwitcher for seamless training/testing mode switching
        pelvis.AddComponent<AutoBehaviorSwitcher>();
        
        TrainArenaDebugManager.Log($"✅ Blockman ragdoll created: 13 bones, 6 joints, ML-Agents ready for training!", TrainArenaDebugManager.DebugLogLevel.Important);
        
        return root;
    }

    /// <summary>
    /// LEGACY METHOD - DEPRECATED - Use BlockmanRagdollBuilder.Build() instead
    /// This method has been replaced by the more stable BlockmanRagdollBuilder system
    /// </summary>
    [System.Obsolete("Use BlockmanRagdollBuilder.Build() instead", true)]
    public static GameObject CreateReferenceRagdoll(string name, Vector3 position, ReferenceRagdollConfiguration config)
    {
        TrainArenaDebugManager.Log($"🚀 BUILDING REFERENCE EXACT RAGDOLL: {name}", TrainArenaDebugManager.DebugLogLevel.Important);

        var root = new GameObject(name);
        root.transform.position = position;
        
        // BONE 1: PELVIS (root of hierarchy, center of mass)
        var pelvis = CreateReferenceBodyPart("Pelvis", Vector3.zero, config.pelvisScale, PrimitiveType.Cube, root.transform, config.pelvisMass, config);
        
        // BONE 2: CHEST (connected to pelvis)
        Vector3 chestPos = new Vector3(0, config.pelvisScale.y * 0.5f + config.chestScale.y * 0.5f, 0);
        var chest = CreateReferenceBodyPart("Chest", chestPos, config.chestScale, PrimitiveType.Cube, pelvis.transform, config.chestMass, config);
        
        // BONE 3: HEAD (connected to chest)
        Vector3 headPos = new Vector3(0, config.chestScale.y * 0.5f + config.headScale.y * 0.5f, 0);
        var head = CreateReferenceBodyPart("Head", headPos, config.headScale, PrimitiveType.Sphere, chest.transform, config.headMass, config);
        
        // BONES 4-5: LEFT ARM (connected to chest) - Extend horizontally from chest
        Vector3 leftShoulderPos = new Vector3(-config.chestScale.x * 0.5f - config.upperArmScale.y * 0.5f, config.chestScale.y * 0.25f, 0);
        var leftUpperArm = CreateReferenceBodyPart("LeftUpperArm", leftShoulderPos, config.upperArmScale, PrimitiveType.Capsule, chest.transform, config.upperArmMass, config);
        // Rotate upper arm to point outward horizontally
        leftUpperArm.transform.localRotation = Quaternion.Euler(0, 0, 90);
        
        Vector3 leftElbowPos = new Vector3(-config.upperArmScale.y * 0.5f - config.lowerArmScale.y * 0.5f, 0, 0);
        var leftLowerArm = CreateReferenceBodyPart("LeftLowerArm", leftElbowPos, config.lowerArmScale, PrimitiveType.Capsule, leftUpperArm.transform, config.lowerArmMass, config);
        // Keep forearm horizontal
        leftLowerArm.transform.localRotation = Quaternion.Euler(0, 0, 0);
        
        // BONES 6-7: RIGHT ARM (connected to chest) - Extend horizontally from chest
        Vector3 rightShoulderPos = new Vector3(config.chestScale.x * 0.5f + config.upperArmScale.y * 0.5f, config.chestScale.y * 0.25f, 0);
        var rightUpperArm = CreateReferenceBodyPart("RightUpperArm", rightShoulderPos, config.upperArmScale, PrimitiveType.Capsule, chest.transform, config.upperArmMass, config);
        // Rotate upper arm to point outward horizontally
        rightUpperArm.transform.localRotation = Quaternion.Euler(0, 0, -90);
        
        Vector3 rightElbowPos = new Vector3(config.upperArmScale.y * 0.5f + config.lowerArmScale.y * 0.5f, 0, 0);
        var rightLowerArm = CreateReferenceBodyPart("RightLowerArm", rightElbowPos, config.lowerArmScale, PrimitiveType.Capsule, rightUpperArm.transform, config.lowerArmMass, config);
        // Keep forearm horizontal
        rightLowerArm.transform.localRotation = Quaternion.Euler(0, 0, 0);
        
        // BONES 8-11: LEFT LEG (connected to pelvis)
        Vector3 leftHipPos = new Vector3(-config.pelvisScale.x * 0.25f, -config.pelvisScale.y * 0.5f - config.upperLegScale.y * 0.5f, 0);
        var leftUpperLeg = CreateReferenceBodyPart("LeftUpperLeg", leftHipPos, config.upperLegScale, PrimitiveType.Capsule, pelvis.transform, config.upperLegMass, config);
        
        Vector3 leftKneePos = new Vector3(0, -config.upperLegScale.y * 0.5f - config.lowerLegScale.y * 0.5f, 0);
        var leftLowerLeg = CreateReferenceBodyPart("LeftLowerLeg", leftKneePos, config.lowerLegScale, PrimitiveType.Capsule, leftUpperLeg.transform, config.lowerLegMass, config);
        
        Vector3 leftAnklePos = new Vector3(0, -config.lowerLegScale.y * 0.5f - config.footScale.y * 0.5f, config.footScale.z * 0.25f);
        var leftFoot = CreateReferenceBodyPart("LeftFoot", leftAnklePos, config.footScale, PrimitiveType.Sphere, leftLowerLeg.transform, config.footMass, config);
        
        // BONES 12-13: RIGHT LEG (connected to pelvis) 
        Vector3 rightHipPos = new Vector3(config.pelvisScale.x * 0.25f, -config.pelvisScale.y * 0.5f - config.upperLegScale.y * 0.5f, 0);
        var rightUpperLeg = CreateReferenceBodyPart("RightUpperLeg", rightHipPos, config.upperLegScale, PrimitiveType.Capsule, pelvis.transform, config.upperLegMass, config);
        
        Vector3 rightKneePos = new Vector3(0, -config.upperLegScale.y * 0.5f - config.lowerLegScale.y * 0.5f, 0);
        var rightLowerLeg = CreateReferenceBodyPart("RightLowerLeg", rightKneePos, config.lowerLegScale, PrimitiveType.Capsule, rightUpperLeg.transform, config.lowerLegMass, config);
        
        Vector3 rightAnklePos = new Vector3(0, -config.lowerLegScale.y * 0.5f - config.footScale.y * 0.5f, config.footScale.z * 0.25f);
        var rightFoot = CreateReferenceBodyPart("RightFoot", rightAnklePos, config.footScale, PrimitiveType.Sphere, rightLowerLeg.transform, config.footMass, config);
        
        int boneCount = 13;
        int jointCount = 12;
        TrainArenaDebugManager.Log($"🎯 Reference ragdoll created with {boneCount} bones", TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Add the 12 joints with EXACT reference physics settings
        // Calculate proper anchor positions based on body part scales
        
        // Shoulder joints - connect at top of arms to side of chest
        AddReferenceJoint(leftUpperArm, chest, new Vector3(0, config.upperArmScale.y * 0.5f, 0), config); // Left shoulder
        AddReferenceJoint(rightUpperArm, chest, new Vector3(0, config.upperArmScale.y * 0.5f, 0), config); // Right shoulder  
        
        // Elbow joints - connect at bottom of upper arms to top of lower arms
        AddReferenceJoint(leftLowerArm, leftUpperArm, new Vector3(0, config.lowerArmScale.y * 0.5f, 0), config); // Left elbow
        AddReferenceJoint(rightLowerArm, rightUpperArm, new Vector3(0, config.lowerArmScale.y * 0.5f, 0), config); // Right elbow
        
        // Spine joint - connect at top of pelvis to bottom of chest
        AddReferenceJoint(chest, pelvis, new Vector3(0, config.chestScale.y * -0.5f, 0), config); // Spine
        
        // Neck joint - connect at top of chest to bottom of head
        AddReferenceJoint(head, chest, new Vector3(0, config.headScale.y * -0.5f, 0), config); // Neck
        
        // Hip joints - connect at top of upper legs to bottom of pelvis
        AddReferenceJoint(leftUpperLeg, pelvis, new Vector3(0, config.upperLegScale.y * 0.5f, 0), config); // Left hip
        AddReferenceJoint(rightUpperLeg, pelvis, new Vector3(0, config.upperLegScale.y * 0.5f, 0), config); // Right hip
        
        // Knee joints - connect at top of lower legs to bottom of upper legs
        AddReferenceJoint(leftLowerLeg, leftUpperLeg, new Vector3(0, config.lowerLegScale.y * 0.5f, 0), config); // Left knee
        AddReferenceJoint(rightLowerLeg, rightUpperLeg, new Vector3(0, config.lowerLegScale.y * 0.5f, 0), config); // Right knee
        
        // Ankle joints - connect at center of feet to bottom of lower legs
        AddReferenceJoint(leftFoot, leftLowerLeg, new Vector3(0, 0, 0), config); // Left ankle
        AddReferenceJoint(rightFoot, rightLowerLeg, new Vector3(0, 0, 0), config); // Right ankle
        
        TrainArenaDebugManager.Log($"🔗 Added {12} joints with reference physics settings", TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Add ML-Agents components
        root.AddComponent<RagdollAgent>();
        var behaviorParameters = root.AddComponent<BehaviorParameters>();
        behaviorParameters.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(jointCount);
        
        return root;
    }


    
    // Default configuration - can be overridden
    private static ReferenceRagdollConfiguration _currentConfig = null;
    public static ReferenceRagdollConfiguration CurrentConfig 
    { 
        get 
        { 
            if (_currentConfig == null) 
            {
                TrainArenaDebugManager.Log($"⚠️ CurrentConfig was NULL, creating REFERENCE EXACT config", TrainArenaDebugManager.DebugLogLevel.Important);
                _currentConfig = ReferenceRagdollConfiguration.ReferenceExact();
            }
            return _currentConfig;
        } 
        set 
        { 
            TrainArenaDebugManager.Log($"🔄 CurrentConfig being set to REFERENCE configuration", TrainArenaDebugManager.DebugLogLevel.Important);
            _currentConfig = value; 
        } 
    }
    
    /// <summary>
    /// Creates a material compatible with Unity 6.2 URP with specified surface properties
    /// </summary>
    public static Material CreateURPMaterial(float smoothness = 0.0f, float metallic = 0.0f)
    {
        // Try URP Lit shader first (Unity 6.2 default), fall back to Standard for Built-in
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }
        if (shader == null)
        {
            // Final fallback to built-in unlit
            shader = Shader.Find("Unlit/Color");
        }
        
        Material mat = new Material(shader);
        
        // Set surface properties for URP/Lit or Standard shader
        if (shader.name.Contains("Universal Render Pipeline/Lit"))
        {
            // URP Lit shader properties
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
        }
        else if (shader.name == "Standard")
        {
            // Built-in Standard shader properties
            mat.SetFloat("_Glossiness", smoothness);
            mat.SetFloat("_Metallic", metallic);
        }
        
        return mat;
    }
    
    /// <summary>
    /// Creates a complete CubeAgent with proper materials, physics, and trail
    /// </summary>
    public static GameObject CreateAgent(string name = "CubeAgent")
    {
        return CreateAgent(name, Vector3.zero, new Color(0.216f, 0.490f, 1.0f)); // Default blue color - #377DFF in linear space
    }
    
    /// <summary>
    /// Creates a complete CubeAgent with proper materials, physics, and trail at specified position and color
    /// </summary>
    public static GameObject CreateAgent(string name, Vector3 position, Color color)
    {
        var agent = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(agent.GetComponent<Collider>());
        agent.name = name;
        agent.transform.position = position;
        
        // Set up material for the agent (smooth finish for nice look)
        var mr = agent.GetComponent<Renderer>();
        Material mat = CreateURPMaterial(smoothness: 0.6f, metallic: 0.1f);
        mat.color = color;
        mat.name = "AgentMaterial";
        mr.sharedMaterial = mat;
        
        // Use BoxCollider to match cube shape perfectly
        var col = agent.AddComponent<BoxCollider>();
        col.center = Vector3.zero; // Centered on the cube
        col.size = Vector3.one; // 1x1x1 cube

        var rb = agent.AddComponent<Rigidbody>();
        rb.mass = 1f;
        
        // Add trail for path visualization
        AddTrail(agent);
        
        // Add simple face to show agent orientation
        AddAgentFace(agent);
        
        return agent;
    }
    
    /// <summary>
    /// Creates a goal sphere with proper materials and effects
    /// </summary>
    public static GameObject CreateGoal(float height, string name = "Goal")
    {
        var goal = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goal.transform.localScale = Vector3.one * height;
        var mr = goal.GetComponent<Renderer>();
        
        // Goal material with emission for visibility
        Material mat = CreateURPMaterial(smoothness: 0.3f, metallic: 0.0f);
        var goldColor = new Color(1.0f, 0.835f, 0.290f); // #FFD54A in linear space
        mat.color = goldColor;
        mat.name = "GoalMaterial";
        
        // Add emission for better visibility
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.SetColor("_EmissionColor", goldColor * 2f);
            mat.EnableKeyword("_EMISSION");
        }
        
        mr.sharedMaterial = mat;
        goal.name = name;
        
        return goal;
    }
    
    /// <summary>
    /// Creates an obstacle cube with matte materials
    /// </summary>
    public static GameObject CreateObstacle(float height, float width = 0.8f, float depth = 0.8f, string name = "Obstacle")
    {
        var obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obs.transform.localScale = new Vector3(width, height, depth);
        var mr = obs.GetComponent<Renderer>();
        
        // Matte finish for obstacles as requested
        Material mat = CreateURPMaterial(smoothness: 0.0f, metallic: 0.0f);
        mat.color = new Color(0.906f, 0.298f, 0.235f); // #E74C3C in linear space
        mat.name = "ObstacleMaterial";
        mr.sharedMaterial = mat;
        
        obs.name = name;
        obs.tag = "Obstacle"; // Ensure proper tag for agent detection
        
        return obs;
    }
    
    /// <summary>
    /// Creates a ground plane with appropriate materials
    /// </summary>
    public static GameObject CreateGround(string name = "Ground")
    {
        return CreateGround(Vector3.zero, Vector3.one, new Color(0.788f, 0.788f, 0.788f), name); // Default ground color - #C9C9C9 in linear space
    }
    
    /// <summary>
    /// Creates a ground plane with appropriate materials at specified position and scale
    /// </summary>
    public static GameObject CreateGround(Vector3 position, Vector3 scale, Color color, string name = "Ground")
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = name;
        ground.transform.position = position;
        ground.transform.localScale = scale;
        
        // Slightly rough surface for ground
        Material groundMat = CreateURPMaterial(smoothness: 0.2f, metallic: 0.0f);
        groundMat.name = "GroundMaterial";
        ground.GetComponent<Renderer>().sharedMaterial = groundMat;
        
        ground.isStatic = true; // Ground should be static for performance
        
        return ground;
    }
    
    /// <summary>
    /// Adds trail renderer to agents for path visualization
    /// </summary>
    public static void AddTrail(GameObject agent)
    {
        var tr = agent.AddComponent<TrailRenderer>();
        tr.time = 0.35f; 
        tr.startWidth = 0.25f; 
        tr.endWidth = 0.02f;
        
        // Create and configure material before assigning to avoid edit mode leaks
        var trailMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        trailMat.SetColor("_BaseColor", new Color(0.25f, 0.7f, 1f, 0.6f));
        trailMat.name = "AgentTrailMaterial";
        tr.sharedMaterial = trailMat;
        
        tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        
        TrainArenaDebugManager.Log($"Added trail to {agent.name}", TrainArenaDebugManager.DebugLogLevel.Verbose);
    }
    
    /// <summary>
    /// Adds simple "eyes" to show agent forward direction
    /// </summary>
    public static void AddAgentFace(GameObject agent)
    {
        // Create left eye - positioned ON the cube surface, not inside
        var leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftEye.name = "LeftEye";
        leftEye.transform.parent = agent.transform;
        leftEye.transform.localPosition = new Vector3(-0.2f, 0.2f, 0.51f); // Outside cube surface (0.5 + 0.01 margin)
        leftEye.transform.localScale = Vector3.one * 0.15f; // Slightly larger for visibility
        
        // Create right eye  
        var rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightEye.name = "RightEye";
        rightEye.transform.parent = agent.transform;
        rightEye.transform.localPosition = new Vector3(0.2f, 0.2f, 0.51f); // Outside cube surface
        rightEye.transform.localScale = Vector3.one * 0.15f;
        
        // Make eyes white with slight gloss for realism
        var eyeMaterial = CreateURPMaterial(smoothness: 0.8f, metallic: 0.0f);
        eyeMaterial.color = Color.white;
        eyeMaterial.name = "EyeMaterial";
        
        leftEye.GetComponent<Renderer>().sharedMaterial = eyeMaterial;
        rightEye.GetComponent<Renderer>().sharedMaterial = eyeMaterial;
        
        // Remove colliders from eyes so they don't interfere with physics
        Object.DestroyImmediate(leftEye.GetComponent<Collider>());
        Object.DestroyImmediate(rightEye.GetComponent<Collider>());
    }
    
    /// <summary>
    /// Ensures a tag exists in Unity's TagManager
    /// </summary>
    public static void EnsureTagExists(string tagName)
    {
#if UNITY_EDITOR
        // First check if tag already exists
        if (UnityEditorInternal.InternalEditorUtility.tags.Contains(tagName))
            return;
            
        // Add the tag using Unity's built-in method
        UnityEditorInternal.InternalEditorUtility.AddTag(tagName);
#endif
    }

    /// <summary>
    /// LEGACY - DEPRECATED - Use BlockmanRagdollBuilder.Build() instead
    /// This method has been replaced by the more stable BlockmanRagdollBuilder system
    /// </summary>
    [System.Obsolete("Use BlockmanRagdollBuilder.Build() instead", true)]
    public static GameObject CreateAnatomicalRagdoll(string name, Vector3 position, ReferenceRagdollConfiguration config)
    {
        TrainArenaDebugManager.Log($"🚀 CREATING ANATOMICALLY CORRECT RAGDOLL: {name}", TrainArenaDebugManager.DebugLogLevel.Important);

        var root = new GameObject(name);
        root.transform.position = position;
        
        // Create pelvis at root (center of mass)
        var pelvis = CreateAnatomicalBodyPart("Pelvis", Vector3.zero, config.pelvisScale, root.transform, config);
        
        // Calculate anatomically correct positions for leg attachment
        float pelvisBottom = -config.pelvisScale.y * 0.5f;  // Bottom of pelvis capsule
        float hipWidth = config.pelvisScale.x * 0.35f;      // Hip socket width (slightly inset from pelvis edge)
        
        // LEFT LEG CHAIN - each part connects to the bottom of its parent
        Vector3 leftHipPos = new Vector3(-hipWidth, pelvisBottom - config.hipSocketDepth, 0);
        var leftThigh = CreateAnatomicalBodyPart("LeftThigh", leftHipPos, config.thighScale, pelvis.transform, config);
        
        float thighBottom = leftHipPos.y - config.thighScale.y * 0.5f;
        Vector3 leftKneePos = new Vector3(0, thighBottom - config.kneeConnectionHeight, 0);
        var leftShin = CreateAnatomicalBodyPart("LeftShin", leftKneePos, config.shinScale, leftThigh.transform, config);
        
        float shinBottom = leftKneePos.y - config.shinScale.y * 0.5f; 
        Vector3 leftAnklePos = new Vector3(0, shinBottom - config.ankleConnectionHeight, 0);
        var leftFoot = CreateAnatomicalBodyPart("LeftFoot", leftAnklePos, config.footScale, leftShin.transform, config);
        
        // RIGHT LEG CHAIN - mirror the left leg
        Vector3 rightHipPos = new Vector3(hipWidth, pelvisBottom - config.hipSocketDepth, 0);
        var rightThigh = CreateAnatomicalBodyPart("RightThigh", rightHipPos, config.thighScale, pelvis.transform, config);
        
        Vector3 rightKneePos = new Vector3(0, thighBottom - config.kneeConnectionHeight, 0);
        var rightShin = CreateAnatomicalBodyPart("RightShin", rightKneePos, config.shinScale, rightThigh.transform, config);
        
        Vector3 rightAnklePos = new Vector3(0, shinBottom - config.ankleConnectionHeight, 0);
        var rightFoot = CreateAnatomicalBodyPart("RightFoot", rightAnklePos, config.footScale, rightShin.transform, config);

        // Create anatomically correct joints - child's top connects to parent's bottom
        TrainArenaDebugManager.Log("🔧 Creating ANATOMICALLY CORRECT JOINTS", TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Hip joints - thigh top connects to pelvis hip socket  
        AddAnatomicalJoint(leftThigh, pelvis, Vector3.up * 0.5f, Vector3.down * 0.5f, new Vector3(config.hipJointLimit, 0, 0), config);
        AddAnatomicalJoint(rightThigh, pelvis, Vector3.up * 0.5f, Vector3.down * 0.5f, new Vector3(config.hipJointLimit, 0, 0), config);
        
        // Knee joints - shin top connects to thigh bottom
        AddAnatomicalJoint(leftShin, leftThigh, Vector3.up * 0.5f, Vector3.down * 0.5f, new Vector3(config.kneeJointLimit, 0, 0), config);
        AddAnatomicalJoint(rightShin, rightThigh, Vector3.up * 0.5f, Vector3.down * 0.5f, new Vector3(config.kneeJointLimit, 0, 0), config);
        
        // Ankle joints - foot connects to shin bottom
        AddAnatomicalJoint(leftFoot, leftShin, Vector3.up * 0.5f, Vector3.down * 0.5f, new Vector3(config.ankleJointLimit, 0, 0), config);
        AddAnatomicalJoint(rightFoot, rightShin, Vector3.up * 0.5f, Vector3.down * 0.5f, new Vector3(config.ankleJointLimit, 0, 0), config);

        // Add RagdollAgent to pelvis
        var ragdollAgent = pelvis.AddComponent<RagdollAgent>();
        ragdollAgent.pelvis = pelvis.transform;
        
        // Add BehaviorParameters for ML-Agents
        var behaviorParams = pelvis.AddComponent<Unity.MLAgents.Policies.BehaviorParameters>();
        behaviorParams.BehaviorName = "RagdollAgent";
        behaviorParams.BrainParameters.VectorObservationSize = 4; // Will be calculated automatically
        behaviorParams.BrainParameters.NumStackedVectorObservations = 1;
        behaviorParams.BrainParameters.ActionSpec = Unity.MLAgents.Actuators.ActionSpec.MakeContinuous(6); // 6 joints
        behaviorParams.BehaviorType = Unity.MLAgents.Policies.BehaviorType.Default;
        
        TrainArenaDebugManager.Log($"🎭 Ragdoll created: {name} with improved proportions - 6 joints, {behaviorParams.BrainParameters.ActionSpec.NumContinuousActions} actions", 
            TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Collect all PDJointControllers and assign to agent
        var joints = new System.Collections.Generic.List<PDJointController>();
        joints.AddRange(root.GetComponentsInChildren<PDJointController>());
        ragdollAgent.joints = joints;

        return root;
    }

    /// <summary>
    /// Creates a reference exact body part with specified primitive type and mass
    /// </summary>
    private static GameObject CreateReferenceBodyPart(string name, Vector3 localPosition, Vector3 size, PrimitiveType primitiveType, Transform parent, float mass, ReferenceRagdollConfiguration config)
    {
        var bodyPart = GameObject.CreatePrimitive(primitiveType);
        bodyPart.name = name;
        bodyPart.transform.parent = parent;
        bodyPart.transform.localPosition = localPosition;
        bodyPart.transform.localScale = size;
        
        // Apply URP material like CubeAgent
        var mr = bodyPart.GetComponent<Renderer>();
        Material mat = CreateURPMaterial(smoothness: 0.6f, metallic: 0.1f);
        
        // Color body parts differently for easy identification
        if (name.Contains("Head"))
        {
            mat.color = new Color(1.0f, 0.8f, 0.6f); // Skin tone for head
        }
        else if (name.Contains("Arm") || name.Contains("Leg"))
        {
            mat.color = new Color(0.8f, 0.6f, 0.4f); // Slightly darker for limbs
        }
        else
        {
            mat.color = new Color(0.9f, 0.7f, 0.5f); // Medium tone for torso
        }
        
        mat.name = $"{name}Material";
        mr.sharedMaterial = mat;
        
        // Add physics components with EXACT reference masses
        var rb = bodyPart.AddComponent<Rigidbody>();
        rb.mass = mass; // Use the exact mass from reference
        rb.linearDamping = config.linearDamping;
        rb.angularDamping = config.angularDamping;
        rb.interpolation = config.interpolation;
        
        // Configure collider with physics material
        var collider = bodyPart.GetComponent<Collider>();
        if (collider is CapsuleCollider capsuleCollider)
        {
            capsuleCollider.material = CreatePhysicsMaterial(config);
        }
        else if (collider is BoxCollider boxCollider)
        {
            boxCollider.material = CreatePhysicsMaterial(config);
        }
        else if (collider is SphereCollider sphereCollider)
        {
            sphereCollider.material = CreatePhysicsMaterial(config);
        }
        
        // Add blinking eyes if this is the head
        if (name.Contains("Head"))
        {
            AddBlinkingEyes(bodyPart);
        }
        
        TrainArenaDebugManager.Log($"🦴 Created {name}: {primitiveType}, mass={mass}kg, pos={localPosition}, size={size}", TrainArenaDebugManager.DebugLogLevel.Verbose);
        
        return bodyPart;
    }

    /// <summary>
    /// Adds animated blinking eyes to the head sphere
    /// </summary>
    private static void AddBlinkingEyes(GameObject head)
    {
        // Create left eye
        var leftEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftEye.name = "LeftEye";
        leftEye.transform.parent = head.transform;
        leftEye.transform.localPosition = new Vector3(-0.15f, 0.1f, 0.4f);
        leftEye.transform.localScale = new Vector3(0.15f, 0.15f, 0.1f);
        
        // Remove collider from eye (decorative only)
        Object.DestroyImmediate(leftEye.GetComponent<Collider>());
        
        // Make eye black
        var leftEyeMat = CreateURPMaterial(smoothness: 0.9f, metallic: 0.0f);
        leftEyeMat.color = Color.black;
        leftEyeMat.name = "LeftEyeMaterial";
        leftEye.GetComponent<Renderer>().sharedMaterial = leftEyeMat;
        
        // Create right eye
        var rightEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightEye.name = "RightEye";
        rightEye.transform.parent = head.transform;
        rightEye.transform.localPosition = new Vector3(0.15f, 0.1f, 0.4f);
        rightEye.transform.localScale = new Vector3(0.15f, 0.15f, 0.1f);
        
        // Remove collider from eye (decorative only)
        Object.DestroyImmediate(rightEye.GetComponent<Collider>());
        
        // Make eye black
        var rightEyeMat = CreateURPMaterial(smoothness: 0.9f, metallic: 0.0f);
        rightEyeMat.color = Color.black;
        rightEyeMat.name = "RightEyeMaterial";
        rightEye.GetComponent<Renderer>().sharedMaterial = rightEyeMat;
        
        // Add blinking animation component
        var blinkAnimator = head.AddComponent<EyeBlinkAnimator>();
        blinkAnimator.leftEye = leftEye.transform;
        blinkAnimator.rightEye = rightEye.transform;
    }
    
    /// <summary>
    /// LEGACY - Creates an anatomically correct body part with proper joint connections
    /// </summary>
    private static GameObject CreateAnatomicalBodyPart(string name, Vector3 localPosition, Vector3 size, Transform parent, ReferenceRagdollConfiguration config)
    {
        var bodyPart = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bodyPart.name = name;
        bodyPart.transform.parent = parent;
        bodyPart.transform.localPosition = localPosition;
        bodyPart.transform.localScale = size;
        
        // Rotate feet to be flat on ground
        if (name.Contains("Foot"))
        {
            bodyPart.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }
        
        // Add physics components
        var rb = bodyPart.AddComponent<Rigidbody>();
        rb.mass = GetAnatomicalMass(name, config);
        rb.linearDamping = config.linearDamping;
        rb.angularDamping = config.angularDamping;
        rb.interpolation = config.interpolation;
        
        // Configure collider with physics material
        var collider = bodyPart.GetComponent<CapsuleCollider>();
        collider.material = CreatePhysicsMaterial(config);
        
        TrainArenaDebugManager.Log($"🦴 Created {name}: pos={localPosition}, size={size}", TrainArenaDebugManager.DebugLogLevel.Verbose);
        
        return bodyPart;
    }
    
    /// <summary>
    /// Get anatomically appropriate mass for body part
    /// </summary>
    private static float GetAnatomicalMass(string partName, ReferenceRagdollConfiguration config)
    {
        if (partName.Contains("Pelvis")) return config.pelvisMass;
        if (partName.Contains("Thigh")) return config.thighMass;
        if (partName.Contains("Shin")) return config.shinMass;
        if (partName.Contains("Foot")) return config.footMass;
        return 1f; // Default
    }
    
    /// <summary>
    /// LEGACY - Get anatomically appropriate mass for body part
    /// </summary>
    private static float GetAnatomicalMass(string partName, RagdollConfiguration config)
    {
        if (partName.Contains("Pelvis")) return config.pelvisMass;
        if (partName.Contains("Thigh")) return config.thighMass;
        if (partName.Contains("Shin")) return config.shinMass;
        if (partName.Contains("Foot")) return config.footMass;
        return 1f; // Default
    }

    /// <summary>
    /// LEGACY METHOD - Creates a single body part (capsule with rigidbody and collider)
    /// </summary>
    private static GameObject CreateRagdollBodyPart(string name, Vector3 localPosition, Vector3 size, Transform parent, RagdollConfiguration config)
    {
        TrainArenaDebugManager.Log($"🔨 CREATING BODY PART: {name}", TrainArenaDebugManager.DebugLogLevel.Important);
        TrainArenaDebugManager.Log($"🔨 INPUT - localPosition: {localPosition}, size: {size}, parent: {parent.name}", TrainArenaDebugManager.DebugLogLevel.Important);
        
        var bodyPart = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        TrainArenaDebugManager.Log($"🔨 CREATED PRIMITIVE - name before: {bodyPart.name}", TrainArenaDebugManager.DebugLogLevel.Important);
        
        bodyPart.name = name;
        TrainArenaDebugManager.Log($"🔨 SET NAME - name after: {bodyPart.name}", TrainArenaDebugManager.DebugLogLevel.Important);
        
        bodyPart.transform.parent = parent;
        TrainArenaDebugManager.Log($"🔨 SET PARENT - parent: {bodyPart.transform.parent.name}, position before setting local: {bodyPart.transform.position}", TrainArenaDebugManager.DebugLogLevel.Important);
        
        bodyPart.transform.localPosition = localPosition;
        TrainArenaDebugManager.Log($"� SET LOCAL POSITION - requested: {localPosition}, actual local: {bodyPart.transform.localPosition}, world: {bodyPart.transform.position}", TrainArenaDebugManager.DebugLogLevel.Important);
        
        bodyPart.transform.localScale = size;
        TrainArenaDebugManager.Log($"🔨 SET SCALE - requested: {size}, actual: {bodyPart.transform.localScale}", TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Rotate feet to be horizontal (flat on ground)
        if (name.Contains("Foot"))
        {
            bodyPart.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            TrainArenaDebugManager.Log($"🔨 ROTATED FOOT - {name} rotated to be flat on ground", TrainArenaDebugManager.DebugLogLevel.Important);
        }
        
        // Add rigidbody using configuration settings
        var rb = bodyPart.AddComponent<Rigidbody>();
        // Set mass based on body part name
        rb.mass = name == "Pelvis" ? config.pelvisMass : 
                 (name.Contains("Thigh") ? config.thighMass : 
                 (name.Contains("Shin") ? config.shinMass : config.footMass));
        rb.linearDamping = config.linearDamping;
        rb.angularDamping = config.angularDamping;
        rb.interpolation = config.interpolation;
        
        // Configure collider
        var collider = bodyPart.GetComponent<CapsuleCollider>();
        collider.material = CreatePhysicsMaterial(config);
        
        return bodyPart;
    }

    /// <summary>
    /// Adds a reference exact joint between two body parts with EXACT reference physics settings
    /// </summary>
    private static void AddReferenceJoint(GameObject child, GameObject parent, Vector3 anchorPos, ReferenceRagdollConfiguration config)
    {
        var joint = child.AddComponent<ConfigurableJoint>();
        joint.connectedBody = parent.GetComponent<Rigidbody>();
        
        // EXACT anchor positioning from reference
        joint.anchor = anchorPos;
        
        // Calculate the connected anchor based on parent-child relationship
        Vector3 connectedAnchorPos = CalculateConnectedAnchor(child, parent, config);
        joint.connectedAnchor = connectedAnchorPos;
        joint.autoConfigureConnectedAnchor = false;
        
        // EXACT motion settings from reference (3 DOF rotation)
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Free;
        
        // EXACT drive settings from reference (spring=125, damper=0.5, maxForce=3.4e+38)
        var jointDrive = new JointDrive();
        jointDrive.positionSpring = config.jointSpring;        // 125
        jointDrive.positionDamper = config.jointDamper;        // 0.5
        jointDrive.maximumForce = config.jointMaxForce;        // 3.4e+38
        
        joint.slerpDrive = jointDrive;
        joint.rotationDriveMode = RotationDriveMode.Slerp;
        
        // EXACT limits matching reference (wide range for natural movement)
        var limit = new SoftJointLimit();
        limit.limit = 60f;
        limit.bounciness = 0f;
        limit.contactDistance = 0f;
        
        joint.lowAngularXLimit = limit;
        joint.highAngularXLimit = limit;
        joint.angularYLimit = limit;
        joint.angularZLimit = limit;
        
        TrainArenaDebugManager.Log($"🔗 Added reference joint: {child.name} -> {parent.name} at {anchorPos} -> {connectedAnchorPos}", TrainArenaDebugManager.DebugLogLevel.Verbose);
    }

    /// <summary>
    /// Calculates the proper connected anchor position based on parent-child body part relationship
    /// </summary>
    private static Vector3 CalculateConnectedAnchor(GameObject child, GameObject parent, ReferenceRagdollConfiguration config)
    {
        // Determine connection points based on body part names
        string childName = child.name.ToLower();
        string parentName = parent.name.ToLower();
        
        // Leg connections
        if (childName.Contains("upperleg") && parentName.Contains("pelvis"))
        {
            // Upper leg connects to bottom of pelvis
            return new Vector3(0, -config.pelvisScale.y * 0.5f, 0);
        }
        else if (childName.Contains("lowerleg") && parentName.Contains("upperleg"))
        {
            // Lower leg connects to bottom of upper leg
            return new Vector3(0, -config.upperLegScale.y * 0.5f, 0);
        }
        else if (childName.Contains("foot") && parentName.Contains("lowerleg"))
        {
            // Foot connects to bottom of lower leg
            return new Vector3(0, -config.lowerLegScale.y * 0.5f, 0);
        }
        // Arm connections
        else if (childName.Contains("upperarm") && parentName.Contains("chest"))
        {
            // Upper arm connects to side of chest
            float sideOffset = childName.Contains("left") ? -config.chestScale.x * 0.5f : config.chestScale.x * 0.5f;
            return new Vector3(sideOffset, config.chestScale.y * 0.25f, 0);
        }
        else if (childName.Contains("lowerarm") && parentName.Contains("upperarm"))
        {
            // Lower arm connects to bottom of upper arm
            return new Vector3(0, -config.upperArmScale.y * 0.5f, 0);
        }
        // Torso connections
        else if (childName.Contains("chest") && parentName.Contains("pelvis"))
        {
            // Chest connects to top of pelvis
            return new Vector3(0, config.pelvisScale.y * 0.5f, 0);
        }
        else if (childName.Contains("head") && parentName.Contains("chest"))
        {
            // Head connects to top of chest
            return new Vector3(0, config.chestScale.y * 0.5f, 0);
        }
        
        // Default to center if no specific rule found
        return Vector3.zero;
    }

    /// <summary>
    /// LEGACY - Creates an anatomically correct joint connection between body parts
    /// </summary>
    private static void AddAnatomicalJoint(GameObject child, GameObject parent, Vector3 childAnchor, Vector3 parentAnchor, Vector3 limits, ReferenceRagdollConfiguration config)
    {
        var joint = child.AddComponent<ConfigurableJoint>();
        joint.connectedBody = parent.GetComponent<Rigidbody>();
        
        // ANATOMICALLY CORRECT ANCHORS - child's top connects to parent's bottom
        joint.anchor = childAnchor;                    // Top of child capsule
        joint.connectedAnchor = parentAnchor;          // Bottom of parent capsule
        joint.autoConfigureConnectedAnchor = false;    // Manual control for precision
        
        // Configure realistic joint movement
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Limited;  // Primary flexion/extension
        joint.angularYMotion = ConfigurableJointMotion.Locked;   // No side bending for now  
        joint.angularZMotion = ConfigurableJointMotion.Locked;   // No twisting for now
        
        // Set anatomical joint limits
        var lowLimit = joint.lowAngularXLimit;
        lowLimit.limit = -limits.x;
        lowLimit.bounciness = config.jointBounciness;
        joint.lowAngularXLimit = lowLimit;

        var highLimit = joint.highAngularXLimit;
        highLimit.limit = limits.x;
        highLimit.bounciness = config.jointBounciness;
        joint.highAngularXLimit = highLimit;
        
        // Configure joint drive for muscle-like behavior
        var drive = joint.angularXDrive;
        drive.positionSpring = config.jointSpring;
        drive.positionDamper = config.jointDamper;
        drive.maximumForce = config.jointMaxForce;
        joint.angularXDrive = drive;
        
        // Add PD controller for ML-Agents
        var pdController = child.AddComponent<PDJointController>();
        pdController.joint = joint;
        pdController.minAngle = -limits.x;
        pdController.maxAngle = limits.x;
        pdController.kp = config.pdKp;
        pdController.kd = config.pdKd;
        
        TrainArenaDebugManager.Log($"🔗 Anatomical joint: {child.name} -> {parent.name} (child anchor: {childAnchor}, parent anchor: {parentAnchor})", 
            TrainArenaDebugManager.DebugLogLevel.Verbose);
    }

    /// <summary>
    /// LEGACY METHOD - Adds a ConfigurableJoint and PDJointController optimized for AI training
    /// </summary>
    private static void AddRagdollJoint(GameObject child, GameObject parent, Vector3 axis, Vector3 limits, RagdollConfiguration config)
    {
        var joint = child.AddComponent<ConfigurableJoint>();
        joint.connectedBody = parent.GetComponent<Rigidbody>();
        
        // PROPER ANATOMICAL JOINT ANCHORING - prevents collapse and ensures realistic articulation
        Vector3 childAnchor = Vector3.up * 0.5f;      // Top of child capsule (connection point)
        Vector3 parentAnchor = Vector3.down * 0.5f;   // Bottom of parent capsule (connection point)
        
        joint.anchor = childAnchor;                    // Connect at top of child
        joint.connectedAnchor = parentAnchor;          // Connect at bottom of parent
        joint.autoConfigureConnectedAnchor = false;    // Manual control for precision
        
        // DEBUG: Log joint configuration
        TrainArenaDebugManager.Log($"🔗 Created joint: {child.name} -> {parent.name} with anatomical anchors (child top to parent bottom)", TrainArenaDebugManager.DebugLogLevel.Verbose);
        
        // Configure motion constraints
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;
        
        // Set joint limits using configuration
        var lowLimit = joint.lowAngularXLimit;
        lowLimit.limit = -limits.x;
        lowLimit.bounciness = config.jointBounciness;
        joint.lowAngularXLimit = lowLimit;

        var highLimit = joint.highAngularXLimit;
        highLimit.limit = limits.x;
        highLimit.bounciness = config.jointBounciness;
        joint.highAngularXLimit = highLimit;
        
        // Configure joint drive using configuration
        var drive = joint.angularXDrive;
        drive.positionSpring = config.jointSpring;
        drive.positionDamper = config.jointDamper;
        drive.maximumForce = config.jointMaxForce;
        joint.angularXDrive = drive;
        
        // Add PD controller using configuration
        var pdController = child.AddComponent<PDJointController>();
        pdController.joint = joint;
        pdController.minAngle = -limits.x;
        pdController.maxAngle = limits.x;
        pdController.kp = config.pdKp;
        pdController.kd = config.pdKd;
        
        TrainArenaDebugManager.Log($"Created joint: {child.name} -> {parent.name}, limits: ±{limits.x}°", 
            TrainArenaDebugManager.DebugLogLevel.Verbose);
    }

    /// <summary>
    /// Creates physics material for ragdoll parts using ReferenceRagdollConfiguration
    /// </summary>
    private static PhysicsMaterial CreatePhysicsMaterial(ReferenceRagdollConfiguration config)
    {
        var material = new PhysicsMaterial("ReferenceRagdollMaterial");
        material.dynamicFriction = config.friction;
        material.staticFriction = config.friction;
        material.bounciness = config.bounciness;
        return material;
    }

    /// <summary>
    /// LEGACY - Creates physics material for ragdoll parts using old RagdollConfiguration
    /// </summary>
    private static PhysicsMaterial CreatePhysicsMaterial(RagdollConfiguration config)
    {
        var material = new PhysicsMaterial("RagdollMaterial");
        material.dynamicFriction = config.dynamicFriction;
        material.staticFriction = config.staticFriction;
        material.bounciness = config.bounciness;
        return material;
    }

    // Unity Editor menu items for ragdoll creation
    #if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/ML Hack/Create Test Ragdoll")]
    public static void CreateTestRagdoll()
    {
        var ragdoll = CreateRagdoll("TestRagdoll", Vector3.zero);
        UnityEditor.Selection.activeGameObject = ragdoll;
        TrainArenaDebugManager.Log($"🎭 Created test ragdoll using BlockmanRagdollBuilder system. Joints: {ragdoll.GetComponentsInChildren<PDJointController>().Length}", TrainArenaDebugManager.DebugLogLevel.Important);
    }
    #endif
}

/// <summary>
/// Simple eye blinking animation component for ragdoll heads
/// </summary>
public class EyeBlinkAnimator : MonoBehaviour
{
    [Header("Eye References")]
    public Transform leftEye;
    public Transform rightEye;
    
    [Header("Blink Settings")]
    public float blinkInterval = 3.0f;      // Time between blinks
    public float blinkDuration = 0.15f;     // How long each blink lasts
    public float blinkVariation = 1.5f;     // Random variation in blink timing
    
    private Vector3 leftEyeOriginalScale;
    private Vector3 rightEyeOriginalScale;
    private bool isBlinking = false;
    
    void Start()
    {
        if (leftEye != null) leftEyeOriginalScale = leftEye.localScale;
        if (rightEye != null) rightEyeOriginalScale = rightEye.localScale;
        
        StartCoroutine(BlinkRoutine());
    }
    
    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            // Wait for random interval before next blink
            float waitTime = blinkInterval + Random.Range(-blinkVariation, blinkVariation);
            yield return new WaitForSeconds(waitTime);
            
            // Perform blink
            if (!isBlinking)
            {
                StartCoroutine(PerformBlink());
            }
        }
    }
    
    IEnumerator PerformBlink()
    {
        isBlinking = true;
        
        float halfDuration = blinkDuration * 0.5f;
        
        // Blink close (scale Y to 0)
        float elapsedTime = 0;
        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            float scaleY = Mathf.Lerp(1f, 0.05f, t); // Almost close, not completely flat
            
            if (leftEye != null)
                leftEye.localScale = new Vector3(leftEyeOriginalScale.x, leftEyeOriginalScale.y * scaleY, leftEyeOriginalScale.z);
            if (rightEye != null)
                rightEye.localScale = new Vector3(rightEyeOriginalScale.x, rightEyeOriginalScale.y * scaleY, rightEyeOriginalScale.z);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Blink open (scale Y back to 1)
        elapsedTime = 0;
        while (elapsedTime < halfDuration)
        {
            float t = elapsedTime / halfDuration;
            float scaleY = Mathf.Lerp(0.05f, 1f, t);
            
            if (leftEye != null)
                leftEye.localScale = new Vector3(leftEyeOriginalScale.x, leftEyeOriginalScale.y * scaleY, leftEyeOriginalScale.z);
            if (rightEye != null)
                rightEye.localScale = new Vector3(rightEyeOriginalScale.x, rightEyeOriginalScale.y * scaleY, rightEyeOriginalScale.z);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Ensure eyes are fully open
        if (leftEye != null) leftEye.localScale = leftEyeOriginalScale;
        if (rightEye != null) rightEye.localScale = rightEyeOriginalScale;
        
        isBlinking = false;
    }
}