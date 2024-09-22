using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Events;

public class TCPClientChat : MonoBehaviour
{
    [SerializeField] private string ip;
    [SerializeField] private int port;
    [SerializeField] private ConnectionStatus connectionStatus;
    TcpClient clientSocket;
    NetworkStream stream;
    private Thread listentThread;
    private byte[] buffer= new byte[1024];
    public UnityAction<string> OnReceiveMsg;
    public UnityAction OnConnectSuccess;
    public UnityAction<string> OnConnectFail;
    public string UserName;
    // Start is called before the first frame update
    void Start()
    {
       
    }
    public void Connect()
    {
		clientSocket = new TcpClient();
		clientSocket.Connect(ip, port);
		if (clientSocket != null)
		{
			OnConnectSuccess?.Invoke();
			stream = clientSocket.GetStream();
		}
		listentThread = new Thread(ListenForMessages);
		listentThread.Start();
	}
    private void ListenForMessages()
    {
        try
        {
			byte[] buffer = new byte[1024];
			int byteCount = stream.Read(buffer, 0, buffer.Length);
			while (byteCount > 0)
			{
				string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
				Debug.Log($"message receive from server {message}");
				OnReceiveMsg?.Invoke(message);
                connectionStatus= ConnectionStatus.Success;
                SendMsg($" user {UserName} has connect to server");
			}
        }
        catch(Exception ex)
        {
            Debug.LogError(ex.ToString());
            connectionStatus= ConnectionStatus.Error;
            OnConnectFail?.Invoke(ex.ToString());
        }
    }
    public void SendMsg(string message)
    {
        try
        {
			byte[] msgBt = Encoding.UTF8.GetBytes(message);
			stream.Write(msgBt, 0, message.Length);
        }
        catch(Exception e) 
        {
            Debug.LogError($"send data fail due to {e.ToString()}");
        }
      
    }
    [ContextMenu("TestSendToserver")]
	private void SendTest()
    {
        byte[] msgBt = Encoding.UTF8.GetBytes($"Draco test send");
		stream.Write(msgBt, 0, msgBt.Length);
	}
	// Update is called once per frame
	void Update()
    {
        
    }
}
