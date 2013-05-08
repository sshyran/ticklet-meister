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
using System.IO;

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
        private Cryptocus crypt = new Cryptocus();
        private String serverKey = null;
        private object keyLock = new object();
        private String myIP;
        private Thread cooldownThread;
        private bool cooldown; //prevent buttons from being pressed too frequently, thus overwhelming the server
        private object coolLock = new object();

        public Viewportletlet_Window()
        {
            selectedTicklet = null;
            parseServerAddress();
            InitializeComponent();
            FindMyIP();
            voiceButton.Enabled = false;
            InitializeTickList();
            InitializeAndSubscribeViewer();
            cooldownThread = new System.Threading.Thread(Cooldown);
            cooldownThread.Start();
            socketThread = new System.Threading.Thread(InitializeServerSocket);
            socketThread.Start();
            
        }

        private void Cooldown() // half second cooldown on abilities
        {
            while (true)
            {
                Thread.Sleep(500);
                lock (coolLock)
                {
                    cooldown = true;
                }
            }
        }

        private bool cool()
        {
            lock (coolLock)
            {
                return cooldown;
            }
        }

        private void setCooldown()
        {
            lock (coolLock)
            {
                cooldown = false;
            }
        }

        private void InitializeTickList()
        {
            tickList.DataSource = dummyNuts ;
        }

        /**
         * Updates the visual representation of the ticklet queue
         * with the specified data (replacing the old data)
         * */
        private void refreshTickList(String[] elms)
        {
            Action update = () => {
                int index = tickList.SelectedIndex;
                tickList.DataSource = elms;
                tickList.SelectedIndex = Math.Min(elms.Length - 1, index);
            };
            this.Invoke(update);
        }

        /**
         * given a "Ticklet" request string
         * in the format of "Ticklet <ID> <Ticklet>",
         * the corresponding ID is returned
         * */
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
            Application.ApplicationExit += CloseAndShutDown;
           // rdpViewer = new RDPViewer();
           // Console.WriteLine(axRDPViewer1 == null);
            //rdpViewer.OnConnectionEstablished += new _IRDPSessionEvents_OnConnectionEstablishedEventHandler(OnConnectToClient);
            axRDPViewer1.OnConnectionEstablished += new EventHandler(OnConnectToClient);
           // rdpViewer.OnConnectionTerminated += new _IRDPSessionEvents_OnConnectionTerminatedEventHandler(OnDisconnectFromClient);
            axRDPViewer1.OnConnectionTerminated += new _IRDPSessionEvents_OnConnectionTerminatedEventHandler(OnDisconnectFromClient);
          //  rdpViewer.OnConnectionFailed += new _IRDPSessionEvents_OnConnectionFailedEventHandler(OnConnectionFail);
            axRDPViewer1.OnConnectionFailed += new EventHandler(OnConnectionFail);
          //  rdpViewer.OnError += new _IRDPSessionEvents_OnErrorEventHandler(OnError);
            axRDPViewer1.OnError += new _IRDPSessionEvents_OnErrorEventHandler(OnError);
            
        }

        private void CloseAndShutDown(object sender, EventArgs e)
        {
            ShuffaShutdown();
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

        private void listenForMessage(IAsyncResult ar, AsyncCallback callback, int buffSize)
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
               // Console.WriteLine("failed to recieve from server");
               // Console.WriteLine(e.Message);
            }
            catch (ObjectDisposedException e)
            {
                //we are closing
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
                Message.sendPublicKeyTo(serverSocket, crypt.getPublicKey()); //send our public key to the server
                
                
               // while (serverKeyNull()) //while we don't know the server's public key...
               // {
                    listenForMessage(ar, new AsyncCallback(OnFirstRecieve), Message.BUFF_SIZE_UNENCRYPT); //listen for it
               // }
                    
                while (true)
                { //and continute to listen for encrypted messages (the server should know our public key by now)
                    
                    listenForMessage(ar, new AsyncCallback(OnRecieve), Message.BUFF_SIZE+Message.OFFSET);
                }
            }
            catch (SocketException e)
            {
               // Console.WriteLine("unable to connect to socket ");
               // Console.WriteLine(e.Message);
            }
            catch (ObjectDisposedException e)
            {
                //we are closing

            }
            InitMRE.Set();

        }

        /**
         * This is called when the guru recieves a message from the server
         * and doesn't know the server's public key
         * */
        private void OnFirstRecieve(IAsyncResult ar)
        {
            recieve(ar, Message.decodeMessage);

            AuthenticateSelf(); //then tell the server we are a guru
            
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

               // Console.WriteLine("".Equals(message));
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
            catch (Exception e)
            {
               // Console.WriteLine(e.Message);
               // Console.WriteLine(e.StackTrace);
            }
            MRE.Set();
        }

        /**
         * This is called whenever the guru recieves data from the server
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
            if (tag.Equals(PublicKey.KEYCOMMAND)) //public key is recieved from the server
            {
                handleMessageIncomingPublicKey(data);
            }
            else if (tag.Equals("DisplayText")) //text from server is displayed in window
            {
                handleMessageDisplayText(data);
            }
            else if (tag.Equals("T")) //result of "Poll" request returned from server
            {
                handleMessageT(data);
            }
            else if (tag.Equals("PollFail")) //indicator of unsuccessful "Poll" request
            {
                handleMessagePollFail(data);
            }
            else if (tag.Equals("GoGoVoiceChat")) //initiate voice chat with provided peer
            {
                handleMessageGoGoVoiceChat(data);
            }
            else if (tag.Equals("RefreshList")) //refresh list of active ticklets
            {
                handleMessageRefreshList(data);
            }
            else if (tag.Equals("SendText"))
            {
                handleMessageSendText(data);
            }
        }

        private void handleMessageSendText(String data)
        {
            string id = data.Substring(0,  data.IndexOf(' '));
            string text = data.Substring(data.IndexOf(' ') + 1);

            Action SetText = () =>
            {
                textOutputBox.Text = textOutputBox.Text + "\r\n" + "Client #"+id + ": " + text;
            };
            this.Invoke(SetText);
        }


        private void handleMessageIncomingPublicKey(String data)
        {
            lock (keyLock)
            {
                serverKey = data;
            }
        }

        private void handleMessageDisplayText(String data)
        {
            displayOutputText(data);
        }

        /**
         * T <ID> <ConnectionString>
         * */
        private void handleMessageT(String data)
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
                        Action SetStuff = () =>
                        {
                            selectButton.Enabled = false;
                            pollButton.Enabled = false;
                            connectButton.Enabled = true;
                            discoButton.Enabled = true;
                        };
                        this.Invoke(SetStuff);
                    }
                    else
                    {
                        //Console.WriteLine("unable to parse ID#: " + idString);

                    }
                    
                }
                catch (IndexOutOfRangeException e)
                {
                    //Console.WriteLine("invalid Ticklet signature: " + data);
                }
                
               
            }
        }

        private void handleMessagePollFail(String data)
        {
            lock (LoxyPants)
            {
                waitingForServer = false;   
            }
            Action SetStuff = () =>
            {
                pollButton.Enabled = true;
                selectButton.Enabled = true;
            };
            this.Invoke(SetStuff);
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
                           // Console.WriteLine("unable to parse ports from data string: " + tokens[1] + " " + tokens[2]);
                        }
                    }
                    else
                    {
                       // Console.WriteLine("invalid message format: " + data); //ignore message
                    }
                }
                catch (SocketException e)
                {
                   // Console.WriteLine("unable to open voice chat");
                }
            }

        }

        /**
         * RefreshList <ID0>;<ID1>;<ID2>;<ID3> ...
         * || RefreshList nothing
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
            HideTickletList();
        }

        private void OnDisconnectFromClient(object sender, _IRDPSessionEvents_OnConnectionTerminatedEvent e)
        {
            try
            {
                axRDPViewer1.Disconnect();
            }
            catch { }
            ShowTickletList();
        }

        private void OnConnectionFail(object sender, EventArgs e)
        {
            try
            {
                axRDPViewer1.Disconnect();
            }
            catch { }
            ShowTickletList();
        }

        private void OnError(object sender, _IRDPSessionEvents_OnErrorEvent e)
        {
            try
            {
                axRDPViewer1.Disconnect();
            }
            catch { }
            ShowTickletList();
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
                
            
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            if (cool())
            {
                //TODO remove this line as well as corresponding text area
                //SelectTicklet( new Ticklet("<E><A KH=\"+GFA1Dtyk3xs1Ho4ecDRK5Ddloo=\" ID=\"TickletMeister\"/><C><T ID=\"1\" SID=\"4120104621\"><L P=\"2851\" N=\"2002:81ba:bcc2::81ba:bcc2\"/><L P=\"2852\" N=\"129.186.188.194\"/></T></C></E>", "RandyButternubs"));

                if (AttemptConnectionToSelectedTicklet())
                {
                    connectButton.Enabled = false;
                    discoButton.Enabled = true;
                    toggleButton.Enabled = true;
                }
                else
                {
                    DisplayClientConnectionErrorMessage();
                } 
                setCooldown();
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
            if (cool())
            {
                lock (voiceLock)
                {
                    if (vc != null)
                    {
                        vc.endChat();
                        //vc = null;
                    }
                }

                lock (LoxyPants)
                {
                    axRDPViewer1.Disconnect();
                    SelectTicklet(null);
                }
                discoButton.Enabled = false;
                connectButton.Enabled = false;
                voiceButton.Enabled = false;
                selectButton.Enabled = true;
                pollButton.Enabled = true;
                toggleButton.Enabled = false;
                setCooldown();
            }
        }

        private void ShuffaShutdown()
        {

            try
            {
                if (!serverKeyNull())
                {
                    sendMessageToServer(new Message("Disconnect", "Me"));
                }
                serverSocket.Close();
            }
            catch
            {
                //we are closing
            }
            lock (voiceLock)
            {
                if (vc != null)
                {
                    vc.endChat();
                    vc = null;
                }
            }
            try
            {
                axRDPViewer1.Disconnect();
            }
            catch
            {
                //we are closing
            }
            socketThread.Abort();
            cooldownThread.Abort();
            //Console.WriteLine("Sharing Session Closed");
            //Application.Exit();
        }



        private void alertButton_Click(object sender, EventArgs e)
        {
            if (cool())
            {
                if (axRDPViewer1.Focused)
                {
                    textInputBox.Focus();
                }
                else
                {
                    axRDPViewer1.Focus();
                }
                setCooldown();
            }
            
        }

        //private void alertButton_Click(object sender, EventArgs e)
        //{
        //    if (cool())
        //    {
        //        String text = textInputBox.Text;
        //        Message m = new Message("AlertAll", text);
        //        sendMessageToServer(m);
        //        setCooldown();
        //    }
        //    axRDPViewer1.Focus();
        //}

        private void endButton_Click(object sender, EventArgs e)
        {
            if (cool())
            {
                ShuffaShutdown();
                setCooldown();
            }
        }

        private void pollButton_Click(object sender, EventArgs e)
        {
            if (cool())
            {
                if (!serverKeyNull())
                {
                    lock (LoxyPants)
                    {
                        bool changeButtons = false;
                        if (!waitingForServer)
                        {
                            //SelectTicklet(null);
                            Message m = new Message("Poll", "dgaf");
                            waitingForServer = true;
                            changeButtons = sendMessageToServer(m);
                        }
                        if (changeButtons)
                        {
                            discoButton.Enabled = false;
                            pollButton.Enabled = false;
                            selectButton.Enabled = false;
                        }
                    }
                }
                else
                {
                    displayOutputText("unable to connect to server");
                }
                setCooldown();
            }

        }

        private void selectButton_Click(object sender, EventArgs e)
        {
            if (cool())
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
                    if (serverKeyNull())
                    {
                        throw new ArgumentNullException("no connection made to server");
                    }
                    int index = getIDFromTickListEntry(sel);

                    lock (LoxyPants)
                    {
                        bool changeButtons = false;
                        if (!waitingForServer)
                        {
                            Message m = new Message("Poll", index + "");
                            waitingForServer = true;
                            changeButtons = sendMessageToServer(m);
                        }
                        if (changeButtons)
                        {
                            discoButton.Enabled = false;
                            selectButton.Enabled = false;
                            pollButton.Enabled = false;
                        }
                    }
                }
                catch (InvalidCastException ex)
                {
                    //do nothing
                }
                catch (ArgumentNullException ex)
                {
                    displayOutputText(ex.Message);
                }
                catch (InvalidExpressionException ex)
                {
                    // Console.WriteLine("Update improperly formatted");
                    // Console.WriteLine(ex.Message);
                }
                finally
                {
                    setCooldown();
                }
            }
        }

        private void voiceButton_Click(object sender, EventArgs e)
        {
            if (cool())
            {
                lock (LoxyPants)
                {
                    if (selectedTicklet != null)
                    {
                        voiceButton.Enabled = false;
                        int id = selectedTicklet.getID();
                        int inPort = 5555;
                        int outPort = 6666;
                        String messageString = id + " " + inPort + " " + outPort;
                        sendMessageToServer(new Message("DesireVoice", messageString));
                    }
                }
                setCooldown();
            }
            axRDPViewer1.Focus();
            
        }

        private void displayOutputText(String text)
        {
            Action SetText = () => { tickletSelectionBox.Text = text; };
            this.Invoke(SetText);
        }

        /**
         *  use this to send communication between the Clientlet and the Serverlet
         * */
        public bool sendMessageToServer(Message message)
        {
            try
            {
                byte[] encoding = crypt.encrypt(Message.encodeMessage(message), serverKey);
                serverSocket.Send(encoding, 0, encoding.Length, SocketFlags.None);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Viewportletlet_Window_Load(object sender, EventArgs e)
        {

        }

        private void tickList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textInputBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tickletSelectionBox_TextChanged(object sender, EventArgs e)
        {

        }

        //private void axRDPViewer1_Enter(object sender, EventArgs e)
        //{

        //}

        private void textOutputBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            if (cool())
            {
                lock (LoxyPants)
                {
                    if (selectedTicklet == null)
                    {
                        textOutputBox.Text = textOutputBox.Text + "\r\n" + "!!NO CLIENT CONNECTED!!";
                    }
                    else
                    {
                        String messageData = Message.trimString(selectedTicklet.getID() + " " + textInputBox.Text, "SendText");
                        Message message = new Message("SendText", messageData);

                        textOutputBox.Text = textOutputBox.Text + "\r\n" + "Guru: " + messageData.Substring(messageData.IndexOf(' ') + 1);
                        sendMessageToServer(message);
                    }
                }
                textInputBox.Text = "";
                setCooldown();
            }
        }
     

        
    }
}
