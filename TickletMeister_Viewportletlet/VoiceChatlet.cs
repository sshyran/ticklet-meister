using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Voice;

namespace TickletMeister_VoiceLib
{

    public class VoiceChatlet
    {

        private Socket r;
        private Thread t;
        private bool connected = false;
        private int inPort;
        private int outPort;
        private String otherEnd;

        private WaveOutPlayer m_Player;
        private WaveInRecorder m_Recorder;
        private FifoStream m_Fifo = new FifoStream();

        private byte[] m_PlayBuffer;
        private byte[] m_RecBuffer;

        public VoiceChatlet(String peerAddress, int inputPort, int outputPort)
        {
            otherEnd = peerAddress;
            inPort = inputPort;
            outPort = outputPort;
            r = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            t = new Thread(new ThreadStart(Voice_In));
        }

        private void Voice_In()
        {
            try
            {
                byte[] br;
                r.Bind(new IPEndPoint(IPAddress.Any, inPort));
                while (true)
                {
                    br = new byte[16384];
                    r.Receive(br);
                    m_Fifo.Write(br, 0, br.Length);
                }
            }
            catch
            {
                return;//end it ones and for all!
            }
        }

        private void Voice_Out(IntPtr data, int size)
        {
            //for Recorder
            try
            {
                if (m_RecBuffer == null || m_RecBuffer.Length < size)
                    m_RecBuffer = new byte[size];
                System.Runtime.InteropServices.Marshal.Copy(data, m_RecBuffer, 0, size);
                //Microphone ==> data ==> m_RecBuffer ==> m_Fifo
                r.SendTo(m_RecBuffer, new IPEndPoint(IPAddress.Parse(otherEnd), outPort));
            }
            catch
            {
                return;
            }
        }

        private void Start()
        {
            Stop();
            try
            {
                WaveFormat fmt = new WaveFormat(44100, 16, 2);
                m_Player = new WaveOutPlayer(-1, fmt, 16384, 3, new BufferFillEventHandler(Filler));
                m_Recorder = new WaveInRecorder(-1, fmt, 16384, 3, new BufferDoneEventHandler(Voice_Out));
            }
            catch
            {
                return;
            }
        }

        private void Stop()
        {
            if (m_Player != null)
                try
                {
                    m_Player.Dispose();
                }
                finally
                {
                    m_Player = null;
                }
            if (m_Recorder != null)
                try
                {
                    m_Recorder.Dispose();
                }
                finally
                {
                    m_Recorder = null;
                }
            m_Fifo.Flush(); // clear all pending data
        }

        private void Filler(IntPtr data, int size)
        {
            if (m_PlayBuffer == null || m_PlayBuffer.Length < size)
                m_PlayBuffer = new byte[size];
            if (m_Fifo.Length >= size)
                m_Fifo.Read(m_PlayBuffer, 0, size);
            else
                for (int i = 0; i < m_PlayBuffer.Length; i++)
                    m_PlayBuffer[i] = 0;
            System.Runtime.InteropServices.Marshal.Copy(m_PlayBuffer, 0, data, size);
            // m_Fifo ==> m_PlayBuffer==> data ==> Speakers
        }

        public void startChat()
        {
            if (connected == false)
            {
                t.Start();
                connected = true;
            }

            Start();
        }

        public void endChat()
        {

            try
            {
                t.Abort();
                t = null;
            }
            catch
            {
                //try to stop the thread
            }
            try
            {
                r.Close();
                r = null;
            }
            catch
            {
                //try to close the socket
            }
            connected = false;
            Stop();
        }


    }
}

