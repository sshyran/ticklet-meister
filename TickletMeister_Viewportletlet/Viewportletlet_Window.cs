using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using RDPCOMAPILib;
using AxRDPCOMAPILib;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TickletMeister_VoiceLib;

namespace TickletMeister_Viewportletlet
{
    public partial class Viewportletlet_Window : Form
    {
        //private RDPViewer rdpViewer;
        
        private Ticklet selectedTicklet;
        object LoxyPants = new object(); //a lock for selectedTicklet
        private Socket serverSocket;
        private ManualResetEvent MRE = new ManualResetEvent(false); //for dealing with async calls
        private Thread socketThread;
        private VoiceChatlet vc;
        private object voiceLock = new object();
        private Boolean waitingForServer = false; //indicates whether or not we are anticipating a "Ticklet" response from the server
        private String[] dummyNuts = new String[0];

        public Viewportletlet_Window()
        {
            selectedTicklet = null;
            
            InitializeComponent();
            voiceButton.Enabled = false;
            InitializeTickList();
            InitializeAndSubscribeViewer();
            socketThread = new System.Threading.Thread(InitializeServerSocket);
            socketThread.Start();
            
        }

        private void InitializeTickList()
        {
            tickList.DataSource = dummyNuts ;
        }

        private void refreshTickList(String[] elms)
        {
            Action update = () => { tickList.DataSource = elms; };
            this.Invoke(update);
        }

        private int getIDFromTickListEntry(String entry)
        {
            try
            {
                String can = entry.Substring(entry.IndexOf(' ') + 1);
                int index;
                bool suc = int.TryParse(can, out index);
                if (!suc)
                {
                    throw new InvalidExpressionException(entry + " is not properly formatted: unable to parse "+can);
                }
                return index;
            }
            catch (IndexOutOfRangeException e)
            {
                throw new InvalidExpressionException(entry + " is not properly formatted");
            }
        }

        private void AuthenticateSelf()
        {
            sendMessageToServer(new Message("Authenticate", "Me"));
        }

        private void InitializeAndSubscribeViewer()
        {
           // rdpViewer = new RDPViewer();
            Console.WriteLine(axRDPViewer1 == null);
            //rdpViewer.OnConnectionEstablished += new _IRDPSessionEvents_OnConnectionEstablishedEventHandler(OnConnectToClient);
            axRDPViewer1.OnConnectionEstablished += new EventHandler(OnConnectToClient);
           // rdpViewer.OnConnectionTerminated += new _IRDPSessionEvents_OnConnectionTerminatedEventHandler(OnDisconnectFromClient);
            axRDPViewer1.OnConnectionTerminated += new _IRDPSessionEvents_OnConnectionTerminatedEventHandler(OnDisconnectFromClient);
          //  rdpViewer.OnConnectionFailed += new _IRDPSessionEvents_OnConnectionFailedEventHandler(OnConnectionFail);
            axRDPViewer1.OnConnectionFailed += new EventHandler(OnConnectionFail);
          //  rdpViewer.OnError += new _IRDPSessionEvents_OnErrorEventHandler(OnError);
            axRDPViewer1.OnError += new _IRDPSessionEvents_OnErrorEventHandler(OnError);
            
        }

        private String parseServerAddress()
        {
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

            AuthenticateSelf();

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
            else if (tag.Equals("Ticklet"))
            {
                handleMessageTicklet(data);
            }
            else if (tag.Equals("PollFail"))
            {
                handleMessagePollFail(data);
            }
            else if (tag.Equals("GoGoVoiceChat"))
            {
                handleMessageGoGoVoiceChat(data);
            }
            else if (tag.Equals("RefreshList"))
            {
                handleMessageRefreshList(data);
            }
        }

        private void handleMessageDisplayText(String data)
        {
            displayOutputText(data);
        }

        /**
         * Ticklet <ID> <ConnectionString>
         * */
        private void handleMessageTicklet(String data)
        {
            lock (LoxyPants)
            {
                try
                {
                    int divIndex = data.IndexOf(' ');
                    String idString = data.Substring(0, divIndex);
                    String m = data.Substring(divIndex + 1);
                    int id;
                    bool suc = int.TryParse(idString, out id);
                    if (suc)
                    {
                        SelectTicklet(new Ticklet(m, id, "RandyButternubs")); //TODO make this meaningful?
                        waitingForServer = false;
                    }
                    else
                    {
                        Console.WriteLine("unable to parse ID#: " + idString);

                    }
                    
                }
                catch (IndexOutOfRangeException e)
                {
                    Console.WriteLine("invalid Ticklet signature: " + data);
                }
                
               
            }
        }

        private void handleMessagePollFail(String data)
        {
            waitingForServer = false; //stop waiting TODO make this better?
            displayOutputText("unable to poll from ticklet queue");
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

        /**
         * UpdateList <ID0>;<ID1>;<ID2>;<ID3> ...
         * */
        private void handleMessageRefreshList(String data)
        {
            String[] elems = dummyNuts;
            if (!"nothing".Equals(data))
            {
                elems = data.Split(';');
            }
            refreshTickList(elems);
        }

        private void OnConnectToClient(object sender, EventArgs e)
        {
            
        }

        private void OnDisconnectFromClient(object sender, _IRDPSessionEvents_OnConnectionTerminatedEvent e)
        {

        }

        private void OnConnectionFail(object sender, EventArgs e)
        {

        }

        private void OnError(object sender, _IRDPSessionEvents_OnErrorEvent e)
        {

        }

        private void OnConnectToClient()
        {
            HideTickletList();
            
        }

        private void OnError(object info)
        {

        }

        private void OnDisconnectFromClient(int reason, int info)
        {
            ShowTickletList();
            
        }

        private void OnConnectionFail()
        {
            
        }

        private void ShowTickletList()
        {
            lock (tickList)
            {
                tickList.Enabled = true;
            }
        }

        private void HideTickletList()
        {
            lock (tickList)
            {
                tickList.Enabled = false;
            }
        }

        private void SelectTicklet(Ticklet t)
        {
            
                selectedTicklet = t;
                if (t != null)
                {
                    Action SetText = () => { tickletSelectionBox.Text = "ID: " + t.getID() + " Connection:" + t.getConnectionString(); };
                    this.Invoke(SetText);
                }
                else
                {
                    Action SetText = () => { tickletSelectionBox.Text = "no ticklet selected"; };
                    this.Invoke(SetText);
                }
                Console.WriteLine("selected: " + t);
            
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            //TODO remove this line as well as corresponding text area
           //SelectTicklet( new Ticklet("<E><A KH=\"+GFA1Dtyk3xs1Ho4ecDRK5Ddloo=\" ID=\"TickletMeister\"/><C><T ID=\"1\" SID=\"4120104621\"><L P=\"2851\" N=\"2002:81ba:bcc2::81ba:bcc2\"/><L P=\"2852\" N=\"129.186.188.194\"/></T></C></E>", "RandyButternubs"));

            if (AttemptConnectionToSelectedTicklet())
            {
                Console.WriteLine("Successfully connected to " + selectedTicklet.getClientID() + "!");
            }
            else
            {
                DisplayClientConnectionErrorMessage();
                
            }
        }

        private bool AttemptConnectionToSelectedTicklet()
        {
            lock (LoxyPants)
            {
                if (selectedTicklet == null)
                {
                    return false;
                }
            }
                try
                    {

                        //rdpViewer.StartReverseConnectListener(selectedTicklet.getConnectionString(), selectedTicklet.getClientID(), "");

                        //rdpViewer.Connect(selectedTicklet.getConnectionString(), selectedTicklet.getClientID(), "");
                        axRDPViewer1.Connect(selectedTicklet.getConnectionString(), selectedTicklet.getClientID(), "");
                        axRDPViewer1.Show();
                        connectButton.Enabled = false;
                        voiceButton.Enabled = true;


                    }
                    catch (System.Runtime.InteropServices.COMException e)
                    {
                        return false;
                    }
                    catch (ArgumentException e)
                    {
                        return false;
                    }
                return true;
            
        }

        private void DisplayClientConnectionErrorMessage()
        {
            //TODO indicate that there was an error connecting to the client at the selected ticklet
            displayOutputText("unable to connect to client");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //do nothing
        }

        private void discoButton_Click(object sender, EventArgs e)
        {
            
            lock (voiceLock)
            {
                if (vc != null)
                {
                    vc.endChat();
                    vc = null;
                }
            }
            
            lock (LoxyPants)
            {
                SelectTicklet(null);
            }
            axRDPViewer1.Disconnect();
            connectButton.Enabled = true;
            voiceButton.Enabled = false;
        }

        private void ShuffaShutdown()
        {

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
                //do nothing
            }
        }

        private void alertButton_Click(object sender, EventArgs e)
        {
            String text = textInputBox.Text;
            Message m = new Message("AlertAll", text);
            sendMessageToServer(m);
        }

        private void endButton_Click(object sender, EventArgs e)
        {
            ShuffaShutdown();
        }

        private void pollButton_Click(object sender, EventArgs e)
        {
            lock (LoxyPants)
            {
                if (!waitingForServer)
                {
                    SelectTicklet(null);
                    Message m = new Message("Poll", "dgaf");
                    waitingForServer = true;
                    sendMessageToServer(m);
                }
            }

        }

        private void selectButton_Click(object sender, EventArgs e)
        {
            try
            {
                    String sel = null;
                    lock (tickList)
                    {
                        sel = (String)tickList.SelectedItem;
                    }
                    if (sel == null)
                    {
                        throw new ArgumentNullException("no items selected");
                    }
                    int index = getIDFromTickListEntry(sel);

                    lock (LoxyPants)
                    {
                        if (!waitingForServer)
                        {
                            Message m = new Message("Poll", index + "");
                            waitingForServer = true;
                            sendMessageToServer(m);
                        }
                    }
            }
            catch (InvalidCastException ex)
            {
                //do nothing
            }
            catch (ArgumentNullException ex)
            {
                //do nothing
            }
            catch (InvalidExpressionException ex)
            {
                Console.WriteLine("Update improperly formatted");
                Console.WriteLine(ex.Message);
            }
        }

        private void voiceButton_Click(object sender, EventArgs e)
        {
            lock (LoxyPants)
            {
                if (selectedTicklet == null)
                    return;
                voiceButton.Enabled = false;
                int id = selectedTicklet.getID();
                int inPort = 5000;
                int outPort = 6000;
                String messageString = id + " " + inPort + " " + outPort;
                sendMessageToServer(new Message("DesireVoice", messageString));
            }
            
        }

        private void displayOutputText(String text)
        {
            Action SetText = () => { textOutputBox.Text = text; };
            this.Invoke(SetText);
        }

     

        
    }
}
