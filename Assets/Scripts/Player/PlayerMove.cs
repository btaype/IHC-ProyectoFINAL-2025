using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;


    private float[] lanes = new float[] { -5f, 0f, 5f }; 
    private int currentLane = 1; 
    private float laneChangeSpeed = 10f; 
    private bool isMoving = false;


    private bool movingFoward = false;
    void Update()
    {



        if (Input.GetKey(KeyCode.Space))
        {
            movingFoward = true;
        }

        if (movingFoward)
        {
            transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed, Space.World);
        }
        
        Vector3 targetPosition = new Vector3(lanes[currentLane], transform.position.y, transform.position.z);

        
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * laneChangeSpeed);

        
        if (!isMoving)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ChangeLane(-1); 
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                ChangeLane(1); 
            }
        }

        
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            isMoving = false;
            transform.position = targetPosition; 
        }
    }

    void ChangeLane(int direction)
    {
        
        int newLane = currentLane + direction;

        
        if (newLane >= 0 && newLane < lanes.Length)
        {
            currentLane = newLane; 
            isMoving = true; 
        }
    }
}