using Assets.Scripts.ServerIntegration;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : Singleton<MenuManager>
{
    private Dictionary<eMenuType, IMenu> menus;

    public void Start()
    {
        menus = new();
        MenuBase[] allMenus = FindObjectsOfType<MenuBase>(true); 

        
        foreach (MenuBase menu in allMenus)
        {
            if (menus.ContainsKey(menu.MenuType))
            {
                Debug.LogWarning($"❗ Duplicate menu type: {menu.MenuType}");
                continue;
            }

            menus[menu.MenuType] = menu;
            menu.Hide();
        }

        ShowMenu(eMenuType.Main);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsMenuOpen(eMenuType.Main))
        {
            if (!IsMenuOpen(eMenuType.Pause))
                PauseGame();
            else
                ResumeGame();
        }
    }

    public void ShowMenu(eMenuType type)
    {
        if (type != eMenuType.Pause)
        {
            PopupManager.Instance.CloseAllPopups();
        }
        
        Time.timeScale = 0f;
        if (menus.TryGetValue(type, out IMenu menu))
        {
            menu.Show();
        }
    }

    public void HideMenu(eMenuType type)
    {
        // Time.timeScale = 1f;
        // if (menus.TryGetValue(type, out IMenu menu))
        // {
        //     menu.Hide();
        // }

        if (menus.TryGetValue(type, out IMenu menu))
        {
            menu.Hide();
        }

        bool anyMenuOpen = false;
        foreach (var kvp in menus)
        {
            if (((MonoBehaviour)kvp.Value).gameObject.activeSelf)
            {
                anyMenuOpen = true;
                break;
            }
        }

        Time.timeScale = anyMenuOpen ? 0f : 1f;

    }


    public T GetMenu<T>(eMenuType type) where T : class, IMenu
    {
        if (menus.TryGetValue(type, out IMenu menu))
        {
            return menu as T;
        }
        Debug.LogError($"❌ Menu of type {type} not found or wrong type.");
        return null;
    }


    public bool IsMenuOpen(eMenuType type)
    {
        return menus.TryGetValue(type, out IMenu menu) && ((MonoBehaviour)menu).gameObject.activeSelf;
    }

    public void PauseGame()
    {
        ShowMenu(eMenuType.Pause);
    }

    public void ResumeGame()
    {
        HideMenu(eMenuType.Pause);
    }

    public void QuitToMainMenu()
    {
        StartCoroutine(GameManager.Instance.resetAction());
        ShowMenu(eMenuType.Main);
    }
}
