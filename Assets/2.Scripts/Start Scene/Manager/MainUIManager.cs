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
        // ���� ���� UI�� ����
        if (currentUI != null)
        {
            currentUI.gameObject.SetActive(false);
        }

        // �� UI�� ã��
        Transform child = transform.Find(UIname);

        if (child == null)
        {
            Debug.Log("�ش��ϴ� UI �̸��� ã�� ���Ͽ����ϴ�.");
            return;
        }

        // �� UI�� �ѱ�
        child.gameObject.SetActive(true);

        // ���� UI�� �� UI�� ������Ʈ
        currentUI = child;
    }
}
