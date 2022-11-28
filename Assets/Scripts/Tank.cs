using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    public Transform bulletPos;
    public Transform turret;
    [SerializeField] private float speed = 30;
    [SerializeField] private float turretRotationSpeed = 20;
    private Rigidbody rb;
    private GameObject bullet;

    [SerializeField] private float rotationSpeed;

    [Tooltip("Shoot Cooldown in seconds")]
    [SerializeField] float maxShootCooldown = 5f;
    float shootCooldown = 0f;
    bool canShoot = true;
    

    private Vector3 _point = Vector3.zero;

    public virtual void Start(){
        rb = GetComponent<Rigidbody>();
        bullet = Resources.Load<GameObject>("Models/Bullet");
        shootCooldown = maxShootCooldown;
    }

    public virtual void Update(){
        if(shootCooldown < 0){
            shootCooldown = 0f;
            canShoot = true;
        } else{
            shootCooldown -= Time.deltaTime;
        }

    }

    public void Move(float movementInput, float rotationValue){
        transform.Rotate(new Vector3(0,rotationSpeed * rotationValue * Time.deltaTime, 0));
        transform.Translate(Vector3.forward * movementInput * speed * Time.deltaTime);
    }

    public void Shoot(BulletType bulletType, bool isPlayer = false){
        if(canShoot){
            GameObject bulletObject = Instantiate(bullet, bulletPos.position, turret.rotation);
            canShoot = false;
            shootCooldown = maxShootCooldown;
        }
    }

    public void RotateTurretToPoint(Vector3 point){
        _point = point;
        Quaternion lookRotation = Quaternion.LookRotation(point - turret.position).normalized;
        turret.rotation = Quaternion.Slerp(turret.rotation, lookRotation, Time.deltaTime * turretRotationSpeed);
        // turret.LookAt(point);
        ClampTurretRotation();
    }

    private void ClampTurretRotation(){
        turret.eulerAngles = new Vector3(0, turret.eulerAngles.y, 0);
    }

    private void OnDrawGizmos(){
        Gizmos.DrawLine(turret.position, _point);
    }
}
