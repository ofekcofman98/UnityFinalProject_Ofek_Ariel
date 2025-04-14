using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QueryPreviewManager : MonoBehaviour
{
    [SerializeField] public GameObject QueryPanel;
    [SerializeField] private Transform selectSection;
    [SerializeField] private Transform fromSection;
    [SerializeField] private Transform whereSection;
    public TextMeshProUGUI queryPreviewText;
    public Button executeButton;


    void Start()
    {
        QueryPanel.SetActive(false);

    }

    private Transform matchClauseToSection(IQueryClause i_Clause)
    {
        Transform section = selectSection;

        string name = i_Clause.DisplayName;
        if (name == QueryConstants.Select)
        {
            section = selectSection;
        }
        else if (name == QueryConstants.From)
        {
            section = fromSection;
        }
        else if (name == QueryConstants.Where)
        {
            section = whereSection;
        }
        
        return section;
    }
}
