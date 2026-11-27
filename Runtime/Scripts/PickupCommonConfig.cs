
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// Just a place for World Creators to find all options that matters to them in one place.
/// </summary>
public class PickupCommonConfig : UdonSharpBehaviour
{
    [Header("-- Size Config ---")]
    [Tooltip("Any player height beyond this value will trigger a resize of the SphereCollider.")]
    [SerializeField] private float _referenceSize = 1.7f;

    [Tooltip("Requires the player trying to pickup another to be at least twice their size.")]
    [SerializeField] private bool _pickupSizeRestriction = true;

    [Tooltip("How bigger a grabber needs to be for the Pickup Size Restriction to work? Default 1.4f = 40% bigger.")]
    [SerializeField] private float _sizeRestrictionDifference = 1.4f;

    [Header("--- Grab Config ---")]
    [Tooltip("If player moves, they'll get removed from their grabber's grip. Rotation doesn't affects it.")]
    [SerializeField] private bool _allowMoveToGetFree = true;

    [Tooltip("How much does player need to move to get free? 1 = move the stick to the maximum, 0 = any movement. " +
        "Keyboard inputs will always send the maximum value 1.")]
    [SerializeField] private float _getFreeThreshold = 0.5f;

    [Tooltip("Allows players to grab someone that is already grabbed.")]
    [SerializeField] private bool _allowStealingFromGrab = true;

    [Header("--- Dynamic Tutorial ---")]
    [Tooltip("TextMesh Pro Text element to display your current size difference restriction to the users.")]
    [SerializeField] private TMPro.TextMeshProUGUI _sizeRestrictionText;
    [SerializeField] private GameObject _sizeRestrictionDisplay;
    [SerializeField] private GameObject _moveToGetFreeDisplay;
    [SerializeField] private GameObject _stealFromGrabDisplay;

    [Header("--- Debug ---")]
    [SerializeField] private bool _debugLogs = false;

    public bool PickupSizeRestriction => _pickupSizeRestriction;
    public float SizeRestrictionDifference => _sizeRestrictionDifference;
    public float ReferenceSize => _referenceSize;
    public bool AllowMoveToGetFree => _allowMoveToGetFree;
    public float GetFreeThreshold => _getFreeThreshold;
    public bool AllowStealingFromGrab => _allowStealingFromGrab;
    public bool DebugLogs => _debugLogs;

    private void Start()
    {
        SetConfigTutorial();
    }

    public void SetConfigTutorial()
    {
        if (_sizeRestrictionDisplay != null && _sizeRestrictionText != null)
        {
            _sizeRestrictionDisplay.SetActive(PickupSizeRestriction);
            _sizeRestrictionText.text = $"You have to be {(SizeRestrictionDifference - 1)*100}% taller than a player to pick them up.\n";
        }
        if (_moveToGetFreeDisplay != null)
            _moveToGetFreeDisplay.SetActive(AllowMoveToGetFree);
        if (_stealFromGrabDisplay != null)
            _moveToGetFreeDisplay.SetActive(AllowStealingFromGrab);
    }
}
