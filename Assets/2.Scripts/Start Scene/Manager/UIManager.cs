using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;

public class UIManager : MonoBehaviour
{
	[Header("사용자 입력 정보")]
	[SerializeField] InputField _inputName;
	[SerializeField] InputField _sessionCodeHost;
	[SerializeField] InputField _sessionCodeJoin;

	[Header("버튼 모음")]
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
	[SerializeField] Button _cansleBtn;

	[SerializeField] Button[] _backToLobbyBtn;
	[SerializeField] Button[] _backToSelectModeBtn;
	[SerializeField] Button[] _exitBtns;

	[SerializeField] GameObject _connectWaitScreen; // 연결 대기 UI 패널
	[SerializeField] Text _connectWaitText;
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
		_cansleBtn.onClick.AddListener(CansleConecting);

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
	public void StartToLobby()
	{
		if (string.IsNullOrEmpty(_inputName.text))
		{
			Debug.Log("이름을 입력하세요!!");
			return;
		}

		string playerName = _inputName.text.Trim();


		//  PlayerManager에 이름 저장
		if (PlayerManager._instance != null)
		{
			PlayerManager._instance.SetLocalPlayerName(playerName);
		}

		Debug.LogFormat("로비 입장 완료! 당신의 이름은 {0}입니다.", playerName);
		ChangeUI("Lobby Screen");
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

	public async void CreateRoom()
	{
		if (string.IsNullOrEmpty(_sessionCodeHost.text))
		{
			Debug.Log("세션코드를 입력하세요!!");
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

			StartCoroutine(WaitForPlayerManagerAndSendName());
		}
		else
		{
			Debug.Log("방 생성 실패!");
		}
	}

	public async void JoinRoom()
	{
		if (string.IsNullOrEmpty(_sessionCodeJoin.text))
		{
			Debug.Log("세션코드를 입력하세요!!");
			return;
		}

		ShowConnectingUI();
		_sessionCode = _sessionCodeJoin.text;

		bool success = await RoomManager._instance.JoinRoom(_sessionCode);
		HideConnectingUI();

		if (success)
		{
			ChangeUI("Waiting Room Screen");
			_ReadyBtn.gameObject.SetActive(true);

			StartCoroutine(WaitForPlayerManagerAndSendName());
		}
		else
		{
			Debug.Log("방 참가 실패");
		}
	}

	private IEnumerator WaitForPlayerManagerAndSendName()
	{
		while (PlayerManager._instance == null || !PlayerManager._instance.Object || !PlayerManager._instance.Object.IsValid)
		{
			yield return null; // 네트워크 오브젝트가 스폰될 때까지 대기
		}

		PlayerManager._instance.SendNameToServer();
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
			_connectWaitText.text = baseText + " " + new string('.', dotCount);
			dotCount = (dotCount + 1) % 5; // 0 ~ 4 반복 (최대 4개 점)
			yield return new WaitForSeconds(0.5f);
		}
	}





	void StartGame()
	{
		Debug.Log("게임 스타트");
	}
	void ReadyGame()
	{
		Debug.Log("레디");
	}
	void LeaveRoom()
	{
		RoomManager._instance.LeaveRoom();

		_isHost = false;
		_startGameBtn.gameObject.SetActive(false);
		_ReadyBtn.gameObject.SetActive(false);

		ChangeUI("Lobby Screen");
	}


	void CansleConecting()
	{
		HideConnectingUI();
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
	public void ChangeUI(string uiName)
	{
		GameObject[] allUIs = GameObject.FindGameObjectsWithTag("UI");
		foreach (var ui in allUIs) ui.SetActive(false);

		Transform child = transform.Find(uiName);
		if (child != null) child.gameObject.SetActive(true);
	}



}
