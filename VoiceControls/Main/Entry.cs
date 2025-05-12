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

            Vars.SetUpLogger(this.Logger);
            GorillaTagger.OnPlayerSpawned(delegate
            {
                Vars.Manager = new GameObject("VoiceControlsManager");
                Vars.Manager.AddComponent<Callbacks>();
                DontDestroyOnLoad(Vars.Manager);

                CommandsSetup();
                Vars.AllCommands.Add(new CommandInfo()
                {
                    CommandActivationWord = "stop",
                    CommandAction = () => { this.Logger.Log(BepInEx.Logging.LogLevel.Info, "Stopped Listening"); },
                    CommandDescription = "stops command",
                    TypeOfCommand = CommandInfo.CommandType.All
                });
                Vars.Spotify = new KeywordRecognizer(new string[] { "MUSIC" });
                Vars.SpotifyCommand = new KeywordRecognizer(Vars.AllCommands.Where(t => t.TypeOfCommand == CommandInfo.CommandType.Spotify || t.TypeOfCommand == CommandInfo.CommandType.All).Select(c => c.CommandActivationWord).ToArray());

                Vars.Default = new KeywordRecognizer(new string[] { VoiceActivationWord.Value.ToUpper() });
                Vars.DefaultCommand = new KeywordRecognizer(Vars.AllCommands.Where(t => t.TypeOfCommand == CommandInfo.CommandType.Default || t.TypeOfCommand == CommandInfo.CommandType.All).Select(c => c.CommandActivationWord).ToArray());

                Vars.Global = new KeywordRecognizer(new string[] { "GLOBAL" });
                Vars.GlobalCommand = new KeywordRecognizer(Vars.AllCommands.Where(t => t.TypeOfCommand == CommandInfo.CommandType.Global || t.TypeOfCommand == CommandInfo.CommandType.All).Select(c => c.CommandActivationWord).ToArray());

                Vars.Global.Start();
                Vars.Spotify.Start();
                Vars.Default.Start();

                Vars.Spotify.OnPhraseRecognized += delegate
                {
                    Vars.StarterRecognised?.Invoke(CommandInfo.CommandType.Spotify);
                    Vars.Spotify.Stop();
                    Vars.SpotifyCommand.Start();
                };

                Vars.SpotifyCommand.OnPhraseRecognized += delegate (PhraseRecognizedEventArgs speech)
                {
                    CommandInfo command = Vars.AllCommands.Where(t => t.TypeOfCommand == CommandInfo.CommandType.Spotify || t.TypeOfCommand == CommandInfo.CommandType.All).FirstOrDefault(c => c.CommandActivationWord.ToLower() == speech.text.ToLower());
                    try
                    {
                        if (command != null)
                        {
                            this.Logger.Log($"Command: {command.CommandActivationWord}, Description: {command.CommandDescription}");
                            Vars.CommandLogs.Add($"[{command.TypeOfCommand.ToString().ToUpper()}] {command.CommandActivationWord}");
                            command.CommandAction.Invoke();
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex);
                    }
                    Vars.CommandEnded?.Invoke(command == null ? CommandInfo.CommandType.Error : CommandInfo.CommandType.Spotify);
                    Vars.Spotify.Start();
                    Vars.SpotifyCommand.Stop();
                };

                Vars.Default.OnPhraseRecognized += delegate
                {
                    Vars.StarterRecognised?.Invoke(CommandInfo.CommandType.Default);
                    Vars.Default.Stop();
                    Vars.DefaultCommand.Start();
                };
                Vars.DefaultCommand.OnPhraseRecognized += delegate (PhraseRecognizedEventArgs speech)
                {
                    CommandInfo command = Vars.AllCommands.Where(t => t.TypeOfCommand == CommandInfo.CommandType.Default || t.TypeOfCommand == CommandInfo.CommandType.All).FirstOrDefault(c => c.CommandActivationWord.ToLower() == speech.text.ToLower());
                    try
                    {
                        if (command != null)
                        {
                            this.Logger.Log($"Command: {command.CommandActivationWord}, Description: {command.CommandDescription}");
                            Vars.CommandLogs.Add($"[{command.TypeOfCommand.ToString().ToUpper()}] {command.CommandActivationWord}");
                            command.CommandAction.Invoke();
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex);
                    }
                    Vars.CommandEnded?.Invoke(command == null ? CommandInfo.CommandType.Error : CommandInfo.CommandType.Default);
                    Vars.Default.Start();
                    Vars.DefaultCommand.Stop();
                };

                Vars.GlobalCommand.OnPhraseRecognized += delegate (PhraseRecognizedEventArgs speech)
                {
                    CommandInfo command = Vars.AllCommands.Where(t => t.TypeOfCommand == CommandInfo.CommandType.Global || t.TypeOfCommand == CommandInfo.CommandType.All).FirstOrDefault(c => c.CommandActivationWord.ToLower() == speech.text.ToLower());
                    try
                    {
                        if (command != null)
                        {
                            this.Logger.Log($"Command: {command.CommandActivationWord}, Description: {command.CommandDescription}");
                            Vars.CommandLogs.Add($"[{command.TypeOfCommand.ToString().ToUpper()}] {command.CommandActivationWord}");
                            command.CommandAction.Invoke();
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex);
                    }
                    Vars.CommandEnded?.Invoke(command == null ? CommandInfo.CommandType.Error : CommandInfo.CommandType.Global);
                    Vars.Global.Start();
                    Vars.GlobalCommand.Stop();

                };

                Vars.Global.OnPhraseRecognized += delegate
                {
                    Vars.StarterRecognised?.Invoke(CommandInfo.CommandType.Global);
                    Vars.Global.Stop();
                    Vars.GlobalCommand.Start();
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

                        SpotifyOn = assetBundle.LoadAsset<AudioClip>("music-on"),
                        SpotifyOff = assetBundle.LoadAsset<AudioClip>("music-off"),

                        GlobalOn = assetBundle.LoadAsset<AudioClip>("global-on"),
                        GlobalOff = assetBundle.LoadAsset<AudioClip>("power-off"),

                        MicrophoneObject = ConsoleCanvasObject.transform.GetChild(0).GetChild(0).gameObject,
                        SpeakingDotObject = ConsoleCanvasObject.transform.GetChild(0).GetChild(1).gameObject,

                        UsePlayersColorForMicrophoneDot = UsePlayerColorEntry.Value,
                        UseCustomColor = UseHexadecimalColor.Value,
                        HexColor = color,

                        UserSpeakingType = CommandInfo.CommandType.Default,
                        SpeakingType = SpeakingMicrophone.SpecialColorType.None,
                    };
                    Vars.SM.SpeakingType = Vars.SM.UsePlayersColorForMicrophoneDot ? SpeakingMicrophone.SpecialColorType.PlayerColor : (Vars.SM.UseCustomColor ? SpeakingMicrophone.SpecialColorType.CustomHexColor : SpeakingMicrophone.SpecialColorType.None);
                    Vars.SM.UserSpeakingType = CommandInfo.CommandType.Default;
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

                Vars.StarterRecognised += delegate (CommandInfo.CommandType CommandType)
                {
                    Vars.SM.SpeakingDotObject.SetActive(true);
                    Vars.SM.UserSpeakingType = CommandType;
                    Vars.SM.SpeakingDotColor = DotColor(CommandType);

                    AudioClip Sound = CommandType == CommandInfo.CommandType.Error ? Vars.SM.MicrophoneOn : (CommandType == CommandInfo.CommandType.Default ? Vars.SM.MicrophoneOn : (CommandType == CommandInfo.CommandType.Spotify ? Vars.SM.SpotifyOn : Vars.SM.GlobalOn));

                    AudioSource.PlayClipAtPoint(Sound, GorillaTagger.Instance.offlineVRRig.headMesh.transform.position, 99f);
                };
                Vars.CommandEnded += delegate (CommandInfo.CommandType type)
                {
                    Vars.SM.SpeakingDotObject.SetActive(false);
                    this.Logger.Log(BepInEx.Logging.LogLevel.Message, $"Command Type Ran: {type}");

                    AudioClip Sound = type == CommandInfo.CommandType.Error ? Vars.SM.MicrophoneOff : (type == CommandInfo.CommandType.Default ? Vars.SM.MicrophoneOff : (type == CommandInfo.CommandType.Spotify ? Vars.SM.SpotifyOff : Vars.SM.GlobalOff));
                    AudioSource.PlayClipAtPoint(Sound, GorillaTagger.Instance.offlineVRRig.headMesh.transform.position, 99f);
                };

                string Commands = "### Spotify Commands\n";
                foreach (CommandInfo info in Vars.AllCommands.Where(t => t.TypeOfCommand == CommandInfo.CommandType.Spotify))
                {
                    Commands += $"[MUSIC] Name: {info.CommandActivationWord}, Description: {info.CommandDescription}\n";
                }
                Commands += "\n### Default Commands\n";
                foreach (CommandInfo info in Vars.AllCommands.Where(t => t.TypeOfCommand == CommandInfo.CommandType.Default))
                {
                    Commands += $"[{VoiceActivationWord.Value}] Name: {info.CommandActivationWord}, Description: {info.CommandDescription}\n";
                }
                Commands += "\n### Global Commands\n";
                foreach (CommandInfo info in Vars.AllCommands.Where(t => t.TypeOfCommand == CommandInfo.CommandType.All))
                {
                    Commands += $"[{VoiceActivationWord.Value}] Name: {info.CommandActivationWord}, Description: {info.CommandDescription}\n";
                }


                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), @"BepInEx\VoiceControls\Commands.txt"), Commands);
                Vars.Log("Created SpeakingMicrophone and Effects");
                Vars.Log($"Config Settings: Use Player Color: {UsePlayerColorEntry.Value}, Use Custom Color: {UseHexadecimalColor.Value}, Custom Color Hexadecimal, Custom Color RGB: R({Vars.SM.HexColor.r}) G({Vars.SM.HexColor.g}) B({Vars.SM.HexColor.b})");

                GameObject HeadTapObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                HeadTapObject.transform.SetParent(GorillaTagger.Instance.offlineVRRig.headConstraint, false);
                HeadTapObject.AddComponent<HeadTouchable>();
            });
        }
        Color DotColor(CommandInfo.CommandType CommandType)
        {
            if (UsePlayerColorEntry.Value == false && UseHexadecimalColor.Value == false)
            {
                if (CommandType == CommandInfo.CommandType.Spotify) return Color.green;
                else if (CommandType == CommandInfo.CommandType.Global)
                {
                    Color color;
                    return ColorUtility.TryParseHtmlString("#836313", out color) ? color : Color.yellow;
                }
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
            Vars.AllCommands.Add(new CommandInfo()
            {
                CommandActivationWord = "play",
                CommandDescription = "Plays/Pauses you're currently spotify song",
                CommandAction = () => SendKey(Vars.SpotifyKeyCodes.PlayOrPause),
                TypeOfCommand = CommandInfo.CommandType.Spotify
            });
            Vars.AllCommands.Add(new CommandInfo()
            {
                CommandActivationWord = "next",
                CommandDescription = "Plays the next song in Queue",
                CommandAction = () => SendKey(Vars.SpotifyKeyCodes.Next),
                TypeOfCommand = CommandInfo.CommandType.Spotify
            });
            Vars.AllCommands.Add(new CommandInfo()
            {
                CommandActivationWord = "last",
                CommandDescription = "The song that was playing last",
                CommandAction = () => SendKey(Vars.SpotifyKeyCodes.Previous),
                TypeOfCommand = CommandInfo.CommandType.Spotify
            });

            // Default Commands

            Vars.AllCommands.Add(new CommandInfo()
            {
                CommandActivationWord = "ping",
                CommandDescription = "pinging system for your GT Friends",
                CommandAction = () => StartCoroutine(Modules.PingPlayers(false)),
                TypeOfCommand = CommandInfo.CommandType.Default
            });

            Vars.AllCommands.Add(new CommandInfo()
            {
                CommandActivationWord = "reload",
                CommandDescription = "Reloads config files, incase you are to lazy to restart your game, yes Im looking at you james",
                CommandAction = () => CreateConfigEntries(),
                TypeOfCommand = CommandInfo.CommandType.Default
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
