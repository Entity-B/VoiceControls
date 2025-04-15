using System.Linq.Expressions;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using VoiceControl;
using VoiceControls.Tools;

namespace VoiceControl.Managers
{
    internal class MicrophoneManager : MonoBehaviour
    {
        public void Awake()
        {
            transform.position = Vector3.zero;
            if (XRSettings.isDeviceActive)
            {
                gameObject.SetLayer(UnityLayer.FirstPersonOnly);
            }
            transform.SetParent(GorillaTagger.Instance.mainCamera.transform, false);
            transform.localPosition = new Vector3(-0.069f, -0.0485f, 0.0962f);
            transform.localScale = new Vector3(0.012f, 0.013f, 0.01f);

            Vars.StarterRecognised += delegate (bool IsSpotify)
            {
                Vars.SM.SpeakingDotObject.SetActive(true);
                Vars.SM.UserSpeakingType = IsSpotify ? SpeakingMicrophone.SpeakingType.Spotify : SpeakingMicrophone.SpeakingType.Regular;
                Vars.SM.SpeakingDotColor = Vars.SM.UsePlayersColorForMicrophoneDot ? (Vars.SM.UserSpeakingType == SpeakingMicrophone.SpeakingType.Regular ? VariableTools.NineRGBTo255RGB(GorillaTagger.Instance.offlineVRRig.playerColor) : Color.green) : (Vars.SM.UseCustomColor ? Vars.SM.HexColor : Color.cyan);

                AudioSource.PlayClipAtPoint(Vars.SM.MicrophoneOn, GorillaTagger.Instance.offlineVRRig.headMesh.transform.position, 99f);
            };
            Vars.CommandEnded += delegate
            {
                Vars.SM.SpeakingDotObject.SetActive(false);
                AudioSource.PlayClipAtPoint(Vars.SM.MicrophoneOff, GorillaTagger.Instance.offlineVRRig.headMesh.transform.position, 99f);
            };
        }
        void Update()
        {
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
