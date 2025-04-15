﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace VoiceControls.Main
{
    public static class Vars
    {
        public static GameObject Manager;
        public static List<CommandInfo> SpotifyCommands = new List<CommandInfo>();

        public static KeywordRecognizer Spotify;
        public static KeywordRecognizer SpotifyCommand;
    }
}
