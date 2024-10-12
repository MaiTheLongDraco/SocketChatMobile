using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
   [SerializeField] private TextMeshProUGUI chatDisplay;
   [SerializeField] private ClientToServerOperationCode commandType;
  public ServerService serverService=>ServerService.Instance;

  private void Start()
  {
      sendButton.onClick.AddListener(Send);
        //Lắng nghe tin nhắn public từ server
      serverService.SubscribeOperationHandler<PublicMessageDTO>(ServerToClientOperationCode.MessageReceived,OnGetPublicMessage);
        // Lắng nghe khi có người dùng mới vào server
      serverService.SubscribeOperationHandler<NotifyNewPlayerDTO>(ServerToClientOperationCode.NotifyNewPlayer,OnNotiNewPlayer);
        // Lắng nghe khi connect thành công
      serverService.SubscribeOperationHandler<ClientIdDto>(ServerToClientOperationCode.UpdatePlayerId,OnConnectSuccess);
		//Lắng nghe tin nhắn private từ server
		serverService.SubscribeOperationHandler<PrivateMessageDTO>(ServerToClientOperationCode.GetMessageResponse, OnGetPrivateMessage);
  }

  private void OnGetPublicMessage(PublicMessageDTO data)
    {
        Debug.Log($"message broadcast {data.ToString()}");
        Enqueu(() =>
        {
			chatDisplay.text += data.Content + "\n";
        });
  }
	private void OnGetPrivateMessage(PrivateMessageDTO data)
	{
		Debug.Log($"message private {data.ToString()}");
	}
	private void OnNotiNewPlayer(NotifyNewPlayerDTO data)
  {
      Debug.Log($"Sender ID {data.SenderId} sender name {data.SenderName} content {data.Content}");
        UIManager.instance.MakeNotiSlider(data.Content);
  }
    private void Enqueu(Action callBack)
    {
        MainThreadDispatcher.Instance.Enqueue(callBack);
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
		string privateMsg = $"<color=red>[PRIVATE]</color> <color=#00E9FF><link=PLAYER_NAME>{serverService.GetClientName()}</link></color>: " +
	  $"<color=#FFFFFFFF>{inputTextChat.text}</color>";

		bool isPrivate = string.Empty == targetID ? false : true;
          if (isPrivate)
          {
			commandType = ClientToServerOperationCode.SendPrivateMessage;
			serverService.SendPrivate(targetID, privateMsg);
          }
          else
          {
            commandType = ClientToServerOperationCode.SendMessage;
              serverService.SendPublic(newMessgage, commandType);
          }  
  }
}
//<sprite index=1> <link=PLAYER_NAME>Thông Báo</link>: <color=#00E9FF>Hiện đang trong gian đoạn Võ lâm liên đấu, hôm nay từ PLAYER_NAME>234234sdfw</link>: <color=#FFFFFFFF>rtyrtyrty</color>
//< sprite name = "EmojiOne_0" > < link = PLAYER_NAME > 234234sdfw </ link >: < color =#FFFFFFFF>ryrtyry</color>
//< sprite name = "Near" > < link = PLAYER_NAME > 234234sdfw </ link >: < color =#FFFFFFFF>rtyrtyr</color>
