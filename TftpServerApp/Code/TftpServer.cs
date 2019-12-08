using System;
using System.Net;
using System.ComponentModel;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TftpServerApp.Code
{
    public class TftpServer
    {
        private UdpClient tftpServer;
        private string path;

        public TftpServer()
        {
        }
        public string Path
        {
            set
            {
                path = value;
            }
            get
            {
                return path;
            }
        }
        public void StopTftopService()
        {
            tftpServer.Close();
        }

        public void StartTftpServices(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker1 = sender as BackgroundWorker;
            tftpServer = new UdpClient(69);
            byte[] sendBuffer;
            byte[] recBuffer;
            while (true)
            {
                if (worker1.CancellationPending)
                {
                    tftpServer.Close();
                    break;
                }
                int packetNr = 0;
                IPEndPoint iep = new IPEndPoint(IPAddress.Any, 0);
                recBuffer = tftpServer.Receive(ref iep);
                if (((Opcode)recBuffer[1]) == Opcode.READ_REQUEST)
                {
                    string fileName = MyEnum.GetFileName(recBuffer);
                    BinaryReader fileStream = new BinaryReader(new FileStream(path + fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    sendBuffer = MyEnum.CreateDataPacket(++packetNr, fileStream.ReadBytes(512));
                    tftpServer.Send(sendBuffer, sendBuffer.Length, iep);
                    while (true)
                    {
                        recBuffer = tftpServer.Receive(ref iep);

                        if ((((Opcode)recBuffer[1]) == Opcode.ACK) && (((recBuffer[2] << 8) & 0xff00) | recBuffer[3]) == packetNr)
                        {
                            sendBuffer = MyEnum.CreateDataPacket(++packetNr, fileStream.ReadBytes(512));
                            tftpServer.Send(sendBuffer, sendBuffer.Length, iep);
                        }

                        if (sendBuffer.Length < 516)
                        {
                            break;
                        }
                    }
                    fileStream.Close();
                }
                else if (((Opcode)recBuffer[1]) == Opcode.WRITE_REQUEST)
                {
                    packetNr = 0;
                    string fileName = MyEnum.GetFileName(recBuffer);

                    sendBuffer = MyEnum.CreateAckPacket(packetNr++);
                    tftpServer.Send(sendBuffer, sendBuffer.Length, iep);
                    BinaryWriter fileStream = new BinaryWriter(new FileStream(path + fileName, FileMode.Create, FileAccess.Write, FileShare.Read));
                    recBuffer = tftpServer.Receive(ref iep);
                    while (true)
                    {
                        if ((((recBuffer[2] << 8) & 0xff00) | recBuffer[3]) == packetNr)
                        {
                            fileStream.Write(recBuffer, 4, recBuffer.Length - 4);

                            sendBuffer = MyEnum.CreateAckPacket(packetNr++);
                            tftpServer.Send(sendBuffer, sendBuffer.Length, iep);
                        }
                        recBuffer = tftpServer.Receive(ref iep);
                        if (recBuffer.Length < 516)
                        {
                            break;
                        }
                    }
                    fileStream.Close();
                }
            }
        }
    }
}
