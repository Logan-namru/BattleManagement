using System;
using UDPConnector;
using UnityEngine;

public class UDPListenerUsageExample : MonoBehaviour
{
	// Listener Variables
	private UDPListener myListener;
	private int listenerPort = 12345;
	private string ipAddress = "127.0.0.1";

	void Start()
	{
		myListener = new UDPListener(listenerPort);
		myListener.MessageRecieved += new MessageReceivedEventHandler(MessageProcess);
		myListener.ErrorOccured += new EventHandler(ErrorProcess);
		myListener.Start();
	}

	private void MessageProcess(object sender, MessageReceivedEventArgs e)
	{
		string data = (sender as UDPConnector.UDPListener).Message;
		Debug.Log("Message received: " + data);
	}

	private void ErrorProcess(object sender, EventArgs e)
	{
		Debug.Log("An error occured!");
	}
}