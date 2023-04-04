using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Armor : ScriptableObject
{
    [Range(0, 1)]
    public float physicalDamageModifier;
    [Range(0, 1)]
    public float lightDamageModifier;
    [Range(0, 1)]
    public float heavyDamageModifier;
    [Range(0, 1)]
    public float cartridgeDamageModifier;
    [Range(0, 1)]
    public float plasmaDamageModifier;
    [Range(0, 1)]
    public float explosiveDamageModifier;

    [Range(0, 1)]
    public float RadiationModifier;
    [Range(0, 1)]
    public float RadiationReductionModifier;

    [Range(0, 1)]
    public float temperatureModifier;
    [Range(0, 1)]
    public float suffocationModifier;
}
