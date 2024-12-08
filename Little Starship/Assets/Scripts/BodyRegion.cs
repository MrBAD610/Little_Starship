using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyRegion : MonoBehaviour
{
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

    public RegionType regionType;
    public InjuryStatus regionStatus;
    public float stabilizationTime;
    public float stabilizationSpeed;
    public float stabilizationProgress;

    private void Awake() { }
    private void Update() { }
}
