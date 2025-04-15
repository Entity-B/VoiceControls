using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Windows.Speech;
using VoiceControls.Components;
using VoiceControls.Tools;

namespace VoiceControls.Main
{
    [BepInPlugin(BepinexEntry.GUID, BepinexEntry.Name, BepinexEntry.Version)]
    public class Entry : BaseUnityPlugin
    {
        public static Entry Instance { get; private set; }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

        // I may have borrowed this from graze, so full credits to them :)
        internal static void SendKey(Vars.SpotifyKeyCodes virtualKeyCode) => keybd_event((uint)virtualKeyCode, 0, 0, 0);

        void Start()
        {
            Instance = this;
            Harmony harm = new Harmony(BepinexEntry.GUID);
            harm.PatchAll();

            ConfigFile Config = new ConfigFile(Path.Combine(Directory.GetCurrentDirectory(), @"BepInEx\config\HeadVolume.cfg"), true);
            ConfigEntry<bool> UsePlayerColorEntry = Config.Bind("Microphone Color Settings", "Microphone Dot Uses Player Color", false, "If this is enabled, the dot that is next to the microphone that is used for finding out if you are using the speech recognition, if it isnt spotify mode, will be your ingame player color ( overrides Microphone Dot Uses Hex Color )");
            ConfigEntry<bool> UseHexadecimalColor = Config.Bind("Microphone Color Settings", "Microphone Dot Uses Hex Color", false, "If this is enabled, the dot that is next to the microphone that is used for finding out if you are using the speech recognition, if it isnt spotify mode, will be the hex color you choose");
            ConfigEntry<string> HexColor = Config.Bind("Microphone Color Settings", "Microphone Dot Uses Hex Color", "#33bbff", "The color the Speaking Dot will be if (Microphone Dot Uses Hex Color) Is Enabled and (Microphone Dot Uses Player Color) is Disabled, This must be a hexadecimal color, Heres a link if needed: https://htmlcolorcodes.com/");

            Vars.SetUpLogger(this.Logger);
            GorillaTagger.OnPlayerSpawned(delegate
            {
                Vars.Manager = new GameObject("VoiceControlsManager");
                Vars.Manager.AddComponent<Callbacks>();

                CommandsSetup();

                Vars.Spotify = new KeywordRecognizer(new string[] { "SPOTIFY" });
                Vars.SpotifyCommand = new KeywordRecognizer(Vars.SpotifyCommands.Select(c => c.CommandActivationWord).ToArray());

                Vars.Spotify.OnPhraseRecognized += delegate
                {
                    Vars.StarterRecognised?.Invoke(true);
                    Vars.Spotify.Stop();
                    Vars.SpotifyCommand.Start();
                };

                Vars.SpotifyCommand.OnPhraseRecognized += delegate (PhraseRecognizedEventArgs speech)
                {
                    try
                    {
                        CommandInfo command = Vars.SpotifyCommands.FirstOrDefault(c => c.CommandActivationWord.ToLower() == speech.text.ToLower());
                        if (command != null)
                        {
                            Logger.Log($"Command: {command.CommandActivationWord}, Description: {command.CommandDescription}");
                            command.CommandAction.Invoke();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex);
                    }
                    Vars.CommandEnded?.Invoke();
                    Vars.Spotify.Start();
                    Vars.SpotifyCommand.Stop();
                };
                using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VoiceControls.Resources.microphonestuff"))
                {
                    AssetBundle assetBundle = AssetBundle.LoadFromStream(manifestResourceStream);
                    GameObject ConsoleCanvasObject = UnityEngine.Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("Icon"));
                    ColorUtility.TryParseHtmlString(HexColor.Value.StartsWith('#') ? HexColor.Value : $"#{HexColor.Value}", out Color color);
                    Vars.SM = new SpeakingMicrophone()
                    {
                        Muted = assetBundle.LoadAsset<Texture>("muted"),
                        Default = assetBundle.LoadAsset<Texture>("regular"),
                        LoudnessLevel1 = assetBundle.LoadAsset<Texture>("LevelOne"),
                        LoudnessLevel2 = assetBundle.LoadAsset<Texture>("LevelTwo"),

                        MicrophoneOn = assetBundle.LoadAsset<AudioClip>("power-on"),
                        MicrophoneOff = assetBundle.LoadAsset<AudioClip>("power-off"),

                        MicrophoneObject = ConsoleCanvasObject.transform.GetChild(0).GetChild(0).gameObject,
                        SpeakingDotObject = ConsoleCanvasObject.transform.GetChild(0).GetChild(1).gameObject,

                        UsePlayersColorForMicrophoneDot = UsePlayerColorEntry.Value,
                        UseCustomColor = UseHexadecimalColor.Value,
                        HexColor = color,
                       
                        UserSpeakingType = Vars.SM.UsePlayersColorForMicrophoneDot ? SpeakingMicrophone.SpeakingType.PlayerColor : ( Vars.SM.UseCustomColor ? SpeakingMicrophone.SpeakingType.CustomHexColor : SpeakingMicrophone.SpeakingType.Regular)
                    };
                }
                Vars.Log("Created SPeakingMicorphone and Effects");
                Vars.Log($"Config Settings: Use Player Color: {UsePlayerColorEntry.Value}, Use Custom Color: {UseHexadecimalColor.Value}, Custom Color Hexadecimal: {HexColor.Value}, Custom Color RGB: R({Vars.SM.HexColor.r}) G({Vars.SM.HexColor.g}) B({Vars.SM.HexColor.b})");
            });
        }
        void Update()
        {

        }
        void CommandsSetup()
        {
            Vars.SpotifyCommands.Add(new CommandInfo()
            {
                CommandActivationWord = "play",
                CommandDescription = "Plays/Pauses you're currently spotify song",
                CommandAction = () => { SendKey(Vars.SpotifyKeyCodes.PlayOrPause); }
            });
            Vars.SpotifyCommands.Add(new CommandInfo()
            {
                CommandActivationWord = "next",
                CommandDescription = "Plays the next song in Queue",
                CommandAction = () => { SendKey(Vars.SpotifyKeyCodes.Next); }
            });
            Vars.SpotifyCommands.Add(new CommandInfo()
            {
                CommandActivationWord = "last",
                CommandDescription = "The song that was playing last",
                CommandAction = () => { SendKey(Vars.SpotifyKeyCodes.Previous);  }
            });
        }
    }
    internal class BepinexEntry
    {
        public const string GUID = "com.entityb.voicecontrols";
        public const string Name = "VoiceControls";
        public const string Version = "1.0.0";
    }
}
