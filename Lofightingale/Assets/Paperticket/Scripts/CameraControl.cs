﻿using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    [Header("References")]

    public Transform[] targets;             // All the targets the camera needs to encompass.
    Camera camera;                          // Used for referencing the camera.

    public Collider2D leftCollider;
    public Collider2D rightCollider;
    public Collider2D topCollider;
    public Collider2D bottomCollider;  

    
    [Header("Camera Controls")]

    public float dampTime = 0.2f;           // Approximate time for the camera to refocus.
    public float screenEdgeBuffer = 4f;     // Space between the top/bottom most target and the screen edge.
    public float minSize = 6.5f;            // The smallest orthographic size the camera can be.
    public float maxSize = 20f;            // The smallest orthographic size the camera can be.

    float zoomSpeed;                        // Reference speed for the smooth damping of the orthographic size.
    Vector3 moveVelocity;                   // Reference velocity for the smooth damping of the position.
    Vector3 desiredPosition;                // The position the camera is moving towards.


    [Header("Collider Controls")]

    public Vector2 leftColliderPos = new Vector2 (0, 0.5f);
    public Vector2 rightColliderPos = new Vector2(1, 0.5f);
    public Vector2 topColliderPos = new Vector2(0.5f, 1);
    public Vector2 bottomColliderPos = new Vector2(0.5f, 0);

       

    private void Awake() {

        camera = GetComponent<Camera>();

        if (!leftCollider || !rightCollider || !topCollider || !bottomCollider) {
            Debug.LogError("[CameraControl] A bounding collider missing from camera!");
        } 
    }


    private void FixedUpdate() {

        // Move the camera towards a desired position.
        Move();

        // Change the size of the camera based.
        Zoom();

        // Adjust the position of the bounding colliders
        Colliders();

    }


    private void Colliders() {

        leftCollider.transform.position = camera.ViewportToWorldPoint(new Vector3(leftColliderPos.x, leftColliderPos.y, camera.nearClipPlane));
        rightCollider.transform.position = camera.ViewportToWorldPoint(new Vector3(rightColliderPos.x, rightColliderPos.y, camera.nearClipPlane));
        topCollider.transform.position = camera.ViewportToWorldPoint(new Vector3(topColliderPos.x, topColliderPos.y, camera.nearClipPlane));
        bottomCollider.transform.position = camera.ViewportToWorldPoint(new Vector3(bottomColliderPos.x, bottomColliderPos.y, camera.nearClipPlane));

    }



    private void Move() {

        // Find the average position of the targets.
        FindAveragePosition();

        // Smoothly transition to that position.
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref moveVelocity, dampTime);

    }


    private void FindAveragePosition() {

        Vector3 averagePos = new Vector3();
        int numTargets = 0;

        // Go through all the targets and add their positions together.
        for (int i = 0; i < targets.Length; i++) {
            // If the target isn't active, go on to the next one.
            if (!targets[i].gameObject.activeSelf)
                continue;

            // Add to the average and increment the number of targets in the average.
            averagePos += targets[i].position;
            numTargets++;
        }

        // If there are targets divide the sum of the positions by the number of them to find the average.
        if (numTargets > 0)
            averagePos /= numTargets;

        // Keep the same y value.
        averagePos.z = transform.position.z;

        // The desired position is the average position;
        desiredPosition = averagePos;

    }


    private void Zoom() {

        // Find the required size based on the desired position and smoothly transition to that size.
        float requiredSize = FindRequiredSize();
        camera.orthographicSize = Mathf.SmoothDamp(camera.orthographicSize, requiredSize, ref zoomSpeed, dampTime);

    }


    private float FindRequiredSize() {

        // Find the position the camera rig is moving towards in its local space.
        Vector3 desiredLocalPos = transform.InverseTransformPoint(desiredPosition);

        // Start the camera's size calculation at zero.
        float size = 0f;

        // Go through all the targets...
        for (int i = 0; i < targets.Length; i++) {
            // ... and if they aren't active continue on to the next target.
            if (!targets[i].gameObject.activeSelf)
                continue;

            // Otherwise, find the position of the target in the camera's local space.
            Vector3 targetLocalPos = transform.InverseTransformPoint(targets[i].position);

            // Find the position of the target from the desired position of the camera's local space.
            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

            // Choose the largest out of the current size and the distance of the target 'up' or 'down' from the camera.
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

            // Choose the largest out of the current size and the calculated size based on the target being to the left or right of the camera.
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / camera.aspect);
        }

        // Add the edge buffer to the size.
        size += screenEdgeBuffer;

        // Make sure the camera's size isn't below the minimum.
        size = Mathf.Clamp(size, minSize, maxSize);                 // Mathf.Max(size, minSize);

        return size;

    }


    public void SetStartPositionAndSize() {

        // Find the desired position.
        FindAveragePosition();

        // Set the camera's position to the desired position without damping.
        transform.position = desiredPosition;

        // Find and set the required size of the camera.
        camera.orthographicSize = FindRequiredSize();

    }
}