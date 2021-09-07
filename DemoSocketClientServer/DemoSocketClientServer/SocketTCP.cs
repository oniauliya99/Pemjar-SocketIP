using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoSocketClientServer
{
    class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 256;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }
    class SocketTCP
    {
        public static ManualResetEvent connectDone = new ManualResetEvent(false);
        public static ManualResetEvent sendDone = new ManualResetEvent(false);
        public static ManualResetEvent receiveDone = new ManualResetEvent(false);
        public static String reponse = String.Empty;
        //############################
        //       SOCKET CLIENT
        //############################
        public static void StartClient(string pesan)
        {
            IPAddress ip = IPAddress.Parse(varGlobal.alamatIPServer);
            IPEndPoint remoteEP = new IPEndPoint(ip, varGlobal.port);
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var result = client.BeginConnect(remoteEP, new AsyncCallback(SocketTCP.ConnectCallback), client);
            result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
            Thread.Sleep(10);
            if (client.Connected)
            {
                Send(client, pesan);
                Thread.Sleep(500);
                sendDone.WaitOne();
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
        }
        public static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                connectDone.Set();
            }
            catch
            {
            }
        }
        public static void Send(Socket client, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int byteSent = client.EndSend(ar);
                sendDone.Set();
            }
            catch
            {
            }
        }
        public static void Receive(Socket client)
        {
            StateObject state = new StateObject();
            state.workSocket = client;
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }
        private static void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;
            int bytesRead = client.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            else
            {
                if (state.sb.Length > 1)
                {
                    reponse = state.sb.ToString();
                    varGlobal.terimapesandariserver = reponse;
                }
                receiveDone.Set();
            }
        }
        //##################################
        //          SOCKET SERVER
        //##################################
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static void StartListening()
        {
            IPAddress ipAddress = IPAddress.Parse(varGlobal.alamatIPServer);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, varGlobal.port);
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100); // jumlah client max <= 100 client
                while (true)
                {
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        public static void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }
        public static void ReadCallback(IAsyncResult ar)
        {
            String terimaPesan = String.Empty;
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                terimaPesan = state.sb.ToString();
                if (terimaPesan != "")
                {
                    varGlobal.terimapesandiserver = terimaPesan;
                    Send(handler, terimaPesan);
                }
            }
        }
    }
}
    
