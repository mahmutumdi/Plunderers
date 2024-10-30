using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float followDistance;
    [SerializeField] private float heightOffset;
    [SerializeField] private Transform cameraHolder;
    
    [Header("Movement Settings")]
    [SerializeField] private float mouseSensitivity;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float smoothTime;
    
    float verticalLookRotation;
    bool grounded;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;
    Rigidbody pRigidbody;
    private PhotonView photonView;
    PlayerInventory playerInventory;
    private Animator animator;

    private void Awake()
    {
        pRigidbody = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        playerInventory = GetComponent<PlayerInventory>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (!photonView.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(pRigidbody);
        }
    }
    private void Update()
    {
        if(!photonView.IsMine || !MultiplayerGameManager.canAct)
            return;
        
        Look();
        Move();
        Jump();
        UpdateAnimationStates();
    }
    
    private void UpdateAnimationStates()
    {
        float horizontalSpeed = new Vector3(pRigidbody.velocity.x, 0, pRigidbody.velocity.z).magnitude;

        // if the "E" key is pressed
        // if (Input.GetKey(KeyCode.E) && grounded)
        // {
        //     PlayAnimation("Lift");
        // }
        if (horizontalSpeed > 0.1f && Input.GetKey(KeyCode.LeftShift) && grounded)
        {
            PlayAnimation("Sprint");
        }
        else if (horizontalSpeed > 0.1f && grounded)
        {
            PlayAnimation("Walk");
        }
        else if (horizontalSpeed < 0.1f && grounded)
        {
            PlayAnimation("Idle");
        }
        else if (!grounded)
        {
            PlayAnimation("Jump");
        }
    }

    
    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up, mouseX);

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -20f, 60f); // Limit to avoid weird angles
        cameraTransform.localEulerAngles = Vector3.right * verticalLookRotation;

        Vector3 targetPosition = transform.position - transform.forward * followDistance + Vector3.up * heightOffset;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, smoothTime);
        cameraTransform.LookAt(transform.position + Vector3.up * 1.5f);
    }
    
    void Move()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        if (moveDirection.magnitude > 0)
        {
            Vector3 relativeDirection = transform.TransformDirection(moveDirection);
            
            float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
            moveAmount = Vector3.SmoothDamp(moveAmount, relativeDirection * speed, ref smoothMoveVelocity, smoothTime);
            
            Quaternion targetRotation = Quaternion.LookRotation(relativeDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothTime);
        }
        else
        {
            moveAmount = Vector3.zero;
        }
    }

    void Jump()
    {
        if (Input.GetKey(KeyCode.Space) && grounded)
        {
            pRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            grounded = false;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Wood"))
        {
            CollectibleWood wood = other.GetComponent<CollectibleWood>();
            playerInventory.CollectWood(wood.GetWoodPoints());
            
            PhotonView woodPhotonView = other.GetComponent<PhotonView>();
            if (woodPhotonView != null && woodPhotonView.IsMine)
            {
                PhotonNetwork.Destroy(other.gameObject);
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
    }
    
    public void SetGrounded(bool _grounded)
    {
        grounded = _grounded;
    }

    private void FixedUpdate()
    {
        if(!photonView.IsMine)
            return;
        
        pRigidbody.velocity = new Vector3(moveAmount.x, pRigidbody.velocity.y, moveAmount.z);
    }
    
    private void PlayAnimation(string animationName)
    {
        animator.Play(animationName);
    }
}