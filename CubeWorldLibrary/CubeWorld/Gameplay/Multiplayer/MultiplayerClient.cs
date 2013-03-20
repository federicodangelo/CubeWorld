using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace CubeWorld.Gameplay.Multiplayer
{
    public class MultiplayerClient
    {
        private const int MAX_SERVER_SIDE_DATA_AMOUNT = 32 * 1024;
        private const int MAX_CLIENT_SIDE_DATA_AMOUNT = 1024 * 1024;

        public int id = -1;

        private MultiplayerStats stats;
        private TcpClient tcpClient;
        private IMultiplayerClientListener clientListener;

        private List<MultiplayerAction> actionsQueue = new List<MultiplayerAction>();

        private byte[] currentDataToSend;
        private int currentDataToSendOffset;

        private byte[] currentDataToReceive;
        private int currentDataToReceiveOffset;

        private MultiplayerServer server;

        private bool serverSide = false;
        private bool firstByte = true;
        public bool sendingPolicy = false;
        private bool policySent = false;

        public MultiplayerClient(string server, int port, IMultiplayerClientListener clientListener)
            : this(new TcpClient(server, port), null, clientListener)
        {
            tcpClient.Client.Send(new byte[] { 1 });

            serverSide = false;
        }

        public MultiplayerClient(TcpClient tcpClient, MultiplayerServer server, IMultiplayerClientListener clientListener)
        {
            this.stats = MultiplayerStats.Singleton;
            this.clientListener = clientListener;

            this.server = server;
            this.tcpClient = tcpClient;

            this.tcpClient.NoDelay = true;
            this.tcpClient.Client.Blocking = false;

            serverSide = true;
        }

        public void Clear()
        {
            tcpClient.Close();
        }

        public void AddAction(MultiplayerAction action)
        {
            actionsQueue.Add(action);
        }

        public bool Update(int limitActions)
        {
            if (serverSide && firstByte == true)
            {
                ReceiveFirstByte();
            }
            else if (sendingPolicy == false)
            {
                SendActions();
                ReceiveActions(limitActions);
            }
            else if (sendingPolicy == true)
            {
                return policySent == false;
            }

            return true;            
        }

        private const string PolicyString =
                @"<?xml version='1.0'?>
                    <cross-domain-policy>
                    <allow-access-from domain=""*"" to-ports=""*"" />
                </cross-domain-policy>";

        public void ReceiveFirstByte()
        {
            if (tcpClient.Client.Available > 0)
            {
                byte[] buffer = new byte[1];

                if (tcpClient.Client.Receive(buffer) == 1)
                {
                    firstByte = false;

                    if (buffer[0] == 1)
                    {
                        sendingPolicy = false;
                        server.ClientAccepted(this, false);
                    }
                    else
                    {
                        sendingPolicy = true;
                        server.ClientAccepted(this, true);
                        tcpClient.Client.Send(Encoding.UTF8.GetBytes(PolicyString));
                        tcpClient.Client.LingerState = new LingerOption(true, 20);
                        tcpClient.Client.BeginDisconnect(false, new AsyncCallback(DisconnectCallback), null);
                    }
                }
            }
        }

        private void DisconnectCallback(IAsyncResult ar)
        {
            policySent = true;
            tcpClient.Client.EndDisconnect(ar);
        }


        private void ReceiveActions(int limitActions)
        {
            while (tcpClient.Client.Available > 0 && limitActions > 0)
            {
                if (currentDataToReceive == null)
                {
                    if (tcpClient.Client.Available >= 4)
                    {
                        byte[] dataSizeBuffer = new byte[4];
                        tcpClient.Client.Receive(dataSizeBuffer, 0, 4, SocketFlags.None);
                        int dataSize =
                            (dataSizeBuffer[0] << 0) |
                            (dataSizeBuffer[1] << 8) |
                            (dataSizeBuffer[2] << 16) |
                            (dataSizeBuffer[3] << 24);

                        if (serverSide == true && dataSize > MAX_SERVER_SIDE_DATA_AMOUNT ||
                            serverSide == false && dataSize > MAX_CLIENT_SIDE_DATA_AMOUNT)
                        {
                            throw new Exception("Invalid amount of data to receive [" + dataSize + "]");
                        }

                        currentDataToReceive = new byte[dataSize];
                        currentDataToReceiveOffset = 0;
                    }
                    else
                    {
                        return;
                    }
                }

                if (currentDataToReceive != null)
                {
                    int bytesRead = tcpClient.Client.Receive(currentDataToReceive, currentDataToReceiveOffset, currentDataToReceive.Length - currentDataToReceiveOffset, SocketFlags.None);
                    if (bytesRead < 0)
                        throw new Exception("Socket Read Bytes < 0");

                    currentDataToReceiveOffset += bytesRead;

                    if (currentDataToReceiveOffset == currentDataToReceive.Length)
                    {
                        MultiplayerAction action = MultiplayerProtocol.Deserialize(currentDataToReceive);

                        stats.receivedActions[(int)action.action]++;

                        clientListener.ClientActionReceived(this, action);

                        currentDataToReceive = null;

                        limitActions--;
                    }
                }
            }
        }

        private void SendActions()
        {
            if (currentDataToSend == null)
            {
                if (actionsQueue.Count > 0)
                {
                    List<byte> bytesToSend = new List<byte>();

                    foreach (MultiplayerAction action in actionsQueue)
                    {
                        stats.sentActions[(int)action.action]++;

                        byte[] data = MultiplayerProtocol.Serialize(action);

                        bytesToSend.Add((byte)((data.Length >> 0) & 0xFF));
                        bytesToSend.Add((byte)((data.Length >> 8) & 0xFF));
                        bytesToSend.Add((byte)((data.Length >> 16) & 0xFF));
                        bytesToSend.Add((byte)((data.Length >> 24) & 0xFF));
                        bytesToSend.AddRange(data);
                    }

                    currentDataToSend = bytesToSend.ToArray();
                    currentDataToSendOffset = 0;

                    actionsQueue.Clear();
                }
            }

            if (currentDataToSend != null)
            {
                int bytesSent = 0;
                try
                {
                    bytesSent = tcpClient.Client.Send(currentDataToSend, currentDataToSendOffset, currentDataToSend.Length - currentDataToSendOffset, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    //Try to ignore non-serious errors
                    if (ex.SocketErrorCode != SocketError.WouldBlock &&
                        ex.SocketErrorCode != SocketError.IOPending &&
                        ex.SocketErrorCode != SocketError.NoBufferSpaceAvailable)
                    {
                        //It's a serious error.. re-throw the exception
                        throw ex;
                    }
                }

                if (bytesSent < 0)
                    throw new Exception("Socket Sent Bytes < 0");

                currentDataToSendOffset += bytesSent;

                if (currentDataToSendOffset == currentDataToSend.Length)
                    currentDataToSend = null;
            }
        }
    }
}
