using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private BulletType bulletType = BulletType.Normal;
    [SerializeField] private float speed = 30f;

    // Start is called before the first frame update
    void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch(bulletType){
            case BulletType.Normal:
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
                break;
            default:
                break;
        }
    }
}
