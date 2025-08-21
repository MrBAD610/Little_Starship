using UnityEngine;
using System.Collections.Generic;

/// Clamp a ragdoll's world Y position to optional [minY, maxY] bounds.
/// You can toggle the whole system, or individually toggle min/max.
/// Apply as a separate component on the Nebulisk prefab.
[DefaultExecutionOrder(50)]
public class RagdollVerticalRange : MonoBehaviour
{
    [Header("Master Switch")]
    public bool enabledRange = true;

    [Header("World Y Bounds")]
    public bool enableMinBound = true;
    public float minY = 0f;

    public bool enableMaxBound = true;
    public float maxY = 5f;

    public enum Scope { RootOnly, AllRigidbodies }
    [Tooltip("RootOnly: just the main body. AllRigidbodies: every RB under this object.")]
    public Scope scope = Scope.AllRigidbodies;

    [Header("Hit Behavior")]
    [Tooltip("If true, reflect vertical velocity at the bounds; otherwise zero it.")]
    public bool bounceAtLimits = false;
    [Range(0f, 1f)] public float restitution = 0.0f; // 0 = no bounce, 1 = perfectly elastic
    [Range(0f, 1f)] public float lateralDampingOnHit = 0.2f; // reduces X/Z when clamped

    [Header("Gizmos")]
    [Tooltip("Half-extent for the visual gizmo planes (XZ). Editor-only.")]
    public float gizmoExtent = 3f;
    [Tooltip("Draw a translucent band when both bounds are enabled.")]
    public bool drawBandWhenBothEnabled = true;

    // Cache
    readonly List<Rigidbody> _bodies = new List<Rigidbody>();
    Rigidbody _rootRB;

    void OnValidate()
    {
        restitution = Mathf.Clamp01(restitution);
        lateralDampingOnHit = Mathf.Clamp01(lateralDampingOnHit);
        if (enableMinBound && enableMaxBound && maxY < minY) maxY = minY;
    }

    void Awake() { Refresh(); }
    void OnEnable() { Refresh(); }

    /// Call if you add/remove bones at runtime.
    public void Refresh()
    {
        _bodies.Clear();
        _rootRB = null;

        if (scope == Scope.RootOnly)
        {
            _rootRB = GetComponent<Rigidbody>();
            if (_rootRB == null) _rootRB = GetComponentInChildren<Rigidbody>();
            if (_rootRB != null) _bodies.Add(_rootRB);
        }
        else
        {
            GetComponentsInChildren(true, _bodies);
        }
    }

    void FixedUpdate()
    {
        if (!enabledRange || _bodies.Count == 0) return;

        for (int i = 0; i < _bodies.Count; i++)
        {
            var rb = _bodies[i];
            if (!rb || rb.isKinematic) continue;

            Vector3 pos = rb.position;
            Vector3 vel = rb.velocity;

            bool below = enableMinBound && pos.y < minY;
            bool above = enableMaxBound && pos.y > maxY;

            if (!below && !above) continue;

            // Clamp position
            if (below) pos.y = minY;
            if (above) pos.y = maxY;

            // Adjust velocity on hit
            if (bounceAtLimits)
            {
                if (below && vel.y < 0f) vel.y = -vel.y * restitution;
                if (above && vel.y > 0f) vel.y = -vel.y * restitution;
            }
            else
            {
                if (below && vel.y < 0f) vel.y = 0f;
                if (above && vel.y > 0f) vel.y = 0f;
            }

            // Optional lateral damping so it doesn't "ice skate" along the clamp
            vel.x *= (1f - lateralDampingOnHit);
            vel.z *= (1f - lateralDampingOnHit);

            rb.MovePosition(pos);
            rb.velocity = vel;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        var c = transform.position;
        var planeSize = new Vector3(gizmoExtent * 2f, 0.02f, gizmoExtent * 2f);

        if (enableMinBound)
        {
            var p = c; p.y = minY;
            Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
            Gizmos.DrawCube(p, planeSize);
        }

        if (enableMaxBound)
        {
            var p = c; p.y = maxY;
            Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
            Gizmos.DrawCube(p, planeSize);
        }

        // Optional band visualization only when both limits are enabled and ordered
        if (drawBandWhenBothEnabled && enableMinBound && enableMaxBound && maxY >= minY)
        {
            var mid = c; mid.y = (minY + maxY) * 0.5f;
            var bandSize = new Vector3(gizmoExtent * 2f, (maxY - minY), gizmoExtent * 2f);
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.08f);
            Gizmos.DrawCube(mid, bandSize);
        }
    }
#endif

    // Convenience context menu actions
    [ContextMenu("Enable All Bounds")]
    void CtxEnableAll() { enableMinBound = enableMaxBound = true; }

    [ContextMenu("Disable Min Bound (floor)")]
    void CtxDisableMin() { enableMinBound = false; }

    [ContextMenu("Disable Max Bound (ceiling)")]
    void CtxDisableMax() { enableMaxBound = false; }

    [ContextMenu("Disable All Bounds (no clamping)")]
    void CtxDisableAll() { enableMinBound = enableMaxBound = false; }
}
