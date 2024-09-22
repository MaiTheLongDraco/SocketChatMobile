using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject waitingUI;
    [SerializeField] private GameObject connectUI;
    [SerializeField] private GameObject connectFailUI;
	[SerializeField] private TCPClientChat socketClient;
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
	}
	public void OnConnected()
    {
	    Debug.Log($"invoke on connect success");
		waitingUI.SetActive(false);
		connectUI.SetActive(true);
		connectFailUI.SetActive(false);

	}
	public void OnConnectFail(string msg)
	{
		Debug.Log($"invoke on connect fail");
		connectFailUI.SetActive(true);
		connectFailUI.GetComponentInChildren<Text>().text = msg;
	}
}
