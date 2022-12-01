using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientInfoContainer : MonoBehaviour
{
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerIdText;

    private void Start()
    {
        SetClientInfoUI();
    }

    private void SetClientInfoUI()
    {
        var avatar = SteamFriends.GetLargeAvatarAsync(SteamClient.SteamId).Result;
        if (avatar != null)
        {
            Texture2D texture = avatar.Value.Covert();
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            avatarImage.sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        }

        playerNameText.text = SteamClient.Name;
        playerIdText.text = SteamClient.SteamId.Value.ToString();
    }
    
}
