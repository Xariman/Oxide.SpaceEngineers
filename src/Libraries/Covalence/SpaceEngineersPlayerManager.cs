﻿extern alias Oxide;

using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide::ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;

namespace Oxide.Game.SpaceEngineers.Libraries.Covalence
{
    /// <summary>
    /// Represents a generic player manager
    /// </summary>
    public class SpaceEngineersPlayerManager : IPlayerManager
    {
        [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
        private struct PlayerRecord
        {
            public string Name;
            public ulong Id;
        }

        private IDictionary<string, PlayerRecord> playerData;
        private IDictionary<string, SpaceEngineersPlayer> allPlayers;
        private IDictionary<string, SpaceEngineersPlayer> connectedPlayers;

        internal void Initialize()
        {
            Utility.DatafileToProto<Dictionary<string, PlayerRecord>>("oxide.covalence");
            playerData = ProtoStorage.Load<Dictionary<string, PlayerRecord>>("oxide.covalence") ?? new Dictionary<string, PlayerRecord>();
            allPlayers = new Dictionary<string, SpaceEngineersPlayer>();
            connectedPlayers = new Dictionary<string, SpaceEngineersPlayer>();

            foreach (var pair in playerData) allPlayers.Add(pair.Key, new SpaceEngineersPlayer(pair.Value.Id, pair.Value.Name));
        }

        internal void PlayerJoin(ulong userId, string name)
        {
            var id = userId.ToString();

            PlayerRecord record;
            if (playerData.TryGetValue(id, out record))
            {
                record.Name = name;
                playerData[id] = record;
                allPlayers.Remove(id);
                allPlayers.Add(id, new SpaceEngineersPlayer(userId, name));
            }
            else
            {
                record = new PlayerRecord { Id = userId, Name = name };
                playerData.Add(id, record);
                allPlayers.Add(id, new SpaceEngineersPlayer(userId, name));
            }

            ProtoStorage.Save(playerData, "oxide.covalence");
        }

        internal void PlayerConnected(IMyPlayer player)
        {
            allPlayers[player.SteamUserId.ToString()] = new SpaceEngineersPlayer(player);
            connectedPlayers[player.SteamUserId.ToString()] = new SpaceEngineersPlayer(player);
        }

        internal void PlayerDisconnected(IMyPlayer player) => connectedPlayers.Remove(player.SteamUserId.ToString());

        #region Player Finding

        /// <summary>
        /// Gets all players
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IPlayer> All => allPlayers.Values.Cast<IPlayer>();

        /// <summary>
        /// Gets all connected players
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IPlayer> Connected => connectedPlayers.Values.Cast<IPlayer>();

        /// <summary>
        /// Gets all sleeping players
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IPlayer> Sleeping => null; // TODO: Implement if/when possible

        /// <summary>
        /// Finds a single player given unique ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IPlayer FindPlayerById(string id)
        {
            SpaceEngineersPlayer player;
            return allPlayers.TryGetValue(id, out player) ? player : null;
        }

        /// <summary>
        /// Finds a single connected player given game object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public IPlayer FindPlayerByObj(object obj) => connectedPlayers.Values.FirstOrDefault(p => p.Object == obj);

        /// <summary>
        /// Finds a single player given a partial name or unique ID (case-insensitive, wildcards accepted, multiple matches returns null)
        /// </summary>
        /// <param name="partialNameOrId"></param>
        /// <returns></returns>
        public IPlayer FindPlayer(string partialNameOrId)
        {
            var players = FindPlayers(partialNameOrId).ToArray();
            return players.Length == 1 ? players[0] : null;
        }

        /// <summary>
        /// Finds any number of players given a partial name or unique ID (case-insensitive, wildcards accepted)
        /// </summary>
        /// <param name="partialNameOrId"></param>
        /// <returns></returns>
        public IEnumerable<IPlayer> FindPlayers(string partialNameOrId)
        {
            foreach (var player in allPlayers.Values)
            {
                if (player.Name != null && player.Name.IndexOf(partialNameOrId, StringComparison.OrdinalIgnoreCase) >= 0 || player.Id == partialNameOrId)
                    yield return player;
            }
        }

        #endregion Player Finding
    }
}
