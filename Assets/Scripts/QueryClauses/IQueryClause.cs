using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IQueryClause
{
    string DisplayName { get; }
    bool isClicked { get; }
    bool isAvailable { get; set; }
    void Toggle();
    string ToSQL();        
    string ToSupabase(); 
    void UpdateString();
    void Reset();
    void OnQueryUpdated(Query query); 
    List<object> GetOrderedElements();
}
