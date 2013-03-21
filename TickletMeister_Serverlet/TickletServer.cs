using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

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
        public void addEntityEntry(int id, Socket sock, Thread l)
        {
            lock (entities)
            {
                listenThreads.Add(sock, l);
                entities.Add(id, sock);
                idLookup.Add(sock, id);
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
        private ManualResetEvent ClientMRE = new ManualResetEvent(false); //for dealing with async calls regarding client-server dataflow

        private Entities entities = new Entities();

        private Dictionary<int, Ticklet> tickletQueue = new Dictionary<int, Ticklet>(); //TODO this is very primitive; it shall be fixed
        private IDGenerator gen = new SimpleIDGen(0);

        private Thread refreshThread;

        private void startServer(String[] args)
        {
            int port = 8888;


            IPAddress[] AddressAr = null;
            String ServerHostName = "";
            try
            {
                ServerHostName = Dns.GetHostName();
                IPHostEntry ipEntry = Dns.GetHostByName(ServerHostName);
                AddressAr = ipEntry.AddressList;
            }
            catch (Exception) { }

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
                  
                    //con.Bind(new IPEndPoint(AddressAr[0], port));
                    con.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port)); //TODO this should be changed eventually to something not localhost
                }
                catch (SocketException e)
                {
                    port++;
                }
            }
            con.Listen(10);
            Console.WriteLine("server running on port " + port);
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

        /**
         *  This is called whenever a client attempts to connect to the server
         * */
        private void ClientCallback(IAsyncResult ar)
        {
            MRE.Set();
            
            Socket serverSocket = (Socket)ar.AsyncState;
            Socket clientSocket = serverSocket.EndAccept(ar);
            Action listen = () => {
                while (true)
                {
                    try
                    {
                        ClientMRE.Reset();
                        byte[] buffer = new byte[Message.BUFF_SIZE];
                        ConnectionState state = new ConnectionState(clientSocket, buffer);
                        clientSocket.BeginReceive(buffer, 0, Message.BUFF_SIZE, SocketFlags.None, new AsyncCallback(OnRecieve), state);
                        ClientMRE.WaitOne();
                    }
                    catch (SocketException e)
                    {
                        //it's ok... it happens sometimes... don't worry
                    }
                }

            };
            Thread listener = new Thread(new ThreadStart(listen));

            int id = gen.generateID();
            Console.WriteLine("client connected! ... Assigned ID# "+id);

            entities.addEntityEntry(id, clientSocket, listener);
            listener.Start();
        }

        
        /**
         * This is called whenever a client attempts to send data to the server
         * */
        private void OnRecieve(IAsyncResult ar)
        {
            ClientMRE.Set();

            ConnectionState state = (ConnectionState)ar.AsyncState;
            Socket clientSocket = state.getSocket();
            Message message = Message.decodeMessage(state.getBuffer());
            Console.WriteLine("reading message: " + message);
            if(message != null)
            handleMessage(message, clientSocket);
            
        }

        /**
         *  refreshes the list for all currently authenticated gurus
         *  
         * 
         * */
        private void RefreshGuruLists()
        {
            lock (tickletQueue)
            {
                if (tickletQueue.Count > 0)
                {
                    StringBuilder buildy = new StringBuilder();
                    foreach (int id in tickletQueue.Keys)
                    {
                        buildy.Append(";" + "ID# " + id);
                    }
                    String ticks = buildy.ToString().Substring(buildy.ToString().IndexOf(';') + 1); //remove starting ';'
                    sendToAllGurus(new Message("RefreshList", ticks));
                }
                else
                {
                    sendToAllGurus(new Message("RefreshList", "nothing"));
                }
            }

        }

        private void sendToAllGurus(Message message)
        {
            foreach (Socket guru in entities.listGurus())
            {
                try
                {
                    Message.sendMessageTo(message, guru);
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

            if (tag.Equals("Echo"))
            {
                handleMessageEcho(data, clientSocket);
            }
            else if (tag.Equals("Ticklet"))
            {
                handleMessageTicklet(data, clientSocket);
            }
            else if (tag.Equals("Disconnect"))
            {
                handleMessageDisconnect(clientSocket);
            }
            else if (tag.Equals("Poll"))
            {
                handleMessagePoll(data, clientSocket);
            }
            else if (tag.Equals("AlertAll"))
            {
                handleMessageAlertAll(data);
            }
            else if (tag.Equals("DesireVoice"))
            {
                handleMessageDesireVoice(data, clientSocket);
            }
            else if (tag.Equals("Authenticate"))
            {
                handleMessageAuthenticate(data, clientSocket);
            }
        }

        private void handleMessageEcho(String text, Socket clientSocket)
        {
            Message message = new Message("DisplayText", text);
            Message.sendMessageTo(message, clientSocket);
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
            entities.removeEntityEntry(clientSocket);
            clientSocket.Close();
        }

        private void handleMessagePoll(String data, Socket clientSocket)
        {
            
            lock (tickletQueue)
            {
                if (tickletQueue.Count() > 0)
                {
                    if ("dgaf".Equals(data))
                    {
                        Ticklet tick = tickletQueue.Values.ElementAt(0);
                        String messageString = tick.getID() + " " + tick.getConnectionString();
                        tickletQueue.Remove(0); //TODO change this to be a seperate message type; with this setup, we risk dropping ticklets, but it suffices for the time being
                        Message.sendMessageTo(new Message("Ticklet", messageString), clientSocket);
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
                                tickletQueue.Remove(index); //TODO change this to be a seperate message type; with this setup, we risk dropping ticklets, but it suffices for the time being
                                Message.sendMessageTo(new Message("Ticklet", messageString), clientSocket);
                            }
                            else
                            {
                                Message.sendMessageTo(new Message("PollFail", ":("), clientSocket);
                            }

                        }
                        else
                        {
                            Message.sendMessageTo(new Message("PollFail", ":("), clientSocket);
                        }
                    }
                }
                else
                {
                    Message.sendMessageTo(new Message("PollFail", ":("), clientSocket);
                }
            }
        }

        private void handleMessageAlertAll(String data)
        {
            //TODO change this so that it updates the field correctly
            //as of now, it simply replaces all the text in the output field
            Message disp = new Message("DisplayText", data);
            foreach(Socket sock in entities.getSockets())
            {
                try
                {
                    Message.sendMessageTo(disp, sock);
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
                        Message.sendMessageTo(new Message("GoGoVoiceChat", guruString), guruSocket);
                        Message.sendMessageTo(new Message("GoGoVoiceChat", clientString), clientSocket);
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


       
    }
}
