using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New SQL Mission", menuName = "SQL Detective/Mission/SQL Mission")]
public class SQLMissionData : MissionData
{
    public string requiredTable;
    public List<string> requiredColumns;
    public string requiredCondition;

    public string expectedPrimaryKeyField;
    public string expectedRowIdValue;

    private Query _query;
    private JArray _result;
    private QueryValidator _validator;

    public void SetValidationContext(Query query, JArray result, QueryValidator validator)
    {
        _query = query;
        _result = result;
        _validator = validator;
    }

    public override bool Validate()
    {
        return _validator.ValidateQuery(_query, _result, this);
    }

    public override void ShowUI(MissionUIManager uiManager)
    {
        uiManager.DisplayStandardMission(this);
    }

}
