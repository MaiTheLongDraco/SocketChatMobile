using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{
   public SocketClient socketClient;
	private void Start()
	{
		socketClient= GetComponentInChildren<SocketClient>();
	}
}
