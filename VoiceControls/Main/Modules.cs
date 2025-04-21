using System.Collections;
using Photon.Pun;
using UnityEngine;
using VoiceControls.Tools;

namespace VoiceControls.Main
{
    public static class Modules
    {
        public static IEnumerator PingPlayers(bool Friends)
        {
            for (int i = 0; i > Vars.MS.PingAmount; i++)
            {
                foreach (var Players in Friends ? Vars.FriendsInRoom : PhotonNetwork.PlayerListOthers)
                {
                    if (Players.CurrentVRRig().currentMatIndex == GorillaTagger.Instance.offlineVRRig.currentMatIndex)
                    {
                        AudioSource.PlayClipAtPoint(Vars.ModuleEffects.PlayerPingAudio, Players.CurrentVRRig().transform.position);
                        yield return new WaitForSeconds(1.5f);
                    }
                }
                yield return new WaitForSeconds(2f);
            }
        }
    }
}
