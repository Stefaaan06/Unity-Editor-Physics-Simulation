using System;
using System.Collections;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/// <summary>
/// Simple Editor Extension that allows you to simulate physics in the scene view.
/// | Backend code to simulate physics in the scene view.
/// </summary>
/// <author>Stefaaan06</author>
/// <version>1.1.0</version>

public class ScenePhysicsTool : MonoBehaviour
{
    private float _simulationWeight;
    
    /// <summary>
    /// Simulates physics for a specified duration.
    /// </summary>
    /// <param name="time">The duration for which physics should be simulated in seconds.</param>
    /// <param name="runForChildren">Whether to also run physics for child objects.</param>
    /// <param name="simulationWeight">The mass the object should be simulated with</param>
    public void SimulatePhysics(float time, bool runForChildren, float simulationWeight)
    {
        Rigidbody rb;
        ScenePhysicsTool scenePhysicsTool;

        //Arrays to hold the previous rigidbody states
        float[] rememberRigidbodyMass = new float[1 + transform.childCount];
        CollisionDetectionMode[] rememberCollisionDetection = new CollisionDetectionMode[1 + transform.childCount];
        bool[] rememberRigidbodiesBool = new bool[1 + transform.childCount];
        
        //disables Physics for all Rigidbodies
        Rigidbody[] rbs = FindObjectsOfType<Rigidbody>();
        foreach (Rigidbody rigidbody in rbs)
        {
            rigidbody.isKinematic = true;
        }
        
       
        if (runForChildren)
        {
            rememberRigidbodiesBool[0] = false;
            
            int count = 1;
            Undo.RegisterChildrenOrderUndo(this.gameObject.transform, "Simulate Physics");     //remember Object state for unitys Undo system
            
            foreach (Transform child in transform)
            {
                child.TryGetComponent<Rigidbody>(out rb);
                if (rb == null)
                {
                    rb = child.gameObject.AddComponent<Rigidbody>();
                    rememberRigidbodiesBool[count] = false;
                }
                else
                {
                    //remember Rigidbody values
                    rememberRigidbodyMass[count] = rb.mass;
                    rememberRigidbodiesBool[count] = true;
                    rememberCollisionDetection[count] = rb.collisionDetectionMode;
                }
                
                rb.isKinematic = false;
                
                child.TryGetComponent<ScenePhysicsTool>(out scenePhysicsTool);
                if (scenePhysicsTool != null)
                {
                    rb.mass = scenePhysicsTool.GetSimulationWeight();
                }
                else
                {
                    rb.mass = simulationWeight;
                }

                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;   //sets CollisionDetectionMode to ContinuousDynamic to prevent objects from passing through each other

                count++;
            }
        }
        else
        {
            Undo.RecordObject(transform, "Simulate Physics");   //Remember Object state for unity undo system
    
        
            this.TryGetComponent<Rigidbody>(out rb);    

            // if this Object has a Rigidbody, enables Physics for this Object. If not, adds a Rigidbody and enables Physics
            if (rb != null)
            {
                //remember Rigidbody values
                rememberRigidbodyMass[0] = rb.mass;
                rememberCollisionDetection[0] = rb.collisionDetectionMode;
                rememberRigidbodiesBool[0] = true;
            
                rb.mass = simulationWeight;
            }
            else   
            {
                rememberRigidbodiesBool[0] = false;
            
                rb = this.gameObject.AddComponent<Rigidbody>();    
                rb.mass = simulationWeight;
            }
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;   //sets CollisionDetectionMode to ContinuousDynamic to prevent objects from passing through each other
        }
        
        
        //simulate physics
        EditorCoroutineUtility.StartCoroutineOwnerless(simulatePhysics(time, rememberRigidbodiesBool, rememberRigidbodyMass, rememberCollisionDetection, runForChildren, rbs));

    }

    /// <summary>
    /// Coroutine that simulates physics for a specified duration.
    /// </summary>
    /// <param name="time">time to simulate</param>
    /// <param name="rememberRigidbodiesBool">remembers which object had rigidbodies</param>
    /// <param name="rememberRigidbodyMass">remembered mass values</param>
    /// <param name="rememberCollisionDetection">remembered collision detection values</param>
    /// <param name="runForChildren">determines if it should run for children</param>
    /// <param name="rbs">All rigidbodies in the scene</param>
    /// <returns></returns>
    IEnumerator simulatePhysics(float time, bool[] rememberRigidbodiesBool, float[] rememberRigidbodyMass, CollisionDetectionMode[] rememberCollisionDetection, bool runForChildren, Rigidbody[] rbs)
    {
        Rigidbody rb;
        
        
        Physics.autoSimulation = false;
        //simulate physics for a specified duration
        for (; time > 0; time -= 0.01f)
        {
            Physics.Simulate(0.01f);
            yield return new WaitForEndOfFrame();
        }
        Physics.autoSimulation = true;
        
        //reenables Physics for all Rigidbodies
        foreach (Rigidbody rigidbody in rbs)
        {
            rigidbody.isKinematic = false;
        }
        
        /*
         * resets all rigidbodies to their previous state
         */
        
        if (rememberRigidbodiesBool[0])
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = rememberRigidbodyMass[0];
            rb.collisionDetectionMode = rememberCollisionDetection[0];
        }
        else
        {
            DestroyImmediate(GetComponent<Rigidbody>());
        }

        if (runForChildren)
        {
            int count = 1;
            foreach (Transform child in transform)
            {
                if (rememberRigidbodiesBool[count])
                {
                    rb = child.GetComponent<Rigidbody>();

                    rb.mass = rememberRigidbodyMass[count];
                    rb.collisionDetectionMode = rememberCollisionDetection[count];
                }
                else
                {
                    DestroyImmediate(child.GetComponent<Rigidbody>());
                }
            }
        }
    }

    /// <summary>
    /// Setter method to set the simulation weight of this object.
    /// </summary>
    /// <param name="weight">desired weight</param>
    public void SetSimulationWeight(float weight)
    {
        _simulationWeight = weight;
    }
    
    /// <summary>
    /// Getter method to get the simulation weight of this object.
    /// </summary>
    /// <returns>simulation weight</returns>
    public float GetSimulationWeight()
    {
        return _simulationWeight;
    }
}

/// <summary>
/// Simple Editor Extension that allows you to simulate physics in the scene view.
/// | Frontend Code to display Buttons in the Inspector.
/// </summary>
/// <author>Stefaaan06</author>
/// <version>1.1.0</version>
[CustomEditor(typeof(ScenePhysicsTool))]
public class ScenePhysicsToolGUI : Editor
{
    //initial values
    public float simulationTime = 10f;
    public bool runForChildren = false;
    public float simulationWeight = 1f;

    
    private GUIStyle tooltipStyle;

    private void OnEnable()
    {
        // Create a GUIStyle for tooltips
        tooltipStyle = new GUIStyle();
        tooltipStyle.normal.textColor = Color.white;
        tooltipStyle.wordWrap = true;
    }
    
    /// <summary>
    /// Updates GUI elements
    /// </summary>
    public override void OnInspectorGUI()
    {
        GUILayout.Label("Scene Physics Tool");
        
        //Gui elements + Labels
        simulationTime = EditorGUILayout.FloatField(new GUIContent("Simulation Time", "The duration for which physics should be simulated."), simulationTime);
        simulationWeight = EditorGUILayout.FloatField(new GUIContent("Simulation Weight", "The weight of this object when simulating physics."), simulationWeight);
        runForChildren = EditorGUILayout.Toggle(new GUIContent("Run onlys for Children", "Runs only for children. If enabled, will try to find a 'ScenePhysicsTool' component on all child objects, and get the simulation weight from there. If there is no 'ScenePhysicsTool' component on a child object, will use the simulation weight from this object."), runForChildren);
        
        ScenePhysicsTool scenePhysicsTool = (ScenePhysicsTool)target;
        if (GUILayout.Button("Run Physics"))
        {
            
            scenePhysicsTool.SimulatePhysics(simulationTime, runForChildren, simulationWeight);
        }
        
        scenePhysicsTool.SetSimulationWeight(simulationWeight);     //update weight in ScenePhysicsTool script
    }
}
