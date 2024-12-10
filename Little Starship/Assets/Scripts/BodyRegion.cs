using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a region of the body.
/// </summary>
[CreateAssetMenu(fileName = "New Body Region", menuName = "Body Region")]
public class BodyRegion : ScriptableObject
{
    /// <summary>
    /// Types of body regions.
    /// </summary>
    public enum RegionType
    {
        Head,       // the head
        Neck,       // the neck
        Chest,      // the chest area
        Left_Arm,   // the left arm
        Right_Arm,  // the right arm
        Midsection, // the midsection area
        Pelvis,     // the pelvis area
        Left_Leg,   // the left leg
        Right_Leg   // the right leg
    }

    /// <summary>
    /// The type of the body region.
    /// </summary>
    public RegionType bodyRegionType = RegionType.Head;

    /// <summary>
    /// The injury status of the body region.
    /// </summary>
    public InjuryStatus regionInjuryStatus = InjuryStatus.Unharmed;

    /// <summary>
    /// The stabilization time for the body region.
    /// </summary>
    public float stabilizationTime = 0.0f;

    /// <summary>
    /// The stabilization speed for the body region.
    /// </summary>
    public float stabilizationSpeed = 0.0f;

    /// <summary>
    /// The stabilization progress for the body region.
    /// </summary>
    public float stabilizationProgress = 0.0f;
}
