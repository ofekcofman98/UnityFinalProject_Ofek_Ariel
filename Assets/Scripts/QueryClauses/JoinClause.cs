using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinClause : IQueryClause
{
    public string DisplayName => QueryConstants.Join;
    public string JoinPart { get; set; } = QueryConstants.Empty;
    public bool isClicked { get; private set; }
    public bool isAvailable { get; set; }

    private List<JoinEntry> joinEntries = new();

    public void Activate() => isClicked = true;
    public void Deactivate() { isClicked = false; joinEntries.Clear(); }

    public void AddJoin(Table toTable, Column fromCol, Column toCol)
    {
        joinEntries.Add(new JoinEntry(toTable, fromCol, toCol));
        UpdateString();
    }

    public string ToSQL()
    {
        return JoinPart;
    }

    public string ToSupabase()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateString()
    {
        throw new System.NotImplementedException();
    }

    public void Reset()
    {
        throw new System.NotImplementedException();
    }

    public bool CheckAvailableClause(Query query)
    {
        return isAvailable;
    } 

    public bool IsValid()
    {
        throw new System.NotImplementedException();
    }
}
