using System;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;
using Photon.Realtime;

namespace VoiceControls.Components
{
    internal class Callbacks : MonoBehaviourPunCallbacks
    {
        public static Action RoomJoin;
        public static Action RoomLeave;
        public static Action<Player> PlayerJoined;
        public static Action<Player> PlayerLeft;

        // Room Stuff
        public override void OnJoinedRoom() => RoomJoin?.Invoke();
        public override void OnLeftRoom() => RoomLeave?.Invoke();
        public override void OnPlayerEnteredRoom(Player newPlayer) => PlayerJoined?.Invoke(newPlayer);
        public override void OnPlayerLeftRoom(Player otherPlayer) => PlayerLeft?.Invoke(otherPlayer);

    }
}
