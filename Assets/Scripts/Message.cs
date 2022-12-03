using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Message
{
    public PacketType packetType;
    private List<String> elements = new();
    
    public Message(PacketType packetType)
    {
        this.packetType = packetType;
    }

    public void WriteVector3(Vector3 vec3)
    {
        string vec3Element = "";
        vec3Element += $"Vector3({vec3.x}, {vec3.y}, {vec3.z})";
        elements.Add(vec3Element);
    }

    private static Vector3 ReadVector3(string vec3Element)
    {
        
        string onlyBraces = vec3Element.Replace("Vector3(", "").Replace(")", "").Replace("[", "").Replace("]", "");
        string[] splitComma = onlyBraces.Split(",");
        
        List<float> vec3Values = new();
        foreach (var element in splitComma)
        {
            vec3Values.Add(float.Parse(element));
        }
        
        return new Vector3(vec3Values[0], vec3Values[1], vec3Values[2]);
    }
    

    public override string ToString()
    {
        string overall = "";
        overall += $"{packetType}(";
        
        foreach (var element in elements)
        {
            overall += "[";
            overall += element;
            overall += "]";
            overall += "&";
        }
        
        overall = overall.Remove(overall.Length - 1, 1);
        overall += ")";
        return overall;
    }

    public static Message FromString(string messageString)
    {
        string packetTypeString = messageString.Split("(")[0];
        Enum.TryParse(packetTypeString, out PacketType packetType);

        Message message = new Message(packetType);
        
        string elementsString = messageString.Replace(packetTypeString, "").Remove(0,1);
        elementsString = elementsString.Remove(elementsString.Length - 1, 1);

        string[] elements = elementsString.Split("&");
        foreach (var element in elements)
        {
            string elementType = element.Remove(element.Length - 1, 0).Remove(0, 1).Split("(")[0];
            switch (elementType)
            {
                case "Vector3":
                    message.WriteVector3(ReadVector3(element));
                    break;
            }
        }
        

        return message;
    }
}
