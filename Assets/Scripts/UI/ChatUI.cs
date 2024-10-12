using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
   [SerializeField] private InputField inputTextChat;
  [SerializeField] private ScrollRect chatViewScroll;
   [SerializeField] private Button sendButton;
   [SerializeField] private string targetID;
   [SerializeField] private Text displayName;
   [SerializeField] private ClientToServerOperationCode commandType;
  public ServerService serverService=>ServerService.Instance;

  private void Start()
  {
      sendButton.onClick.AddListener(Send);
      serverService.SubscribeOperationHandler<PublicMessageDTO>(ServerToClientOperationCode.MessageReceived,OnGetPublicMessage);
      serverService.SubscribeOperationHandler<NotifyNewPlayerDTO>(ServerToClientOperationCode.NotifyNewPlayer,OnNotiNewPlayer);
      serverService.SubscribeOperationHandler<ClientIdDto>(ServerToClientOperationCode.UpdatePlayerId,OnConnectSuccess);
  }

  private void OnGetPublicMessage(PublicMessageDTO data)
  {
      Debug.Log($"message broadcast {data.ToString()}");
  }
  private void OnNotiNewPlayer(NotifyNewPlayerDTO data)
  {
      Debug.Log($"Sender ID {data.SenderId} sender name {data.SenderName} content {data.Content}");
        UIManager.instance.MakeNotiSlider(data.Content);
  }

  private void OnConnectSuccess(ClientIdDto data)
  {
      MainThreadDispatcher.Instance.Enqueue(() =>
      {
          displayName.text = serverService.GetClientName();
      });
  }
  private void Send()
  {
        string newMessgage = $" <color=#00E9FF><link=PLAYER_NAME>{serverService.GetClientName()}</link></color>: " +
        $"<color=#FFFFFFFF>{inputTextChat.text}</color>";

		  bool isPrivate = string.Empty == targetID ? true : false;
          if (isPrivate)
          {
              serverService.SendPrivate(targetID, newMessgage);
          }
          else
          {
              serverService.SendPublic(newMessgage, commandType);
          }  
  }
}
