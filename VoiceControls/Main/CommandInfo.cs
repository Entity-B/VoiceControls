﻿using System;
using System.Collections.Generic;
using System.Text;

namespace VoiceControls.Main
{
    public class CommandInfo
    {
        public Action CommandAction;
        public string CommandActivationWord;
        public string CommandDescription;
        public CommandType TypeOfCommand;
        public enum CommandType
        {
            Default, 
            Spotify, 
            Global,
            Error,
            All
        }
    }
}
