
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
    private VRCPlayerApi _localPlayer;
    [SerializeField] private Vector3 _offset;

    [Header("--- Grab Settings ---")]
    [Tooltip("Speed at which grabbed player will move toward his grabber's limb.")]
    [SerializeField] private float _followBoneSpeed = 10f;

    [Tooltip("If player moves, they'll get removed from their grabber's grip. Rotation doesn't affects it.")]
    [SerializeField] private bool _moveToGetFree = false;

    [Tooltip("How much does player need to move to get free? 1 = move the stick to the maximum, 0 = any movement. " +
        "Keyboard inputs will always send the maximum movement.")]
    [SerializeField] private float _getFreeThreshold = 0.5f;

    [Header("--- Debug ---")]
    [Tooltip("Will automatically follow the left hand of the next player that joins.")]
    [SerializeField] private bool _debugMode;

    [Tooltip("Exposed only for debug purposes.")]
    [SerializeField] private HumanBodyBones _boneToFollow;

    private Vector3 HipsPosition => _localPlayer.GetBonePosition(HumanBodyBones.Hips);

    void Start()
    {
        _localPlayer = Networking.LocalPlayer;
    }

    private void FixedUpdate()
    {
        if (_grabber == null)
            return;
        var grabberPosition = _grabber.GetBonePosition(_boneToFollow);
        var direction = grabberPosition - HipsPosition - _offset;
        _localPlayer.SetVelocity(direction * _followBoneSpeed);
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (!_debugMode)
            return;
        _grabber = player;
        _boneToFollow = HumanBodyBones.LeftHand;
    }

    [NetworkCallable]
    public void GetGrabbed(string playersGrabbed, int grabberID, int boneToFollow)
    {
        Debug.Log($"Received GetGrabbed network event: {playersGrabbed}, {grabberID}, {boneToFollow}");
        if (!VRCJson.TryDeserializeFromJson(playersGrabbed, out DataToken result))
            return;
        var playersGrabbedList = result.DataList;
        double localID = Networking.LocalPlayer.playerId;

        if (playersGrabbedList.Count == 0 || !playersGrabbedList.Contains(localID))
        {
            Debug.Log($"List was empty or did no contained player: elements in list {playersGrabbedList.Count}");
            return;
        }

        _grabber = VRCPlayerApi.GetPlayerById(grabberID);
        _boneToFollow = (HumanBodyBones)boneToFollow;

        // Set offsets
        var grabberPosition = _grabber.GetBonePosition(_boneToFollow);
        _offset = HipsPosition - grabberPosition;
        Debug.Log("Offsets done");
    }

    [NetworkCallable]
    public void GetReleased(string playersToRelease, int grabberID)
    {
        if (!VRCJson.TryDeserializeFromJson(playersToRelease, out DataToken result))
            return;
        var releaseList = result.DataList;
        double localID = Networking.LocalPlayer.playerId;

        // As a player could be stolen by another from the initial grabber,
        // we check the current grabber matches the one sending release event
        if (_grabber != null && releaseList.Contains(localID) && _grabber.playerId == grabberID)
            _grabber = null;
    }

    #region Move to Get Free

    /// <summary>
    /// Defines if a movement intensity will break the player's being grabbed.
    /// </summary>
    private void ProcessMovement(float value)
    {
        if (_moveToGetFree && _grabber != null && Mathf.Abs(value) > _getFreeThreshold)
            _grabber = null;
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
