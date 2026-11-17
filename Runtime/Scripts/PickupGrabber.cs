
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.BC;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

[RequireComponent(typeof(SphereCollider))]
public class PickupGrabber : UdonSharpBehaviour
{
    [Tooltip("Any player height beyond this value will trigger a resize of the SphereCollider.")]
    [SerializeField] private float _referenceSize = 1.7f;
    [SerializeField] private HandType _handType;
    [SerializeField] private HumanBodyBones _boneToFollow;
    [SerializeField] private bool _reactToDoublePress = false;
    [SerializeField] private PickupTarget _target;

    private bool _inputPressed = false;
    private DataList playersTouched = new DataList();
    private float _lastTapTime = 0f;
    private Vector3 _baseScale;

    private void Update()
    {
        var bonePos = Networking.LocalPlayer.GetBonePosition(_boneToFollow);
        transform.position = bonePos;
    }

    void Start()
    {
        _baseScale = transform.localScale;
    }

    public override void OnAvatarEyeHeightChanged(VRCPlayerApi player, float prevEyeHeightAsMeters)
    {
        base.OnAvatarEyeHeightChanged(player, prevEyeHeightAsMeters);
        transform.localScale = _baseScale * (player.GetAvatarEyeHeightAsMeters() / _referenceSize);
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!_inputPressed && player != Networking.LocalPlayer)
        {
            playersTouched.Add(player.playerId);
            Debug.Log($"Added player {player} to grab");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Detected collision with {other}");

    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!_inputPressed)
        {
            playersTouched.Remove(player.playerId);
            Debug.Log($"Removed player {player} from grab");
        }
    }

    public override void InputGrab(bool value, UdonInputEventArgs args)
    {
        Debug.Log("GrabDetected");
        if (value && args.handType == _handType)
        {
            Debug.Log("Hand Valid");
            if (_reactToDoublePress)
            {
                if (_lastTapTime <= 0f)
                    _lastTapTime = Time.time;
                else if (_lastTapTime - Time.time < 0.2f)
                {
                    GrabPeople();
                    Debug.Log("Double Press");
                }
            }
            else
            {
                Debug.Log("Single Press");
                GrabPeople();
            }
        }
        else if (!value)
        {
            Debug.Log("Release press");
            ReleasePeople();
            _lastTapTime = 0f;
        }
    }

    private void GrabPeople()
    {
        if (!VRCJson.TrySerializeToJson(playersTouched, JsonExportType.Minify, out DataToken playersTouchedJson))
            return;
        Debug.Log("Sending Network Event");

        _target.SendCustomNetworkEvent(
            NetworkEventTarget.Others,
            nameof(_target.GetGrabbed),
            playersTouchedJson.String,
            Networking.LocalPlayer.playerId,
            (int)_boneToFollow
        );
    }

    private void ReleasePeople()
    {
        if (!VRCJson.TrySerializeToJson(playersTouched, JsonExportType.Minify, out DataToken playersTouchedJson))
            return;
        Debug.Log("Sending Network Event");

        _target.SendCustomNetworkEvent(
            NetworkEventTarget.Others,
            nameof(_target.GetReleased),
            playersTouchedJson.String,
            Networking.LocalPlayer.playerId
        );
    }
}
