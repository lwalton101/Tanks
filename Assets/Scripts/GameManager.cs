using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Message message = new Message(PacketType.Spawn);
        message.WriteVector3(new Vector3(5.3412f, 3424.4f, 5.342f));
        message.WriteVector3(new Vector3(69, 420, 21));
        Debug.Log(message.ToString());
        Debug.Log(Message.FromString(message.ToString()).ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
