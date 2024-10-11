using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject waitingUI;
    [SerializeField] private GameObject connectUI;
    [SerializeField] private GameObject connectFailUI;
	[SerializeField] private TCPClientChat socketClient;
	[SerializeField] private Image fillAmount;
	public UnityAction OnLoaddingDone;
	[SerializeField]  private string clientID = ""; // ID được server gán
	private void Start()
	{
		OnStartApp();
		socketClient.OnConnectSuccess += OnConnected;
		socketClient.OnConnectFail += OnConnectFail;
	}
	public void OnStartApp()
    {
        waitingUI.SetActive(true);
        connectUI.SetActive(false);
		connectFailUI.SetActive(false);
		ServerService.Instance.SubscribeOperationHandler<ClientIdDto>(ServerToClientOperationCode.UpdatePlayerId,UpdatePlayerID);
	}

	private void UpdatePlayerID(ClientIdDto playerIdData)
	{
		clientID = playerIdData.Id;
	}
	public void OnConnected()
    {
	    Debug.Log($"invoke on connect success");
		waitingUI.SetActive(false);
		connectUI.SetActive(true);
		connectFailUI.SetActive(false);
		_ = StartLoading();
    }
	public void OnConnectFail(string msg)
	{
		Debug.Log($"invoke on connect fail");
		connectFailUI.SetActive(true);
		connectFailUI.GetComponentInChildren<Text>().text = msg;
	}
	private async UniTask StartLoading()
	{
		if(fillAmount.fillAmount >= 1)
			return;
		fillAmount.fillAmount = 0;
		await UniTask.WaitForSeconds(0.3f);
		fillAmount.fillAmount += 0.1f;
		await UniTask.WaitForSeconds(0.3f);
		_ = StartLoading();
		await UniTask.WaitUntil(() => fillAmount.fillAmount >= 1);
		OnLoaddingDone?.Invoke();


	}
}
