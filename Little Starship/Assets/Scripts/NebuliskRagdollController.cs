using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Toggles a skinned creature between Animated (Animator drives bones)
/// and Ragdoll (physics drives bones). Plays nice with FImpossible Creations' animators.
[DisallowMultipleComponent]
[DefaultExecutionOrder(60)]
public class NebuliskRagdollController : MonoBehaviour
{
    public enum Mode { Animated, Ragdoll }

    [Header("Defaults")]
    public Mode startMode = Mode.Animated;
    [Tooltip("Disable bone colliders while animated (use separate hitboxes if needed).")]
    public bool disableBoneCollidersWhenAnimated = true;
    [Tooltip("Enable gravity automatically when entering ragdoll.")]
    public bool enableGravityInRagdoll = true;

    [Header("References")]
    public Animator animator;                   // auto-filled if null
    public NebuliskRagdollBuilder builder;      // optional; mirrors gravity toggle across all RBs

    [Header("3rd-Party Animators")]
    [Tooltip("Try to find and manage common FImpossible/FIMSpace animators automatically.")]
    public bool autoDetectFImpossible = true;
    [Tooltip("Any Behaviours here will be disabled in Ragdoll and re-enabled in Animated.")]
    public List<Behaviour> disableWhenRagdoll = new List<Behaviour>();

    [Header("Physics Isolation")]
    [Tooltip("If true, ConfigurableJoints are made inert (all motions Free) while Animated, then restored in Ragdoll.")]
    public bool freeConfigurableJointsWhenAnimated = true;

    // cached
    Rigidbody[] rbs;
    Collider[] cols;
    ConfigurableJoint[] cfgJoints;
    HingeJoint[] hingeJoints; // left untouched (RBs being kinematic already isolates them)
    bool initialized;
    Mode currentMode;

    // backup store for ConfigurableJoint settings we change
    struct CjBackup
    {
        public ConfigurableJointMotion x, y, z, ax, ay, az;
        public JointProjectionMode projMode;
        public float projDist, projAngle;
        public bool valid;
    }
    readonly Dictionary<ConfigurableJoint, CjBackup> cjBackups = new Dictionary<ConfigurableJoint, CjBackup>();

    // ---------- Unity lifecycle ----------
    void Reset()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!builder) builder = GetComponent<NebuliskRagdollBuilder>()
                             ?? GetComponentInChildren<NebuliskRagdollBuilder>()
                             ?? GetComponentInParent<NebuliskRagdollBuilder>();
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (!animator) animator = GetComponentInChildren<Animator>();
            if (!builder) builder = GetComponent<NebuliskRagdollBuilder>()
                                 ?? GetComponentInChildren<NebuliskRagdollBuilder>()
                                 ?? GetComponentInParent<NebuliskRagdollBuilder>();
        }
    }

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!builder) builder = GetComponent<NebuliskRagdollBuilder>();
        CacheParts();
        if (autoDetectFImpossible) DetectThirdPartyAnimators();
    }

    void Start()
    {
        SetMode(startMode, immediate: true);
    }

    void CacheParts()
    {
        rbs = GetComponentsInChildren<Rigidbody>(true);
        cols = GetComponentsInChildren<Collider>(true);
        cfgJoints = GetComponentsInChildren<ConfigurableJoint>(true);
        hingeJoints = GetComponentsInChildren<HingeJoint>(true);
        initialized = true;
    }

    void DetectThirdPartyAnimators()
    {
        var behaviours = GetComponentsInChildren<Behaviour>(true);
        foreach (var b in behaviours)
        {
            if (!b || b is Animator) continue;
            var t = b.GetType();
            string n = t.Name;
            string ns = t.Namespace ?? string.Empty;

            bool looksLikeFImpossible =
                ns.Contains("FIMSpace") || ns.Contains("FImpossible") ||
                n.Contains("LegsAnimator") || n.Contains("LegAnimator") ||
                n.Contains("SpineAnimator") || n.Contains("SpineAnimator2");

            if (looksLikeFImpossible && !disableWhenRagdoll.Contains(b))
                disableWhenRagdoll.Add(b);
        }
    }

    // ---------- Public API ----------
    [ContextMenu("Enter Ragdoll")]
    public void EnterRagdoll() => SetMode(Mode.Ragdoll, immediate: true);

    [ContextMenu("Exit Ragdoll (return to Animated)")]
    public void ExitRagdoll() => SetMode(Mode.Animated, immediate: true);

    /// Apply an impulse then ragdoll (nice for deaths/hit reactions).
    public void EnterRagdollWithImpulse(Vector3 worldPoint, Vector3 impulse, ForceMode mode = ForceMode.Impulse)
    {
        SetMode(Mode.Ragdoll, immediate: true);
        Rigidbody heaviest = null; float maxMass = -1f;
        foreach (var rb in rbs) if (rb && rb.mass > maxMass) { heaviest = rb; maxMass = rb.mass; }
        if (heaviest) heaviest.AddForceAtPosition(impulse, worldPoint, mode);
    }

    /// Smoothly blend from Animated to Ragdoll over t seconds (poses handoff).
    public void BlendToRagdoll(float seconds = 0.15f) => StartCoroutine(Co_BlendToRagdoll(seconds));

    // ---------- Core switch ----------
    public void SetMode(Mode target, bool immediate)
    {
        if (!initialized) CacheParts();
        currentMode = target;

        bool toRagdoll = (target == Mode.Ragdoll);

        // 1) Third-party procedural animators (disable in ragdoll, enable in animated)
        foreach (var b in disableWhenRagdoll) if (b) b.enabled = !toRagdoll;

        // 2) Animator on/off
        if (animator) animator.enabled = !toRagdoll;

        // 3) Colliders (bone colliders off while animated)
        foreach (var c in cols)
            if (c && c.attachedRigidbody)
                c.enabled = toRagdoll || !disableBoneCollidersWhenAnimated;

        // 4) ConfigurableJoints: make inert in Animated, restore in Ragdoll
        if (freeConfigurableJointsWhenAnimated)
        {
            if (toRagdoll) RestoreConfigurableJoints();
            else FreeConfigurableJoints();
        }

        // 5) Rigidbodies
        foreach (var rb in rbs)
        {
            if (!rb) continue;

            if (toRagdoll)
            {
                // Make dynamic first, then it's safe to touch velocities
                rb.isKinematic = false;
                rb.useGravity = enableGravityInRagdoll;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                // zero momentum while still dynamic (Unity forbids writing on kinematic)
                if (!rb.isKinematic)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                rb.useGravity = false;
                rb.isKinematic = true;
            }
        }

        // 6) Mirror the builder's global gravity toggle (applies to all RBs)
        if (builder) builder.SetGravity(toRagdoll ? enableGravityInRagdoll : false);

        // 7) When returning to Animated, force an immediate pose write to avoid stretch
        if (!toRagdoll && animator) animator.Update(0f);
    }

    // Make joints inert (no constraints) by freeing motions; store originals for restore
    void FreeConfigurableJoints()
    {
        for (int i = 0; i < cfgJoints.Length; i++)
        {
            var j = cfgJoints[i];
            if (!j) continue;

            if (!cjBackups.TryGetValue(j, out var bkp) || !bkp.valid)
            {
                bkp = new CjBackup
                {
                    x = j.xMotion,
                    y = j.yMotion,
                    z = j.zMotion,
                    ax = j.angularXMotion,
                    ay = j.angularYMotion,
                    az = j.angularZMotion,
                    projMode = j.projectionMode,
                    projDist = j.projectionDistance,
                    projAngle = j.projectionAngle,
                    valid = true
                };
                cjBackups[j] = bkp;
            }

            j.xMotion = j.yMotion = j.zMotion = ConfigurableJointMotion.Free;
            j.angularXMotion = j.angularYMotion = j.angularZMotion = ConfigurableJointMotion.Free;
            j.projectionMode = JointProjectionMode.None;
        }
    }

    // Restore motions/projection from backup
    void RestoreConfigurableJoints()
    {
        // Clean up any destroyed joints
        var toRemove = new List<ConfigurableJoint>();

        foreach (var kv in cjBackups)
        {
            var j = kv.Key;
            if (!j) { toRemove.Add(j); continue; }

            var b = kv.Value;
            if (!b.valid) continue;

            j.xMotion = b.x; j.yMotion = b.y; j.zMotion = b.z;
            j.angularXMotion = b.ax; j.angularYMotion = b.ay; j.angularZMotion = b.az;
            j.projectionMode = b.projMode; j.projectionDistance = b.projDist; j.projectionAngle = b.projAngle;
        }

        if (toRemove.Count > 0)
        {
            foreach (var dead in toRemove) cjBackups.Remove(dead);
        }
    }

    IEnumerator Co_BlendToRagdoll(float t)
    {
        if (t <= 0f) { EnterRagdoll(); yield break; }

        var bones = GetComponentsInChildren<Transform>(true);
        var poseLocal = new Dictionary<Transform, (Vector3 pos, Quaternion rot)>(bones.Length);
        foreach (var b in bones) poseLocal[b] = (b.localPosition, b.localRotation);

        // Disable animator & 3rd-party animators before physics takes over
        foreach (var beh in disableWhenRagdoll) if (beh) beh.enabled = false;
        if (animator) animator.enabled = false;

        // Ensure joints are active for ragdoll
        if (freeConfigurableJointsWhenAnimated) RestoreConfigurableJoints();

        // Turn physics on (non-kinematic) before writing velocities
        foreach (var rb in rbs) if (rb)
            {
                rb.isKinematic = false;
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        foreach (var c in cols) if (c && c.attachedRigidbody) c.enabled = true;

        float elapsed = 0f;
        while (elapsed < t)
        {
            float a = elapsed / t; // 0→1
            foreach (var bone in bones)
            {
                if (!poseLocal.TryGetValue(bone, out var p)) continue;
                // Keep bone close to the snapshot pose as physics wakes up
                bone.localPosition = Vector3.Lerp(p.pos, bone.localPosition, a);
                bone.localRotation = Quaternion.Slerp(p.rot, bone.localRotation, a);
            }
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Release to full ragdoll + gravity
        foreach (var rb in rbs) if (rb) rb.useGravity = enableGravityInRagdoll;
        if (builder) builder.SetGravity(enableGravityInRagdoll);

        currentMode = Mode.Ragdoll;
    }
}
