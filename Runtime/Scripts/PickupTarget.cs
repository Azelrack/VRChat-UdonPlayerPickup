
using UdonSharp;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using VRC.SDK3.Data;
using VRC.SDK3.Persistence;
using VRC.SDK3.UdonNetworkCalling;
using VRC.SDKBase;
using VRC.Udon;

public class PickupTarget : UdonSharpBehaviour
{
    private VRCPlayerApi _grabber;
    private HumanBodyBones _boneToFollow;
    private VRCPlayerApi _localPlayer;
    private Vector3 _offset;
    private Quaternion _rotationOffset;

    [NetworkCallable]
    public void GetGrabbed(string playersGrabbed, int grabberID, int boneToFollow)
    {
        Debug.Log($"Received GetGrabbed network event: {playersGrabbed}, {grabberID}, {boneToFollow}");
        if (!VRCJson.TryDeserializeFromJson(playersGrabbed, out DataToken result))
            return;
        Debug.Log("Deserialized player list");
        var playersGrabbedList = result.DataList;
        double localID = Networking.LocalPlayer.playerId;

        if (playersGrabbedList.Count == 0 || !playersGrabbedList.Contains(localID))
        {
            Debug.Log($"List was empty or did no contained player: elements in list {playersGrabbedList.Count}");
            return;
        }

        Debug.Log("Player is in list");


        _grabber = VRCPlayerApi.GetPlayerById(grabberID);
        _boneToFollow = (HumanBodyBones)boneToFollow;

        // Set offsets
        var bonePos = _grabber.GetBonePosition(_boneToFollow);
        var boneRot = _grabber.GetBoneRotation(_boneToFollow);
        _offset = bonePos - _localPlayer.GetBonePosition(HumanBodyBones.Hips);
        _rotationOffset = Quaternion.Inverse(boneRot) * _localPlayer.GetBoneRotation(HumanBodyBones.Hips);
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

    void Start()
    {
        _localPlayer = Networking.LocalPlayer;
    }

    private void Update()
    {
        if (_grabber == null)
            return;
        var bonePos = _grabber.GetBonePosition(_boneToFollow);
        var boneRot = _grabber.GetBoneRotation(_boneToFollow);
        var moveToward = Vector3.MoveTowards(transform.position, bonePos + _offset, 1f);
        _localPlayer.SetVelocity(moveToward);
        //_localPlayer.TeleportTo(bonePos + _offset, boneRot * _rotationOffset);
    }
}
