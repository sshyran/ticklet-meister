using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RDPCOMAPILib;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TickletMeister_VoiceLib;

namespace TickletMeister_Clientlet
{
    public partial class Clientlet_Window : Form
    {
        private RDPSession rdpSession;
        private Socket serverSocket;
        private ManualResetEvent MRE = new ManualResetEvent(false); //for dealing with async calls
        private Thread socketThread;
        private VoiceChatlet vc;
        private object voiceLock = new object();
        
       // private String clientID;

        public Clientlet_Window()
        {
            //clientID = "RandyButternubs"; //TODO make this unique to each person
            
            InitializeComponent();
            socketThread = new System.Threading.Thread(InitializeServerSocket);
            socketThread.Start();
        }

        private String parseServerAddress()
        {
            String line;
            try
            {
                using (StreamReader sr = new StreamReader("config.txt"))
                {
                    line = sr.ReadLine();
                    line = line.Substring(9);
                }
                return line;
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read: ");
                Console.WriteLine(e.Message);
            }
            return "127.0.0.1";
        }

        private String getMyIP()
        {
            return "127.0.0.1";
        }

        private void InitializeServerSocket()
        {
            
            String serverAddress = parseServerAddress();
            IPAddress serverIP = IPAddress.Parse(serverAddress);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            int port = 8888;
            
            while (!serverSocket.IsBound)
            {
                Console.WriteLine("trying to connect on port " + port);
                try
                {
                    MRE.Reset();
                    serverSocket.BeginConnect(serverIP, port, new AsyncCallback(OnServerConnect), serverSocket);
                    
                    MRE.WaitOne();
                    
                }
                catch (SocketException e)
                {
                    port++;
                    
                }
            }
            
            Console.WriteLine("connected on port " + port);
            
            
            
        }

        /**
         * This is called upon the client's first connection to the server
         * */
        private void OnServerConnect(IAsyncResult ar)
        {
            
            MRE.Set();
            try
            {
                serverSocket.EndConnect(ar);
            }
            catch (SocketException e)
            {
                Console.WriteLine("unable to connect to socket ");
                Console.WriteLine(e.Message);
            }
           
            CreateAndSendTicklet();
            //serverSocket.Listen(10);
            while (true)
            {
                try
                {

                    MRE.Reset();
                    byte[] buffer = new byte[Message.BUFF_SIZE];

                    serverSocket.BeginReceive(buffer, 0, Message.BUFF_SIZE, SocketFlags.None, new AsyncCallback(OnRecieve), buffer);
                    MRE.WaitOne();
                }
                catch (SocketException e)
                {
                    Console.WriteLine("failed to recieve from server");
                    Console.WriteLine(e.Message);
                }
                catch (ObjectDisposedException e)
                {
                    //we are closing
                }
            }
           
        }

    
        /**
         * This is called whenever the clientlet recieves data from the server
         * */
        private void OnRecieve(IAsyncResult ar)
        {
            MRE.Set();
            try
            {
                serverSocket.EndReceive(ar);
                byte[] buffer = (byte[])ar.AsyncState;
                Message message = Message.decodeMessage(buffer);
                handleMessage(message);
            }
            catch (ObjectDisposedException e)
            {
                //we are closing
            }
            catch (SocketException e)
            {
                //we don't need to handle any more messages
            }
            
            

        }

        /**
         * Functionality for each message type is handled here
         * */
        private void handleMessage(Message m)
        {
            String tag = m.getTag();
            String data = m.getData();
            if (tag.Equals("DisplayText"))
            {
                handleMessageDisplayText(data);
            }
            else if (tag.Equals("GoGoVoiceChat"))
            {
                handleMessageGoGoVoiceChat(data);
            }
        }

        private void handleMessageDisplayText(String text)
        {
            Action SetText = () => { textOutputBox.Text = text; };
            this.Invoke(SetText);
            Console.WriteLine(text);
        }

        /*
         * GoGoVoiceChat <peer ip addreess> <inPort> <outPort>
         * */
        private void handleMessageGoGoVoiceChat(String data)
        {
            
            lock (voiceLock)
            {
                try
                {

                    if (vc != null)
                    {
                        vc.endChat();
                        vc = null;
                    }
                    String[] tokens = data.Split(' ');
                    if (tokens.Length == 3)
                    {
                        String otherEnd = tokens[0];
                        int inPort;
                        int outPort;
                        bool inSuc = int.TryParse(tokens[1], out inPort);
                        bool outSuc = int.TryParse(tokens[2], out outPort);
                        if (inSuc && outSuc)
                        {
                            vc = new VoiceChatlet(otherEnd, inPort, outPort);
                            vc.startChat();
                        }
                        else
                        {
                            Console.WriteLine("unable to parse ports from data string: " + tokens[1] + " " + tokens[2]);
                        }
                    }
                    else
                    {
                        Console.WriteLine("invalid message format: " + data); //ignore message
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine("unable to open voice chat");
                }
            }

        }

        private void CreateAndSendTicklet()
        {
            string ticklet = CreateNewTicklet();
            if (!"".Equals(ticklet)) //if the ticklet is not the empty string
            {
                SendTickletToServer(ticklet);
            }
            else
            {
                DisplayUnableToTickletMessage();
            }
        }

        /**
         * starts a new RDP session and a new invitation, and returns the corresponding invite string.
         * returns the empty string if the start failed.
         * */
        private String CreateNewTicklet()
        {
            
            try
            {
                StartRDPSession();
                String invite = GetInviteString();
                Console.WriteLine("Ticklet Creation Succeeded!");
                return invite;
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Console.WriteLine("Ticklet Creation Failed :(");
            }

            return "";
            
        }

        /**
         * This method is called when a ticklet is unable to be created
         * It displays a message to the user indicating the error.
         * */
        private void DisplayUnableToTickletMessage()
        {
            //TODO modify the display window to appropriately indicate the problem
        }

        /**
         * (used by CreateNewTicklet) returns the invitation string of this RDP session, allowing at most 1 attendee (the guru) 
         * */
        private string GetInviteString()
        {
            IRDPSRAPIInvitation pInvitation = rdpSession.Invitations.CreateInvitation("TickletMeister", "RandyButternubs", "", 1);
            return pInvitation.ConnectionString;
        }

        /**
         * Starts a new RDP session and associates the appropriate event methods (see CreateNewTicklet).
         * */
        private void StartRDPSession()
        {
            rdpSession = new RDPSession();
            rdpSession.OnAttendeeConnected += new _IRDPSessionEvents_OnAttendeeConnectedEventHandler(OnGuruConnect);
            rdpSession.OnAttendeeDisconnected += new _IRDPSessionEvents_OnAttendeeDisconnectedEventHandler(OnGuruDisconnect);
            rdpSession.OnControlLevelChangeRequest += new _IRDPSessionEvents_OnControlLevelChangeRequestEventHandler(OnGuruChangeControlLevel);
            
            //OnControlLevelChangeRequest may not be needed

            rdpSession.Open();
            
            
        }

        /** 
         * This is called whenever a Guru connects to this RDP session (see StartRDPSession)
         */
        private void OnGuruConnect(object attendee)
        {
            IRDPSRAPIAttendee guru = attendee as IRDPSRAPIAttendee;
            guru.ControlLevel = CTRL_LEVEL.CTRL_LEVEL_INTERACTIVE;
            Console.WriteLine("The Guru now has control!");
        }

        /** 
         * This is called whenever a Guru disconncets from this RDP session (see StartRDPSession)
         */
        private void OnGuruDisconnect(object e)
        {
            //IRDPSRAPIAttendeeDisconnectInfo info = e as IRDPSRAPIAttendeeDisconnectInfo;
            ShuffaShutdown();
            
        }

        /** 
         * (see StartRDPSession) this whole method may not be needed
         */
        private void OnGuruChangeControlLevel(object attendee, CTRL_LEVEL level) 
        {
            IRDPSRAPIAttendee guru = attendee as IRDPSRAPIAttendee;
            guru.ControlLevel = level;
        }



        


        private void SendTickletToServer(String ticklet)
        {
            //TODO change this to actually send the ticklet to the server

          

            Action SetText = () => { textBox1.Text = ticklet; }; 
            this.Invoke(SetText);
            Message submitTickletMessage = new Message("Ticklet", ticklet);
            
                sendMessageToServer(submitTickletMessage);
                Console.WriteLine("sending message to server: " + submitTickletMessage);
            
            Console.WriteLine();
            Console.WriteLine(ticklet);
            Console.WriteLine();
        }

        private void endButton_Click(object sender, EventArgs e)
        {
            ShuffaShutdown();
        }

        /**
         * closes the RDP Session and shuts down the application
         * also sends a "Disconnect" message to the server if connected
         * */
        private void ShuffaShutdown()
        {
            
            if(rdpSession != null)
            rdpSession.Close();

            if (serverSocket != null)
            {
                sendMessageToServer(new Message("Disconnect", "Me"));
                serverSocket.Close();
            }
            lock (voiceLock)
            {
                if (vc != null)
                {
                    vc.endChat();
                    vc = null;
                }
            }

            socketThread.Abort();
            Console.WriteLine("Sharing Session Closed");
            Application.Exit();
        }

        private void echoButton_Click(object sender, EventArgs e)
        {
            String text = textInputBox.Text;
            Message echoMessage = new Message("Echo", text);
            sendMessageToServer(echoMessage);
        }

        /**
         *  use this to send communicatoin between the Clientlet and the Serverlet
         * */
        private void sendMessageToServer(Message message)
        {
            try
            {
                Message.sendMessageTo(message, serverSocket);
            }
            catch (SocketException e)
            {
                DisplayServerConnectionError();
            }
        }

        private void DisplayServerConnectionError()
        {
            Action SetText = () =>
            {
                textBox1.Text = "Server not responsive; try restarting!";
            };
            this.Invoke(SetText);
        }

        private void Clientlet_Window_Load(object sender, EventArgs e)
        {

        }
        
    }
}
