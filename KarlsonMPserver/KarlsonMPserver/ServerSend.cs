﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KarlsonMPserver
{
    class ServerSend
    {
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].tcp.SendData(_packet.ToArray());
        }
        private static void SendTCPData(int[] _toClients, Packet _packet)
        {
            _packet.WriteLength();
            foreach (int _client in _toClients)
                Server.clients[_client].tcp.SendData(_packet.ToArray());
        }
        private static void SendTCPData(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
                if (Server.clients[i].tcp.socket != null)
                    Server.clients[i].tcp.SendData(_packet.ToArray());
        }
        private static void SendTCPData(Packet _packet, int[] _exceptClients)
        {
            for (int i = 1; i <= Server.MaxPlayers; i++)
                if (Server.clients[i].tcp.socket != null && !_exceptClients.Contains(i))
                    Server.clients[i].tcp.SendData(_packet.ToArray());
        }

        public static void Welcome(int _toClient, string _msg)
        {
            using Packet _packet = new((int)PacketID.welcome);
            _packet.Write(_toClient);
            _packet.Write(_msg);
            SendTCPData(_toClient, _packet);
        }

        public static void EnterScene(int _toClient, int _fromClient)
        {
            using Packet _packet = new((int)PacketID.enterScene);
            _packet.Write(_fromClient);
            SendTCPData(_toClient, _packet);
        }
        public static void LeaveScene(int _toClient, int _fromClient)
        {
            using Packet _packet = new((int)PacketID.leaveScene);
            _packet.Write(_fromClient);
            SendTCPData(_toClient, _packet);
        }

        public static void ClientsInScene(int _toClient, string _scene)
        {
            List<int> _clients = new();
            foreach (Client client in from x in Server.clients
                                      where x.Value.tcp.socket != null && x.Value.player != null
                                         && x.Value.player.scene == _scene
                                      select x.Value)
                _clients.Add(client.id);
            using Packet _packet = new((int)PacketID.clientsInScene);
            _packet.Write(_clients.Count);
            foreach (int i in _clients)
                _packet.Write(i);
            SendTCPData(_toClient, _packet);
        }

        public static void ClientInfo(int _toClient, int _id)
        {
            using Packet _packet = new((int)PacketID.clientInfo);
            _packet.Write(_id);
            _packet.Write(Server.clients[_id].player.username);
            _packet.Write(Server.clients[_id].player.activeGun);
            SendTCPData(_toClient, _packet);
        }

        public static void ClientMove(int _toClient, int _fromClient, Vector3 _pos, Vector3 _rot)
        {
            using Packet _packet = new((int)PacketID.clientMove);
            _packet.Write(_fromClient);
            _packet.Write(_pos);
            _packet.Write(_rot);
            SendTCPData(_toClient, _packet);
        }

        public static void Chat(int _toClient, string _message)
        {
            using Packet _packet = new((int)PacketID.chat);
            _packet.Write(_message);
            SendTCPData(_toClient, _packet);
        }
        public static void Chat(string _message)
        {
            using Packet _packet = new((int)PacketID.chat);
            _packet.Write(_message);
            SendTCPData(_packet);
        }

        public static void PingAll()
        {
            for (int i = 1; i <= Server.MaxPlayers; i++)
                if (Server.clients[i].tcp.socket != null && Server.clients[i].player != null && Server.clients[i].player.lastPing == DateTime.MinValue)
                {
                    Server.clients[i].player.lastPing = DateTime.Now;
                    using Packet _packet = new((int)PacketID.ping);
                    _packet.Write(Server.clients[i].player.ping);
                    SendTCPData(i, _packet);
                }
        }

        public static void ScoreboardAll()
        {
            // this time we'll prepeare the packet ourself and use TCP.SendData
            using Packet _packet = new((int)PacketID.scoreboard);
            _packet.Write(Server.OnlinePlayers());
            _packet.Write(Server.MaxPlayers);
            for(int i = 0; i < 11; i++)
            {
                int currentScene = 0;
                for (int j = 1; j <= Server.MaxPlayers; j++)
                    if (Server.clients[j].tcp.socket != null && Server.clients[j].player != null && Server.clients[j].player.scene == Constants.allowedSceneNames[i])
                        currentScene++;
                _packet.Write(currentScene);
            }
            for (int i = 1; i <= Server.MaxPlayers; i++)
                if (Server.clients[i].tcp.socket != null && Server.clients[i].player != null)
                {
                    _packet.Write(i);
                    _packet.Write(Server.clients[i].player.username);
                    if(Server.clients[i].player.scene == "" || Server.clients[i].player.scene == null)
                        _packet.Write("MainMenu");
                    else
                        _packet.Write(Server.clients[i].player.scene);
                    _packet.Write(Server.clients[i].player.ping);
                }
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
                if (Server.clients[i].tcp.socket != null && Server.clients[i].player != null)
                    Server.clients[i].tcp.SendData(_packet.ToArray());
        }
        
        public static void Rcon(int _toClient, string _response)
        {
            using Packet _packet = new((int)PacketID.rcon);
        }

        public static void ChangeGun(int _client, int _gunIdx)
        {
            using Packet _packet = new((int)PacketID.changeGun);
            _packet.Write(_client);
            _packet.Write(_gunIdx);
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
                if (Server.clients[i].tcp.socket != null && Server.clients[i].player != null && i != _client && Server.clients[i].player.scene == Server.clients[_client].player.scene)
                    Server.clients[i].tcp.SendData(_packet.ToArray());
        }

        public static void ChangeGrapple(int _client, bool _use, Vector3? _pos = null)
        {
            using Packet _packet = new((int)PacketID.changeGrapple);
            _packet.Write(_client);
            _packet.Write(_use);
            if (_use && _pos != null)
                _packet.Write((Vector3)_pos);
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
                if (Server.clients[i].tcp.socket != null && Server.clients[i].player != null && i != _client && Server.clients[i].player.scene == Server.clients[_client].player.scene)
                    Server.clients[i].tcp.SendData(_packet.ToArray());
        }

    }
}
