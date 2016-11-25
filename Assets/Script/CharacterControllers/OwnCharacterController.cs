﻿using UnityEngine;
using System.Collections;

public class OwnCharacterController : MonoBehaviour {

    public float inputDelay = 0.1f; //perform better control with delay in input
    public float forwardVel = 0.8f;
    public float runVel = 1.6f;
    public float rotateVel = 100;   //determine how fast it turn
    public Animation anim;
    public string death_animation = "death",
        flip_animation = "flip",
        idle_animation = "idle",
        jump_animation = "jump",
        kick_animation = "kick",
        punch_animation = "punch",
        run_animation = "run",
        walk_animation = "walk";
    Vector3 Stand = new Vector3(0,0,0);

    Quaternion targetRotation;
    Rigidbody rBody;
    float forwardInput, turnInput;
    private bool canRun = false;
    public bool haveInput = false;
    private MazeCell currentCell;
    public Quaternion TargetRotation
    {
        get { return targetRotation; }
    }

    void Awake()
    {

        targetRotation = transform.rotation;
        if (GetComponent<Rigidbody>())
            rBody = GetComponent<Rigidbody>();
        else
            Debug.LogError("The Character dont have any rigidbody");

        forwardInput = turnInput = 0;

    }

    void GetInput()
    {
        forwardInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
        canRun = Input.GetKey("left shift");
        haveInput = Input.anyKey;

    }

    void Update()   //dont require physic
    {
        GetInput();
        Turn();
    }

    void FixedUpdate()  //need physic calculation
    {
        Move();
    }

    void Move()
    {
        if (Mathf.Abs(forwardInput) > inputDelay)
        {
            //move
            //transform.forward = rBody.
            rBody.velocity = transform.forward * forwardInput * forwardVel;

            if (canRun)
            {
                rBody.velocity = transform.forward * forwardInput * runVel;
                anim.CrossFade(run_animation);
            }
            else
            {
                rBody.velocity = transform.forward * forwardInput * forwardVel;
                anim.CrossFade(walk_animation);
            }
        }
        else
        {
            //dont move
            rBody.velocity = Vector3.zero;
            anim.CrossFade(idle_animation);
        }
    }

    void Turn()
    {   
		//find where to apply this statement
		transform.localEulerAngles = new Vector3 (Camera.main.transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);

		//targetRotation = Camera.main.transform.localEulerAngles.x;
		//transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation (rBody), rotateVel * 2.0f * Time.deltaTime);
		//transform.position += transform.rotation * Vector3.forward; 
        if(Mathf.Abs(turnInput) > inputDelay)
        {
            targetRotation *= Quaternion.AngleAxis(rotateVel * turnInput * Time.deltaTime, Vector3.up);
        }
		transform.rotation = targetRotation;
    }

    public void SetLocation(MazeCell cell)
    {
        currentCell = cell;
        transform.localPosition = cell.transform.localPosition;
    }
}
