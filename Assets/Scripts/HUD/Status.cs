using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HPEvent : UnityEngine.Events.UnityEvent<int, int> { }

[System.Serializable]
public class GoldEvent : UnityEngine.Events.UnityEvent<int> { }

[System.Serializable]
public class ScoreEvent : UnityEngine.Events.UnityEvent<int> { }
public class Status : MonoBehaviour
{
    [HideInInspector]
    public HPEvent onHPEvent = new HPEvent();

    [HideInInspector]
    public GoldEvent onGoldEvent = new GoldEvent();

    [HideInInspector]
    public ScoreEvent onScoreEvent = new ScoreEvent();

    [Header("Walk, Run Speed")]
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;

    [Header("HP")]
    [SerializeField]
    private int maxHP = 100;
    private int currentHP;

    [Header("Gold")]
    //[SerializeField]
    private int Gold;

    [Header("Score")]
    [SerializeField]
    private int Score = 0;

    public float WalkSpeed => walkSpeed;
    public float RunSpeed
    {
        get => runSpeed;
        set { runSpeed = value; }
    }
    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void SetHP(int hp)
    {
        currentHP = hp;
    }

    public void SetGold(int gold)
    {
        Gold = gold;
    }

    public void SetScore(int score)
    {
        Score = score;
    }


    public int GetGold()
    {
        return Gold; 
    }

    public int GetScore()
    {
        return Score;
    }

    public bool DecreaseHP(int damage)
    {
        int previousHP = currentHP;

        currentHP = currentHP - damage > 0 ? currentHP - damage : 0;

        onHPEvent.Invoke(previousHP, currentHP);

        if(currentHP == 0)
        {
            return true;
        }

        return false;
    }

    public void IncreaseHP(int hp)
    {
        int previousHP = currentHP;

        currentHP = currentHP + hp > maxHP ? maxHP : currentHP + hp;

        onHPEvent.Invoke(previousHP, currentHP);
    }

    public void IncreaseGold(int gold)
    {
        Gold += gold;

        onGoldEvent.Invoke(Gold);
    }

    public void DecreaseGold(int gold)
    {
        Gold -= gold;
        onGoldEvent.Invoke(Gold);
    }

    public void IncreaseScore(int score)
    {
        Score += score;
        onScoreEvent.Invoke(Score);
    }
}
