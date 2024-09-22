using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
   public SocketClient socketClient;
	public TCPClientChat tCPClientChat;
	public static ClientManager instance;
	private void Start()
	{
		instance = this;
		tCPClientChat = GetComponentInChildren<TCPClientChat>();
	}
}
