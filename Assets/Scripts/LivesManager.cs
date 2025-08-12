using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivesManager : Singleton<LivesManager>
{
    [SerializeField] private int maxLives = 3;
    public int MaxLives => maxLives;

    public int Lives { get; private set; }
    public event Action<int> OnLivesChanged;

    protected override void Awake()
    {
        base.Awake();
        ResetLives(); // start full
    }

    public void ConfigureMax(int newMax)
    {
        maxLives = Mathf.Max(1, newMax);
        Lives = Mathf.Min(Lives, maxLives);
        OnLivesChanged?.Invoke(Lives);
    }

    public void SetLives(int value)
    {
        Lives = Mathf.Clamp(value, 0, maxLives);
        OnLivesChanged?.Invoke(Lives);
    }

    public void ResetLives()
    {
        Lives = maxLives;
        OnLivesChanged?.Invoke(Lives);
    }

    public int Decrement()
    {
        if (Lives > 0)
        {
            Lives--;
        }
        OnLivesChanged?.Invoke(Lives);
        return Lives;
    }

    public int Increment()
    {
        if (Lives < maxLives)
        {
            Lives++;
        }
        OnLivesChanged?.Invoke(Lives);
        return Lives;
    }
}
