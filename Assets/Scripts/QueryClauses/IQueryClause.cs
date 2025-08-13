using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IQueryClause
{
    string DisplayName { get; }
    bool isClicked { get; }
    bool isAvailable { get; set; }
    // void Toggle();
    void Activate();
    void Deactivate();
    string ToSQL();        
    string ToSupabase(); 
    void UpdateString();
    void Reset();
    bool IsValid();
    public bool CheckAvailableClause(Query query);
}
