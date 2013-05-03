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
using System.Net.Security;

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
        private Cryptocus crypt = new Cryptocus();
        private String serverKey = null;
        private object keyLock = new object();
        private String myIP;
        private String guruID;
        

       // private String clientID;

        public Clientlet_Window()
        {
            //clientID = "RandyButternubs"; //TODO make this unique to each person
            guruID = "";
            InitializeComponent();
            FindMyIP();
            socketThread = new System.Threading.Thread(InitializeServerSocket);
            socketThread.Start();
            Console.WriteLine(crypt.getPublicKey().Length);
        }

        private void FindMyIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    myIP = ip.ToString();
                    return;
                }
            }
            myIP = "127.0.0.1";
        }

        private String parseServerAddress()
        {
            String line;
            try
            {
                using (StreamReader sr = new StreamReader("config.txt"))
                {
                    line = sr.ReadLine();
                   // line = line.Substring(9);
                }
                return line;
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read: ");
                Console.WriteLine(e.Message);
            }
            return "10.26.249.90";
        }

        private String getMyIP()
        {
            return myIP;
        }

        private void InitializeServerSocket()
        {
            ManualResetEvent InitMRE = new ManualResetEvent(true);
            String serverAddress = parseServerAddress();
            IPAddress serverIP = IPAddress.Parse(serverAddress);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            int port = 8888;
            
            while (!serverSocket.IsBound)
            {
                Console.WriteLine("trying to connect on port " + port);
                try
                {
                    InitMRE.Reset();
                    serverSocket.BeginConnect(serverIP, port, new AsyncCallback(OnServerConnect), InitMRE);
                    InitMRE.WaitOne();
                    
                }
                catch (SocketException e)
                {
                    port++;
                    
                }
            }
            
            Console.WriteLine("connected on port " + port);
            
            
            
        }

        private void listenForMessage(AsyncCallback callback, int buffSize)
        {
            try
            {

                MRE.Reset();
                byte[] buffer = new byte[buffSize];
               
                serverSocket.BeginReceive(buffer, 0, buffSize, SocketFlags.None, callback, buffer);
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

        /**
         * returns true if the server's public key has yet to be recieved
         * by this guru
         * */
        private bool serverKeyNull()
        {
            lock (keyLock)
            {
                return serverKey == null;
            }
        }

        /**
         * This is called upon the client's first connection to the server
         * */
        private void OnServerConnect(IAsyncResult ar)
        {
            MRE.Set();
            ManualResetEvent InitMRE = (ManualResetEvent)ar.AsyncState;
            try
            {
                serverSocket.EndConnect(ar);
                InitMRE.Set();
                Message.sendPublicKeyTo(serverSocket, crypt.getPublicKey()); //send our public key to the server (unencrypted)
                
                //while (serverKeyNull()) //while we don't know the server's public key...
                //{
                    listenForMessage(new AsyncCallback(OnFirstRecieve), Message.BUFF_SIZE_UNENCRYPT); //listen for it (it will be unencrypted)
                    
                //}
                while (true)
                { //then continue listening for encrypted messages
                   
                    listenForMessage(new AsyncCallback(OnRecieve), Message.BUFF_SIZE+Message.OFFSET);

                }
                
                
            }
            catch (SocketException e)
            {
                Console.WriteLine("unable to connect to socket ");
                Console.WriteLine(e.Message);
            }
            InitMRE.Set();
           
            
           
        }

        /**
         * This is called when the clientlet recieves a message from the server
         * and doesn't know the server's public key
         * */
        private void OnFirstRecieve(IAsyncResult ar)
        {
            recieve(ar, Message.decodeMessage);
        }

        /*
         * called when a message is recieved
         * see "OnFirstRecieve" and "OnRecieve"
         * as well as "OnServerConnect"
         * */
        private void recieve(IAsyncResult ar, Func<byte[], Message> funk)
        {
            
            try
            {
                serverSocket.EndReceive(ar);
                byte[] buffer = (byte[])ar.AsyncState;
                Message message = funk(buffer);
                Console.WriteLine(message);
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
            MRE.Set();
        }
    
        /**
         * This is called whenever the clientlet recieves data from the server
         * and knows the server's public key
         * */
        private void OnRecieve(IAsyncResult ar)
        {
            recieve(ar, (byte[] mess) => { return Message.decodeMessage(crypt.decrypt(mess)); });
        }

        /**
         * Functionality for each message type is handled here
         * */
        private void handleMessage(Message m)
        {
            String tag = m.getTag();
            String data = m.getData();
            if (tag.Equals(PublicKey.KEYCOMMAND)) //public key recieved from server
            {
                handleMessageIncomingPublicKey(data);
            }
            else if (tag.Equals("DisplayText")) //text from server is displayed in window
            {
                handleMessageDisplayText(data);
            }
            else if (tag.Equals("GoGoVoiceChat")) //voice chat is initiated with the provided peer
            {
                handleMessageGoGoVoiceChat(data);
            }
            else if (tag.Equals("SendText"))
            {
                handleMessageSendText(data);
            }
        }

        private void handleMessageIncomingPublicKey(String data)
        {
            lock (keyLock)
            {
                serverKey = data;
                Console.WriteLine(serverKey);
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

        private void handleMessageSendText(String data)
        {
            // data contains: id messageText
            //parse data for text and id and concat to the output window
            //store the id privately so we can use later
            //use invoke (because its a gui)
            //Action SetText = () => { box.Text = box.Text + newText; }
            //this.Invoke(SetText);


            string id = data.Substring(0,  data.IndexOf(' '));
            string text = data.Substring(data.IndexOf(' ') + 1);
            guruID = id;

            Action SetText = () =>
            {
                chatOutputBox.Text = chatOutputBox.Text + "\n" + id + ": " + text;
            };
            this.Invoke(SetText);
        }

        private void CreateAndSendTicklet()
        {
            Action WaitingForServer = () => { 
                textBox1.Text = "Waiting for server to accept ticklet...";
                submitButton.Enabled = false;
            };
            this.Invoke(WaitingForServer);
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
                Console.WriteLine(e.Message);
                if (rdpSession != null)
                {
                    Action TickletFail = () => { 
                        textBox1.Text = "Ticklet was rejected, please try again";
                        submitButton.Enabled = true;
                    };
                    this.Invoke(TickletFail);
                    rdpSession.Close();
                    rdpSession = null;
                }
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
            //rdpSession.OnControlLevelChangeRequest += new _IRDPSessionEvents_OnControlLevelChangeRequestEventHandler(OnGuruChangeControlLevel);
            
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
            //ShuffaShutdown();
            Action InterfaceMorph = () =>
            {
                //submitButton.Enabled = true;
                //textBox1.Text = "disconnected from server";
                textBox1.Text = "Thank you for shopping with Ticklet Meister!";
            };
            this.Invoke(InterfaceMorph);
            
        }

        /** 
         * (see StartRDPSession) this whole method may not be needed
         */
        private void OnGuruChangeControlLevel(object attendee, CTRL_LEVEL level) 
        {
            IRDPSRAPIAttendee guru = attendee as IRDPSRAPIAttendee;
            guru.ControlLevel = level;

            rdpSession.Close();
            
        }



        


        private void SendTickletToServer(String ticklet)
        {




            Action SetText = () => { textBox1.Text = "Ticklet Accepted... Please wait for service."; }; 
            this.Invoke(SetText);
            Message submitTickletMessage = new Message("Ticklet", ticklet);
            
                bool tickletSent = sendMessageToServer(submitTickletMessage);
                if (!tickletSent)
                {
                    Action ActivateButton = () => { submitButton.Enabled = true; };
                    this.Invoke(ActivateButton);
                }
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
            //Console.WriteLine("Sharing Session Closed");
            Application.Exit();
        }

        private void echoButton_Click(object sender, EventArgs e)
        {
            String text = textInputBox.Text;
            Message echoMessage = new Message("Echo", text);
            sendMessageToServer(echoMessage);
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

        /**
         *  use this to send communication between the Clientlet and the Serverlet
         *  The message is encrypted by means of Cryptocus
         * */
        public bool sendMessageToServer(Message message)
        {
            try
            {
                byte[] encoding = crypt.encrypt(Message.encodeMessage(message), serverKey);
                Console.WriteLine("encoding length: "+encoding.Length);
                serverSocket.Send(encoding, 0, encoding.Length, SocketFlags.None);
                return true;
            }
           
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                DisplayServerConnectionError();
            }
            return false;
        }

        private void submitButton_Click(object sender, EventArgs e)
        {
            

            if (!serverKeyNull())
            {
                CreateAndSendTicklet();
            }
            else
            {
                Action SetText = () => { textBox1.Text = "unable to send ticklet to server"; };
                this.Invoke(SetText);
            }
        }

        private void chatMessageButton_Click(object sender, EventArgs e)
        {
            String messageData = guruID + " " + chatInputBox.Text;
            Message message = new Message("SendText", messageData);


            chatOutputBox.Text = chatOutputBox.Text + "Me: " + chatInputBox.Text + '\n';
            sendMessageToServer(message);
            chatInputBox.Text = " ";
        }

       
        
        
    }
}
