// Assets/Scripts/Ragdoll/BlockmanRagdollBuilder.cs
// Unity 6.x / 2021+
// Key changes vs v1:
//  - Never scales transforms; sizes colliders instead.
//  - Visual child carries the mesh and scale.
//  - Anchors are computed from collider WORLD surfaces (no lossyScale compounding).
//  - Small separation epsilon so parts don't start interpenetrating.

using UnityEngine;
using UnityEditor;
using TrainArena.Configuration;

public class BlockmanRagdollBuilder : MonoBehaviour
{
    // FIXED: Joint limits and axes corrected for arms - shoulder and elbow joints now properly configured for natural movement

    [System.Serializable]
    public class Cfg
    {
        // physical sizes (meters). match the reference look.
        public Vector3 chest = new Vector3(0.50f, 0.60f, 0.30f);
        public Vector3 pelvis = new Vector3(0.45f, 0.40f, 0.30f);
        public Vector3 upperArm = new Vector3(0.55f, 0.12f, 0.12f); // length on X
        public Vector3 lowerArm = new Vector3(0.50f, 0.10f, 0.10f); // length on X
        public Vector3 upperLeg = new Vector3(0.18f, 0.55f, 0.18f); // length on Y
        public Vector3 lowerLeg = new Vector3(0.16f, 0.50f, 0.16f); // length on Y
        public float headRadius = 0.20f;
        public float footRadius = 0.14f;

        // masses (kg) ~85kg total from the screenshots
        public float mHead = 6.6f, mChest = 18.1f, mPelvis = 24.4f;
        public float mUArm = 3.3f, mLArm = 2.2f;        // each
        public float mULeg = 11.0f, mLLeg = 4.4f;       // each
        public float mFoot = 1.1f;                      // each

        // joint limits (deg) - More natural ranges for better ragdoll physics
        public float kneeFlex = 130f;
        public float elbowFlex = 135f;
        public float ankleLow = -30f, ankleHigh = 60f;  // Increased ankle range for better foot placement
        public float hipSwing = 90f;                    // Increased hip range for natural leg movement
        public float shoulderSwing = 90f;               // Increased shoulder range
        public float neckSwing = 45f;                   // Increased neck range
        public float spineBend = 30f;                   // Increased spine bend for natural posture

        // drives - Softer springs with higher damping for more natural physics
        public float spring = 60f, damper = 2.0f, maxForce = 3.4e38f;

        // damping & collision - Increased damping for more stable physics
        public float linDamp = 0.1f, angDamp = 0.15f;
        public float separation = 0.004f; // small gap to avoid initial penetration
    }

    [MenuItem("TrainArena/Ragdoll/Build Reference Ragdoll")]
    static void BuildMenu()
    {
        var root = Build(Vector3.zero, new Cfg());
        Selection.activeGameObject = root;
    }

    public static GameObject Build(Vector3 pos, Cfg cfg)
    {
        var root = new GameObject("BlockmanRagdoll");
        root.transform.position = pos;

        // ---- helpers ----
        GameObject BoxBone(string name, Transform parent, Vector3 size, float mass, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity; // important

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.linearDamping = cfg.linDamp;
            rb.angularDamping = cfg.angDamp;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var col = go.AddComponent<BoxCollider>();
            col.size = size;          // size lives on the collider (transform scale remains 1)
            col.center = Vector3.zero;

            // visual child (scaled)
            var vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyImmediate(vis.GetComponent<BoxCollider>());
            vis.name = "Visual";
            vis.transform.SetParent(go.transform, false);
            vis.transform.localScale = size;
            var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(sh) { color = new Color(0.9f, 0.25f, 0.25f) };
            vis.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        GameObject SphereBone(string name, Transform parent, float radius, float mass, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = mass; rb.linearDamping = cfg.linDamp; rb.angularDamping = cfg.angDamp;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var col = go.AddComponent<SphereCollider>();
            col.radius = radius; col.center = Vector3.zero;

            var vis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            DestroyImmediate(vis.GetComponent<SphereCollider>());
            vis.name = "Visual";
            vis.transform.SetParent(go.transform, false);
            vis.transform.localScale = Vector3.one * (radius * 2f);
            var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(sh) { color = new Color(0.9f, 0.25f, 0.25f) };
            vis.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        // ---- build bones (no transform scaling anywhere) ----
        var pelvis = BoxBone(RagdollJointNames.Pelvis, root.transform, cfg.pelvis, cfg.mPelvis, Vector3.zero);
        var chest = BoxBone(RagdollJointNames.Chest, pelvis.transform, cfg.chest, cfg.mChest,
                             new Vector3(0, (cfg.pelvis.y + cfg.chest.y) * 0.5f + cfg.separation, 0));
        var head = SphereBone(RagdollJointNames.Head, chest.transform, cfg.headRadius, cfg.mHead,
                             new Vector3(0, cfg.chest.y * 0.5f + cfg.headRadius + cfg.separation, 0));

        // Position shoulders higher up on chest for realistic anatomy
        float shoulderY = cfg.chest.y * 0.4f; // Higher up the chest
        // Position upper arms so their inner end connects to chest outer surface
        float shoulderX = cfg.chest.x * 0.5f + cfg.upperArm.x * 0.5f + cfg.separation;

        var lUArm = BoxBone(RagdollJointNames.LeftUpperArm, chest.transform, cfg.upperArm, cfg.mUArm, new Vector3(-shoulderX, shoulderY, 0));
        var rUArm = BoxBone(RagdollJointNames.RightUpperArm, chest.transform, cfg.upperArm, cfg.mUArm, new Vector3(+shoulderX, shoulderY, 0));

        var lLArm = BoxBone(RagdollJointNames.LeftLowerArm, lUArm.transform, cfg.lowerArm, cfg.mLArm,
                             new Vector3(-(cfg.upperArm.x * 0.5f + cfg.lowerArm.x * 0.5f + cfg.separation), 0, 0));
        var rLArm = BoxBone(RagdollJointNames.RightLowerArm, rUArm.transform, cfg.lowerArm, cfg.mLArm,
                             new Vector3((cfg.upperArm.x * 0.5f + cfg.lowerArm.x * 0.5f + cfg.separation), 0, 0));

        float hipX = cfg.pelvis.x * 0.25f;
        var lULeg = BoxBone(RagdollJointNames.LeftUpperLeg, pelvis.transform, cfg.upperLeg, cfg.mULeg,
                             new Vector3(-hipX, -(cfg.pelvis.y * 0.5f + cfg.upperLeg.y * 0.5f + cfg.separation), 0));
        var rULeg = BoxBone(RagdollJointNames.RightUpperLeg, pelvis.transform, cfg.upperLeg, cfg.mULeg,
                             new Vector3(+hipX, -(cfg.pelvis.y * 0.5f + cfg.upperLeg.y * 0.5f + cfg.separation), 0));

        var lLLeg = BoxBone(RagdollJointNames.LeftLowerLeg, lULeg.transform, cfg.lowerLeg, cfg.mLLeg,
                             new Vector3(0, -(cfg.upperLeg.y * 0.5f + cfg.lowerLeg.y * 0.5f + cfg.separation), 0));
        var rLLeg = BoxBone(RagdollJointNames.RightLowerLeg, rULeg.transform, cfg.lowerLeg, cfg.mLLeg,
                             new Vector3(0, -(cfg.upperLeg.y * 0.5f + cfg.lowerLeg.y * 0.5f + cfg.separation), 0));

        var lFoot = SphereBone(RagdollJointNames.LeftFoot, lLLeg.transform, cfg.footRadius, cfg.mFoot,
                             new Vector3(0, -(cfg.lowerLeg.y * 0.5f + cfg.footRadius + cfg.separation), cfg.footRadius * 0.25f));
        var rFoot = SphereBone(RagdollJointNames.RightFoot, rLLeg.transform, cfg.footRadius, cfg.mFoot,
                             new Vector3(0, -(cfg.lowerLeg.y * 0.5f + cfg.footRadius + cfg.separation), cfg.footRadius * 0.25f));

        // ---- joints (anchors from world-space surfaces) ----
        Spherical(chest, pelvis, WorldBottom(chest), WorldTop(pelvis), cfg.spineBend, cfg);
        Spherical(head, chest, WorldBottom(head), WorldTop(chest), cfg.neckSwing, cfg);

        // Shoulder joints: Connect upper arm inner surface to chest outer surface
        // Use direct surface calculations for proper anchor positioning
        Spherical(lUArm, chest, Surface(lUArm, Vector3.right), Surface(chest, Vector3.left), cfg.shoulderSwing, cfg);
        Spherical(rUArm, chest, Surface(rUArm, Vector3.left), Surface(chest, Vector3.right), cfg.shoulderSwing, cfg);

        // Elbow joints: Connect the correct ends to avoid anchor sign issues
        // For left arm: +X end of lower arm connects to -X end of upper arm
        // For right arm: -X end of lower arm connects to +X end of upper arm  
        // Spherically hinge for (semi-)natural elbow bending motion (arms bend up/down around vertical axis)
        Spherical(lLArm, lUArm, Surface(lLArm, Vector3.right), Surface(lUArm, Vector3.left), cfg.elbowFlex, cfg);
        Spherical(rLArm, rUArm, Surface(rLArm, Vector3.left), Surface(rUArm, Vector3.right), cfg.elbowFlex, cfg);

        Spherical(lULeg, pelvis, WorldTop(lULeg), WorldBottom(pelvis), cfg.hipSwing, cfg);
        Spherical(rULeg, pelvis, WorldTop(rULeg), WorldBottom(pelvis), cfg.hipSwing, cfg);

        HingeX(lLLeg, lULeg, WorldTop(lLLeg), WorldBottom(lULeg), 0f, cfg.kneeFlex, cfg); // knees
        HingeX(rLLeg, rULeg, WorldTop(rLLeg), WorldBottom(rULeg), 0f, cfg.kneeFlex, cfg);

        HingeX(lFoot, lLLeg, WorldTop(lFoot), WorldBottom(lLLeg), cfg.ankleLow, cfg.ankleHigh, cfg);
        HingeX(rFoot, rLLeg, WorldTop(rFoot), WorldBottom(rLLeg), cfg.ankleLow, cfg.ankleHigh, cfg);

        return root;
    }

    // ---------- WORLD-SURFACE HELPERS ----------
    static Vector3 WorldTop(GameObject go) => Surface(go, Vector3.up);
    static Vector3 WorldBottom(GameObject go) => Surface(go, Vector3.down);
    static Vector3 WorldOuterX(GameObject go, int sign) => Surface(go, sign < 0 ? Vector3.left : Vector3.right);
    static Vector3 WorldInnerX(GameObject go, int sign) => Surface(go, sign < 0 ? Vector3.right : Vector3.left);

    static Vector3 Surface(GameObject go, Vector3 worldDir)
    {
        var t = go.transform;
        if (go.TryGetComponent(out BoxCollider box))
        {
            // convert the world direction to the bone's local space to pick the correct face
            Vector3 dL = t.InverseTransformDirection(worldDir.normalized);
            Vector3 half = box.size * 0.5f;
            Vector3 local = new Vector3(
                Mathf.Sign(dL.x) * half.x,
                Mathf.Sign(dL.y) * half.y,
                Mathf.Sign(dL.z) * half.z
            ) + box.center;
            return t.TransformPoint(local);
        }
        if (go.TryGetComponent(out SphereCollider sph))
        {
            float r = sph.radius;
            // move a bit outside so anchors aren't inside the collider
            return t.position + worldDir.normalized * (r + 0.0005f);
        }
        return t.position;
    }

    // ---------- JOINT BUILDERS ----------
    static void CommonJointTuning(ConfigurableJoint j)
    {
        j.autoConfigureConnectedAnchor = false;
        j.enablePreprocessing = true;
        j.projectionMode = JointProjectionMode.PositionAndRotation;
        j.projectionDistance = 0.02f;
        j.projectionAngle = 10f;
    }

    static void SetAnchors(ConfigurableJoint j, Vector3 worldOnChild, Vector3 worldOnParent)
    {
        j.anchor = j.transform.InverseTransformPoint(worldOnChild);
        j.connectedAnchor = j.connectedBody.transform.InverseTransformPoint(worldOnParent);
    }

    static ConfigurableJoint Spherical(GameObject child, GameObject parent, Vector3 wChild, Vector3 wParent, float limitDeg, Cfg c)
    {
        var cj = child.AddComponent<ConfigurableJoint>();
        cj.connectedBody = parent.GetComponent<Rigidbody>();
        CommonJointTuning(cj); SetAnchors(cj, wChild, wParent);

        cj.xMotion = cj.yMotion = cj.zMotion = ConfigurableJointMotion.Locked;
        cj.angularXMotion = cj.angularYMotion = cj.angularZMotion = ConfigurableJointMotion.Limited;

        cj.lowAngularXLimit = new SoftJointLimit { limit = -limitDeg };
        cj.highAngularXLimit = new SoftJointLimit { limit = limitDeg };
        cj.angularYLimit = new SoftJointLimit { limit = limitDeg };
        cj.angularZLimit = new SoftJointLimit { limit = limitDeg };

        cj.rotationDriveMode = RotationDriveMode.Slerp;
        cj.slerpDrive = new JointDrive { positionSpring = c.spring, positionDamper = c.damper, maximumForce = c.maxForce };

        return cj;
    }

    static ConfigurableJoint HingeX(GameObject child, GameObject parent, Vector3 wChild, Vector3 wParent,
                       float lowDeg, float highDeg, Cfg c)
    {
        var cj = child.AddComponent<ConfigurableJoint>();
        cj.connectedBody = parent.GetComponent<Rigidbody>();
        CommonJointTuning(cj); SetAnchors(cj, wChild, wParent);

        cj.axis = Vector3.right; cj.secondaryAxis = Vector3.up;

        cj.xMotion = cj.yMotion = cj.zMotion = ConfigurableJointMotion.Locked;
        cj.angularXMotion = ConfigurableJointMotion.Limited;
        cj.angularYMotion = ConfigurableJointMotion.Locked;
        cj.angularZMotion = ConfigurableJointMotion.Locked;

        cj.lowAngularXLimit = new SoftJointLimit { limit = lowDeg };
        cj.highAngularXLimit = new SoftJointLimit { limit = highDeg };

        cj.rotationDriveMode = RotationDriveMode.XYAndZ;
        cj.angularXDrive = new JointDrive { positionSpring = c.spring, positionDamper = c.damper, maximumForce = c.maxForce };

        return cj;
    }

    static ConfigurableJoint HingeZ(GameObject child, GameObject parent, Vector3 wChild, Vector3 wParent,
                       float lowDeg, float highDeg, Cfg c)
    {
        var cj = child.AddComponent<ConfigurableJoint>();
        cj.connectedBody = parent.GetComponent<Rigidbody>();
        CommonJointTuning(cj);
        SetAnchors(cj, wChild, wParent);

        cj.axis = Vector3.forward; cj.secondaryAxis = Vector3.up;

        cj.xMotion = cj.yMotion = cj.zMotion = ConfigurableJointMotion.Locked;
        cj.angularXMotion = ConfigurableJointMotion.Locked;
        cj.angularYMotion = ConfigurableJointMotion.Locked;
        cj.angularZMotion = ConfigurableJointMotion.Limited;

        // Use symmetric limit magnitude; your controller can target a resting bend.
        cj.angularZLimit = new SoftJointLimit { limit = Mathf.Max(Mathf.Abs(lowDeg), Mathf.Abs(highDeg)) };

        cj.rotationDriveMode = RotationDriveMode.Slerp;
        cj.slerpDrive = new JointDrive { positionSpring = c.spring, positionDamper = c.damper, maximumForce = c.maxForce };

        return cj;
    }

    static ConfigurableJoint HingeY(GameObject child, GameObject parent, Vector3 wChild, Vector3 wParent,
                       float lowDeg, float highDeg, Cfg c)
    {
        var cj = child.AddComponent<ConfigurableJoint>();
        cj.connectedBody = parent.GetComponent<Rigidbody>();
        CommonJointTuning(cj);
        SetAnchors(cj, wChild, wParent);

        cj.axis = Vector3.up; cj.secondaryAxis = Vector3.right;

        cj.xMotion = cj.yMotion = cj.zMotion = ConfigurableJointMotion.Locked;
        cj.angularXMotion = ConfigurableJointMotion.Locked;
        cj.angularYMotion = ConfigurableJointMotion.Limited;
        cj.angularZMotion = ConfigurableJointMotion.Locked;

        // For Y-axis rotation, use the symmetric angularYLimit
        cj.angularYLimit = new SoftJointLimit { limit = Mathf.Max(Mathf.Abs(lowDeg), Mathf.Abs(highDeg)) };

        cj.rotationDriveMode = RotationDriveMode.XYAndZ;
        cj.angularYZDrive = new JointDrive { positionSpring = c.spring, positionDamper = c.damper, maximumForce = c.maxForce };

        return cj;
    }


}
