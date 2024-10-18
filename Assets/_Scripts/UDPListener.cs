using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace UDPConnector
{
	public class UDPListener
	{
		private bool isRunning;
		private Thread listeningThread;
		private IPEndPoint endPoint;
		private UdpClient client;
		private string message;
		private byte[] byteData;

		public string Message
		{
			get
			{
				lock (message)
				{
					return message;
				}
			}
		}
		public byte[] ByteData
		{
			get
			{
				lock (byteData)
				{
					return byteData;
				}
			}
		}

		public event MessageReceivedEventHandler MessageRecieved;
		public event EventHandler ErrorOccured;

		public UDPListener(int portNumber)
		{
			endPoint = new IPEndPoint(IPAddress.Any, portNumber);

			Initialize(portNumber);
		}

		public UDPListener(string ipAddress, int portNumber)
		{
			endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), portNumber);

			Initialize(portNumber);
		}

		private void Initialize(int portNumber)
		{
			isRunning = false;
			listeningThread = new Thread(new ThreadStart(ListenToMessages));
			listeningThread.IsBackground = true;
		}

		private void ListenToMessages()
		{
			string receivedData;
			byte[] receiveByteArray;
			IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
			EndPoint remote = (EndPoint)sender;

			while (isRunning)
			{
				try
				{
					byteData = client.Receive(ref sender);
					message = Encoding.ASCII.GetString(byteData, 0, byteData.Length);

					if (MessageRecieved != null)
						MessageRecieved(this, new MessageReceivedEventArgs() { IPAddress = sender.Address.ToString() });
				}
				catch (SocketException) { /*expected to occur when closed, since it will be waiting on data to be received. Want execution to continue normally.*/ }
				catch (ObjectDisposedException) { isRunning = false; }
				catch (InvalidOperationException e) //Can occur when disposing of objects;
				{
					if (!(e.Message.Contains("Invoke") && e.Message.Contains("BeginInvoke")))
					{
						if (ErrorOccured != null)
						{
							ErrorOccured(e.Message, new EventArgs());
						}
						isRunning = false;
					}
				}
				catch (Exception e)
				{
					if (ErrorOccured != null)
					{
						ErrorOccured(e.Message, new EventArgs());
					}
					isRunning = false;
				}
			}
		}

		public void Start()
		{
			if (listeningThread.ThreadState != ThreadState.Running)
			{
				isRunning = true;

				if (listeningThread.ThreadState == ThreadState.Stopped)
					listeningThread = new Thread(new ThreadStart(ListenToMessages));

				var isConnected = false;
				var attemptCount = 0;
				while (!isConnected)
				{
					try
					{
						client = new UdpClient();
						client.Client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
						client.Client.ExclusiveAddressUse = false;
						client.Client.Bind(endPoint);

						isConnected = true;
					}
					catch (SocketException)
					{
						attemptCount++;

						if (attemptCount == 10)
						{
							if (ErrorOccured != null)
							{
								ErrorOccured("Exceeded max attempts to connect listeners", new EventArgs());
							}
							isRunning = false;
							return;
						}
					}
				}

				listeningThread.Start();
			}
		}

		public void Stop()
		{
			isRunning = false;

			if (client != null)
			{
				client.Close();
			}
		}
	}

	public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);
	public class MessageReceivedEventArgs : EventArgs
	{
		public MessageReceivedEventArgs() : base() { }
		public string IPAddress { get; set; }
	}
}
