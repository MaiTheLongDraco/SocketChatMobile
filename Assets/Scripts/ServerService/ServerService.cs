using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class ServerService : MonoBehaviour
{
    private OperationHandler m_OperationHandler;
    private static ServerService instance;
    public static ServerService Instance { get { return instance; } }
    private void Awake()
    {
        m_OperationHandler= new OperationHandler();
    }

    public void SubscribeOperationHandler<T>(
        ServerToClientOperationCode operationCode, OperationHandler.OperationHandleDelegate<T> operationHandleDelegate)
    {
        m_OperationHandler.SubscribeOperationHandler(operationCode,operationHandleDelegate);
    }

    public void UnSubscribeOperationHandler<T>(
        ServerToClientOperationCode operationCode, OperationHandler.OperationHandleDelegate<T> operationHandleDelegate)
    {
        m_OperationHandler.UnSubscribeOperationHandler(operationCode,operationHandleDelegate);
    }
    public void HandleMessage(string msg)
    {
        m_OperationHandler.HandleMessage(msg);
    }
}
// OperationHandler.cs
public class OperationHandler
{
    // Định nghĩa delegate cho các handler
    public delegate void OperationHandleDelegate<T>(T data);

    // Dictionary lưu trữ các handler cho từng operation code
    // Lưu trữ cả delegate gốc và Action<JObject>
    private readonly Dictionary<ServerToClientOperationCode, List<(Delegate originalDelegate, Action<JObject> handler)>> _handlers 
        = new Dictionary<ServerToClientOperationCode, List<(Delegate, Action<JObject>)>>();

    private readonly object _lock = new object();

    /// <summary>
    /// Đăng ký một handler cho một operation code cụ thể.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu của Data trong ProtocolMessage.</typeparam>
    /// <param name="operationCode">Mã operation từ server.</param>
    /// <param name="operationHandleDelegate">Delegate xử lý dữ liệu nhận được.</param>
    public void SubscribeOperationHandler<T>(
        ServerToClientOperationCode operationCode,
        OperationHandleDelegate<T> operationHandleDelegate)
    {
        lock (_lock)
        {
            if (!_handlers.ContainsKey(operationCode))
            {
                _handlers[operationCode] = new List<(Delegate, Action<JObject>)>();
            }

            // Tạo Action<JObject> từ OperationHandleDelegate<T>
            Action<JObject> handler = (jObject) =>
            {
                // Deserialize dữ liệu từ JObject thành kiểu T và gọi delegate
                T data = jObject.ToObject<T>();
                operationHandleDelegate?.Invoke(data);
            };

            // Thêm handler vào danh sách
            _handlers[operationCode].Add((operationHandleDelegate, handler));
        }
    }



    /// <summary>
    /// Hủy đăng ký một handler cho một operation code cụ thể.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu của Data trong ProtocolMessage.</typeparam>
    /// <param name="operationCode">Mã operation từ server.</param>
    /// <param name="operationHandleDelegate">Delegate xử lý dữ liệu nhận được cần hủy đăng ký.</param>
    public void UnSubscribeOperationHandler<T>(
        ServerToClientOperationCode operationCode,
        OperationHandleDelegate<T> operationHandleDelegate)
    {
        lock (_lock)
        {
            if (_handlers.ContainsKey(operationCode))
            {
                var handlerList = _handlers[operationCode];
                // Tìm và loại bỏ các handler có delegate gốc trùng với operationHandleDelegate
                handlerList.RemoveAll(h => h.originalDelegate.Equals(operationHandleDelegate));

                // Nếu danh sách handlers cho operationCode rỗng, xóa entry khỏi dictionary
                if (handlerList.Count == 0)
                {
                    _handlers.Remove(operationCode);
                }
            }
        }
    }

    /// <summary>
    /// Xử lý thông điệp nhận được từ server và gọi các handler tương ứng.
    /// </summary>
    /// <param name="jsonMessage">Thông điệp JSON nhận được từ server.</param>
    public void HandleMessage(string jsonMessage)
    {
        // Deserialize ProtocolMessage từ JSON
        var protocolMessage = Newtonsoft.Json.JsonConvert.DeserializeObject<ProtocolMessage<JObject>>(jsonMessage);
        if (protocolMessage.ProtocolType==-1||protocolMessage.Data==null)
        {
            Console.WriteLine("Invalid message format.");
            return;
        }

        // Chuyển đổi ProtocolType thành enum
        if (!Enum.IsDefined(typeof(ServerToClientOperationCode), protocolMessage.ProtocolType))
        {
            Debug.LogError($"Unknown ProtocolType: {protocolMessage.ProtocolType}");
            return;
        }

        var operationCode = (ServerToClientOperationCode)protocolMessage.ProtocolType;

        lock (_lock)
        {
            if (_handlers.ContainsKey(operationCode))
            {
                foreach (var handlerPair in _handlers[operationCode])
                {
                    handlerPair.handler(protocolMessage.Data);
                }
            }
            else
            {
                Debug.LogError($"No handlers subscribed for operation code: {operationCode}");
            }
        }
    }
}
// ServerToClientOperationCode.cs
public enum ServerToClientOperationCode
{
    UpdatePlayerId=0,
    GetMessageResponse = 1,
    MessageReceived = 2,
    // Thêm các operation code khác nếu cần
}

// ClientToServerOperationCode.cs
public enum ClientToServerOperationCode
{
    GetMessage = 1,
    SendMessage = 2,
    // Thêm các operation code khác nếu cần
}
public struct MessageDTO
{
    public string SenderId { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
}

// ProtocolMessage.cs
public struct ProtocolMessage<T>
{
    public int ProtocolType { get; set; }
    public T Data { get; set; }
}

