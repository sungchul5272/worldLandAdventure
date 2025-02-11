using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Collections;

public class UIManager : MonoBehaviour
{
	[Header("����� �Է� ����")]
	[SerializeField] InputField _inputName;
	[SerializeField] InputField _sessionCodeHost;
	[SerializeField] InputField _sessionCodeJoin;

	[Header("��ư ����")]
	[SerializeField] Button _enterBtn;
	[SerializeField] Button _startBtn;
	[SerializeField] Button _optionBtn;
	[SerializeField] Button _hostBtn;
	[SerializeField] Button _joinBtn;
	[SerializeField] Button _createRoomBtn;
	[SerializeField] Button _joinRoomBtn;
	[SerializeField] Button _startGameBtn;
	[SerializeField] Button _ReadyBtn;
	[SerializeField] Button _leaveRoomBtn;

	[SerializeField] Button[] _backToLobbyBtn;
	[SerializeField] Button[] _backToSelectModeBtn;
	[SerializeField] Button[] _exitBtns;

	[SerializeField]  GameObject _connectWaitScreen; // ���� ��� UI �г�
	[SerializeField]  Text _connectWaitText;
	public string _playerName { get; set; }
	string _sessionCode;
	bool _isHost;
	bool _isConnecting;


	void Start()
	{
		ChangeUI("Start Screen");
		ResetButton();
		_isHost = false;
		_isConnecting = false;


	}


	void ResetButton()
	{
		_enterBtn.onClick.AddListener(StartToLobby);
		_optionBtn.onClick.AddListener(LobbyToOption);
		_startBtn.onClick.AddListener(LobbyToSelectMode);
		_hostBtn.onClick.AddListener(SelectModeToCreateRoom);
		_joinBtn.onClick.AddListener(SelectModeToJoinRoom);
		_createRoomBtn.onClick.AddListener(CreateRoom);
		_joinRoomBtn.onClick.AddListener(JoinRoom);
		_startGameBtn.onClick.AddListener(StartGame);
		_ReadyBtn.onClick.AddListener(ReadyGame);
		_leaveRoomBtn.onClick.AddListener(LeaveRoom);

		foreach (Button backToLobbyBtn in _backToLobbyBtn)
		{
			backToLobbyBtn.onClick.AddListener(BackToLobby);
		}

		foreach (Button backToSelectModeBtn in _backToSelectModeBtn)
		{
			backToSelectModeBtn.onClick.AddListener(BackToSelectMode);
		}

		foreach (Button exitBtn in _exitBtns)
		{
			exitBtn.onClick.AddListener(ExitGame);
		}

	}
	void StartToLobby()
	{
		if (string.IsNullOrEmpty(_inputName.text))
		{
			Debug.Log("�̸��� �Է��ϼ���!!");
			return;
		}
		else
		{
			_playerName = _inputName.text;
			Debug.LogFormat("�κ� ����Ϸ� ����� �̸��� {0}�Դϴ�.", _playerName);
			ChangeUI("Lobby Screen");
		}
	}
	void LobbyToOption()
	{
		ChangeUI("Option Screen");

	}
	void LobbyToSelectMode()
	{
		ChangeUI("Select Mode Screen");
	}
	void SelectModeToCreateRoom()
	{
		ChangeUI("Create Room Screen");
	}
	void SelectModeToJoinRoom()
	{
		ChangeUI("Join Room Screen");
	}
	//void CreateRoom()
	//{
	//	if (string.IsNullOrEmpty(_sessionCodeHost.text))
	//	{
	//		Debug.Log("�����ڵ带 �Է��ϼ���!!");
	//		return;
	//	}
	//	else
	//	{
	//		_sessionCode = _sessionCodeHost.text;
	//		RoomManager._instance.OpenRoom(_sessionCode);
	//		ChangeUI("Waiting Room Screen");
	//		_startGameBtn.gameObject.SetActive(true);
	//		_isHost = true;
	//	}
	//}
	public async void CreateRoom()
	{
		if (string.IsNullOrEmpty(_sessionCodeHost.text))
		{
			Debug.Log("�����ڵ带 �Է��ϼ���!!");
			return;
		}
		ShowConnectingUI();

		_sessionCode = _sessionCodeHost.text;


		bool success = await RoomManager._instance.OpenRoom(_sessionCode);
		HideConnectingUI();

		if (success)
		{
			ChangeUI("Waiting Room Screen");
			_startGameBtn.gameObject.SetActive(true);
			_isHost = true;
		}
		else
		{
			Debug.Log("�� ���� ����!");
		}
	}
	public async void JoinRoom()
	{
		if (string.IsNullOrEmpty(_sessionCodeJoin.text))
		{
			Debug.Log("�����ڵ带 �Է��ϼ���!!");
			return;
		}
		ShowConnectingUI();

		_sessionCode = _sessionCodeJoin.text;
		
		bool success = await RoomManager._instance.JoinRoom(_sessionCode); // �񵿱� ����
		HideConnectingUI();

		if (success)
		{
			ChangeUI("Waiting Room Screen"); // �� ���� ���� �� UI ����
			_ReadyBtn.gameObject.SetActive(true);
		}
		else
		{
			Debug.Log("�� ���� ����");
		}
	}


	public void ShowConnectingUI()
	{
		_connectWaitScreen.SetActive(true);
		_isConnecting = true;
		StartCoroutine(AnimateConnectingText());
	}
	public void HideConnectingUI()
	{
		_isConnecting = false;
		_connectWaitScreen.SetActive(false);
		StopCoroutine(AnimateConnectingText());
	}

	IEnumerator AnimateConnectingText()
	{
		string baseText = "Connecting";
		int dotCount = 0;

		while (_isConnecting)
		{
			_connectWaitText.text = baseText + new string('.', dotCount);
			dotCount = (dotCount + 1) % 5; // 0 ~ 4 �ݺ� (�ִ� 4�� ��)
			yield return new WaitForSeconds(0.5f);
		}
	}






	void StartGame()
	{
		Debug.Log("���� ��ŸƮ");
	}
	void ReadyGame()
	{
		Debug.Log("����");
	}


	void LeaveRoom()
	{
		if(_isHost)
		{
			//ȣ��Ʈ�ϰ�� ��Ƽ ���� �ı�
			ChangeUI("Lobby Screen");
			_isHost = false;
			_startGameBtn.gameObject.SetActive(false);
			_ReadyBtn.gameObject.SetActive(false);
		}
		else
		{
			//�׳� ������ ��������
			ChangeUI("Lobby Screen");
			_isHost = false;
			_startGameBtn.gameObject.SetActive(false);
			_ReadyBtn.gameObject.SetActive(false);
		}	
	}
	void BackToLobby()
	{
		ChangeUI("Lobby Screen");
	}
	void BackToSelectMode()
	{
		ChangeUI("Select Mode Screen");
	}
	void ExitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
	}
	void ChangeUI(string uiName)
	{
		GameObject[] allUIs = GameObject.FindGameObjectsWithTag("UI");
		foreach (var ui in allUIs) ui.SetActive(false);

		Transform child = transform.Find(uiName);
		if (child != null) child.gameObject.SetActive(true);
	}

}
