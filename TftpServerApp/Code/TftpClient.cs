using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TftpServerApp.Code
{
    public class TftpClient
    {
        private int tftpPort;
        private string tftpServer = "";

        public TftpClient(string server) : this(server, 69)
        {
        }

        public TftpClient(string server, int port)
        {
            Server = server;
            Port = port;
        }
        public int Port
        {
            get { return tftpPort; }
            private set { tftpPort = value; }
        }
        public string Server
        {
            get { return tftpServer; }
            private set { tftpServer = value; }
        }
        public void Get(string remoteFile, string localFile)
        {
            Get(remoteFile, localFile, Mode.OCTET);
        }
        public class TFTPException : Exception
        {

            public string ErrorMessage = "";
            public int ErrorCode = -1;

            public TFTPException(int errCode, string errMsg)
            {
                ErrorCode = errCode;
                ErrorMessage = errMsg;
            }
            public override string ToString()
            {
                return String.Format("TFTPException: ErrorCode: {0} Message: {1}", ErrorCode, ErrorMessage);
            }
        }
        public void Get(string remoteFile, string localFile, Mode tftpMode)
        {
            int packetNr = 1;
            byte[] sendBuffer = MyEnum.CreateRequestPacket(Opcode.READ_REQUEST, remoteFile, tftpMode);
            byte[] recBuffer = new byte[516];

            BinaryWriter fileStream = new BinaryWriter(new FileStream(localFile, FileMode.Create, FileAccess.Write, FileShare.Read));
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(tftpServer), tftpPort);
            UdpClient tftpSocket = new UdpClient();

            tftpSocket.Send(sendBuffer, sendBuffer.Length, serverEP);
            tftpSocket.Client.ReceiveTimeout = 1000;
            recBuffer = tftpSocket.Receive(ref serverEP);

            while (true)
            {
                if (((Opcode)recBuffer[1]) == Opcode.ERROR)
                {
                    fileStream.Close();
                    tftpSocket.Close();
                    throw new TFTPException(((recBuffer[2] << 8) & 0xff00) | recBuffer[3], Encoding.ASCII.GetString(recBuffer, 4, recBuffer.Length - 5).Trim('\0'));
                }
                if ((((recBuffer[2] << 8) & 0xff00) | recBuffer[3]) == packetNr)
                {
                    fileStream.Write(recBuffer, 4, recBuffer.Length - 4);

                    sendBuffer = MyEnum.CreateAckPacket(packetNr++);
                    tftpSocket.Send(sendBuffer, sendBuffer.Length, serverEP);
                }
                if (recBuffer.Length < 516)
                {
                    break;
                }
                else
                {
                    recBuffer = tftpSocket.Receive(ref serverEP);
                }
            }

            tftpSocket.Close();
            fileStream.Close();
        }
        public void Put(string remoteFile, string localFile)
        {
            Put(remoteFile, localFile, Mode.OCTET);
        }

        public void Put(string remoteFile, string localFile, Mode tftpMode)
        {
            int packetNr = 0;
            byte[] sendBuffer = MyEnum.CreateRequestPacket(Opcode.WRITE_REQUEST, remoteFile, tftpMode);
            byte[] recBuffer = new byte[516];

            BinaryReader fileStream = new BinaryReader(new FileStream(localFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse(tftpServer), tftpPort);
            UdpClient tftpSocket = new UdpClient(); IPHostEntry hostEntry = Dns.GetHostEntry(tftpServer);

            tftpSocket.Send(sendBuffer, sendBuffer.Length, serverEP);
            tftpSocket.Client.ReceiveTimeout = 1000;
            recBuffer = tftpSocket.Receive(ref serverEP);

            while (true)
            {
                if ((((Opcode)recBuffer[1]) == Opcode.ACK) && (((recBuffer[2] << 8) & 0xff00) | recBuffer[3]) == packetNr)
                {
                    sendBuffer = MyEnum.CreateDataPacket(++packetNr, fileStream.ReadBytes(512));
                    tftpSocket.Send(sendBuffer, sendBuffer.Length, serverEP);
                }

                if (sendBuffer.Length < 516)
                {
                    break;
                }
                else
                {
                    recBuffer = tftpSocket.Receive(ref serverEP);
                }
            }

            tftpSocket.Close();
            fileStream.Close();
        }
    }
}
