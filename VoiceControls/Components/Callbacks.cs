using System;
using System.Collections.Generic;
using System.Text;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

namespace VoiceControls.Components
{
    internal class Callbacks : MonoBehaviourPunCallbacks
    {
        // Room Actions
        public static Action RoomJoin;
        public static Action RoomLeave;
        public static Action<Hashtable> RoomPropsUpdated;
        // Player Actions
        public static Action<Player> PlayerJoined;
        public static Action<Player> PlayerLeft;
        public static Action<Player, Hashtable> PlayerPropsUpdate;
        // Extras
        public static Action<Player> FriendJoined;
        public static Action<Player> FriendLeft;

        // Room Stuff
        public override void OnJoinedRoom() => RoomJoin?.Invoke();
        public override void OnLeftRoom() => RoomLeave?.Invoke();
        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) => RoomPropsUpdated?.Invoke(propertiesThatChanged);
        // Player Stuff
        public override void OnPlayerEnteredRoom(Player newPlayer) => PlayerJoined?.Invoke(newPlayer);
        public override void OnPlayerLeftRoom(Player otherPlayer) => PlayerLeft?.Invoke(otherPlayer);
        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) => PlayerPropsUpdate?.Invoke(targetPlayer, changedProps);

    }
}
