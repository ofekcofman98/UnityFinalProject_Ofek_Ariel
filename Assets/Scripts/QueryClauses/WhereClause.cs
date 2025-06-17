using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WhereClause : IQueryClause
{
    public string DisplayName => QueryConstants.Where;
    public string WherePart { get; private set; } = QueryConstants.Empty;
    public List<Condition> Conditions;
    public Condition newCondition { get; set; }
    public bool isClicked { get; private set; } = false;
    public bool isAvailable { get; set; } = false;

    public WhereClause()
    {
        Conditions = new List<Condition>();     
    }

//     public void Toggle()
//     {
//         isClicked = !isClicked;

//         if (!isClicked)
//         {
//             clearConditions();
//         }
// Debug.Log($"[WhereClause] Toggle() called — isClicked now: {isClicked}");
//         UpdateString();
//     }

    public void Activate()
    {
        isClicked = true;   
    }

    public void Deactivate()
    {
        if (isClicked)
        {
            clearConditions();
        }
        else
        {
            isClicked = false;
        }
    }

    public void CreateNewCondition(Column i_Column)
    {
        newCondition = new Condition();
        newCondition.OnConditionUpdated += UpdateString; 
        newCondition.Column = i_Column;

        UpdateString();
    }

    public void AddCondition()
    {
        if (newCondition == null || newCondition.Value == null)
        {
            Debug.LogWarning("Cannot add condition: Condition or Value is null.");
            return;
        }

        Conditions.Add(newCondition);
        newCondition = null;
        UpdateString();
    }


    public void UpdateString()
    {
        if (isClicked)
        {
            List<string> allConditions = Conditions.Select(cond => cond.ConditionString).ToList();

            if (newCondition != null && !string.IsNullOrWhiteSpace(newCondition.ConditionString))
            {
                allConditions.Add(newCondition.ConditionString);
            }

            if (allConditions.Count > 0)
            {
                WherePart = QueryConstants.Where + " " + string.Join(QueryConstants.Comma, allConditions);
            }
            else
            {
                WherePart = QueryConstants.Where;
            }

            // Debug.Log($"where PArt: {WherePart}");
        }
        else 
        {
            WherePart = QueryConstants.Empty;
        }
    }

    public string ToSQL()
    {
        return WherePart;
    }

    public string ToSupabase()
    {
        return Conditions.Count > 0 ? string.Join(QueryConstants.And, Conditions.Select(cond => cond.ConditionStringSupaBase)) : "";
    }

    public void OnQueryUpdated(Query query)
    {
        bool wasAvailable = isAvailable;
        isAvailable = query.IsValid;
        if (wasAvailable != isAvailable)
        {
            query.CheckAvailableClause();
        }

        if (isAvailable)
        {
            UpdateString();
        }
        else
        {
            Reset();
        }

    }

    public bool NeedsCondition()
    {
        if (!isClicked)
        {
            return false; // Clause not active
        }
        if (newCondition == null)
        {
            return false; // No pending condition
        }    

        // A condition is "incomplete" if it has no operator or no value
        return newCondition.Operator == null || newCondition.Value == null;
    }

    public List<object> GetOrderedElements()
    {
        List<object> elements = new List<object>();

        if (isClicked)
        {
            elements.Add(this); // WHERE clause first
            elements.AddRange(Conditions); // Then all conditions
        }

        return elements;
    }


    private void clearConditions()
    {
        Conditions.Clear();
        newCondition = null;
    }
    
    public void Reset()
    {
        isClicked = false;
        isAvailable = false;
        WherePart = QueryConstants.Empty;
        clearConditions();
    }

}
