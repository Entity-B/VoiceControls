using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;
using UnityEngine;
using VoiceControls.Tools;

namespace VoiceControls.Main
{
    public static class Modules
    {
        public static IEnumerator PingPlayers(bool Friends)
        {
            foreach (var Players in Friends ? Vars.FriendsInRoom : PhotonNetwork.PlayerListOthers)
            {
                if (Players.CurrentVRRig().currentMatIndex == GorillaTagger.Instance.offlineVRRig.currentMatIndex)
                {
                    AudioSource.PlayClipAtPoint(Vars.VFX.PlayerPingAudio, Players.CurrentVRRig().transform.position);
                    yield return new WaitForSeconds(1.5f);
                }
            }
        }
    }
}
