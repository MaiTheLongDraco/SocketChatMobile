using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
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
    private byte[] buffer= new byte[4056];
    public UnityAction<string> OnReceiveMsg;
    public UnityAction OnConnectSuccess;
    public UnityAction<string> OnConnectFail;
    public string UserName;
    private bool isConnected = false;
    public ServerService ServerService;
    [SerializeField]  public string clientID = ""; // ID được server gán

    private void Start()
    {
        ServerService.SubscribeOperationHandler<ClientIdDto>(ServerToClientOperationCode.UpdatePlayerId,UpdatePlayerID);

    }
    private void UpdatePlayerID(ClientIdDto playerIdData)
    {
        clientID = playerIdData.Id;
        isConnected = true;
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
        byte[] buffer = new byte[4096];
        int byteCount;

        try
        {
            while ((byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string receivedData = Encoding.UTF8.GetString(buffer, 0, byteCount).Trim();
                // Có thể có nhiều thông điệp trong một buffer, tách chúng bằng '\n'
                string[] messages = receivedData.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                connectionStatus = ConnectionStatus.Success;
                foreach (var message in messages)
                {
					Debug.Log($"messages data  {message}");
					ServerService.HandleMessage(message);
				}
			}
        }
        catch (Exception ex)
        {
            Debug.LogError("Error receiving message: " + ex.Message);
            connectionStatus= ConnectionStatus.Error;
            OnConnectFail?.Invoke(ex.ToString());
        }
     
    }
    /// <summary>
    /// Hàm này dùng để broadcast message cho tất cả user khác
    /// </summary>
    /// <param name="message"></param>
 public  void SendMessageToServer(string message,ClientToServerOperationCode messageType)
    {
        if (!isConnected) return;

        try
        {
            if (messageType == ClientToServerOperationCode.NotifyNewPlayer)
            {
                var messageDTO = new NotifyNewPlayerDTO()
                {
                    SenderId = clientID,
                    Content = message,
                    SenderName = UserName,
                };
                var protocolMessage = new ProtocolMessage<NotifyNewPlayerDTO>
                {
                    ProtocolType = (int)messageType,
                    Data = messageDTO
                };
                string json = JsonConvert.SerializeObject(protocolMessage) + "\n";
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                Debug.Log($" send notify new player to server");
                stream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                var messageDTO = new PublicMessageDTO()
                {
                    SenderId = clientID,
                    Content = message,
                    SenderName = UserName,
                    Timestamp = DateTime.Now
                };
                var protocolMessage = new ProtocolMessage<PublicMessageDTO>
                {
                    ProtocolType = (int)messageType,
                    Data = messageDTO
                };
                string json = JsonConvert.SerializeObject(protocolMessage) + "\n";
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                stream.Write(buffer, 0, buffer.Length);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error sending message: " + ex.Message);
        }
    }
/// <summary>
/// Hàm này dùng để send message đến client với 1 id cụ thể
/// </summary>
/// <param name="targetId"></param>
/// <param name="message"></param>


public   void SendMessageToSpecificClient(string targetId, string message)
    {
        if (!isConnected) return;

        try
        {
            var messageDTO = new PrivateMessageDTO
            {
                SenderId = clientID,
                TargetID = targetId,
                Content = message,
                SenderName = UserName,
                Timestamp = DateTime.Now
            };

            var protocolMessage = new ProtocolMessage<PrivateMessageDTO>
            {
                ProtocolType = (int)ClientToServerOperationCode.SendPrivateMessage,
                Data = messageDTO
            };
            string json = JsonConvert.SerializeObject(protocolMessage) + "\n";
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            stream.Write(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error sending message to specific client: " + ex.Message);
        }
    }

   
}
