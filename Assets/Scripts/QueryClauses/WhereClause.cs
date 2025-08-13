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

    public bool CheckAvailableClause(Query query)
    {
        bool newlyAvailable = query.selectClause.IsValid();
        if (!newlyAvailable && isClicked) Deactivate();

        isAvailable = newlyAvailable;
        return isAvailable;
    } 


    public bool IsValid()
    {
        return !isClicked || (Conditions.Count > 0 &&
                              Conditions.All(c => c.Column != null && c.Operator != null && c.Value != null));
    }

    public bool IsValidForOperator()
    {
        // if (newCondition != null)
        // {
        //     return newCondition.Column != null;
        // }
        // else if (Conditions.Count > 0)
        // {
        //     return Conditions.Last().Column != null;
        // }
        bool res = false;
        Condition condition = FindLastCondition();
        if (condition != null)
        {
            res = condition.Column != null;
        }

        return res;
    }

    public bool IsValidForValue()
    {
        bool res = false;

        // if (newCondition != null)
        // {
        //     res = newCondition.Operator != null;
        //     Debug.Log($"[IsValidForValue] newCondition.Operator != null = {res}");
        // }

        // else if (Conditions.Count > 0)
        // {
        //     res = Conditions.Last().Operator != null;
        //     Debug.Log($"[IsValidForValue] Conditions.Last().Operator != null = {res}");
        // }

        Condition condition = FindLastCondition();
        if (condition != null)
        {
            res = condition.Operator != null;
        }
        return res;
    }


    public void RemoveConditionsByColumn(Column columnToRemove)
    {
        if (columnToRemove == null) return;

        // Check how many conditions use this column
        int count = Conditions.Count(cond => cond.Column == columnToRemove);

        if (count == 1)
        {
            // Remove the only one using this column
            Conditions.RemoveAll(cond => cond.Column == columnToRemove);
        }
        else if (count > 1)
        {
            // ⚠️ Smart choice: removing this column breaks a range (e.g., age ≥ 30 AND age ≤ 40)
            // In this case, better to clear all conditions using that column
            Conditions.RemoveAll(cond => cond.Column == columnToRemove);
        }

        // Always remove `newCondition` if it references the column
        if (newCondition?.Column == columnToRemove)
        {
            newCondition = null;
        }

        UpdateString();
    }

    public void SetOperator(IOperatorStrategy i_operator)
    {
        // if (newCondition != null)
        // {
        //     newCondition.Operator = i_Operator;
        // }
        // else if (whereClause.Conditions.Count > 0)
        // {
        //     whereClause.Conditions.Last().Operator = i_Operator;
        // }
        Condition condition = FindLastCondition();

        if (condition != null)
        {
            condition.Operator = i_operator;
        }
    }

    public void RemoveOperator()
    {
        Condition condition = FindLastCondition();

        if (condition != null)
        {
            Conditions.Remove(condition);
            CreateNewCondition(condition.Column);
        }
    }

    public void SetValue(object i_Value)
    {
        Condition last = FindLastCondition();
        if (last != null)
        {
            last.Value = i_Value;
            AddCondition();
        }
    }

    public void RemoveValue()
    {
        Condition last = FindLastCondition();
        if (last != null)
        {
            Conditions.Remove(last);
            CreateNewCondition(last.Column);
            SetOperator(last.Operator);
        }
    }

    private void clearConditions()
    {
        Conditions.Clear();
        newCondition = null;
    }

    public Condition FindLastCondition()
    {
        Condition condition;

        if (newCondition != null)
        {
            condition = newCondition;
        }
        else if (Conditions.Count > 0)
        {
            condition = Conditions.Last();
        }
        else
        {
            condition = null;
        }

        return condition;
    }

    public void Reset()
    {
        isClicked = false;
        isAvailable = false;
        WherePart = QueryConstants.Empty;
        clearConditions();
    }

    public bool IsEmpty()
    {
        return Conditions.Count == 0;
    }
}
