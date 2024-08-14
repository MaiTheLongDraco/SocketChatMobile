using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject waitingUI;
    [SerializeField] private GameObject connectUI;
    [SerializeField] private GameObject connectFailUI;
	private void Start()
	{
		OnStartApp();
	}
	public void OnStartApp()
    {
        waitingUI.SetActive(true);
        connectUI.SetActive(false);
		connectFailUI.SetActive(false);
	}
	public void OnConnected()
    {
		waitingUI.SetActive(false);
		connectUI.SetActive(true);
		connectFailUI.SetActive(false);

	}
	public void OnConnectFail(string msg)
	{
		connectFailUI.SetActive(true);
		connectFailUI.GetComponentInChildren<Text>().text = msg;
	}
}
