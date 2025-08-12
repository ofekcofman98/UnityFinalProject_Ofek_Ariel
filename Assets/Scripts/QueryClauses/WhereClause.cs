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

    public void Activate()
    {
        isClicked = true;
    }

    public void Deactivate()
    {
        if (isClicked)
        {
            clearConditions();
            Debug.Log("im clearing conditions right now");
        }

        isClicked = false;
    }

    public void StartNewCondition()
    {
        newCondition = new Condition();
        newCondition.OnConditionUpdated += UpdateString;
        UpdateString();
    }


    public void CreateNewCondition(Column i_Column)
    {
        if (newCondition == null)
            StartNewCondition();      // reuse instead of always replacing
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
            List<string> allConditions = Conditions
                .Select(cond => cond.ConditionString)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();


            if (newCondition != null && !string.IsNullOrWhiteSpace(newCondition.ConditionString))
            {
                allConditions.Add(newCondition.ConditionString);
            }

            if (allConditions.Count > 0)
            {
                WherePart = QueryConstants.Where + " " + string.Join($" {QueryConstants.And} ", allConditions);
            }
            else
            {
                WherePart = QueryConstants.Where;
            }
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
        return Conditions.Count > 0
            ? string.Join("&", Conditions.Select(cond => cond.ConditionStringSupaBase))
            : "";
    }


    public void OnQueryUpdated(Query query)
    {
        bool wasAvailable = isAvailable;
        isAvailable = query.selectClause.IsValid();

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

    public bool IsValid()
    {
        return !isClicked || (Conditions.Count > 0 &&
                              Conditions.All(c => c.Column != null && c.Operator != null && c.Value != null));
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
