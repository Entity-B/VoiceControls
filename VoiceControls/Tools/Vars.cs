using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using VoiceControls.Main;

namespace VoiceControls.Tools
{
    public static class Vars
    {
        private static ManualLogSource MlS;
        public static void SetUpLogger(ManualLogSource MLS)
        {
            if (MLS != null) return;
            MlS = MLS;
        }
        public static void Log(string message)
        {
#if DEBUG
            if (MlS != null)
            {
                MlS.Log(message);
            }
            else
            {
                Debug.Log($"[{BepinexEntry.Name}] {message}");
            }
#endif
        }
        public static GameObject Manager;
        public static List<CommandInfo> SpotifyCommands = new List<CommandInfo>();

        public static KeywordRecognizer Spotify;
        public static KeywordRecognizer SpotifyCommand;

        public static KeywordRecognizer Default;
        public static KeywordRecognizer DefaultCommand;

        public static List<CommandInfo> DefaultCommands = new List<CommandInfo>();

        public static SpeakingMicrophone SM;

        public static Effects ModuleEffects;

        public static Action<bool> StarterRecognised;
        public static Action CommandEnded;

        public static Player[] FriendsInRoom;
        public static Effects VFX;

        public static VRRig CurrentVRRig(this Player value) => GorillaGameManager.instance.FindPlayerVRRig(value);
        internal enum SpotifyKeyCodes : uint
        {
            Next = 0xB0,
            Previous = 0xB1,
            PlayOrPause = 0xB3,
        }
    }

    public class SpeakingMicrophone
    {
        public enum SpeakingType
        {
            Spotify,
            Regular,
            PlayerColor,
            CustomHexColor
        }

        public GameObject MicrophoneObject;
        public Texture MicrophoneCurrentTexture
        {
            get => MicrophoneObject.GetComponent<RawImage>().texture;
            set
            {
                MicrophoneObject.GetComponent<RawImage>().texture = value;
            }
        }
        public Texture Default;
        public Texture Muted;
        public Texture LoudnessLevel1;
        public Texture LoudnessLevel2;

        public GameObject SpeakingDotObject;
        public Color SpeakingDotColor
        {
            get => SpeakingDotObject.GetComponent<RawImage>().material.color;
            set { SpeakingDotObject.GetComponent<RawImage>().material.color = value; }
        }

        public AudioClip MicrophoneOn;
        public AudioClip MicrophoneOff;

        public bool UsePlayersColorForMicrophoneDot;
        public bool UseCustomColor;

        public Color HexColor;

        public SpeakingType UserSpeakingType;
    }

    public class Effects
    {
        public AudioClip PlayerPingAudio;
    }
}
