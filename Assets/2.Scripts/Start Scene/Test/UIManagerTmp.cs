using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;

public class UIManagerTmp : MonoBehaviour
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
    [SerializeField] CanvasGroup _createCanvasGroup;
    [SerializeField] CanvasGroup _joinCanvasGroup;
    public string _playerName { get; set; }
    string _sessionCode;
    bool _isConnecting;


    void Start()
    {
        ChangeUI("Start Screen");
        ResetButton();
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
            return;
        }

        string playerName = _inputName.text.Trim();
        PlayerDataTmp._instance.SetPlayerName(playerName);
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
        _createCanvasGroup.blocksRaycasts = false;

        bool success = await RoomManagerTmp._instance.OpenRoom(_sessionCode);
        HideConnectingUI();
        _createCanvasGroup.blocksRaycasts = true;

        if (success)
        {
            ChangeUI("Waiting Room Screen");
            _startGameBtn.gameObject.SetActive(true);

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
        _joinCanvasGroup.blocksRaycasts = false;

        bool success = await RoomManagerTmp._instance.JoinRoom(_sessionCode);
        HideConnectingUI();
        _joinCanvasGroup.blocksRaycasts = true;

        if (success)
        {
            ChangeUI("Waiting Room Screen");
            _ReadyBtn.gameObject.SetActive(true);

        }
        else
        {
            Debug.Log("방 참가 실패");
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
        Debug.Log("레디버튼 클릭");
        PlayerManagerTmp._instance.ToggleReadyState();
    }
    void LeaveRoom()
    {
        RoomManagerTmp._instance.LeaveRoom();

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
