
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PickupCommonConfig : UdonSharpBehaviour
{
    [Header("-- Size Config ---")]
    [Tooltip("Any player height beyond this value will trigger a resize of the SphereCollider.")]
    [SerializeField] private float _referenceSize = 1.7f;

    [Tooltip("Requires the player trying to pickup another to be at least twice their size.")]
    [SerializeField] private bool _pickupSizeRestriction = true;

    [Tooltip("How bigger a grabber needs to be for the Pickup Size Restriction to work? Default 1.4f = 40% bigger.")]
    [SerializeField] private float _sizeRestrictionDifference = 1.4f;

    [Header("--- Debug ---")]
    [SerializeField] private bool _debugLogs = false;

    public bool PickupSizeRestriction => _pickupSizeRestriction;

    public float SizeRestrictionDifference => _sizeRestrictionDifference;

    public bool DebugLogs => _debugLogs;

    public float ReferenceSize => _referenceSize;
}
