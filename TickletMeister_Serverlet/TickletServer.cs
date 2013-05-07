using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.IO;

namespace TickletMeister_Serverlet
{

    interface IDGenerator
    {
        int generateID();
    }

    class SimpleIDGen : IDGenerator
    {
        private int next;
        public SimpleIDGen(int init)
        {
            next = init;
        }
        public int generateID()
        {
            return next++;
        }
    }

    class Entities
    {
        private Dictionary<int, Socket> entities = new Dictionary<int, Socket>(); //maps a unique id to each socket :: ONLY entities WILL BE USED AS LOCK
        private Dictionary<Socket, int> idLookup = new Dictionary<Socket, int>(); //maps the same socket to its id
        private Dictionary<Socket, Thread> listenThreads = new Dictionary<Socket, Thread>(); //maps each client socket to the respective listen thread
        private Dictionary<int, Socket> gurus = new Dictionary<int, Socket>(); //keeps track of the connected gurus
        private Dictionary<Socket, String> encrypters = new Dictionary<Socket, String>(); //maps each connection to its corresponding public key
        private int CLIENT_LIMIT = 33; //maximum number of entities that can have socket connections with the server
        public bool addEntityEntry(int id, Socket sock, Thread l)
        {
            lock (entities)
            {
                if (listenThreads.Count() < CLIENT_LIMIT)
                {
                    listenThreads.Add(sock, l);
                    entities.Add(id, sock);
                    idLookup.Add(sock, id);
                    encrypters.Add(sock, null);
                    return true;
                }
                return false;
            }
        }
        public void associateKey(Socket sock, String publicKey)
        {
            lock (entities)
            {
                encrypters.Remove(sock);
                encrypters.Add(sock, publicKey);
            }
        }
        public bool containsSocket(Socket sock)
        {
            lock (entities)
            {
                return entities.Values.Contains(sock);
            }
        }
        public bool authenticateGuru(Socket sock)
        {
            lock (entities)
            {
                int id;
                bool suc = idLookup.TryGetValue(sock, out id);
                if (suc)
                {
                    gurus.Add(id, sock);
                }
                return suc;
            }
        }
        public void removeEntityEntry(Socket sock)
        {
            lock (entities)
            {
                Thread t;
                int id;
                bool hasThread = listenThreads.TryGetValue(sock, out t);
                bool hasID = idLookup.TryGetValue(sock, out id);
                if (!hasID)
                {
                    throw new Exception("Something went terribly wrong! A socket was never assigned an ID!");
                }
                if (hasThread)
                {
                    t.Abort();
                    listenThreads.Remove(sock);
                }
                idLookup.Remove(sock);
                entities.Remove(id);
                gurus.Remove(id);
                encrypters.Remove(sock);
            }
        }
        public Socket IDtoSocket(int id)
        {
            lock (entities)
            {
                Socket sock;
                entities.TryGetValue(id, out sock);
                return sock;
            }
        }
        public Thread IDToThread(int id)
        {
            lock (entities)
            {
                Thread t;
                Socket s;
                entities.TryGetValue(id, out s);
                listenThreads.TryGetValue(s, out t);
                return t;
            }
        }
        public Thread SocketToThread(Socket s)
        {
            lock (entities)
            {
                Thread t;
                listenThreads.TryGetValue(s, out t);
                return t;
            }
        }
        public String SocketToKey(Socket s)
        {
            lock (entities)
            {
                String k;
                encrypters.TryGetValue(s, out k);
                return k;
                
            }
        }
        public int SocketToID(Socket s)
        {
            lock (entities)
            {
                int id;
                idLookup.TryGetValue(s, out id);
                return id;
            }
        }
        public Dictionary<Socket,Thread>.ValueCollection getThreads()
        {
            lock (entities)
            {
                return listenThreads.Values;
            }
        }
        public Dictionary<Socket, Thread>.KeyCollection getSockets()
        {
            lock (entities)
            {
                return listenThreads.Keys;
            }
        }
        public Dictionary<int, Socket>.KeyCollection getIDs()
        {
            lock (entities)
            {
                return entities.Keys;
            }
        }
        public Dictionary<int, Socket>.ValueCollection listGurus()
        {
            lock (entities)
            {
                return gurus.Values;
            }
        }
        public static String SocketToIP(Socket s)
        {
            return (s.RemoteEndPoint as IPEndPoint).Address.ToString();
        }
    }

    class TickletServer
    {
       
        private Socket con = null;
        private ManualResetEvent MRE = new ManualResetEvent(false); //for dealing with async calls regarding initial connection establishment

        private Entities entities = new Entities();

        private Dictionary<int, Ticklet> tickletQueue = new Dictionary<int, Ticklet>(); //TODO this is very primitive; it shall be fixed
        private IDGenerator gen = new SimpleIDGen(0);

        private Thread refreshThread;

        private Cryptocus crypt = new Cryptocus();

        private IPAddress myIP;

        private void SetMyIP()
        {
            try //to read config.txt
            {
                String address = parseAddressFromConfigFile();
                myIP = IPAddress.Parse(address);
            }
            catch //if we can't read config.txt or the address is invalid, then we'll just use our own inferred ip.
            {
                FindMyIP();
            }
        }

        private String parseAddressFromConfigFile()
        {
            String line;
            
                using (StreamReader sr = new StreamReader("config.txt"))
                {
                    line = sr.ReadLine();
                    // line = line.Substring(9);
                }
                return line;
            
        }

        private void FindMyIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    myIP = ip;
                    Console.WriteLine(myIP.ToString());
                    return;
                }
            }
            myIP = IPAddress.Parse("127.0.0.1");
        }

        private void startServer(String[] args)
        {
            
            int port = 8888;
            SetMyIP();
         

            if (args.Length > 0)
            {
                try
                {
                    port = int.Parse(args[0]);

                }
                catch (Exception e)
                {
                    //default back to 8888
                    port = 8888;
                }
            }
            InitializeRefreshThread(2000);
            con = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            while (!con.IsBound)
            {
                try
                {
                  
                    
                    con.Bind(new IPEndPoint(myIP, port)); 
                }
                catch (SocketException e)
                {
                    port++;
                }
            }
            con.Listen(10);
            Console.WriteLine("server running on " +myIP.ToString() +":"+ port);
            while (true)
            {
                MRE.Reset();
                con.BeginAccept(new AsyncCallback(ClientCallback), con);
                MRE.WaitOne();
            }
            
        }

        private void InitializeRefreshThread(int freq)
        {
            Action action = () => {
                while (true)
                {
                    Thread.Sleep(freq);
                    RefreshGuruLists();
                }
            };
            refreshThread = new Thread(new ThreadStart(action));
            refreshThread.Start();
        }

        static void Main(string[] args)
        {
           new TickletServer().startServer(args);
           
        }

        private void listenForMessage(Socket clientSocket, AsyncCallback callback, int buffSize, ManualResetEvent ClientMRE)
        {
            try
            {
                ClientMRE.Reset();
                byte[] buffer = new byte[buffSize];
                ConnectionState state = new ConnectionState(clientSocket, buffer, ClientMRE);
                clientSocket.BeginReceive(buffer, 0, buffSize, SocketFlags.None, callback, state);
                ClientMRE.WaitOne();
            }
            catch
            {
                //it's ok... it happens sometimes... don't worry
            }
        }


        /**
         *  This is called whenever a client attempts to connect to the server
         * */
        private void ClientCallback(IAsyncResult ar)
        {
            
            
            Socket serverSocket = (Socket)ar.AsyncState;
            Socket clientSocket = serverSocket.EndAccept(ar);
            MRE.Set();
            Action listen = () => {
               // while (true)
               // {
                    ManualResetEvent ClientMRE = new ManualResetEvent(false); //for dealing with async calls regarding client-server dataflow
                    lock (entities)
                    {
                        String pkey = entities.SocketToKey(clientSocket);
                        if (pkey == null)
                        {
                            
                            listenForMessage(clientSocket, new AsyncCallback(OnFirstRecieve), Message.BUFF_SIZE_UNENCRYPT, ClientMRE);
                            
                        }
                       
                    }
                //}
                Console.WriteLine("now listening for encrypted stuff");
                while (true)
                {
                         
                        listenForMessage(clientSocket, new AsyncCallback(OnRecieve), Message.BUFF_SIZE+Message.OFFSET, ClientMRE);
                        
                }

            };
            Thread listener = new Thread(new ThreadStart(listen));

            lock (gen)
            {
                int id = gen.generateID();
                Console.WriteLine("client connected! ... Assigned ID# " + id);

                bool suc = entities.addEntityEntry(id, clientSocket, listener);
                if (suc)
                {
                    listener.Start();
                }
                else
                {
                    Console.WriteLine("Too many clients connected! " + id + " was let go... :(");
                }
            }
        }


        private void OnFirstRecieve(IAsyncResult ar)
        {
            recieve(ar, Message.decodeMessage);
        }

        private void recieve(IAsyncResult ar, Func<byte[], Message> funk)
        {
            

            ConnectionState state = (ConnectionState)ar.AsyncState;
            Socket clientSocket = state.getSocket();
            Message message = funk(state.getBuffer());
            Console.WriteLine("reading message: " + message);
            if (message != null)
                handleMessage(message, clientSocket);

            state.getMRE().Set();
        }

        /**
         * This is called whenever a client attempts to send data to the server
         * we assume that the public key for the client has been determined
         * */
        private void OnRecieve(IAsyncResult ar)
        {
            try
            {
                recieve(ar, (byte[] mess) => { return Message.decodeMessage(crypt.decrypt(mess)); });
            }
            catch
            {
                Console.WriteLine("client running its mouth...");
                ConnectionState state = (ConnectionState)ar.AsyncState;
                Socket clientSocket = state.getSocket();
                terminateRelationship(clientSocket);
                
            }
        }

        private void terminateRelationship(Socket clientSocket)
        {
            if (entities.containsSocket(clientSocket))
            {
                int id = entities.SocketToID(clientSocket);
                lock (tickletQueue)
                {
                    if (tickletQueue.Keys.Contains(id))
                    {
                        tickletQueue.Remove(id);
                    }
                }
                entities.removeEntityEntry(clientSocket);
                Console.WriteLine("...relationship terminated");
            }
            clientSocket.Close();
        }

        /**
         *  refreshes the list for all currently authenticated gurus
         *  
         * 
         * */
        private void RefreshGuruLists()
        {
            String ticks = "nothing";
            lock (tickletQueue)
            {
                if (tickletQueue.Count > 0)
                {
                    StringBuilder buildy = new StringBuilder();
                    foreach (int id in tickletQueue.Keys)
                    {
                        buildy.Append(";" + "ID# " + id);
                    }
                    ticks = buildy.ToString().Substring(buildy.ToString().IndexOf(';') + 1); //remove starting ';'
                    
                }
               
            }
            sendToAllGurus(new Message("RefreshList", ticks));

        }

        private void sendToAllGurus(Message message)
        {
            Dictionary<int, Socket>.ValueCollection list = entities.listGurus(); //clone the list
                foreach (Socket guru in list)
                {
                    try
                    {
                        sendMessageTo(message, guru);
                    }
                    catch (SocketException e)
                    {
                        //ignore if sending didn't work
                    }
                }
            
        }

        /**
         * Functionality for each message type is handled here
         * */
        private void handleMessage(Message message, Socket clientSocket)
        {
            String tag = message.getTag();
            String data = message.getData();

            if(tag.Equals(PublicKey.KEYCOMMAND)) //public key recieved from entity
            {
                handleIncomingPublicKeyMessage(data, clientSocket);
            }
            else if (tag.Equals("Echo")) //echo specified text back to entity
            {
                handleMessageEcho(data, clientSocket);
            }
            else if (tag.Equals("T")) //add specified ticklet to ticklet queue
            {
                handleMessageTicklet(data, clientSocket);
            }
            else if (tag.Equals("Disconnect")) //disconnect the entity
            {
                handleMessageDisconnect(clientSocket);
            }
            else if (tag.Equals("Poll")) //poll a ticklet from the ticklet queue
            {
                handleMessagePoll(data, clientSocket);
            }
            else if (tag.Equals("AlertAll")) //send text message to all connected entities
            {
                handleMessageAlertAll(data);
            }
            else if (tag.Equals("DesireVoice")) //send request for voice chat with specified client
            {
                handleMessageDesireVoice(data, clientSocket);
            }
            else if (tag.Equals("Authenticate")) //designates the entity as a guru; subscribes to periodic tickletQueue refreshment
            {
                handleMessageAuthenticate(data, clientSocket);
            }
			else if (tag.Equals("SendText")) //data contains destination and message text, clientsocket is who message is from
			{
				handleMessageText(data, clientSocket);
			}
        }
       
        private void handleIncomingPublicKeyMessage(String data, Socket clientSocket)
        {
            
                entities.associateKey(clientSocket, data);
                Message.sendPublicKeyTo(clientSocket, crypt.getPublicKey());
            
        }

        private void handleMessageEcho(String text, Socket clientSocket)
        {
            Message message = new Message("DisplayText", text);
            sendMessageTo(message, clientSocket);
        }

        /**
         * Ticklet <ConnectionString>
         * */
        private void handleMessageTicklet(String data, Socket clientSocket)
        {
            //update ticklet queue with new entry
            lock (tickletQueue)
            {
                int id = entities.SocketToID(clientSocket);
                if (id == null)
                    throw new Exception("A client connected without recieving an ID!");
                Ticklet newTick = new Ticklet(id, data);
                tickletQueue.Add(id, newTick); //TODO change this to also include additional information about the client
            }
            Console.WriteLine("updated ticklet queue (in theory)");
            
        }

        private void handleMessageDisconnect(Socket clientSocket)
        {
            terminateRelationship(clientSocket);
        }

        /**
         * Poll <index> //polls the specified index from tickletQueue
         * || Poll dgaf //polls the next element (don't care which one) from tickletQueue
         * */
        private void handleMessagePoll(String data, Socket clientSocket)
        {
            
            lock (tickletQueue)
            {
                if (tickletQueue.Count() > 0)
                {
                    if ("dgaf".Equals(data))
                    {
                        Ticklet tick = tickletQueue.Values.ElementAt(0);
                        int tickID = tickletQueue.Keys.ElementAt(0);
                        String messageString = tick.getID() + " " + tick.getConnectionString();
                        tickletQueue.Remove(tickID);
                        sendMessageTo(new Message("T", messageString), clientSocket);
                    }
                    else
                    {
                        int index;
                        bool parsed = int.TryParse(data, out index);
                        Console.WriteLine(index + " " + parsed);
                        if (parsed)
                        {

                            Ticklet tick;
                            bool hasTick = tickletQueue.TryGetValue(index, out tick);
                            if (hasTick)
                            {
                                int id = tick.getID();
                                String conString = tick.getConnectionString();
                                Console.WriteLine("polled ID# " + id + " from queue");
                                String messageString = id + " " + conString;
                                tickletQueue.Remove(index);
                                sendMessageTo(new Message("T", messageString), clientSocket);
                            }
                            else
                            {
                                sendMessageTo(new Message("PollFail", ":("), clientSocket);
                            }

                        }
                        else
                        {
                            sendMessageTo(new Message("PollFail", ":("), clientSocket);
                        }
                    }
                }
                else
                {
                    sendMessageTo(new Message("PollFail", ":("), clientSocket);
                }
            }
        }

        /**
         * AlertAll <message>
         * */
        private void handleMessageAlertAll(String data)
        {
            //TODO change this so that it updates the field correctly
            //as of now, it simply replaces all the text in the output field
            Message disp = new Message("DisplayText", data);
            foreach(Socket sock in entities.getSockets())
            {
                try
                {
                    sendMessageTo(disp, sock);
                }
                catch (SocketException e)
                {
                    //TODO unable to send alert to sock
                }
            }
        }

        /**
         *  This message will only be sent from the Viewportletlet aka guru
         *  
         *  DesireVoice <ClientID> <inPort> <outPort>
         * */
        private void handleMessageDesireVoice(String data, Socket guruSocket)
        {
            String[] tokens = data.Split(' ');
            if (tokens.Length != 3)
            {
                Console.WriteLine("improper arguments for DesireVoice"); //ignore request
            }
            else
            {
                int id;
                int inPort;
                int outPort;
                bool idSuc = int.TryParse(tokens[0], out id);
                bool inSuc = int.TryParse(tokens[1], out inPort);
                bool outSuc = int.TryParse(tokens[2], out outPort);
                if (idSuc && inSuc && outSuc)
                {
                    Socket clientSocket = entities.IDtoSocket(id);
                    if (clientSocket != null)
                    {
                        String guruAddress = Entities.SocketToIP(guruSocket);
                        String clientAddress = Entities.SocketToIP(clientSocket);
                        String guruString = clientAddress+" "+inPort+" "+outPort;
                        String clientString = guruAddress+" "+outPort+" "+inPort;
                        Console.WriteLine("Guru: " + guruString);
                        Console.WriteLine("Client: " + clientString);
                        sendMessageTo(new Message("GoGoVoiceChat", guruString), guruSocket);
                        sendMessageTo(new Message("GoGoVoiceChat", clientString), clientSocket);
                    }
                }
                else
                {
                    Console.WriteLine("error parsing DesireVoice request: " + data);
                }
            }
        }

        private void handleMessageAuthenticate(String data, Socket guruSocket)
        {
            entities.authenticateGuru(guruSocket);
        }
		
		private void handleMessageText(String data, Socket clientSocket)
		{
			string id = data.Substring(0,  data.IndexOf(' '));
            string text = data.Substring(data.IndexOf(' ') + 1);
            int idd;
            int.TryParse(id, out idd);
			 //data contains destination and message text, clientsocket is who message is from
			 
			try
			{
			Socket destination = entities.IDtoSocket(idd);
			int senderID = entities.SocketToID(clientSocket);
			
			text = senderID + " " + text;
			
			Message newMessage = new Message("SendText", text);
			sendMessageTo(newMessage, destination);
			}
			catch
			{
				//do nothing -- dont try to send message
			}
			
		}

        public void sendMessageTo(Message message, Socket clientSocket)
        {
            try
            {
                String pkey = entities.SocketToKey(clientSocket);
                if (pkey != null)
                {
                    byte[] encoding = crypt.encrypt(Message.encodeMessage(message), pkey);
                    clientSocket.Send(encoding, 0, Message.BUFF_SIZE + Message.OFFSET, SocketFlags.None);
                }
                else
                {
                    //do something on error that the key is invalid/null?
                }
            }
            catch //if we can't send to a client, then kick them off
            {
                terminateRelationship(clientSocket);
            }
        }


       
    }
}
