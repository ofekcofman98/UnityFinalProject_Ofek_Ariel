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
    public const int k_MaxConditions = 2;
    // public int CurrentEditingConditionIndex { get; private set; } = -1;

    public int NewConditionIndex => Conditions.Count;
    public bool firstConditionWasRemoved = false;

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

    public bool CanStartAnotherCondition()
        => isClicked && (Conditions.Count < k_MaxConditions) && CompletedCondition(); // first is complete

    public bool HasActiveEditingCondition()
        => newCondition != null && !newCondition.IsComplete;


    public void StartNewCondition()
    {
        if (Conditions.Count >= k_MaxConditions)
        {
            Debug.Log("Max 2 conditions reached.");
            return;
        }

        // CurrentEditingConditionIndex = Conditions.Count; // 0 if it's first, 1 if second
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
        if (newCondition == null || !newCondition.IsComplete)
        {
            Debug.LogWarning("Cannot add condition: Condition or Value is null.");
            return;
        }
        if (Conditions.Count >= k_MaxConditions)
        {
            Debug.Log("Max 2 conditions reached.");
            newCondition = null; // drop it
            UpdateString();
            return;
        }

        Conditions.Add(newCondition);
        newCondition = null;
        // ResetIndex();
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
        if (!isClicked) return true;
        if (newCondition != null) return false;           // editing → not valid
        if (Conditions.Count == 0) return false;
        if (Conditions.Count == 1) return Conditions[0].IsComplete;
        if (Conditions.Count == 2) return Conditions[0].IsComplete && Conditions[1].IsComplete;

        return false;
    }


    public bool IsValidForConditionColumn(int conditionIndex)
    {
        if (!isClicked)
            return false;

        if (conditionIndex == 0)
        {
            return true;
        }

        if (conditionIndex == 1)
        {
            return GameManager.Instance.CurrentQuery.andClause.isClicked &&
                   Conditions.Count > 0 &&
                   Conditions[0].IsComplete &&
                   Conditions.Count <= k_MaxConditions;
        }

        return false;
    }

    public bool IsValidForOperator(int conditionIndex)
    {
        Condition cond = GetConditionByIndex(conditionIndex);

        // Debug.Log($"[IsValidForOperator] conditionIndex: {conditionIndex}");
        // Debug.Log($"[IsValidForOperator] cond?.Column != null: {cond?.Column != null}");
        if (cond?.Column != null)
        {
            // Debug.Log($"[IsValidForOperator] Column: {cond?.Column.Name}");
            return true;
        }
        return false;
    }

    public bool IsValidForValue(int conditionIndex)
    {
        Condition cond = GetConditionByIndex(conditionIndex);
        // Debug.Log($"[IsValidForValue] conditionIndex: {conditionIndex}");
        // Debug.Log($"[IsValidForValue] cond?.Operator != null: {cond?.Operator != null}");
        if (cond?.Operator != null)
        {
            // Debug.Log($"[IsValidForValue] Op: {cond?.Operator.GetSQLRepresentation()}");
            return true;
        }
        return false;
    }


    public void SetOperator(IOperatorStrategy i_operator)
    {
        Condition condition = FindLastCondition();

        if (condition != null)
        {
            condition.Operator = i_operator;
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

    private void clearConditions()
    {
        Conditions.Clear();
        newCondition = null;
    }

    private Condition GetConditionByIndex(int index)
    {
        if (index == -1 || index == Conditions.Count)
            return newCondition;

        if (index >= 0 && index < Conditions.Count)
            return Conditions[index];

        return null;
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

    public bool CompletedCondition()
    {
        if (newCondition != null) return false;
        if (Conditions.Count == 0) return false;
        return Conditions.Last().IsComplete;

        // return false;
    }

    internal int GetConditionIndexByColumn(Column col)
    {
        for (int i = 0; i < Conditions.Count; i++)
        {
            if (Conditions[i].Column == col)
            {
                // Debug.Log($"[GetConditionIndexByColumn]: {i}");
                return i;
            }
        }

        if (newCondition?.Column == col)
        {
            // Debug.Log($"[GetConditionIndexByColumn]: {-1}");
            return -1; // Special index for newCondition
        }

        // Debug.Log($"[GetConditionIndexByColumn]: {-2}");
        return -2; // Not found
    }

    public void RemoveSecondCondition()
    {
        // Debug.Log($"[RemoveSecondCondition] Conditions.Count = {Conditions.Count}");
        if (Conditions.Count > 1)
        {
            RemoveConditionByIndex(1);
            // Debug.Log($"[RemoveSecondCondition] removed the sceond condition");
        }
        // Case 2: In-progress (newCondition is being edited as second)
        else if (newCondition != null && NewConditionIndex == 1)
        {
            // Debug.Log($"[RemoveSecondCondition] im here");
            RemoveConditionByIndex(-1);
        }

        // Debug.Log($"[RemoveSecondCondition] Conditions.Count: {Conditions.Count}]");

        if (newCondition == null)
        {
            newCondition = Conditions.FirstOrDefault();
        }
    }

    public void RemoveConditionByIndex(int conditionIndex)
    {
        if (conditionIndex == -1)
        {
            // Debug.Log("[RemoveConditionByIndex]: Removing newCondition (-1)");
            newCondition = null;
            return;
        }

        if (conditionIndex == 0)
        {
            // Debug.Log("[RemoveConditionByIndex]: Removing first condition (0)");
            firstConditionWasRemoved = true;

            if (Conditions.Count == 0)
            {
                // Debug.Log("[RemoveConditionByIndex]: Conditions.Count == 0");
                if (newCondition != null)
                {
                    // Debug.Log("[RemoveConditionByIndex]: newCondition != null");
                    newCondition = null;
                }
            }
            else if (Conditions.Count == 1)
            {
                // Debug.Log("[RemoveConditionByIndex]: Conditions.Count == 1");
                Conditions.RemoveAt(0);

                if (newCondition != null)
                {
                    // Debug.Log("[RemoveConditionByIndex]: newCondition != null");
                    newCondition = null;
                }
            }
            else if (Conditions.Count == 2)
            {
                // Debug.Log("[RemoveConditionByIndex]: Conditions.Count == 2");
                Conditions.Clear();
            }

            return;
        }
        else if (conditionIndex == 1)
        {
            // Debug.Log("[RemoveConditionByIndex]: Removing second condition (1)");
            if (Conditions.Count == 1)
            {
                // Debug.Log("[RemoveConditionByIndex]: its probably newCondition");
                if (newCondition != null)
                {
                    // Debug.Log("[RemoveConditionByIndex]: newCondition != null");
                    newCondition = null;
                }
            }
            else if (Conditions.Count == 2)
            {
                // Debug.Log("[RemoveConditionByIndex]: they're both completed");
                Conditions.RemoveAt(1);
            }

            return;
        }

        // Debug.Log("[RemoveConditionByIndex]: wtf");
    }


    public void RemoveOperatorByIndex(int index)
    {
        Condition old;
        if (index == -1)
        {
            // Debug.Log("[RemoveConditionByIndex]: Removing newCondition (-1)");
            newCondition = null;
            return;
        }

        if (index == 0)
        {
            // Debug.Log("[RemoveConditionByIndex]: Removing first condition (0)");
            firstConditionWasRemoved = true;

            if (Conditions.Count == 0)
            {
                // Debug.Log("[RemoveConditionByIndex]: Conditions.Count == 0");
                if (newCondition != null)
                {
                    // Debug.Log("[RemoveConditionByIndex]: newCondition != null");
                    old = newCondition;
                    CreateNewCondition(old.Column);
                }
            }
            else if (Conditions.Count == 1)
            {
                // Debug.Log("[RemoveConditionByIndex]: Conditions.Count == 1");

                old = Conditions[0];
                Conditions.RemoveAt(index);
                CreateNewCondition(old.Column);

                if (newCondition != null)
                {
                    // Debug.Log("[RemoveConditionByIndex]: newCondition != null");
                    newCondition = null; // removing the second condition 
                    CreateNewCondition(old.Column); // making the first condition -> newCondition
                }
            }
            else if (Conditions.Count == 2)
            {
                // Debug.Log("[RemoveConditionByIndex]: Conditions.Count == 2");
                old = Conditions[0];
                Conditions.RemoveAt(1);
                Conditions.RemoveAt(0);
                CreateNewCondition(old.Column);
            }

            return;
        }
        else if (index == 1) // second condition
        {
            // Debug.Log("[RemoveConditionByIndex]: Removing second condition (1)");
            if (Conditions.Count == 1)
            {
                // Debug.Log("[RemoveConditionByIndex]: its probably newCondition");
                if (newCondition != null)
                {
                    // Debug.Log("[RemoveConditionByIndex]: newCondition != null");
                    old = newCondition;
                    CreateNewCondition(old.Column);
                }
            }
            else if (Conditions.Count == 2)
            {
                // Debug.Log("[RemoveConditionByIndex]: they're both completed");
                old = Conditions[1];
                Conditions.RemoveAt(1);
                CreateNewCondition(old.Column);
            }

            return;
        }

        // Debug.Log("[RemoveConditionByIndex]: wtf");
    }


    public void RemoveValueByIndex(int index)
    {
        Condition old;
        if (index == -1)
        {
            // Debug.Log("[RemoveConditionByIndex]: Removing newCondition (-1)");
            newCondition = null;
            return;
        }

        if (index == 0)
        {
            // Debug.Log("[RemoveConditionByIndex]: Removing first condition (0)");
            firstConditionWasRemoved = true;

            if (Conditions.Count == 0)
            {
                // Debug.Log("[RemoveConditionByIndex]: Conditions.Count == 0");
                if (newCondition != null)
                {
                    // Debug.Log("[RemoveConditionByIndex]: newCondition != null");
                    old = newCondition;
                    CreateNewCondition(old.Column);
                    SetOperator(old.Operator);
                }
            }
            else if (Conditions.Count == 1)
            {
                // Debug.Log("[RemoveConditionByIndex]: Conditions.Count == 1");

                old = Conditions[0];
                Conditions.RemoveAt(index);
                CreateNewCondition(old.Column);
                SetOperator(old.Operator);

                if (newCondition != null)
                {
                    // Debug.Log("[RemoveConditionByIndex]: newCondition != null");
                    newCondition = null; // removing the second condition 
                    CreateNewCondition(old.Column); // making the first condition -> newCondition
                    SetOperator(old.Operator);
                }
            }
            else if (Conditions.Count == 2)
            {
                // Debug.Log("[RemoveConditionByIndex]: Conditions.Count == 2");
                old = Conditions[0];
                Conditions.RemoveAt(1);
                Conditions.RemoveAt(0);
                CreateNewCondition(old.Column);
                SetOperator(old.Operator);
            }

            return;
        }
        else if (index == 1) // second condition
        {
            // Debug.Log("[RemoveConditionByIndex]: Removing second condition (1)");
            if (Conditions.Count == 1)
            {
                // Debug.Log("[RemoveConditionByIndex]: its probably newCondition");
                if (newCondition != null)
                {
                    // Debug.Log("[RemoveConditionByIndex]: newCondition != null");
                    old = newCondition;
                    CreateNewCondition(old.Column);
                    SetOperator(old.Operator);
                }
            }
            else if (Conditions.Count == 2)
            {
                // Debug.Log("[RemoveConditionByIndex]: they're both completed");
                old = Conditions[1];
                Conditions.RemoveAt(1);
                CreateNewCondition(old.Column);
                SetOperator(old.Operator);
            }

            return;
        }

        // Debug.Log("[RemoveConditionByIndex]: wtf");
    }

}
