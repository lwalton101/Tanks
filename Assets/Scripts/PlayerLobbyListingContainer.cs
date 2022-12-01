using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLobbyListingContainer : MonoBehaviour
{
    public Friend player;
    public Image AvatarImage;
    public TextMeshProUGUI PlayerNameObject;

    public void SetListingInfo()
    {
        var avatar = player.GetLargeAvatarAsync().Result;
        if (avatar != null)
        {
            Texture2D texture = avatar.Value.Covert();
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            AvatarImage.sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        }

        PlayerNameObject.text = player.Name;
        PlayerNameObject.color = SteamManager.Instance.playerReadyDict[player.Id] ? SteamManager.Instance.notReadyColor : SteamManager.Instance.readyColor;

    }
    
    
    
    
}
