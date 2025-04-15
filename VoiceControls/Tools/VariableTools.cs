using System;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;
using UnityEngine;

namespace VoiceControls.Tools
{
    public class VariableTools
    {
        public static Color NineRGBTo255RGB(Color color) => new Color(Mathf.RoundToInt(color.r * 9), Mathf.RoundToInt(color.g * 9), Mathf.RoundToInt(color.b * 9));

        public static bool IsCurrentRoomModded() => PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.CustomProperties["gameMode"].ToString().ToLower().Contains("modded") : false;
    }
}
