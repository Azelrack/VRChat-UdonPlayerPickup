
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDK3.Persistence;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class PickupTarget : UdonSharpBehaviour
{
    private VRCPlayerApi _grabber;
    private bool _isThrown = false;
    private Vector3 _releaseVelocity;

    [SerializeField] private Vector3 _offset;

    [Header("--- Grab Settings ---")]
    [Tooltip("Speed at which grabbed player will move toward his grabber's limb.")]
    [SerializeField] private float _followBoneSpeed = 20f;

    [Tooltip("If player moves, they'll get removed from their grabber's grip. Rotation doesn't affects it.")]
    [SerializeField] private bool _moveToGetFree = true;

    [Tooltip("How much does player need to move to get free? 1 = move the stick to the maximum, 0 = any movement. " +
        "Keyboard inputs will always send the maximum movement.")]
    [SerializeField] private float _getFreeThreshold = 0.5f;

    [Tooltip("Allows players to grab someone that is already grabbed.")]
    [SerializeField] private bool _allowStealingFromGrab = true;

    [Header("--- Debug ---")]
    [Tooltip("Will automatically follow the current BoneToFollow of the next player that joins.")]
    [SerializeField] private bool _debugMode = false;

    [SerializeField] private bool _debugLogs = false;

    [Tooltip("Exposed only for debug purposes.")]
    [SerializeField] private HumanBodyBones _boneToFollow;

    private VRCPlayerApi LocalPlayer => Networking.LocalPlayer;
    private Vector3 HipsPosition => LocalPlayer.GetBonePosition(HumanBodyBones.Hips);
    private int LocalPlayerID => LocalPlayer.playerId;

    private void FixedUpdate()
    {
        // Fixed velocity following the grabber's limb.
        if (_grabber != null)
        {
            var grabberPosition = _grabber.GetBonePosition(_boneToFollow);
            var direction = grabberPosition - HipsPosition - _offset;
            LocalPlayer.SetVelocity(_followBoneSpeed * direction);
        }
        // Boosted velocity when grabber releases the target
        else if (_isThrown)
        {
            LocalPlayer.SetVelocity(_releaseVelocity);
            _isThrown = false;
            TryPublishDebug("Grab released and player is now being thrown.");
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (_debugMode && !player.isLocal)
        {
            _grabber = player;
            _boneToFollow = HumanBodyBones.LeftHand;
        }
    }

    private void TryPublishDebug(string message)
    {
        if (_debugLogs)
            Debug.Log($"#{LocalPlayerID}: {message}");
    }

    [NetworkCallable]
    public void GetGrabbed(int playerGrabbed, int grabberID, int boneToFollow, Vector3 offset)
    {
        if (playerGrabbed != Networking.LocalPlayer.playerId || !_allowStealingFromGrab)
            return;

        _grabber = VRCPlayerApi.GetPlayerById(grabberID);
        _boneToFollow = (HumanBodyBones)boneToFollow;
        _offset = offset;

        TryPublishDebug($"Player is getting grabbed by #{grabberID}'s {(HumanBodyBones)boneToFollow}");
    }

    [NetworkCallable]
    public void GetReleased(int playerToRelease, int grabberID, Vector3 releaseVelocity)
    {
        // As a player could have been stolent from its initial grabber,
        // it's important to check current _grabber matches as PickupGrabber is not aware of that.
        if (playerToRelease != LocalPlayerID || _grabber == null || grabberID != _grabber.playerId)
            return;

        _grabber = null;
        _isThrown = true;
        _releaseVelocity = releaseVelocity * 2; // Exagerate a little bit the throw force
        TryPublishDebug($"#{grabberID} released player from grab, applying release velocity: {_releaseVelocity}");
    }

    #region Move to Get Free

    /// <summary>
    /// Defines if a movement intensity will break the player's being grabbed.
    /// </summary>
    private void ProcessMovement(float value)
    {
        if (_moveToGetFree && _grabber != null && Mathf.Abs(value) > _getFreeThreshold)
        {
            TryPublishDebug($"Player got out of grab themselves.");
            _grabber = null;
        }
    }

    public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
    {
        ProcessMovement(value);
    }

    public override void InputMoveVertical(float value, UdonInputEventArgs args)
    {
        ProcessMovement(value);
    }

    #endregion
}
