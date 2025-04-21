using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Configuration;
using GorillaNetworking;
using HarmonyLib;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Windows.Speech;
using VoiceControl.Managers;
using VoiceControls.Components;
using VoiceControls.Tools;

namespace VoiceControls.Main
{
    [BepInPlugin(BepinexEntry.GUID, BepinexEntry.Name, BepinexEntry.Version)]
    public class Entry : BaseUnityPlugin
    {
        public static Entry Instance { get; private set; }
        public ConfigEntry<bool> UsePlayerColorEntry;
        public ConfigEntry<bool> UseHexadecimalColor;
        public ConfigEntry<string> HexColor;
        public ConfigEntry<string> VoiceActivationWord;

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern void keybd_event(uint bVk, uint bScan, uint dwFlags, uint dwExtraInfo);

        // I may have borrowed this from graze, so full credits to them :)
        internal static void SendKey(Vars.SpotifyKeyCodes virtualKeyCode) => keybd_event((uint)virtualKeyCode, 0, 0, 0);

        void Start()
        {
            Instance = this;
            Harmony harm = new Harmony(BepinexEntry.GUID);
            harm.PatchAll();

            CreateConfigEntries();

            Vars.SetUpLogger(base.Logger);
            GorillaTagger.OnPlayerSpawned(delegate
            {
                Vars.Manager = new GameObject("VoiceControlsManager");
                Vars.Manager.AddComponent<Callbacks>();
                DontDestroyOnLoad(Vars.Manager);

                CommandsSetup();
                Vars.SpotifyCommands.Add(new CommandInfo()
                {
                    CommandActivationWord = "stop",
                    CommandAction = null,
                    CommandDescription = "stops command"
                });
                Vars.DefaultCommands.Add(new CommandInfo()
                {
                    CommandActivationWord = "stop",
                    CommandAction = null,
                    CommandDescription = "stops command"
                });
                Vars.Spotify = new KeywordRecognizer(new string[] { "MUSIC" });
                Vars.SpotifyCommand = new KeywordRecognizer(Vars.SpotifyCommands.Select(c => c.CommandActivationWord).ToArray());

                Vars.Default = new KeywordRecognizer(new string[] { VoiceActivationWord.Value.ToUpper() });
                Vars.DefaultCommand = new KeywordRecognizer(Vars.DefaultCommands.Select(c => c.CommandActivationWord).ToArray());

                Vars.Spotify.Start();
                Vars.Default.Start();

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

                Vars.Default.OnPhraseRecognized += delegate
                {
                    Vars.StarterRecognised?.Invoke(false);
                    Vars.Default.Stop();
                    Vars.DefaultCommand.Start();
                };
                Vars.DefaultCommand.OnPhraseRecognized += delegate (PhraseRecognizedEventArgs speech)
                {
                    CommandInfo command = Vars.SpotifyCommands.FirstOrDefault(c => c.CommandActivationWord.ToLower() == speech.text.ToLower());
                    try
                    {
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
                    Vars.Default.Start();
                    Vars.DefaultCommand.Stop();
                };
                using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VoiceControls.Resources.microphone"))
                {
                    AssetBundle assetBundle = AssetBundle.LoadFromStream(manifestResourceStream);
                    GameObject ConsoleCanvasObject = UnityEngine.Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("Icon"));
                    ConsoleCanvasObject.AddComponent<MicrophoneManager>();
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

                        UserSpeakingType = SpeakingMicrophone.SpeakingType.Regular
                    };
                    Vars.SM.UserSpeakingType = Vars.SM.UsePlayersColorForMicrophoneDot ? SpeakingMicrophone.SpeakingType.PlayerColor : (Vars.SM.UseCustomColor ? SpeakingMicrophone.SpeakingType.CustomHexColor : SpeakingMicrophone.SpeakingType.Regular);
                    Vars.SM.SpeakingDotObject.SetActive(false);
                }
                using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VoiceControls.Resources.speechrecognitioneffects"))
                {
                    AssetBundle assetBundle = AssetBundle.LoadFromStream(manifestResourceStream);
                    Vars.ModuleEffects = new Effects()
                    {
                        PlayerPingAudio = assetBundle.LoadAsset<AudioClip>("PlayerPing")
                    };
                }

                Vars.StarterRecognised += delegate (bool IsSpotify)
                {
                    Vars.SM.SpeakingDotObject.SetActive(true);
                    Vars.SM.UserSpeakingType = IsSpotify ? SpeakingMicrophone.SpeakingType.Spotify : SpeakingMicrophone.SpeakingType.Regular;
                    Vars.SM.SpeakingDotColor = DotColor(IsSpotify);

                    AudioSource.PlayClipAtPoint(Vars.SM.MicrophoneOn, GorillaTagger.Instance.offlineVRRig.headMesh.transform.position, 99f);
                };
                Vars.CommandEnded += delegate
                {
                    Vars.SM.SpeakingDotObject.SetActive(false);
                    AudioSource.PlayClipAtPoint(Vars.SM.MicrophoneOff, GorillaTagger.Instance.offlineVRRig.headMesh.transform.position, 99f);
                };

                string Commands = "### Spotify Commands\n";
                foreach (CommandInfo info in Vars.SpotifyCommands)
                {
                    Commands += $"[MUSIC] Name: {info.CommandActivationWord}, Description: {info.CommandDescription}\n";
                }
                Commands += "\n### Default Commands\n";
                foreach (CommandInfo info in Vars.DefaultCommands)
                {
                    Commands += $"[{VoiceActivationWord.Value}] Name: {info.CommandActivationWord}, Description: {info.CommandDescription}\n";
                }
                
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), @"BepInEx\VoiceControls\Commands.txt"), Commands);
                Vars.Log("Created SpeakingMicrophone and Effects");
                Vars.Log($"Config Settings: Use Player Color: {UsePlayerColorEntry.Value}, Use Custom Color: {UseHexadecimalColor.Value}, Custom Color Hexadecimal, Custom Color RGB: R({Vars.SM.HexColor.r}) G({Vars.SM.HexColor.g}) B({Vars.SM.HexColor.b})");
            });
        }
        Color DotColor(bool IsSpotify)
        {
            if (UsePlayerColorEntry.Value == false && UseHexadecimalColor.Value == false)
            {
                if (IsSpotify) return Color.green;
                else return Color.cyan;
            }
            else
            {
                if (UseHexadecimalColor.Value == true) return Vars.SM.HexColor;
                else return GorillaTagger.Instance.offlineVRRig.playerColor;
            }
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

            // Default Commands

            Vars.DefaultCommands.Add(new CommandInfo()
            {
                CommandActivationWord = "ping",
                CommandDescription = "pinging system for your GT Friends",
                CommandAction = () => { StartCoroutine(Modules.PingPlayers(true)); }
            });
        }
        // I made this to organise stuff
        void CreateConfigEntries()
        {
            ConfigFile Config = new ConfigFile(Path.Combine(Directory.GetCurrentDirectory(), @"BepInEx\VoiceControls\SpeechRecognition.cfg"), true);
            // Microphone Color Settings
            UsePlayerColorEntry = Config.Bind("Microphone Color Settings", "Microphone Dot Uses Player Color", false, "If this is enabled, the dot that is next to the microphone that is used for finding out if you are using the speech recognition, if it isnt spotify mode, will be your ingame player color ( overrides Microphone Dot Uses Hex Color )");
            UseHexadecimalColor = Config.Bind("Microphone Color Settings", "Microphone Dot Uses Hex Color", false, "If this is enabled, the dot that is next to the microphone that is used for finding out if you are using the speech recognition, if it isnt spotify mode, will be the hex color you choose");
            HexColor = Config.Bind("Microphone Color Settings", "Microphone Dot Hex Color", "#33bbff", "The color the Speaking Dot will be if (Microphone Dot Uses Hex Color) Is Enabled and (Microphone Dot Uses Player Color) is Disabled. This must be a hexadecimal color");
            ConfigEntry<bool> UseColorOnMicrophone = Config.Bind("Microphone Color Settings", "Use Color On Microphone", false, "This makes it so the color of the microphone will either be the color of the player, or the custom color of your choice");

            // Default Voice Activation
            VoiceActivationWord = Config.Bind("Default Voice Activiation", "Activiation Word", "JARVIS", "So basically this is the first word you will say before a command, so [word] [command]");

            // Module Settings
            Vars.MS = new ModuleSettings()
            {
                PingAmount = Config.Bind("Module Settings", "Ping Amount", 1, "The amount of times your friend will be pinged").Value
            };
        }
    }
    internal class BepinexEntry
    {
        public const string GUID = "com.entityb.voicecontrols";
        public const string Name = "VoiceControls";
        public const string Version = "1.0.0";
    }
}
