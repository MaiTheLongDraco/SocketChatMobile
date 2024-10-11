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
    [SerializeField] private GameObject chatUI;
	[SerializeField] private TCPClientChat socketClient;
	[SerializeField] private Image fillAmount;
	public UnityAction OnLoaddingDone;
	private void Start()
	{
		OnStartApp();
		socketClient.OnConnectSuccess += OnConnected;
		socketClient.OnConnectFail += OnConnectFail;
	}
	public void OnStartApp()
	{
		DisActiveOther(waitingUI);

	}
	public void OnConnected()
    {
	    Debug.Log($"invoke on connect success");
	    DisActiveOther(connectUI);
		_ = StartLoading();
    }
	public void OnConnectFail(string msg)
	{
		Debug.Log($"invoke on connect fail");
		connectFailUI.SetActive(true);
		connectFailUI.GetComponentInChildren<Text>().text = msg;
		DisActiveOther(connectFailUI);
	}
	private async UniTask StartLoading()
	{
		fillAmount.fillAmount = 0;
		while (true)
		{
			if (fillAmount.fillAmount >= 1)
			{
				break;
			}
			fillAmount.fillAmount += 0.1f;
			await UniTask.WaitForSeconds(Time.deltaTime+0.5f);
			if (fillAmount.fillAmount >= 1)
			{
				DisActiveOther(chatUI);
				OnLoaddingDone?.Invoke();
				break;
			}
		}
	}

	private void DisActiveOther(GameObject go)
	{
		List<GameObject> listUI = new List<GameObject>();
		listUI.Add(waitingUI);
		listUI.Add(connectUI);
		listUI.Add(connectFailUI);
		listUI.Add(chatUI);
		foreach (var ui in listUI)
		{
			ui.SetActive(ui == go);
		}
	}
}
