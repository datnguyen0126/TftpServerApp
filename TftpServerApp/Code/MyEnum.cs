using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TftpServerApp.Code
{
    public enum Error
    {
        UNKNOWN = 0,
        FILE_NOT_FOUND,
        ACCESS_VIOLATION,
        DISK_FULL_OR_ALLOCATION_EXCEEDED,
        ILLEGAL_TFTP_OPERATION,
        UNKNOWN_TRANSFER_ID,
        FILE_ALREADY_EXIST
    }
    public enum Opcode
    {
        READ_REQUEST = 1,
        WRITE_REQUEST,
        DATA,
        ACK,
        ERROR
    }
    public enum Mode
    {
        NET_ASCII = 1,
        OCTET = 2,
        MAIL = 3
    }
    public class MyEnum
    {
        public static byte[] CreateRequestPacket(Opcode opCode, string remoteFile, Mode tftpMode)
        {
            int pos = 0;
            string modeAscii = tftpMode.ToString().ToLowerInvariant();
            byte[] ret = new byte[modeAscii.Length + remoteFile.Length + 4];

            ret[pos++] = 0;
            ret[pos++] = (byte)opCode;

            pos += Encoding.ASCII.GetBytes(remoteFile, 0, remoteFile.Length, ret, pos);
            ret[pos++] = 0;
            pos += Encoding.ASCII.GetBytes(modeAscii, 0, modeAscii.Length, ret, pos);
            ret[pos] = 0;

            return ret;
        }
        public static byte[] CreateDataPacket(int blockNr, byte[] data)
        {
            byte[] ret = new byte[4 + data.Length];

            ret[0] = 0;
            ret[1] = (byte)Opcode.DATA;
            ret[2] = (byte)((blockNr >> 8) & 0xff);
            ret[3] = (byte)(blockNr & 0xff);
            Array.Copy(data, 0, ret, 4, data.Length);
            return ret;
        }

        public static byte[] CreateAckPacket(int blockNr)
        {
            byte[] ret = new byte[4];

            ret[0] = 0;
            ret[1] = (byte)Opcode.ACK;

            ret[2] = (byte)((blockNr >> 8) & 0xff);
            ret[3] = (byte)(blockNr & 0xff);
            return ret;
        }
        public static string GetFileName(byte[] data)
        {
            int i;
            for (i = 2; i < data.Length; i++)
                if (data[i] == 0) break;
            return Encoding.ASCII.GetString(data, 2, (i - 2));
        }
        public static byte[] CreateErrorPacket(Error errorNr)
        {
            int pos = 0;
            string errorMsg = errorNr.ToString().ToLowerInvariant();
            byte[] ret = new byte[errorMsg.Length + 5];

            ret[0] = 0;
            ret[1] = (byte)Opcode.ERROR;

            ret[2] = 0;
            ret[3] = (byte)errorNr;

            pos += Encoding.ASCII.GetBytes(errorMsg, 0, errorMsg.Length, ret, pos);
            ret[pos++] = 0;

            return ret;
        }
    }
}
