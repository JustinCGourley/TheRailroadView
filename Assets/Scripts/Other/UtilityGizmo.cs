using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityGizmo : MonoBehaviour
{
    public static UtilityGizmo Instance { get; private set; }

    /// <summary>
    /// sets up singleton class
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public enum DebugObjectType
    {
        sphere,
        line,
        ray
    }

    public struct DebugObject
    {
        public Vector3 location;
        public Vector3 location2;
        public float radius;
        public Color color;
        public DebugObjectType objType;
    }



    List<DebugObject> debugObjects;

    private void Start()
    {
        debugObjects = new List<DebugObject>();
    }

    public void AddDebugObject(DebugObject obj)
    {
        debugObjects.Add(obj);
    }

    public void DrawRay(Vector3 location, Vector3 direction, Color color)
    {
        DebugObject ray = new DebugObject();
        ray.objType = DebugObjectType.ray;
        ray.location = location;
        ray.location2 = direction;

        debugObjects.Add(ray);
    }

    public void DrawSphere(Vector3 location, float radius, Color color)
    {
        DebugObject sphere = new DebugObject();
        sphere.objType = DebugObjectType.sphere;
        sphere.location = location;
        sphere.radius = radius;
        sphere.color = color;

        debugObjects.Add(sphere);
    }

    public void DrawLine(Vector3 location, Vector3 location2, Color color)
    {
        DebugObject line = new DebugObject();
        line.objType = DebugObjectType.line;
        line.location = location;
        line.location2 = location2;
        line.color = color;

        debugObjects.Add(line);
    }

    private void OnDrawGizmos()
    {
        if (debugObjects == null) { return; }
        for (int i = 0; i < debugObjects.Count; i++)
        {
            Gizmos.color = debugObjects[i].color;
            switch (debugObjects[i].objType)
            {
                case DebugObjectType.sphere:
                    Gizmos.DrawSphere(debugObjects[i].location, debugObjects[i].radius);
                    break;
                case DebugObjectType.line:
                    Gizmos.DrawLine(debugObjects[i].location, debugObjects[i].location2);
                    break;
                case DebugObjectType.ray:
                    Gizmos.DrawRay(debugObjects[i].location, debugObjects[i].location2);
                    break;
            }
        }

        debugObjects = new List<DebugObject>();
    }


}
