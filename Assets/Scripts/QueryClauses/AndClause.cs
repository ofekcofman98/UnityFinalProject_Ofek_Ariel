using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AndClause : IQueryClause
{
    public string DisplayName => QueryConstants.And;
    public bool isClicked { get; private set; }
    public bool isAvailable { get; set; }

    private readonly Action _onActivate;

    public AndClause(Action onActivate)
    {
        _onActivate = onActivate;
    }

    public void Activate()
    {
        isClicked = true;
        _onActivate?.Invoke();  // 👈 start a new condition flow
        // Deactivate();           // auto-reset so it behaves like a button
    }

    public void Deactivate() { isClicked = false; }
    public void UpdateString() { }
    public void Reset() { isClicked = false; isAvailable = false; }
    public bool IsValid() { return true; }


    public bool CheckAvailableClause(Query query)
    {
        bool wasAvailable = isAvailable;

        WhereClause wc = query.whereClause;
        isAvailable =
            wc.isClicked &&
            !wc.firstConditionWasRemoved &&
            wc.Conditions.Count >= 1 &&
            wc.Conditions[0].IsComplete &&
            wc.Conditions.Count <= WhereClause.k_MaxConditions;

        wc.firstConditionWasRemoved = false;


        if (!isAvailable)
        {
            Deactivate();
            // if (wasAvailable)
            //     query.RemoveSecondCondition();
            return false;
        }
        else
        {
            return true;
        }
    }

    public string ToSQL() { return QueryConstants.Empty; }     // never prints
    public string ToSupabase() { return QueryConstants.Empty; } // never prints
}

