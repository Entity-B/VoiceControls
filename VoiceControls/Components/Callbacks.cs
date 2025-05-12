using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using VoiceControls.Tools;

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

        public static Action<EventData> RaiseEventRan;

        

        // Awake
        void Awake()
        {
            PhotonNetwork.NetworkingClient.EventReceived += delegate (EventData ED) { RaiseEventRan?.Invoke(ED); };
            PlayerJoined += delegate (Player Person)
            {
                if (FriendBackendController.Instance.FriendsList.FirstOrDefault(f => f.Presence.FriendLinkId == Person.UserId) != null) FriendJoined?.Invoke(Person);
            };
            PlayerLeft += delegate (Player Person)
            {
                if (FriendBackendController.Instance.FriendsList.FirstOrDefault(f => f.Presence.FriendLinkId == Person.UserId) != null) FriendLeft?.Invoke(Person);
            };

            FriendJoined += delegate (Player Friend) { Vars.FriendsInRoom.Append(Friend); };
            FriendLeft += delegate (Player Friend) { Vars.FriendsInRoom.ToList().Remove(Friend); };
        }
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
