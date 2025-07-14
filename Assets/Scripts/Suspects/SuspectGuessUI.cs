    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class SuspectGuessUI : MonoBehaviour
    {
        [SerializeField] private Transform suspectListContainer; // set this to Content inside Scroll View
        [SerializeField] private DataGridDisplayer dataGridDisplayer;
        [SerializeField] private GameObject guessButtonPrefab;
        [SerializeField] private Popup popup;

        private void OnEnable()
        {
            PopulateSuspects();
            SuspectsManager.Instance.OnSuspectsChanged += PopulateSuspects;
        }

        private void OnDisable()
        {
            if (SuspectsManager.HasInstance)
            {
                SuspectsManager.Instance.OnSuspectsChanged -= PopulateSuspects;
            }
        }

        public void Open()
        {
            popup.Open();
        }

        public void Close()
        {
            popup.Close();
        }

        private void PopulateSuspects()
        {
            List<SuspectData> suspects = SuspectsManager.Instance.Suspects;

            dataGridDisplayer.DisplayGrid<SuspectData>(
                new List<string> { "ID", "Full Name"},
                new List<float> { 60f, 150f},
                suspects,
                s => new List<string>
                {
                    s.Id,
                    s.FullName,
                    // string.IsNullOrEmpty(s.Description) ? "—" : s.Description
                },
                new List<IDataGridAction<SuspectData>>
                {
                    new GuessSuspectAction(),
                    new RemoveSuspectAction()
                }
            );
        }
    }
