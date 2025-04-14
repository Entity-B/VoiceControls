using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VoiceControls.Main
{
    public static class Vars
    {
        public static GameObject Manager;
        public static Dictionary<string, Action> SpotifyCommands = new Dictionary<string, Action>();
    }
}
