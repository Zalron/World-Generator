﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace WorldGenerator
{
    public class Player : MonoBehaviour
    {
        public bool IsGrounded;
        public bool IsSprinting;

        private Transform cam;
        private World world;

        public float walkSpeed = 3f;
        public float sprintSpeed = 6f;
        public float jumpForce = 5f;
        public float gravity = -9.8f;

        public float playerWidth = 0.15f;

        private float horizontal;
        private float vertical;
        private float mouseHorizontal;
        private float mouseVertical;
        private Vector3 velocity;
        private float verticalMomemtum = 0;
        private bool jumpRequest;

        public Transform PlaceHighlightBlock;
        public Transform DestroyHighlightBlock;
        public float checkIncrement = 0.1f;
        public float reach = 8f;

        public byte selectedBlockIndex = 1;
        private void Start()
        {
            cam = GameObject.Find("Main Camera").transform;
            world = GameObject.Find("World").GetComponent<World>();

            Cursor.lockState = CursorLockMode.Locked;
        }
        public void FixedUpdate()
        {
            CalculateVelocity();

            if (jumpRequest)
            {
                Jump();
            }

            transform.Rotate(Vector3.up * mouseHorizontal);
            cam.Rotate(Vector3.right * -mouseVertical);
            transform.Translate(velocity, Space.World);
        }
        public void Update()
        {
            GetPlayerInputs();
            PlaceCursorBlocks();
        }
        void Jump()
        {
            verticalMomemtum = jumpForce;
            IsGrounded = false;
            jumpRequest = false;
        }
        private void CalculateVelocity()
        {
            //Affect vertical momentum wit gravity
            if (verticalMomemtum > gravity)
            {
                verticalMomemtum += Time.fixedDeltaTime * gravity;
            }
            //Check sprinting, apply sprint multiplier if we are
            if (IsSprinting)
            {
                velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
            }
            else
            {
                velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
            }
            //Apply Vertical Momentum
            velocity += Vector3.up * verticalMomemtum * Time.fixedDeltaTime;
            if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            {
                velocity.z = 0;
            }
            if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            {
                velocity.x = 0;
            }
            if ((velocity.y < 0))
            {
                velocity.y = checkDownSpeed(velocity.y);
            }
            else if (velocity.y > 0)
            {
                velocity.y = checkUpSpeed(velocity.y);
            }
        }
        private void GetPlayerInputs()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            mouseHorizontal = Input.GetAxis("Mouse X");
            mouseVertical = Input.GetAxis("Mouse Y");
            if (Input.GetButtonDown("Sprint"))
            {
                IsSprinting = true;
            }
            if (Input.GetButtonUp("Sprint"))
            {
                IsSprinting = false;
            }
            if (IsGrounded && Input.GetButtonDown("Jump"))
            {
                jumpRequest = true;
            }
            if (DestroyHighlightBlock.gameObject.activeSelf)
            {
                // Destroying block 
                if (Input.GetMouseButtonDown(0))
                {
                    world.GetChunkFromVector3(DestroyHighlightBlock.position).EditBlock(DestroyHighlightBlock.position, 0);
                }
                // Place block 
                if (Input.GetMouseButtonDown(1))
                {
                    world.GetChunkFromVector3(PlaceHighlightBlock.position).EditBlock(PlaceHighlightBlock.position, selectedBlockIndex);
                }
            }
        }
        private void PlaceCursorBlocks()
        {
            float step = checkIncrement;
            Vector3 lastPos = new Vector3();
            while (step < reach)
            {
                Vector3 pos = cam.position + (cam.forward * step);
                if (world.CheckForSolidBlockInChunk(pos))
                {
                    DestroyHighlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                    PlaceHighlightBlock.position = lastPos;
                    DestroyHighlightBlock.gameObject.SetActive(true);
                    PlaceHighlightBlock.gameObject.SetActive(true);
                    return;
                }
                lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                step += checkIncrement;
            }
            PlaceHighlightBlock.gameObject.SetActive(false);
            DestroyHighlightBlock.gameObject.SetActive(false);
        }
        private float checkDownSpeed(float downSpeed)
        {
            if (world.CheckForSolidBlockInChunk(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
                world.CheckForSolidBlockInChunk(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
                world.CheckForSolidBlockInChunk(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
                world.CheckForSolidBlockInChunk(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)))
            {
                IsGrounded = true;
                return 0;
            }
            else
            {
                IsGrounded = false;
                return downSpeed;
            }
        }
        private float checkUpSpeed(float upSpeed)
        {
            if (world.CheckForSolidBlockInChunk(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
                world.CheckForSolidBlockInChunk(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
                world.CheckForSolidBlockInChunk(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) ||
                world.CheckForSolidBlockInChunk(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)))
            {
                return 0;
            }
            else
            {
                return upSpeed;
            }
        }
        public bool front
        {
            get
            {
                if (world.CheckForSolidBlockInChunk(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                    world.CheckForSolidBlockInChunk(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth)))
                {
                    return true;
                }
                else
                {
                    return false; 
                }
            }
        }
        public bool back
        {
            get
            {
                if (world.CheckForSolidBlockInChunk(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                    world.CheckForSolidBlockInChunk(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool left
        {
            get
            {
                if (world.CheckForSolidBlockInChunk(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                    world.CheckForSolidBlockInChunk(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool right
        {
            get
            {
                if (world.CheckForSolidBlockInChunk(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                    world.CheckForSolidBlockInChunk(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
