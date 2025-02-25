using UnityEngine;
using System.Collections;

public class DiceManager : MonoBehaviour
{
	static DiceManager _uniqueInstance;

	public static DiceManager _instance
	{
		get { return _uniqueInstance; }
	}

	[SerializeField] Animator _animator;
	float resetDelay = 1.0f;

	void Awake()
	{
		_uniqueInstance = this;
	}

	public int RollDice()
	{
		int result = Random.Range(1, 7);
		PlayDiceAnimation(result);
		return result;
	}

	public void PlayDiceAnimation(int result)
	{
		if (_animator != null)
		{
			_animator.SetInteger("Value", result);
			_animator.SetBool("IsRolling", true);
			StartCoroutine(ResetIsRollingAfterDelay());
		}
	}

	IEnumerator ResetIsRollingAfterDelay()
	{
		yield return new WaitForSeconds(resetDelay);
		if (_animator != null)
			_animator.SetBool("IsRolling", false);
	}
}
