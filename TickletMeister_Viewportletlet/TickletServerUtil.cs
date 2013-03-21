using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TickletMeister_Viewportletlet
{
    class ConnectionState
    {
        private Socket socket;
        private byte[] buffer;

        public ConnectionState(Socket sock, byte[] buff)
        {
            socket = sock;
            buffer = buff;
        }

        public Socket getSocket()
        {
            return socket;
        }

        public byte[] getBuffer()
        {
            return buffer;
        }
    }

    class Message
    {
        String tag;
        String data;

        public const int BUFF_SIZE = 1024;

        public Message(String t, String d)
        {
            tag = t;
            data = d;
        }

        public String getTag()
        {
            return tag;
        }

        public String getData()
        {
            return data;
        }

        public override string ToString()
        {
            return tag + " " + data;
        }

        public static Message decodeMessage(byte[] buffer)
        {
            Message ret = null;
            char[] trims = new char[2];
            trims[0] = (char)0;
            trims[1] = ' ';
            try
            {
                String decode = System.Text.Encoding.Default.GetString(buffer);
                String tag = decode.Substring(0, decode.IndexOf(' '));
                String data = decode.Substring(decode.IndexOf(' ') + 1).TrimEnd(trims);
                ret = new Message(tag, data);
            }
            catch (Exception e)
            {
                Console.WriteLine("unable to parse message...");
                for (int i = 0; i < buffer.Length; i++)
                {
                    Console.Write(buffer[i]);
                }
                Console.WriteLine();
                Console.WriteLine(e.Message);

            }
            return ret;
        }

        public static byte[] encodeMessage(Message m)
        {
            byte[] ret = new byte[BUFF_SIZE];
            int i = 0;
            String tag = m.getTag();
            String data = m.getData();
            try
            {
                Action<String> fillBuffer = (String s) =>
                {
                    byte[] sBytes = System.Text.Encoding.Default.GetBytes(s);
                    for (int j = 0; j < sBytes.Length; j++)
                    {
                        ret[i++] = sBytes[j];
                    }
                };
                fillBuffer(tag);
                fillBuffer(" ");
                fillBuffer(data);
                return ret;
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("unable to fill buffer: message failed to send: " + m.ToString());
            }
            return null;
        }

        public static void sendMessageTo(Message message, Socket clientSocket)
        {
            byte[] encoding = encodeMessage(message);
            clientSocket.Send(encoding, 0, BUFF_SIZE, SocketFlags.None);
        }

    }


}
