using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainUIManager : MonoBehaviour
{
	Transform currentUI;

	void Start()
	{
		OpenUI("Start Screen");
	}



	public void ExitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
Application.Quit();
#endif
	}

    public void OpenUI(string UIname)
    {
        // 먼저 기존 UI를 끄기
        if (currentUI != null)
        {
            currentUI.gameObject.SetActive(false);
        }

        // 새 UI를 찾기
        Transform child = transform.Find(UIname);

        if (child == null)
        {
            Debug.Log("해당하는 UI 이름을 찾지 못하였습니다.");
            return;
        }

        // 새 UI를 켜기
        child.gameObject.SetActive(true);

        // 현재 UI를 새 UI로 업데이트
        currentUI = child;
    }
}
