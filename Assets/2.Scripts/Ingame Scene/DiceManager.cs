using UnityEngine;
using Fusion;

public class DiceManager : MonoBehaviour
{
    public static DiceManager _instance;

    [SerializeField] Animator _dice1Animator, _dice2Animator;
    [SerializeField] Transform _dice1Transform, _dice2Transform;

    [Networked] int _dice1Value { get; set; }
    [Networked] int _dice2Value { get; set; }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        _dice1Transform.position = new Vector3(-1.5f, 0.4f, 0);
        _dice2Transform.position = new Vector3(1.5f, 0.4f, 0);
    }

    public void RollDice()
    {
            _dice1Value = Random.Range(1, 7);
            _dice2Value = Random.Range(1, 7);

            _dice1Animator.SetInteger("Value", _dice1Value);
            _dice2Animator.SetInteger("Value", _dice2Value);

            _dice1Animator.SetBool("IsRolling", true);
            _dice2Animator.SetBool("IsRolling", true);

            _dice1Transform.position = new Vector3(_dice1Transform.position.x, 0.4f, _dice1Transform.position.z);
            _dice2Transform.position = new Vector3(_dice2Transform.position.x, 0.4f, _dice2Transform.position.z);
        
    }
}
