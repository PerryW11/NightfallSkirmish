﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerBehavior : MonoBehaviour
{
    public float moveSpeed = 4f;
    private float gravity = 10f;

    public GameObject bullet;
    public float bulletSpeed = 60f;

    private float vInput;
    private float hInput;
    private Vector3 moveDir;
    private Quaternion targetRotation;

    public float timeInvincible = 2f;
    public Animator charAnim;

    public AudioSource audWeaponFire;

    //private Rigidbody _rb;
    private CapsuleCollider col;
    private CharacterController charCon;
    public GameBehavior gameManager;
    public CameraFollow cam;

    public Transform spine;
    public GameObject target;
    public Vector3 offset;
    public bool IsIdle;


    public Quaternion TargetRotation
    {
        get { return targetRotation; }
    }

    public CharacterController CharacterControl
    {
        get { return charCon; }
    }

    private void Start()
    {
       // _rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        charCon = GetComponent<CharacterController>();
        targetRotation = transform.rotation;
        gameManager = FindObjectOfType<GameBehavior>();
        spine = charAnim.GetBoneTransform(HumanBodyBones.Spine);
        if(charAnim.isHuman)
        {
            Debug.Log("Is humanoid");
        }
       // gameManager = GameObject.Find("Game Manager").GetComponent<GameBehavior>();
    }


    private void Update()
    {
        vInput = Input.GetAxis("Vertical") * moveSpeed;
        hInput = Input.GetAxis("Horizontal") * moveSpeed;
        IsIdle = hInput == 0 && vInput == 0 ? true : false;
        //targetRotation *= Quaternion.AngleAxis(hInput * Time.deltaTime, Vector3.up);
        if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            Vector3 eulerRotation = new Vector3(transform.eulerAngles.x, cam.transform.eulerAngles.y, transform.eulerAngles.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(eulerRotation), Time.deltaTime * 8f); // Lerp player model to camera's rotation
        }
        DetectMovement();
        DetectFire();
        
    }
    private void LateUpdate()
    {
        if (Input.GetMouseButton(1) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            spine.LookAt(cam.target.transform.position);
            spine.rotation *= Quaternion.Euler(offset);
        }
    }



    private void DetectMovement()
    {
        if(Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed *= 1.1f;
            charAnim.SetFloat("PosX", Input.GetAxis("Horizontal"));
            charAnim.SetFloat("PosY", Input.GetAxis("Vertical"));

        }
        else
        {
            moveSpeed /= 1.1f;
            charAnim.SetFloat("PosX", Input.GetAxis("Horizontal") / 2);
            charAnim.SetFloat("PosY", Input.GetAxis("Vertical") / 2);
        }
        moveSpeed = Mathf.Clamp(moveSpeed, 4f, 7f);

        
        



        if (charCon.isGrounded)
        {
            moveDir = new Vector3(hInput, 0, vInput);
            moveDir = transform.TransformDirection(moveDir);
            if (Input.GetKeyDown(KeyCode.Space))
            {
                moveDir.y = gameManager.jumpVelocity;
            }
            else
            {
                moveDir.y = -0.5f;
            }
        }
        else
        {
            moveDir = new Vector3(hInput, moveDir.y, vInput);
            moveDir = transform.TransformDirection(moveDir);
            moveDir.y -= gravity* Time.deltaTime;
        }
        charCon.Move(moveDir* Time.deltaTime);
    }
    

    private void DetectFire()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !gameManager.GamePaused)
        {
            GameObject newBullet;
            audWeaponFire.Play();
            if(IsIdle)
            {
                newBullet = Instantiate(bullet, spine.transform.position + (spine.transform.forward * 1.2f) + (spine.transform.up * 0.25f) + (spine.transform.right * -0.7f), spine.transform.rotation) as GameObject;
            }
            else
            {
                newBullet = Instantiate(bullet, spine.transform.position + (spine.transform.forward * 1.2f) + (spine.transform.up * 0.25f) + (spine.transform.right * -0.7f), spine.transform.rotation) as GameObject;
            }
           
           
            Rigidbody bulletRB = newBullet.GetComponent<Rigidbody>();
            if(Input.GetMouseButton(1) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
            {
                bulletRB.velocity = cam.transform.forward * bulletSpeed; // Shoot in camera's direction
            }
            else
            {
                bulletRB.velocity = transform.forward * bulletSpeed; // Shoot in player model's direction
            }
            
            // If enemy is within this sphere, it will hear the gun shot and look for the player to attack
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 100);
            int i = 0;
            while(i < hitColliders.Length)
            {
                Collider col = hitColliders[i];
                if(col.gameObject.tag == "Enemy")
                {
                    col.GetComponent<EnemyBehavior>().HandlePlayerSight();
                }
                i++;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!gameManager.playerInvincible && collision.gameObject.CompareTag("Enemy"))
        {
            StartCoroutine(TempInvincibility());
        }
    }

    public void CallTempInvincibility()
    {
        StartCoroutine(TempInvincibility());
    }

    IEnumerator TempInvincibility()
    {
        gameManager.playerInvincible = true;
        MeshRenderer mr = GetComponent<MeshRenderer>();
        float timeStart = Time.time;
        while(Time.time - timeStart < timeInvincible)
        {
            mr.enabled = !mr.enabled;
            yield return new WaitForSeconds(0.2f);
            mr.enabled = !mr.enabled;
            yield return new WaitForSeconds(0.08f);
        }
        mr.enabled = false;
        gameManager.playerInvincible = false;
    }
}
