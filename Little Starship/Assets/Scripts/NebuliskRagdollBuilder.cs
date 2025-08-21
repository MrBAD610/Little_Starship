using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

public class NebuliskRagdollBuilder : MonoBehaviour
{
    [Header("Total Mass Distribution (Proportional)")]
    public bool useTotalMass = true;
    [Tooltip("Total mass of the whole Nebulisk (kg). All rigidbodies are scaled to sum to this.")]
    public float totalMass = 8f;

    [Tooltip("Fraction of totalMass for head (0..1).")]
    [Range(0f, 1f)] public float headFrac = 0.08f;
    [Tooltip("Fraction of totalMass for spine (0..1).")]
    [Range(0f, 1f)] public float spineFrac = 0.62f;
    [Tooltip("Fraction of totalMass for all legs combined (0..1).")]
    [Range(0f, 1f)] public float legsFrac = 0.30f;

    [Tooltip("Minimum per-Rigidbody mass clamp to avoid unstable tiny masses.")]
    public float minRBMass = 0.01f;

    [Header("Spine Mass Taper (0=root → 1=tail)")]
    public AnimationCurve spineTaper = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.25f);

    [Header("Naming")]
    public string[] spineNames = { "spine0", "spine1", "spine2", "spine3", "spine4", "spine5", "spine6", "spine7", "spine8" };
    public string headName = "head";
    public string jawName = "jaw";
    [Tooltip("Suffix used by end-marker bones (e.g., jaw_end, spine8_end, firstleg1_Left_end).")]
    public string endSuffix = "_end";
    [Tooltip("If true, collider lengths prefer the distance to the bone's *_end child when present.")]
    public bool useEndBonesForLength = true;

    [Header("End Bone Safety")]
    [Tooltip("If true, automatically remove RBs/Colliders/Joints from any *_end bones (IK-only).")]
    public bool stripPhysicsFromEndBones = true;

    [Header("Spine Geometry & Joints")]
    public float spineRadius = 0.06f;       // now purely radius intent
    public float spineLengthScale = 0.9f;   // scales measured spine length only
    // Legacy per-segment masses (used if useTotalMass=false)
    public float spineRootMass = 0.7f;
    public float spineTailMass = 0.15f;
    public float spinePitch = 15f;  // AngularX ±
    public float spineSide = 24f;   // AngularZ ±
    public float spineTwist = 10f;  // AngularY ±
    public float spineSpring = 45f;
    public float spineDamper = 3f;

    [Header("Head & Jaw")]
    // Legacy mass when not proportional:
    public float headMass = 0.4f;
    public bool buildJawHinge = true;
    public Vector2 jawOpenRange = new Vector2(0f, 35f);

    [Header("Leg Groups (name pairs: root -> tip)")]
    public string[][] legPairs = new string[][]
    {
        new []{"firstleg0_Left","firstleg1_Left"},
        new []{"firstleg0_Right","firstleg1_Right"},
        new []{"secondleg0_Left","secondleg1_Left"},
        new []{"secondleg0_Right","secondleg1_Right"},
        new []{"thirdleg0_Left","thirdleg1_Left"},
        new []{"thirdleg0_Right","thirdleg1_Right"},
        new []{"backleg0_Left","backleg1_Left"},
        new []{"backleg0_Right","backleg1_Right"},
    };
    public float legRadius = 0.03f;  // now purely radius intent
    // Legacy masses when not proportional:
    public float leg0Mass = 0.22f;
    public float leg1Mass = 0.12f;
    public float hipAbAd = 20f;   // swing sideways (±)
    public float hipFlex = 45f;   // pitch (±)
    public float hipTwist = 10f;  // twist (±)
    public Vector2 kneeRange = new Vector2(0f, 90f); // hinge degrees

    [Header("Collider Sizing")]
    [Tooltip("If true, height comes from bone length and radius is clamped to ≤ height/2. If false, radius may push height up to keep height ≥ 2*radius.")]
    public bool prioritizeLengthOverRadius = true;
    [Tooltip("Minimum capsule length in world units to avoid degenerate colliders.")]
    public float minCapsuleLength = 0.03f;

    [Header("Joint General")]
    public bool useProjection = true;
    public float projDistance = 0.08f;
    public float projAngle = 10f;

    [Header("Gravity Toggle")]
    [Tooltip("If enabled, all ragdoll rigidbodies will use gravity. You can switch this at runtime or in the editor.")]
    public bool gravityEnabled = true;

    // ---------- Deep name lookup cache ----------
    Dictionary<string, Transform> nameToTransform;

    void BuildNameLookup()
    {
        nameToTransform = new Dictionary<string, Transform>();
        var all = GetComponentsInChildren<Transform>(true);
        foreach (var t in all)
        {
            if (nameToTransform.ContainsKey(t.name))
            {
                Debug.LogWarning($"[NebuliskRagdollBuilder] Duplicate name found: '{t.name}'. Using the first encountered under '{nameToTransform[t.name].GetHierarchyPath()}'. Duplicate under '{t.GetHierarchyPath()}'.");
                continue;
            }
            nameToTransform[t.name] = t;
        }
    }

    Transform FindT(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (nameToTransform == null) BuildNameLookup();
        nameToTransform.TryGetValue(name, out var t);
        return t;
    }

    Transform FindEndChild(Transform t)
    {
        if (!t || string.IsNullOrEmpty(endSuffix)) return null;
        return t.Find(t.name + endSuffix); // direct child named "<name>_end"
    }

    // -------- Collider sizing helpers (smart axis & scaling) --------

    // Pick axis (0=X,1=Y,2=Z) that most points toward a target in local space, and keep its sign.
    (int axis, Vector3 localUnitAxis) DominantAxisTo(Transform t, Vector3 worldTargetPos)
    {
        Vector3 vLocal = t.InverseTransformPoint(worldTargetPos);
        Vector3 av = new Vector3(Mathf.Abs(vLocal.x), Mathf.Abs(vLocal.y), Mathf.Abs(vLocal.z));
        int axis = (av.x > av.y && av.x > av.z) ? 0 : (av.y > av.z ? 1 : 2);
        float signed = (axis == 0) ? vLocal.x : (axis == 1 ? vLocal.y : vLocal.z);
        Vector3 dir = (axis == 0) ? Vector3.right : (axis == 1 ? Vector3.up : Vector3.forward);
        return (axis, dir * Mathf.Sign(signed == 0 ? 1f : signed));
    }

    // Capsule height is in local units; divide desired world length by lossy scale on that axis.
    float AxisLocalScale(Transform t, int axis)
    {
        var s = t.lossyScale;
        return axis == 0 ? Mathf.Abs(s.x) : axis == 1 ? Mathf.Abs(s.y) : Mathf.Abs(s.z);
    }

    // For radius conversion, get the max scale in the plane perpendicular to the capsule axis.
    float PerpMaxScale(Transform t, int axis)
    {
        var s = t.lossyScale;
        float ax = Mathf.Abs(s.x), ay = Mathf.Abs(s.y), az = Mathf.Abs(s.z);
        if (axis == 0) return Mathf.Max(ay, az);   // capsule along X ⇒ radius lives in Y/Z
        if (axis == 1) return Mathf.Max(ax, az);   // along Y ⇒ radius in X/Z
        return Mathf.Max(ax, ay);                  // along Z ⇒ radius in X/Y
    }

    // Measure toward an end or fallback target; also return the aim position.
    (float worldLen, Vector3 targetWorldPos) MeasureToward(Transform bone, Transform fallbackOther, float defaultLen)
    {
        if (!bone) return (defaultLen, Vector3.zero);
        var end = (useEndBonesForLength) ? FindEndChild(bone) : null;

        if (end) return (Vector3.Distance(bone.position, end.position), end.position);
        if (fallbackOther) return (Vector3.Distance(bone.position, fallbackOther.position), fallbackOther.position);

        // Last resort: guess along current forward
        return (defaultLen, bone.position + bone.forward * defaultLen);
    }

    // Smart capsule builder that chooses axis, converts world→local height/radius, and biases center.
    void AddCapsuleSmart(Transform t, float desiredWorldRadius, float desiredWorldLength,
                         Vector3 aimWorldPos, float centerBias = 0.25f)
    {
        var col = t.GetComponent<CapsuleCollider>() ?? t.gameObject.AddComponent<CapsuleCollider>();
        var (axis, localUnit) = DominantAxisTo(t, aimWorldPos);
        col.direction = axis;

        // World → local conversions
        float axisScale = Mathf.Max(0.0001f, AxisLocalScale(t, axis));
        float perpScale = Mathf.Max(0.0001f, PerpMaxScale(t, axis));

        float localHeightFromLen = Mathf.Max(desiredWorldLength / axisScale, minCapsuleLength / axisScale);
        float localRadiusRaw = Mathf.Max(0.002f, desiredWorldRadius / perpScale);

        float localHeight, localRadius;
        if (prioritizeLengthOverRadius)
        {
            // Height is authoritative; clamp radius to ≤ height/2 - epsilon
            localHeight = localHeightFromLen;
            float maxRadius = Mathf.Max(0.001f, localHeight * 0.5f - 1e-4f);
            localRadius = Mathf.Min(localRadiusRaw, maxRadius);
        }
        else
        {
            // Old behavior: allow radius to push height up to keep height ≥ 2*radius
            localRadius = localRadiusRaw;
            localHeight = Mathf.Max(localHeightFromLen, localRadius * 2f);
        }

        col.height = localHeight;
        col.radius = localRadius;
        col.center = localUnit.normalized * (localHeight * centerBias * 0.5f);
    }

    // --- End-bone sanitizing (optional) ---
    void StripEndBonePhysics(Transform t)
    {
        if (!t) return;
        var rb = t.GetComponent<Rigidbody>(); if (rb) DestroyImmediate(rb, true);
        foreach (var col in t.GetComponents<Collider>()) DestroyImmediate(col, true);
        foreach (var j in t.GetComponents<Joint>()) DestroyImmediate(j, true);
    }

    void SanitizeAllEndBones(Transform[] spine)
    {
        if (!stripPhysicsFromEndBones) return;

        // spine ends (e.g., spine8_end)
        foreach (var s in spine)
        {
            var end = FindEndChild(s);
            if (end) StripEndBonePhysics(end);
        }

        // head/jaw ends
        var headT = FindT(headName);
        if (headT)
        {
            var headEnd = FindEndChild(headT); if (headEnd) StripEndBonePhysics(headEnd);
        }
        var jawT = FindT(jawName);
        if (jawT)
        {
            var jawEnd = FindEndChild(jawT); if (jawEnd) StripEndBonePhysics(jawEnd);
        }

        // leg tip ends (leg1_*_end)
        foreach (var pair in legPairs)
        {
            var tip = FindT(pair[1]);
            if (tip)
            {
                var tipEnd = FindEndChild(tip); if (tipEnd) StripEndBonePhysics(tipEnd);
            }
        }
    }

    // --------- Rigidbody & joint helpers ---------
    Rigidbody AddRB(Transform t, float mass)
    {
        var rb = t.GetComponent<Rigidbody>() ?? t.gameObject.AddComponent<Rigidbody>();
        rb.mass = Mathf.Max(minRBMass, mass);
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.solverIterations = Mathf.Max(6, rb.solverIterations);
        rb.solverVelocityIterations = Mathf.Max(6, rb.solverVelocityIterations);
        rb.useGravity = gravityEnabled; // respect global toggle on creation
        return rb;
    }

    // find the first ancestor with a different Rigidbody than the child (guard for self-connect).
    Rigidbody FindAncestorDifferentRB(Transform child, Rigidbody fallback = null)
    {
        var self = child.GetComponent<Rigidbody>();
        for (var p = child.parent; p; p = p.parent)
        {
            var rb = p.GetComponent<Rigidbody>();
            if (rb && rb != self) return rb;
        }
        return fallback;
    }

    ConfigurableJoint CJ_RotOnly(Transform child, Rigidbody parentRB,
                                 float pitch, float side, float twist, float spring, float damper)
    {
        var cj = child.GetComponent<ConfigurableJoint>() ?? child.gameObject.AddComponent<ConfigurableJoint>();

        // Guard: never allow self-connection; try to auto-fix if needed
        var childRB = child.GetComponent<Rigidbody>();
        if (parentRB == null || parentRB == childRB)
        {
            parentRB = FindAncestorDifferentRB(child, null);
            if (parentRB == null)
            {
                Debug.LogError($"[NebuliskRagdollBuilder] No valid parent Rigidbody for joint on {child.GetHierarchyPath()} – skipping joint.");
#if UNITY_EDITOR
                DestroyImmediate(cj, true);
#else
                Destroy(cj);
#endif
                return null;
            }
        }

        cj.connectedBody = parentRB;
        cj.xMotion = cj.yMotion = cj.zMotion = ConfigurableJointMotion.Locked;
        cj.angularXMotion = ConfigurableJointMotion.Limited;
        cj.angularYMotion = ConfigurableJointMotion.Limited;
        cj.angularZMotion = ConfigurableJointMotion.Limited;

        cj.lowAngularXLimit = new SoftJointLimit { limit = -pitch };
        cj.highAngularXLimit = new SoftJointLimit { limit = pitch };
        cj.angularZLimit = new SoftJointLimit { limit = side };
        cj.angularYLimit = new SoftJointLimit { limit = twist };

        var sYZ = new SoftJointLimitSpring { spring = spring, damper = damper };
        cj.angularYZLimitSpring = sYZ;
        cj.angularXLimitSpring = new SoftJointLimitSpring { spring = spring, damper = damper };

        cj.enableCollision = false;
        cj.enablePreprocessing = true;
        cj.projectionMode = useProjection ? JointProjectionMode.PositionAndRotation : JointProjectionMode.None;
        cj.projectionDistance = projDistance;
        cj.projectionAngle = projAngle;
        return cj;
    }

    // --------- Gravity control (public API) ---------
    public void SetGravity(bool enabled)
    {
        gravityEnabled = enabled;
        ApplyGravityToAll(enabled);
    }

    public void ApplyGravityToAll(bool enabled)
    {
        var rbs = GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
            if (rb) rb.useGravity = enabled;
    }

    [ContextMenu("Enable Gravity (All RBs)")]
    void CtxEnableGravity() => SetGravity(true);

    [ContextMenu("Disable Gravity (All RBs)")]
    void CtxDisableGravity() => SetGravity(false);

    // --------- Build ---------
    public void Build()
    {
        BuildNameLookup(); // deep name cache

        // Collect spine by name (deep)
        Transform[] spine = new Transform[spineNames.Length];
        for (int i = 0; i < spineNames.Length; i++)
        {
            var t = FindT(spineNames[i]);
            if (!t) { Debug.LogError($"Missing spine bone '{spineNames[i]}'"); return; }
            spine[i] = t;
        }
        var head = FindT(headName);

        // Ensure *_end bones have no physics
        SanitizeAllEndBones(spine);

        // ----- Mass planning -----
        float[] spineMasses = new float[spine.Length];
        float headMassFinal;
        float leg0MassFinal = leg0Mass;
        float leg1MassFinal = leg1Mass;

        if (useTotalMass)
        {
            float sumFracs = Mathf.Max(0.0001f, headFrac + spineFrac + legsFrac);
            float headBudget = totalMass * (headFrac / sumFracs);
            float spineBudget = totalMass * (spineFrac / sumFracs);
            float legsBudget = totalMass * (legsFrac / sumFracs);

            float wsum = 0f;
            for (int i = 0; i < spine.Length; i++)
            {
                float t = (spine.Length == 1) ? 0f : (float)i / (spine.Length - 1);
                float w = Mathf.Max(0.0001f, spineTaper.Evaluate(t));
                spineMasses[i] = w; wsum += w;
            }
            for (int i = 0; i < spine.Length; i++)
                spineMasses[i] = (spineMasses[i] / wsum) * spineBudget;

            headMassFinal = head ? Mathf.Max(minRBMass, headBudget) : 0f;

            int totalLegBones = 0;
            foreach (var pair in legPairs) if (FindT(pair[0]) && FindT(pair[1])) totalLegBones += 2;
            if (totalLegBones > 0)
            {
                float per = Mathf.Max(minRBMass, legsBudget / totalLegBones);
                leg0MassFinal = per;
                leg1MassFinal = per;
            }
        }
        else
        {
            for (int i = 0; i < spine.Length; i++)
            {
                float alpha = (float)i / Mathf.Max(1, spine.Length - 1);
                spineMasses[i] = Mathf.Lerp(spineRootMass, spineTailMass, alpha);
            }
            headMassFinal = headMass;
        }

        // ----- Spine build -----
        for (int i = 0; i < spine.Length; i++)
        {
            var me = spine[i];
            Transform next = (i < spine.Length - 1) ? spine[i + 1] : null;
            Transform prev = (i > 0) ? spine[i - 1] : null;

            var (segWorldLen, segAimPos) = MeasureToward(me, next ? next : prev, spineRadius * 2f);
            AddCapsuleSmart(me, spineRadius, segWorldLen * spineLengthScale, segAimPos, 0.25f);

            var rb = AddRB(me, spineMasses[i]);

            if (i > 0)
            {
                CJ_RotOnly(me, spine[i - 1].GetComponent<Rigidbody>(),
                           spinePitch, spineSide, spineTwist, spineSpring, spineDamper);
            }
        }

        // ----- Head & jaw -----
        if (head)
        {
            var (headWorldLen, headAim) = MeasureToward(head, spine[0], spineRadius * 1.5f);
            AddCapsuleSmart(head, spineRadius * 0.9f, headWorldLen * 0.8f, headAim, 0.15f);
            var rbHead = AddRB(head, headMassFinal);
            CJ_RotOnly(head, spine[0].GetComponent<Rigidbody>(),
                       Mathf.Min(12f, spinePitch), Mathf.Min(15f, spineSide), Mathf.Min(8f, spineTwist),
                       spineSpring, spineDamper);

            var jaw = FindT(jawName);
            if (jaw && buildJawHinge)
            {
                var (jawLen, jawAim) = MeasureToward(jaw, null, 0.08f);
                AddCapsuleSmart(jaw, spineRadius * 0.6f, jawLen, jawAim, 0f);
                var rbJaw = AddRB(jaw, Mathf.Max(minRBMass, headMassFinal * 0.25f));
                var h = jaw.GetComponent<HingeJoint>() ?? jaw.gameObject.AddComponent<HingeJoint>();
                h.connectedBody = rbHead;
                h.useLimits = true;
                h.limits = new JointLimits { min = jawOpenRange.x, max = jawOpenRange.y };
                h.useSpring = true;
                h.spring = new JointSpring { spring = 200f, damper = 8f, targetPosition = 0f };
            }
        }

        // ----- Legs -----
        foreach (var pair in legPairs)
        {
            var root = FindT(pair[0]); var tip = FindT(pair[1]);
            if (!root || !tip) { Debug.LogWarning($"Missing leg bones '{pair[0]}' or '{pair[1]}'"); continue; }

            var parentRB = root.parent ? (root.parent.GetComponent<Rigidbody>() ?? root.parent.GetComponentInParent<Rigidbody>()) : null;
            if (!parentRB) parentRB = FindT(spineNames[0]).GetComponent<Rigidbody>();

            float rootToTip = Vector3.Distance(root.position, tip.position);

            // Root: size toward tip (height purely from length; radius no longer influences height)
            AddCapsuleSmart(root, legRadius, rootToTip * 0.55f, tip.position, 0.3f);
            var rb0 = AddRB(root, leg0MassFinal);
            CJ_RotOnly(root, parentRB, hipFlex, hipAbAd, hipTwist, 40f, 3f);

            // Tip: size toward leg1_end if present (same decoupled behavior)
            var (tipLen, tipAim) = MeasureToward(tip, null, rootToTip * 0.45f);
            AddCapsuleSmart(tip, legRadius * 0.9f, tipLen, tipAim, 0.3f);
            var rb1 = AddRB(tip, leg1MassFinal);

            var hj = tip.GetComponent<HingeJoint>() ?? tip.gameObject.AddComponent<HingeJoint>();
            if (rb0 == rb1)
            {
                Debug.LogError($"[NebuliskRagdollBuilder] Knee hinge would self-connect on {tip.GetHierarchyPath()} – skipping.");
#if UNITY_EDITOR
                DestroyImmediate(hj, true);
#else
                Destroy(hj);
#endif
            }
            else
            {
                hj.connectedBody = rb0;
                hj.useLimits = true;
                hj.limits = new JointLimits { min = kneeRange.x, max = kneeRange.y };
                hj.useSpring = true;
                hj.spring = new JointSpring { spring = 80f, damper = 6f, targetPosition = 0f };
                hj.enableCollision = false;
            }
        }

        // Ensure gravity setting is applied to any pre-existing RBs too
        ApplyGravityToAll(gravityEnabled);

        Debug.Log(useTotalMass
            ? $"Nebulisk ragdoll built with proportional masses. totalMass={totalMass:0.##} kg (smart collider sizing; end bones for lengths: {useEndBonesForLength})"
            : $"Nebulisk ragdoll built (legacy fixed masses). (smart collider sizing; end bones for lengths: {useEndBonesForLength})");
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NebuliskRagdollBuilder))]
    public class BuilderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var b = (NebuliskRagdollBuilder)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ragdoll Gravity", EditorStyles.boldLabel);
            bool newGravity = EditorGUILayout.Toggle("Enable Gravity", b.gravityEnabled);
            if (newGravity != b.gravityEnabled)
            {
                Undo.RecordObject(b, "Toggle Ragdoll Gravity");
                b.SetGravity(newGravity);
                EditorUtility.SetDirty(b);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Enable Gravity Now")) { b.SetGravity(true); EditorUtility.SetDirty(b); }
            if (GUILayout.Button("Disable Gravity Now")) { b.SetGravity(false); EditorUtility.SetDirty(b); }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            if (GUILayout.Button("Build / Rebuild Nebulisk Ragdoll"))
                b.Build();
        }
    }
#endif
}

// -------- helper to print full path in warnings --------
static class TransformPathExt
{
    public static string GetHierarchyPath(this Transform t)
    {
        if (!t) return "<null>";
        System.Text.StringBuilder sb = new System.Text.StringBuilder(t.name);
        var p = t.parent;
        while (p) { sb.Insert(0, p.name + "/"); p = p.parent; }
        return sb.ToString();
    }
}
