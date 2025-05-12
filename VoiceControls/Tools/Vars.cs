using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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

        public static KeywordRecognizer Spotify;
        public static KeywordRecognizer SpotifyCommand;

        public static KeywordRecognizer Default;
        public static KeywordRecognizer DefaultCommand;

        public static KeywordRecognizer Global;
        public static KeywordRecognizer GlobalCommand;
        public static List<string> CommandLogs;

        public static List<CommandInfo> AllCommands = new List<CommandInfo>();

        public static SpeakingMicrophone SM;

        public static Effects ModuleEffects;

        public static Action<CommandInfo.CommandType> StarterRecognised;
        public static Action<CommandInfo.CommandType> CommandEnded;

        public static Player[] FriendsInRoom;

        public static ModuleSettings MS;

        public static string ExtractCodeFromText(string text)
        {
            var match = Regex.Match(text, @"(?:code is|code)?\s*(\w+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }

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
        public enum SpecialColorType
        {
            PlayerColor,
            CustomHexColor,
            None
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
        public Color MircrophoneColor
        {
            get => MicrophoneObject.GetComponent<RawImage>().material.color;
            set { MicrophoneObject.GetComponent<RawImage>().material.color = value; }
        }

        public AudioClip MicrophoneOn;
        public AudioClip MicrophoneOff;

        public AudioClip SpotifyOn;
        public AudioClip SpotifyOff;

        public AudioClip GlobalOn;
        public AudioClip GlobalOff;

        public bool UsePlayersColorForMicrophoneDot;
        public bool UseCustomColor;

        public Color HexColor;

        public CommandInfo.CommandType UserSpeakingType;

        public SpecialColorType SpeakingType;
    }
    public class Effects
    {
        public AudioClip PlayerPingAudio;
    }
}
