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
  public ServerService serverService=>ServerService.Instance;

  private void Start()
  {
      sendButton.onClick.AddListener(Send);
      serverService.SubscribeOperationHandler<PublicMessageDTO>(ServerToClientOperationCode.MessageReceived,OnGetPublicMessage);
      serverService.SubscribeOperationHandler<ClientIdDto>(ServerToClientOperationCode.UpdatePlayerId,OnConnectSuccess);
  }

  private void OnGetPublicMessage(PublicMessageDTO data)
  {
      Debug.Log($"message broadcast {data.ToString()}");
  }

  private void OnConnectSuccess(ClientIdDto data)
  {
      displayName.text = serverService.GetClientName();
  }
  private void Send()
  {
      bool isPrivate = string.Empty == targetID ? true : false;
      if (isPrivate)
      {
          serverService.SendPrivate(targetID,inputTextChat.text);
      }
      else
      {
          serverService.SendPublic(inputTextChat.text);
      }
  }
}
