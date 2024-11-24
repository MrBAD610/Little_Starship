using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Medical Emergency")]
public class MedicalEmergency : ScriptableObject
{
    public string emergencyName;    // What the emergency is
    public List<BodyRegion> presetAffectedRegions; // list of preset regions affected by the medical emergency
    public List<BodyRegion> randomAffectedRegions; // list of possible regions affected by the medical emergency
    public float stabilizationTime; // Time to stabilize this emergency
}
