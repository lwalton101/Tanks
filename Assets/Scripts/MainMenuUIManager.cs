using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{
    
    public void CreateLobby()
    {
        SteamManager.Instance.CreateLobby();    
    }

    public void RefreshLobbies(GameObject contentPanel)
    {
        SteamManager.Instance.RefreshLobbies(contentPanel);
    }

}
