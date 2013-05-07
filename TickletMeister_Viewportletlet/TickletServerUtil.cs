using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TickletMeister_Viewportletlet
{
    

    public class Message
    {
        String tag;
        String data;

        // public const int BUFF_SIZE = 217; //325 required to encode ticklet
        //public const int BUFF_SIZE =317;
        //public const int BUFF_SIZE = 325;
        public const int BUFF_SIZE = 400;
        public const int OFFSET = 41;
        public const int BUFF_SIZE_UNENCRYPT = 1024;

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

        /**
         * Given an input string s and a tag, this function
         * returns a truncated version of s that will fit in a buffer
         * with the contents "<tag> <space> <s>"
         * 
         * if the input string is empty, a space character is returned
         * */
        public static String trimString(String s, String tag)
        {
            int tagLength = tag.Length + 1;
            if (s.Length + tagLength >= BUFF_SIZE)
            {
                return s.Substring(0, BUFF_SIZE - tagLength);
            }
            else if("".Equals(s))
            {
                return " ";
            }
            else return s;
        }

        public static Message decodeMessage(byte[] buffer)
        {
            Message ret = null;
            char[] trims = new char[1];
            trims[0] = (char)0;
            //trims[1] = ' ';
            try
            {
                String decode = System.Text.Encoding.Default.GetString(buffer);
                //Console.WriteLine(decode);
                String tag = decode.Substring(0, decode.IndexOf(' '));
                String data = decode.Substring(decode.IndexOf(' ') + 1).TrimEnd(trims);
                ret = new Message(tag, data);
            }
            catch (Exception e)
            {
                //Console.WriteLine("unable to parse message...");  NOTE THAT Console.WritieLine CAN CAUSE DEADLOCKS WHEN CALLED FROM MULTIPLE THREADS!!!
               // for (int i = 0; i < buffer.Length; i++)
               // {
               //     Console.Write(buffer[i]);
               // }
               // Console.WriteLine();
               // Console.WriteLine(e.Message);

            }
            return ret;
        }

        private static byte[] encodeMessageHelper(Message m, int buffSize)
        {
            byte[] ret = new byte[buffSize];
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
                //Console.WriteLine("unable to fill buffer: message failed to send: " + m.ToString());
            }
            return null;
        }


        public static byte[] encodeMessage(Message m)
        {
            return encodeMessageHelper(m, BUFF_SIZE);
        }

        public static byte[] encodeMessageUnencrypted(Message m)
        {
            return encodeMessageHelper(m, BUFF_SIZE_UNENCRYPT);
        }

        public static void sendMessageToUnencrypted(Message message, Socket clientSocket)
        {
            byte[] encoding = encodeMessageUnencrypted(message);
            clientSocket.Send(encoding, 0, BUFF_SIZE_UNENCRYPT, SocketFlags.None);
        }

        public static void sendPublicKeyTo(Socket clientSocket, String key)
        {


            Message message = new Message(PublicKey.KEYCOMMAND, key);
            sendMessageToUnencrypted(message, clientSocket);

        }

    }


}
