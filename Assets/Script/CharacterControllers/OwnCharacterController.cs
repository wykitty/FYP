﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class OwnCharacterController : MonoBehaviour {
    private GameMaster GM;
    private CharacterMotor motor;
    public float inputDelay = 0.1f; //perform better control with delay in input
    public float forwardVel = 0.8f;
    public float runVel = 1.6f;
    public float horizontalVel = 0.6f;
    public Animation anim;
	public int wallTransSkillTime = 3;
    public string death_animation = "death",
        flip_animation = "flip",
        idle_animation = "idle",
        jump_animation = "jump",
        kick_animation = "kick",
        punch_animation = "punch",
        run_animation = "run",
        walk_animation = "walk";
    Vector3 Stand = new Vector3(0,0,0);

	public float sensitivityX = 100f;
	private float rotationX = 0F;
	private float mouseX = 0F;
	private float oldMouseX = 0F;
	private float mouseDelta = 0F;
	private float rotationY = 0F;
	private float finalRotate = 0F;
	float turn = 0F;
	private static bool loggedInputInfo = false;
	public float turnmin = -90.0f;
	public float turnmax = 90.0f;
    private Vector3 prevMaxLocation = new Vector3(0,0,0);
    Vector3 leftright;
	Quaternion originalRotation;
    Quaternion targetRotation;
    Rigidbody rBody;
    float forwardInput, turnInput, directionInput, horizontalInput;
	bool flipInput;
	bool transSkillInput;
    private bool canRun = false;    //store flag to determine run
    private bool haveInput = false;
    private bool isDead = false;
	private bool openDoor = false;
    private MazeCell currentCell;
	private MazeCell currentCellPos;
	private MazeCell cell_arr;
	private Transform target;
	private Maze mazeInstance;
	private MazeWall wallInstance;
	private MazeWall wall;
	private MazeDoor door;
	private MazeDoor doorInstance;
	private MazeDoor doorPrefab;
    /*---------------time control for transkill-----------------*/
    private bool canUseTranSkill = true;
    private int skillCD = 3;
    private int skillPersist = 3;
    /*--------------------------------*/
    float x = 0.0f;
	float y = 0.0f;

    private bool open_chest = false;


    public Quaternion TargetRotation
    {
        get { return targetRotation; }
    }

    void Start()
    {

    }

    void Awake()
    {
        Cursor.visible = false;
        GM = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
        skillCD = GM.tranSkillCD;
        skillPersist = GM.tranSkillPersist;
        Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;
		originalRotation = transform.localRotation;
        targetRotation = transform.rotation;
		mazeInstance = FindObjectOfType (typeof(Maze)) as Maze;
        if (mazeInstance == null) mazeInstance = GetComponent(typeof(Maze)) as Maze;
		mazeInstance = GameObject.FindObjectOfType (typeof(Maze)) as Maze;
		if (wall == null) wall = GameObject.FindObjectOfType(typeof(MazeWall)) as MazeWall;
        if (GetComponent<Rigidbody>())
            rBody = GetComponent<Rigidbody>();
        else
            Debug.LogError("The Character dont have any rigidbody");
        forwardInput = turnInput  = horizontalInput = 0;
    }

    void GetInput()
    {
        haveInput = Input.anyKey;
        forwardInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
        canRun = Input.GetKey("left shift");
        flipInput = Input.GetKey("space");
        mouseX = Input.GetAxis("Mouse X");
        transSkillInput = (Input.GetKey(KeyCode.E)) ? true : false;
        openDoor = (Input.GetKey(KeyCode.Q)) ? true : false;
        open_chest = (Input.GetKey(KeyCode.F)) ? true : false;
    }

    void Update()   //dont require physic
    {
        wallTransSkill();

    }

    void FixedUpdate()  //need physic calculation
    {
        oldMouseX = mouseX;
        GetInput();
        if (!isDead)
        {
            Move();
            if (Mathf.Abs(mouseX - oldMouseX) > 0)
            {
                rotationX += mouseX * sensitivityX;
            }
            else
            {
                rotationX = 0;
            }
            //Convert Mouse X rotation into Quaternion
            Quaternion rotation = Quaternion.Euler(0, rotationX, 0);
            turnInput = rotationX;
            //turn = turnInput;
            turn = turnInput * Time.deltaTime;
            turn = Mathf.Clamp(turn, turnmin, turnmax);

            //Rotate the player centering at it's Y-axis
            targetRotation *= Quaternion.AngleAxis(turn, Vector3.up);
            targetRotation *= Quaternion.AngleAxis(0, Vector3.right);
            targetRotation *= Quaternion.AngleAxis(0, Vector3.forward);
            transform.rotation = targetRotation;
            //Flip animation Input
            if (flipInput) anim.CrossFade(flip_animation);
            if (openDoor) checkCellHvDoor(T2IntVector2());
            if (open_chest) GM.control_chest();

        }


    }


	//Forward input, with walk, run or idle animation
    void Move()
    {
        if (Mathf.Abs(horizontalInput) > inputDelay && Mathf.Abs(forwardInput) > inputDelay)
        {
            if (canRun)
            {
                rBody.velocity = transform.right * horizontalInput * horizontalVel + transform.forward * forwardInput * runVel;
                if(forwardInput <0) rBody.velocity = transform.right * horizontalInput * horizontalVel + transform.forward * forwardInput * forwardVel;
                anim.CrossFade(run_animation);
            }
            else
            {
                rBody.velocity = transform.right * horizontalInput * horizontalVel + transform.forward * forwardInput * forwardVel;
                anim.CrossFade(walk_animation);
            }
            prevMaxLocation = rBody.position;
        }
        else if (Mathf.Abs(horizontalInput) > inputDelay)
        {
            rBody.velocity = transform.right * horizontalInput * horizontalVel;
            anim.CrossFade(walk_animation);
            prevMaxLocation = rBody.position;
        }
        else if (Mathf.Abs(forwardInput) > inputDelay)
        {

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
            prevMaxLocation = rBody.position;
        }
        else
        {
            //dont move
            rBody.velocity = Vector3.zero;
            if(prevMaxLocation != Vector3.zero)
                rBody.MovePosition(prevMaxLocation);
            anim.CrossFade(idle_animation);
        }
    }

	//Death animation
    public void Death()
    {
        rBody.velocity = Vector3.zero;
        anim.CrossFade(death_animation);
        isDead = true;
    }

	//Clamp Angles
	public static float ClampAngle(float angle, float min, float max){
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp(angle, min, max);
	}

	//Set at Maze location
	public void SetLocation(MazeCell cell){
		currentCell = cell;
		transform.localPosition = cell.transform.localPosition;
	}

	public IntVector2 T2IntVector2(){
		Vector3 pos = GameObject.FindGameObjectWithTag ("Player").transform.position;
		IntVector2 intVectorVar;
        //Math.Round(2.5, MidpointRounding.AwayFromZero) ==> 3
        intVectorVar.x = (int)Math.Round(pos.x + 9.5, MidpointRounding.AwayFromZero);
		intVectorVar.z = (int)Math.Round(pos.z + 9.5, MidpointRounding.AwayFromZero);
		return intVectorVar;
	}

	public void checkCellHvDoor(IntVector2 posIntVectorVar){
        GM.openCellDoor(posIntVectorVar);

    }


	public void wallTransSkill(){

		if (transSkillInput&& canUseTranSkill) {
			         //GM.walltransAll();

            mazeInstance.transSkills();
            Invoke("SkillCD", skillPersist);
            canUseTranSkill = false;
        }
	}

    private void SkillCD()
    {
        mazeInstance.revertTransSkills();
        canUseTranSkill = true;
    }


}
