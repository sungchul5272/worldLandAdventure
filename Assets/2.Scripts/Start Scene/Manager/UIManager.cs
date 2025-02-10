using UnityEngine;
using UnityEngine.UI;
using Fusion;

public class UIManager : MonoBehaviour
{
	public InputField sessionCodeInput;  // ���� �ڵ� �Է� �ʵ�
	public Text sessionCodeText;         // ���� �ڵ� ǥ�� UI
	public Text playerCountText;         // ���� �ο� ǥ�� UI
	public Text playerListText;          // �÷��̾� ��� UI
	public InputField playerNameInput;   // �÷��̾� �̸� �Է� �ʵ�

	[Header("��ư ����")]
	[SerializeField] Button _enterBtn;
	[SerializeField] Button _startBtn;
	[SerializeField] Button _optionBtn;
	[SerializeField] Button _hostBtn;
	[SerializeField] Button _joinBtn;
	[SerializeField] Button _createRoomBtn;
	[SerializeField] Button _joinRoomBtn;

	[SerializeField] Button[] _backToLobbyBtn;
	[SerializeField] Button[] _backToPlayModeBtn;
	[SerializeField] Button[] _leaveRoomBtn;
	[SerializeField] Button[] _exitBtns;

	private RoomManager _roomManager;

	void Start()
	{
		_roomManager = FindObjectOfType<RoomManager>();

		if (_roomManager == null)
		{
			Debug.LogError("RoomManager�� ������ �߰ߵ��� �ʾҽ��ϴ�. RoomManager�� �����մϴ�.");
			GameObject roomManagerObj = new GameObject("RoomManager");
			_roomManager = roomManagerObj.AddComponent<RoomManager>();
		}

		_roomManager.OnRoomUpdated += UpdateUI;

		_enterBtn.onClick.AddListener(StartToLobby);
		_startBtn.onClick.AddListener(LobbyToPlayMode);
		_optionBtn.onClick.AddListener(LobbyToOption);
		_hostBtn.onClick.AddListener(PlayModeToCreateRoom);
		_joinBtn.onClick.AddListener(PlayModeToJoinRoom);
		_createRoomBtn.onClick.AddListener(CreateRoom);
		_joinRoomBtn.onClick.AddListener(JoinRoom);

		foreach (Button backToLobbyBtn in _backToLobbyBtn)
			backToLobbyBtn.onClick.AddListener(BackToLobby);

		foreach (Button backToPlayModeBtn in _backToPlayModeBtn)
			backToPlayModeBtn.onClick.AddListener(BackToPlayMode);

		foreach (Button leaveRoomBtn in _leaveRoomBtn)
			leaveRoomBtn.onClick.AddListener(LeaveRoom);

		foreach (Button exitBtn in _exitBtns)
			exitBtn.onClick.AddListener(ExitGame);

		ShowUI("Start Screen");
	}

	void StartToLobby()
	{
		string playerName = playerNameInput.text;
		if (string.IsNullOrEmpty(playerName))
		{
			Debug.LogWarning("�÷��̾� �̸��� �Է��ϼ���.");
			return;
		}

		PlayerPrefs.SetString("PlayerName", playerName);
		PlayerPrefs.Save();
		ShowUI("Lobby Screen");
	}

	void LobbyToPlayMode() => ShowUI("PlayMode Screen");
	void LobbyToOption() => ShowUI("Option Screen");
	void PlayModeToCreateRoom() => ShowUI("Create Room Screen");
	void PlayModeToJoinRoom() => ShowUI("Join Room Screen");

	void CreateRoom()
	{
		string sessionCode = sessionCodeInput.text;
		if (string.IsNullOrEmpty(sessionCode))
		{
			Debug.LogWarning("���� �ڵ带 �Է��ϼ���.");
			return;
		}

		_roomManager.CreateRoom(sessionCode);
		ShowUI("Room Screen Host");
	}

	void JoinRoom()
	{
		_roomManager.JoinRoom();
		ShowUI("Room Screen Join");
	}

	void LeaveRoom()
	{
		_roomManager.LeaveRoom();
		ShowUI("Lobby Screen");
	}

	void BackToLobby() => ShowUI("Lobby Screen");
	void BackToPlayMode() => ShowUI("PlayMode Screen");

	void ExitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
	}

	void UpdateUI()
	{
		sessionCodeText.text = $"�� �ڵ�: {_roomManager.SessionCode}";
		playerCountText.text = $"�÷��̾�: {_roomManager.PlayerCount}/4";
		playerListText.text = _roomManager.GetPlayerList();
	}

	void ShowUI(string uiName)
	{
		GameObject[] allUIs = GameObject.FindGameObjectsWithTag("UI");
		foreach (var ui in allUIs) ui.SetActive(false);

		Transform child = transform.Find(uiName);
		if (child != null) child.gameObject.SetActive(true);
	}

}
