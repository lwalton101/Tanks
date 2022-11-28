using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTank : Tank
{
    [SerializeField] private Transform trackingPoint;
    [SerializeField] private Camera mainCam;
    [SerializeField] private PlayerInput playerInput;


    Controls controls;
    
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        controls = new Controls();
        controls.Enable();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        Ray mouseWorldPosRay = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(mouseWorldPosRay, out hit)){
            trackingPoint.position = hit.point;
        }
        RotateTurretToPoint(trackingPoint.position);   



        float movementValue = controls.Movement.Movement.ReadValue<float>();
        float rotationValue = controls.Movement.Rotation.ReadValue<float>();
        bool shoot = controls.Movement.Shoot.ReadValue<float>() == 1;

        Move(movementValue, rotationValue);
        if(shoot){
            Shoot(BulletType.Normal, isPlayer: true);
        }
    
    }
    
}
