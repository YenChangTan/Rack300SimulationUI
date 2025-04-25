using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Rack300SimulationUI
{
    public class TCPService
    {
        private TcpListener _listener;
        private ITCPControl _iTcpControl;
        private ConcurrentBag<TcpClient> _clients = new ConcurrentBag<TcpClient>();
        private System.Timers.Timer HeartBeatChecker;
        public TCPService(ITCPControl iTcpControl)
        {
            _iTcpControl = iTcpControl;

        }

        private readonly string[] CommandStr = new string[]
        {
            "Rd_Err",//0
            "Rd_Sen",//1
            "RstErr",//2
            "Rd_Led",//3
            "Fil_On",//4
            "FilOff",//5
            "Rmv_On",//6*
            "RmvOff",//7
            "Wr_LED",//12*
            "Rly1On",//13
            "Rly1Of",//14
            "Rly2On",//15
            "Rly2Of",//16
            "Rly3On",//17
            "Rly3Of",//18
            "Rly4On",//19
            "Rly4Of",//20
        };



        public async Task StartAsync(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();

            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _iTcpControl.ResetHeartBeat();
                _clients.Add(client);
                Console.WriteLine($"[Server] New client connected. Total clients: {_clients.Count}");

                _ = HandleClientAsync(client);
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            var buffer = new byte[1024];
            var stream = client.GetStream();

            try
            {
                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length); // non-blocking
                    _iTcpControl.ResetHeartBeat();
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    byte[] RcvByte = new byte[bytesRead];
                    Array.Copy(buffer, RcvByte, bytesRead);
                    if (!CheckValidated(RcvByte))
                    {
                        await BroadcastAsync(Encoding.ASCII.GetBytes("RcvError"));
                        continue;
                    }
                    await BroadcastAsync(RcvByte);
                    if (Encoding.ASCII.GetString(RcvByte,2,6) == "Rd_Sen" || Encoding.ASCII.GetString(RcvByte, 2, 6) == "Rd_Err")
                    {
                        byte[] ToSend = HandleRd_SenOrRd_Err(RcvByte);
                        await BroadcastAsync(ToSend);
                    }
                    else
                    {
                        HandleActionExceptRd_SenAndRd_Err(RcvByte);
                        
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Server] Error: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }
        private bool CheckValidated(byte[] rcvData)
        {
            if (!CheckCRC(rcvData))
            {
                return false;
            }
            string rcvCommand = Encoding.ASCII.GetString(rcvData, 2, 6);
            foreach (var command in CommandStr)
            {
                if (rcvCommand == command)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CheckCRC(byte[] data)
        {
            ushort crc = 0xFFFF;
            foreach (byte element in data)
            {
                crc ^= element;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x0001) == 1)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc == 0x00;
        }
        public byte[] CRCCal(byte[] data)
        {
            ushort crc = 0xFFFF;
            foreach (byte element in data)
            {
                crc ^= element;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x0001) == 1)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            byte[] CRC = { (byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF) };
            return CRC;
        }

        private async Task BroadcastAsync(byte[] ByteToSend)
        {

            foreach (var client in _clients)
            {
                if (!client.Connected) continue;

                try
                {
                    var stream = client.GetStream();
                    await stream.WriteAsync(ByteToSend, 0, ByteToSend.Length);
                }
                catch
                {
                    // skip dead clients
                }
            }
        }

        private void HandleActionExceptRd_SenAndRd_Err(byte[] rcvByte)
        {
            string command = Encoding.ASCII.GetString(rcvByte, 2, 6);
            switch (command)
            {
                case "RstErr":
                    _iTcpControl.RstErr();
                    break;
                case "Fil_On":
                    _iTcpControl.FillOnOff(true);
                    break;
                case "FilOff":
                    _iTcpControl.FillOnOff(false);
                    break;
                case "Rmv_On":
                    try
                    {
                        byte[] RemoveLocationByte = new byte[rcvByte.Length - 10];
                        Array.Copy(rcvByte, 8, RemoveLocationByte, 0, RemoveLocationByte.Length);
                        _iTcpControl.RmvOnOff(true, RemoveLocationByte);
                        
                    }
                    catch
                    {
                        Debug.WriteLine("Error here");
                    }
                    break;
                case "RmvOff":
                    _iTcpControl.RmvOnOff(false);
                    break;
                case "Wr_LED":
                    byte[] LedBytes = new byte[rcvByte.Length - 10];
                    Array.Copy(rcvByte, 8, LedBytes, 0, LedBytes.Length);
                    _iTcpControl.Wr_Led(LedBytes);
                    break;
                case "Rly1On":
                    _iTcpControl.Rly1OnOff(true);
                    break;
                case "Rly1Of":
                    _iTcpControl.Rly1OnOff(false);
                    break;
                case "Rly2On":
                    _iTcpControl.Rly2OnOff(true);
                    break;
                case "Rly2Of":
                    _iTcpControl.Rly2OnOff(false);
                    break;
                case "Rly3On":
                    _iTcpControl.Rly3OnOff(true);
                    break;
                case "Rly3Of":
                    _iTcpControl.Rly3OnOff(false);
                    break;
                case "Rly4On":
                    _iTcpControl.Rly4OnOff(true);
                    break;
                case "Rly4Of":
                    _iTcpControl.Rly3OnOff(false);
                    break;
            }
        }

        private byte[] HandleRd_SenOrRd_Err(byte[] rcvByte)
        {
            try
            {
                string command = Encoding.ASCII.GetString(rcvByte, 2, 6);
                byte[] dataBytes = new byte[48];
                byte[] withoutCRC = new byte[104];
                byte[] toSend = new byte[106];
                withoutCRC[0] = 1;
                withoutCRC[1] = 0;
                Encoding.ASCII.GetBytes(command).CopyTo(withoutCRC, 2);
                switch (command)
                {
                    case "Rd_Sen":
                        dataBytes = _iTcpControl.Rd_Sen();
                        break;
                    case "Rd_Err":
                        dataBytes = _iTcpControl.Rd_Err();
                        break;
                }
                
                dataBytes.CopyTo(withoutCRC, 8);
                withoutCRC.CopyTo(toSend, 0);
                CRCCal(withoutCRC).CopyTo(toSend, 104);

                return toSend;
            }
            catch
            {
                Debug.WriteLine("Error here");
                throw new Exception();
            }
        }
    }
}
