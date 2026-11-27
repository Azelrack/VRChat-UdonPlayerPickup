using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(VRCVelocityTracker))]
public class PickupGrabber : UdonSharpBehaviour
{
    #region Fields

    /// <summary>
    /// Players currently being hit by the collider.
    /// </summary>
    private readonly DataList _playersInContact = new DataList();

    /// <summary>
    /// Players currently being grabbed.
    /// </summary>
    private DataList _playersInGrab = new DataList();

    private float _lastPressTime = 0f;
    private Vector3 _baseScale;
    private bool _isUserInVR;

    [Header("--- Grab Side and Bone ---")]
    [Tooltip("Hand that will trigger the command.")]
    [SerializeField] private HandType _handType;

    [Tooltip("Bone to attach the grabbed player to.")]
    [SerializeField] private HumanBodyBones _boneToFollow;

    [Header("--- Double Press Setup ---")]
    [Tooltip("Grab will be only triggered with a double tap.")]
    [SerializeField] private bool _triggerOnDoublePress = false;

    [Tooltip("Speed in seconds at which players have to repeat the grab gesture to toggle the Trigger On Double Press option.")]
    [SerializeField] private float _doublePressSpeed = 0.2f;

    [Header("--- Dependencies ---")]
    [Tooltip("External dependency required to communicate grab events.")]
    [SerializeField] private PickupTarget _target;

    [SerializeField] private VRCVelocityTracker _velocityTracker;

    [SerializeField] private PickupCommonConfig _pickupCommonConfig;

    #endregion

    #region Properties

    private Vector3 BonePosition => Networking.LocalPlayer.GetBonePosition(_boneToFollow);

    /// <summary>
    /// Requires the player trying to pickup another to be bigger.
    /// </summary>
    private bool SizeRestriction => _pickupCommonConfig.PickupSizeRestriction;

    /// <summary>
    /// How bigger a grabber needs to be for the Pickup Size Restriction to work? Default 1.4f = 40% bigger.
    /// </summary>
    private float SizeRestrictionDifference => _pickupCommonConfig.SizeRestrictionDifference;

    /// <summary>
    /// Any player height beyond this value will trigger a resize of the SphereCollider.
    /// </summary>
    private float ReferenceSize => _pickupCommonConfig.ReferenceSize;

    #endregion

    #region Unity Messages

    public override void PostLateUpdate()
    {
        transform.position = BonePosition;
    }

    private void Start()
    {
        _baseScale = transform.localScale;
        _isUserInVR = Networking.LocalPlayer.IsUserInVR();
    }

    private void OnEnable()
    {
        TryPublishDebug($"Player enable {_boneToFollow} grab.");
    }

    private void OnDisable()
    {
        ReleasePlayers();
        _playersInContact.Clear();
        _playersInGrab.Clear();
        TryPublishDebug($"Player disabled {_boneToFollow} grab.");
    }

    #endregion

    #region Udon Methods

    public override void OnAvatarEyeHeightChanged(VRCPlayerApi player, float prevEyeHeightAsMeters)
    {
        if (!player.isLocal)
            return;
        transform.localScale = _baseScale * (player.GetAvatarEyeHeightAsMeters() / ReferenceSize);
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!player.isLocal)
            _playersInContact.Add(player.playerId);
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!player.isLocal)
            _playersInContact.Remove(player.playerId);
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        // OnPlayerTriggerExit does not detects players who left...
        if (_playersInContact.Contains(player.playerId))
            _playersInContact.Remove(player.playerId);

        // Not super important to empty this one, but it could save us network calls
        if (_playersInGrab.Contains(player.playerId))
            _playersInGrab.Remove(player.playerId);
    }

    public override void InputGrab(bool value, UdonInputEventArgs args)
    {
        if (args.handType != _handType)
            return;
        if (value)
        {
            if (_triggerOnDoublePress)
            {
                if (Time.time - _lastPressTime <= _doublePressSpeed)
                {
                    TryPublishDebug("Grab Double Press");
                    GrabPlayers();
                }
                else
                    _lastPressTime = Time.time;
            }
            else
            {
                TryPublishDebug("Grab Single Press");
                GrabPlayers();
            }
        }
        else
        {
            TryPublishDebug("Grab Release Press");
            ReleasePlayers();
        }
    }

    #endregion

    private void TryPublishDebug(string message)
    {
        if (_pickupCommonConfig.DebugLogs)
            Debug.Log($"#{Networking.LocalPlayer.playerId}: {message}");
    }

    /// <summary>
    /// Calculates grabbed offset and sends network event for <see cref="PickupTarget"/>
    /// to start to follow <see cref="_boneToFollow"/>.
    /// </summary>
    private void GrabPlayers()
    {
        if (!_isUserInVR)
            return;
        foreach (DataToken playerToken in _playersInContact.ToArray())
        {
            var playerToGrab = VRCPlayerApi.GetPlayerById(playerToken.Int);
            if (!Utilities.IsValid(playerToGrab))
                continue;

            // Compares size of grabber with
            if (SizeRestriction)
            {
                var grabberSize = Networking.LocalPlayer.GetAvatarEyeHeightAsMeters();
                var otherPlayerSize = playerToGrab.GetAvatarEyeHeightAsMeters();
                if (grabberSize / otherPlayerSize <= SizeRestrictionDifference)
                {
                    TryPublishDebug($"Could not grab player #{playerToGrab.playerId}, not enough size difference : {grabberSize / otherPlayerSize}, required: {SizeRestrictionDifference}");
                    continue;
                }
            }

            // Calculcate the offset right here to avoid adding latency on target side.
            var offset =  BonePosition - playerToGrab.GetBonePosition(HumanBodyBones.Hips);

            _target.SendCustomNetworkEvent(
                NetworkEventTarget.Others,
                nameof(_target.GetGrabbed),
                playerToGrab.playerId,
                Networking.LocalPlayer.playerId,
                (int)_boneToFollow,
                offset
            );

            _playersInGrab.Add(playerToken);
            TryPublishDebug($"Sent Grab Event for #{playerToGrab.playerId}");
        }
    }

    /// <summary>
    /// Sends release network event for each player accounted in grab.
    /// </summary>
    private void ReleasePlayers()
    {
        var releaseVelocity = _velocityTracker.GetLinearVelocity();

        foreach (DataToken playerToken in _playersInGrab.ToArray())
        {
            _target.SendCustomNetworkEvent(
                NetworkEventTarget.Others,
                nameof(_target.GetReleased),
                playerToken.Int,
                Networking.LocalPlayer.playerId,
                releaseVelocity
            );
            TryPublishDebug($"Sent Release Grab Event for {playerToken.Int}");
        }
        _playersInGrab.Clear();
    }
}
