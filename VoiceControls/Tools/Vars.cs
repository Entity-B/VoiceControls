using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using VoiceControls.Main;

namespace VoiceControls.Tools
{
    public static class Vars
    {
        public static GameObject Manager;
        public static List<CommandInfo> SpotifyCommands = new List<CommandInfo>();

        public static KeywordRecognizer Spotify;
        public static KeywordRecognizer SpotifyCommand;

        public static SpeakingMicrophone SM;

        public static Effects ModuleEffects;

        public static Action<bool> StarterRecognised;
        public static Action CommandEnded;

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
            PlayerColor
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

        public SpeakingType UserSpeakingType;
    }

    public class Effects
    {
        public AudioSource PlayerPingAudio;
    }
}
