using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text;

public class Query
{
    public string QueryString { get; set; } 
    public Table table { get; set; }
    public List<Column> Columns { get; set; } 
    public Dictionary<string, object> Conditions { get; set; }
    public List<Dictionary<string, string>> Results { get; set; } 
    public bool IsValid { get; set;}

    public Query()
    {
        Columns = new List<Column>();
        Conditions = new Dictionary<string, object>();
        Results = new List<Dictionary<string, string>>();
    }    
}
