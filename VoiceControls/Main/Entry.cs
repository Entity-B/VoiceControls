using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Windows.Speech;
using VoiceControls.Components;

namespace VoiceControls.Main
{
    [BepInPlugin(BepinexEntry.GUID, BepinexEntry.Name, BepinexEntry.Version)]
    public class Entry : BaseUnityPlugin
    {
        bool inRoom;

        public static Entry Instance { get; private set; }
        void Start()
        {
            Instance = this;
            Harmony harm = new Harmony(BepinexEntry.GUID);
            harm.PatchAll();
            GorillaTagger.OnPlayerSpawned(delegate
            {
                Vars.Manager = new GameObject("VoiceControlsManager");
                Vars.Manager.AddComponent<Callbacks>();

                CommandsSetup();

                Vars.Spotify = new KeywordRecognizer(new string[] { "SPOTIFY" });
                Vars.SpotifyCommand = new KeywordRecognizer(Vars.SpotifyCommands.Select(c => c.CommandActivationWord).ToArray());

                Vars.Spotify.OnPhraseRecognized += delegate
                {
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
                    Vars.Spotify.Start();
                    Vars.SpotifyCommand.Stop();
                };
            });
        }
        void Update()
        {

        }
        void CommandsSetup()
        {
            Vars.SpotifyCommands.Add(new CommandInfo() {
                CommandActivationWord = "stop",
                CommandDescription = "Stops you're currently played spotify song",
                CommandAction = () => { }
            });
            Vars.SpotifyCommands.Add(new CommandInfo()
            {
                CommandActivationWord = "play",
                CommandDescription = "Plays you're currently spotify song",
                CommandAction = () => { }
            });
            Vars.SpotifyCommands.Add(new CommandInfo()
            {
                CommandActivationWord = "next",
                CommandDescription = "Plays the next song in Queue",
                CommandAction = () => { }
            });
            Vars.SpotifyCommands.Add(new CommandInfo()
            {
                CommandActivationWord = "last",
                CommandDescription = "The song that was playing last",
                CommandAction = () => { }
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
