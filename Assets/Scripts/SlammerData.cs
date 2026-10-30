using System;
using UnityEngine;

#region Design Enums
public enum SlammerRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}

public enum SlammerMaterial
{
    Plastic,
    Rubber,
    Wood,
    Iron,
    Brass,
    Steel,
    Titanium,
    Enchanted,
    Cosmic
}

public enum SlammerEffectType
{
    None,
    Rebound,
    TrueAim,
    HeavyImpact,
    RimStrike,
    DeepImpact,
    GyroSpin,
    LuckyFlash,
    MagnetPull,
    Freeze,
    Aftershock,
    ChainShock,
    Singularity,
    Wave,
    RootSnare,
    Corrode,
    SplitFlash,
    VoidFlip,
    Rewind,
    Rebirth,
    Reflection,
    Fortune,
    EssenceHarvest,
    FinalStand
}
#endregion

[Serializable]
public class SlammerStats
{
    [Tooltip("Affects Unity Rigidbody2D mass. Heavier slammers smash straight through stacks.")]
    [Range(1, 100)] public int weight = 15;

    [Tooltip("Reduces input drift variance. Higher values keep the slammer tracking perfectly straight.")]
    [Range(1, 100)] public int accuracy = 70;

    [Tooltip("Drives physical torque logic. High spin causes the slammer to cut diagonally or dance on impact.")]
    [Range(1, 100)] public int spin = 40;

    [Tooltip("Feeds PhysicsMaterial2D bounciness. Controls how high/far the slammer bounces off the floor or pogs.")]
    [Range(0, 100)] public int bounce = 50;

    [Tooltip("Resists mid-air deflection when clipping top pogs, keeping the trajectory reliable.")]
    [Range(1, 100)] public int stability = 50;

    [Tooltip("Directly scaling multiplier applied to the target landing outcome vectors of hit milkcaps.")]
    [Range(1, 100)] public int flipPower = 25;

    [Tooltip("Percentage chance to automatically convert a 'Random' or 'Tails' landing outcome to a forced 'Heads'.")]
    [Range(0, 100)] public int criticalChance = 2;

    [Tooltip("Scales special effect parameters (e.g., radius of Shockwaves, duration of Freezes).")]
    [Range(0, 100)] public int abilityPower = 0;
}

[CreateAssetMenu(fileName = "NewSlammerData", menuName = "Pogs/Slammer Config Data")]
public class SlammerData : ScriptableObject
{
    [Header("Slammer Identity")]
    public string slammerName = "Classic Aluminum";
    [TextArea(2, 5)] public string description = "A reliable, standard tournament slammer.";
    public Sprite slammerSprite;

    [Header("Classification Attributes")]
    public SlammerRarity rarity = SlammerRarity.Common;
    public SlammerMaterial material = SlammerMaterial.Iron;
    public SlammerEffectType specialEffect = SlammerEffectType.None;

    [Header("Abilities & Rules Alterations")]
    [Tooltip("If checked, this specific slammer ignores the milkcap's natural physics randomness and forces a targeted state.")]
    public bool overridesLandingOutcome = false;
    [Tooltip("The forced physical outcome target applied to impacted milkcaps.")]
    public ControlledMilkCapPhysics.LandingOutcome specialOutcomeOverride = ControlledMilkCapPhysics.LandingOutcome.Random;


    [Header("Core Performance Stats")]
    public SlammerStats stats;
}
