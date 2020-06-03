using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct CollisionHit
{
    public Vector3 impactVelocity;
    public Collider collider;
    public Vector3 point;
    public Vector3 normal;
    public float distance;
    public Transform transform;

    public CollisionHit(Vector3 impactVelocity, RaycastHit hit)
    {
        this.impactVelocity = impactVelocity;
        collider = hit.collider;
        point = hit.point;
        normal = hit.normal;
        distance = hit.distance;
        transform = hit.transform;
    }
}

public static class PhysicsHelper
{
    public static List<CollisionHit> PreventCollision(Func<RaycastHit> raycastFunction, ref Vector3 velocity, Transform transform, float DeltaTime, float skinWidth = 0.0f, float bounciness = 0.0f)
    {
        RaycastHit hit;
        List<CollisionHit> collisionHits = new List<CollisionHit>();
        int iterator = 0;
        while ((hit = raycastFunction()).collider != null)
        {
            float distanceToCorrect = skinWidth / Vector3.Dot(velocity.normalized, hit.normal);
            float distanceToMove = hit.distance + distanceToCorrect;

            if (distanceToMove <= velocity.magnitude * DeltaTime)
            {
                collisionHits.Add(new CollisionHit(velocity, hit));
                if (distanceToMove > 0.0f)
                    transform.position += velocity.normalized * distanceToMove;
                velocity += CalculateNormalForce(hit.normal, velocity) * (1.0f + bounciness);
            }
            else
                break;
            
            //Escapes
            if(velocity.magnitude <= 0.001f || iterator > 100)
            {
                velocity = Vector3.zero;
                break;
            }
            iterator++;
        }
        return collisionHits;
    }
    public static Vector3 CalculateNormalForce(Vector3 normal, Vector3 velocity)
    {
        float dot = Vector3.Dot(velocity, normal.normalized);
        return -normal.normalized * (dot > 0.0f ? 0 : dot);
    }

    /// <summary>
    /// Test all collider inside of a cone with a defined base and angle size.
    /// </summary>
    /// <param name="Origin">The place of origin for the cone.</param>
    /// <param name="Direction">Direction of the cone.</param>
    /// <param name="LengthRadius">Length of the cone from the origin.</param>
    /// <param name="Angle">Angle of the cone in degrees.</param>
    /// <param name="BaseRadius">Size of the closest part of the cone to the origin.</param>
    /// <param name="BaseOffsetFactor"> Describes how close the base of the cone is to the Origin, 1 will make origin and base touch, while 0 makes it have the max space between them.</param>
    /// <returns></returns>
    public static List<Collider> ConeOverlapAll(Vector3 Origin, Vector3 Direction, float LengthRadius, float Angle, float BaseRadius, LayerMask layermask, float BaseOffsetFactor)
    {
        //Math
        float innerCircleRadius = (BaseRadius / (Mathf.PI * 2f)) * (Angle / 360f);
        float OuterCircleRadius = innerCircleRadius + LengthRadius;
        Vector3 OuterCirlceOrigin = Origin - Direction.normalized * (innerCircleRadius * BaseOffsetFactor);
        //collision
        Collider[] allCollider = Physics.OverlapSphere(OuterCirlceOrigin, OuterCircleRadius, layermask);
        //Cone cutout
        List<Collider> allActualHits = new List<Collider>();
        foreach (Collider c in allCollider)
        {
            float dot = 1f - ((Vector3.Dot(Direction.normalized, (c.transform.position - OuterCirlceOrigin).normalized) + 1f) / 2f);
            if (Vector3.Distance(c.transform.position, OuterCirlceOrigin) > innerCircleRadius && dot < (Angle / 360f))
                allActualHits.Add(c);
        }
        return allActualHits;
    }
    /// <summary>
    /// Test all collider inside of a cone with a defined base and angle size. Ignoring hits behind ViewBlockedLayerMask.
    /// </summary>
    /// <param name="Origin">The place of origin for the cone.</param>
    /// <param name="Direction">Direction of the cone.</param>
    /// <param name="LengthRadius">Length of the cone from the origin.</param>
    /// <param name="Angle">Angle of the cone in degrees.</param>
    /// <param name="BaseRadius">Size of the closest part of the cone to the origin.</param>
    /// <param name="BaseOffsetFactor"> Describes how close the base of the cone is to the Origin, 1 will make origin and base touch, while 0 makes it have the max space between them.</param>
    /// <returns></returns>
    public static List<Collider> ConeOverlapAllInSight(Vector3 Origin, Vector3 Direction, float LengthRadius, float Angle, float BaseRadius, LayerMask layermask, LayerMask ViewBlockLayerMask, float BaseOffsetFactor)
    {
        List<Collider> hits = ConeOverlapAll(Origin, Direction, Angle, LengthRadius, BaseRadius, layermask, BaseOffsetFactor);
        List<Collider> CulledHits = new List<Collider>();
        foreach(Collider hit in hits)
        {
            if (!Physics.Raycast(new Ray(Origin, Direction), Vector3.Distance(Origin, hit.transform.position), ViewBlockLayerMask))
                CulledHits.Add(hit);
        }
        return CulledHits;
    }

    public static bool RayOnPlaneIntersection(Vector3 rayOrigin, Vector3 rayDirection, Vector3 planeOrigin, Vector3 planeNormal, out Vector3 HitPoint)
    {
        HitPoint = Vector3.zero;
        //flee if the ray misses completely
        if (Vector3.Dot(rayOrigin - planeOrigin, rayDirection) > 0)
            return false;

        //Do actual test
        Vector3 offset = Vector3.ProjectOnPlane(rayOrigin - planeOrigin, planeNormal.normalized);
        float planeDistance = ((rayOrigin - planeOrigin) - offset).magnitude;
        HitPoint = rayOrigin + rayDirection.normalized * (planeDistance / Mathf.Abs(Vector3.Dot(planeNormal.normalized, rayDirection.normalized)));
        return true;
    }
}