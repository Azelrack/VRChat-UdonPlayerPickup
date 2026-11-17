
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

    [Header("--- Other ---")]
    [Tooltip("Any player height beyond this value will trigger a resize of the SphereCollider.")]
    [SerializeField] private float _referenceSize = 1.7f;

    [Tooltip("External dependency required to communicate grab events.")]
    [SerializeField] private PickupTarget _target;

    private bool _inputPressed = false;
    private DataList playersTouched = new DataList();
    private float _lastPressTime = 0f;
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
            playersTouched.Add(player.playerId);
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (!_inputPressed)
            playersTouched.Remove(player.playerId);
    }

    public override void InputGrab(bool value, UdonInputEventArgs args)
    {
        if (value && args.handType == _handType)
        {
            if (_triggerOnDoublePress)
            {
                if (_lastPressTime <= 0f)
                    _lastPressTime = Time.time;
                else if (_lastPressTime - Time.time < _doublePressSpeed)
                {
                    Debug.Log("Double Press");
                    GrabPeople();
                    _inputPressed = true;
                }
            }
            else
            {
                Debug.Log("Single Press");
                GrabPeople();
                _inputPressed = true;
            }
        }
        else if (!value)
        {
            Debug.Log("Release press");
            ReleasePeople();
            _lastPressTime = 0f;
            _inputPressed = false;
        }
    }

    private void GrabPeople()
    {
        if (!VRCJson.TrySerializeToJson(playersTouched, JsonExportType.Minify, out DataToken playersTouchedJson))
            return;
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
        _target.SendCustomNetworkEvent(
            NetworkEventTarget.Others,
            nameof(_target.GetReleased),
            playersTouchedJson.String,
            Networking.LocalPlayer.playerId
        );
    }
}
