using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
[Serializable]
public class SocketClient:MonoBehaviour
{
		public  string Host = "127.0.0.1";
		public  int Port = 1234;
		public  Socket request;
		public  string hello = "Hello from client";
		public  byte[] dataReceiveBuffer = new byte[1024];
		public UnityEvent OnConnectSuccess;
		public UnityEvent<string> OnConnectFail;
		public UnityEvent<object> OnReceiveSuccess;
		public UnityEvent<string> OnReceiveFail;
		public ConnectionStatus ConnectionStatus;
		public  void Connect()
		{
			request = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPAddress iPAddress = IPAddress.Parse(Host);
			IPEndPoint endPoint = new IPEndPoint(iPAddress, Port);
			request.BeginConnect(endPoint, OnConnectCallback, null);
			Console.ReadLine();
		}
		private  void OnConnectCallback(IAsyncResult ar)
		{
			try
			{
				request.EndConnect(ar);
				Debug.Log("connect to server success");
				ConnectionStatus = ConnectionStatus.Success;
				MainThreadDispatcher.Instance.Enqueue(BeginReceive);
				byte[] buffer = Encoding.UTF8.GetBytes(hello);
				request.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, OnSendDataToServer, null);
			}
			catch (Exception ex)
			{
				Console.WriteLine("connect to server fail " + ex.ToString());
				OnConnectFail?.Invoke(ex.ToString());
			}
		}

		private void BeginReceive()
		{
			if (ConnectionStatus == ConnectionStatus.Success)
			{
				OnConnectSuccess?.Invoke();
			Debug.Log($"connection success 2");
				request.BeginReceive(dataReceiveBuffer, 0, dataReceiveBuffer.Length, SocketFlags.None, OnReceiveCallBack, null);
			}
		}

		private  void OnSendDataToServer(IAsyncResult ar)
		{
			int byteSend = request.EndSend(ar);
			if (byteSend > 0)
			{
				Debug.Log($" send data success");
			}
			request.BeginReceive(dataReceiveBuffer, 0, dataReceiveBuffer.Length, SocketFlags.None, OnReceiveCallBack, null);
		}

		private  void OnReceiveCallBack(IAsyncResult ar)
		{
			int byteRead = request.EndReceive(ar);
			if (byteRead > 0)
			{
				ST_DATA_TRANFER sT_DATA_TRANFER = default(ST_DATA_TRANFER);
				AppMath.ConvertByteArrToStructure(dataReceiveBuffer, byteRead, ref sT_DATA_TRANFER);
				//Console.WriteLine("receive data success");
				StringBuilder sb = new StringBuilder();
				sb.AppendLine(sT_DATA_TRANFER.DataInt.ToString());
				sb.AppendLine(sT_DATA_TRANFER.DataUshort.ToString());
				sb.AppendLine(sT_DATA_TRANFER.DataBool.ToString());
				sb.AppendLine(sT_DATA_TRANFER.DataString.ToString());
				//sb.AppendLine(sT_DATA_TRANFER.DataByteArr.Count().ToString());
				Debug.Log($"message from server {sb.ToString()}");
				OnReceiveSuccess?.Invoke(sT_DATA_TRANFER);
				request.BeginReceive(dataReceiveBuffer, 0, byteRead, SocketFlags.None, OnReceiveCallBack, null);
			}
		}
}

public enum ConnectionStatus
{
	None,
	Success,
	Error
}
