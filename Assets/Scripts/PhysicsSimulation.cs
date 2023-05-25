using System;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Pool;

public class PhysicsSimulation : MonoBehaviour
{
    private CustomCollider _prefab; 
    
    // Colliders in the scene
    private List<CustomCollider> _colliders;
    private List<PhysicsObject> _physicalObjects;

    // Initialize data
    private void Start()
    {
        _colliders = new List<CustomCollider>(FindObjectsOfType<CustomCollider>());
        _physicalObjects = new List<PhysicsObject>();
    }

    // Emits spheres, compute their position and velocity, and check for collisions
    private void FixedUpdate()
    {
        foreach (PhysicsObject obj in _physicalObjects) // For each sphere 
        {
            obj.ComputePhysics();
            // Collider[] results = Physics.OverlapSphere(obj.Position, .5f);
            foreach (PhysicsObject other in _physicalObjects) // For each collider 
            {
                if (obj == other) continue;
                // Check for and handle collisions
                OnCollision(obj, other);
            }
        }
    }

    public static bool OnCollision(PhysicsObject other, PhysicsObject self)
    {
        if (self == other) return false;
        
        Transform colliderTransform = self.Rigidbody.transform;
        Vector3 colliderSize = colliderTransform.lossyScale; // size of collider

        // Save current localScale value, and temporarily change the collider's
        // world scale to (1,1,1) for our calculations. (Don't modify this)
        Vector3 curLocalScale = colliderTransform.localScale;
        SetWorldScale(colliderTransform, Vector3.one);

        // Position and velocity of the ball in the the local frame of the collider
        Vector3 localPos = colliderTransform.InverseTransformPoint(other.Position);
        Vector3 localVelocity = colliderTransform.InverseTransformDirection(other.Velocity);

        float ballRadius = other.Scale / 2.0f;

        // TODO: In the following if conditions assign these variables appropriately.
        bool collisionOccurred = false;      // if the ball collides with the collider.
        bool isEntering = false;             // if the ball is moving towards the collider.
        Vector3 normal = Vector3.zero;       // normal of the colliding surface.

        if (self.Rigidbody.CompareTag("SphereCollider"))
        {
            normal = localPos.normalized;
            // Collision with a sphere collider
            float colliderRadius = colliderSize.x / 2f;  // We assume a sphere collider has the same x,y, and z scale values

            // TODO: Detect collision with a sphere collider.
            collisionOccurred = localPos.magnitude < colliderRadius + ballRadius;
            isEntering = localVelocity.y < 0;
        }
        else if (self.Rigidbody.CompareTag("PlaneCollider"))
        {
            // Collision with a plane collider
            var planeHeight = colliderSize.x * 10; // height of plane, defined by the x-scale
            var planeWidth = colliderSize.z * 10; // width of plane, defined by the z-scale
            // Note: In Unity, a plane's actual size is its inspector values times 10.

            Vector3 normalPt = new Vector3(localPos.x, 0, localPos.z);
            normalPt.x = Mathf.Clamp(normalPt.x, -planeHeight / 2, planeHeight / 2);
            normalPt.z = Mathf.Clamp(normalPt.z, -planeWidth / 2, planeWidth / 2);

            Vector3 dist = localPos - normalPt;
            normal = (dist).normalized;

            collisionOccurred = dist.magnitude < ballRadius;

            isEntering = localVelocity.y < 0;

            Debug.DrawLine(
                colliderTransform.TransformPoint(normalPt),
                colliderTransform.TransformPoint(localPos)
                );
            
            // Generally, when the sphere is moving on the plane, the restitution alone is not enough
            // to counter gravity and the ball will eventually sink. We solve this by ensuring that
            // the ball stays above the plane.
            if (collisionOccurred && isEntering)
            {
                // TODO: Follow these steps to ensure the sphere always on top of the plane.
                //   1. Find the new localPos of the ball that is always on the plane
                //   2. Convert the localPos to worldPos
                //   3. Update the sphere's position with the new value
                localPos += normal * (ballRadius - dist.magnitude);
                other.Rigidbody.position = colliderTransform.TransformPoint(localPos);
            }
        }

        if (collisionOccurred && isEntering)
        {
            // The sphere needs to bounce.
            // TODO: Update the sphere's velocity, remember to bring the velocity to world space
            Vector3 V_N = Vector3.Dot(normal, localVelocity) * normal;
            Vector3 V_T = localVelocity - V_N;
            Vector3 V = colliderTransform.TransformDirection(V_T + -V_N);
            other.Rigidbody.velocity = V;
        }


        colliderTransform.localScale = curLocalScale; // Revert the collider scale back to former value
        return collisionOccurred;
    }

    // Set the world scale of an object
    public static void SetWorldScale(Transform transform, Vector3 worldScale)
    {
        transform.localScale = Vector3.one;
        Vector3 lossyScale = transform.lossyScale;
        transform.localScale = new Vector3(worldScale.x / lossyScale.x, worldScale.y / lossyScale.y,
            worldScale.z / lossyScale.z);
    }
}
