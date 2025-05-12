using System.Linq.Expressions;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using VoiceControls.Tools;

namespace VoiceControl.Managers
{
    internal class MicrophoneManager : MonoBehaviour
    {
        private Vector3 DefaultSize;
        public void Awake()
        {
            transform.position = Vector3.zero;
            if (XRSettings.isDeviceActive)
            {
                gameObject.SetLayer(UnityLayer.FirstPersonOnly);
            }
            DefaultSize = new Vector3(0.012f, 0.013f, 0.01f);
            transform.SetParent(GorillaTagger.Instance.mainCamera.transform, false);
            transform.localPosition = new Vector3(-0.069f, -0.0485f, 0.0962f);
            transform.localScale = new Vector3(0.012f, 0.013f, 0.01f);
        }
        void Update()
        {
            if (GorillaTagger.Instance.offlineVRRig.GetComponent<GorillaSpeakerLoudness>().IsSpeaking)
            {
                if (transform.localScale != DefaultSize + new Vector3(0.05f, 0.05f, 0.05f))
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, DefaultSize + new Vector3(0.05f, 0.05f, 0.05f), 0.5f);
                }
            }
            else
            {
                if (transform.localScale != DefaultSize)
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, DefaultSize, 0.5f);
                }
            }
            if (!PhotonNetwork.InRoom)
            {
                if (Vars.SM.MicrophoneObject.GetComponent<RawImage>().texture != Vars.SM.Muted)
                {
                    Vars.SM.MicrophoneObject.GetComponent<RawImage>().texture = Vars.SM.Muted;
                }
                return;
            }
            if (GorillaTagger.Instance.offlineVRRig.GetComponent<GorillaSpeakerLoudness>().IsSpeaking)
            {
                if (Vars.SM.MicrophoneObject.GetComponent<RawImage>().texture != GorillaTagger.Instance.offlineVRRig.GetComponent<GorillaSpeakerLoudness>().SmoothedLoudness < 0.03f ? Vars.SM.LoudnessLevel1 : Vars.SM.LoudnessLevel2)
                {
                    Vars.SM.MicrophoneObject.GetComponent<RawImage>().texture = GorillaTagger.Instance.offlineVRRig.GetComponent<GorillaSpeakerLoudness>().SmoothedLoudness < 0.03f ? Vars.SM.LoudnessLevel1 : Vars.SM.LoudnessLevel2;
                }
            }
            else if (GorillaComputer.instance.pttType == "PUSH TO TALK" && !GorillaTagger.Instance.myRecorder.TransmitEnabled)
            {
                if (Vars.SM.MicrophoneObject.GetComponent<RawImage>().texture != Vars.SM.Muted)
                {
                    Vars.SM.MicrophoneObject.GetComponent<RawImage>().texture = Vars.SM.Muted;
                }
            }
            else if (GorillaComputer.instance.pttType == "PUSH TO MUTE" && GorillaTagger.Instance.myRecorder.TransmitEnabled)
            {
                if (Vars.SM.MicrophoneObject.GetComponent<RawImage>().texture != Vars.SM.Muted)
                {
                    Vars.SM.MicrophoneObject.GetComponent<RawImage>().texture = Vars.SM.Muted;
                }
            }
            else if (!GorillaTagger.Instance.offlineVRRig.IsMicEnabled)
            {
                if (Vars.SM.MicrophoneObject.GetComponent<RawImage>().texture != Vars.SM.Muted)
                {
                    Vars.SM.MicrophoneObject.GetComponent<RawImage>().texture = Vars.SM.Muted;
                }
            }
            else
            {
                if (Vars.SM.MicrophoneObject.GetComponent<RawImage>().texture != Vars.SM.Default)
                {
                    Vars.SM.MicrophoneObject.GetComponent<RawImage>().texture = Vars.SM.Default;
                }
            }
        }
    }
}
