using UnityEditor;
using UnityEngine;

namespace EditorPhysicsSimulation
{
    
    /// <summary>
    /// Simple Editor Extension that allows you to simulate physics in the scene view.
    /// | Backend code to simulate physics in the scene view.
    /// </summary>
    /// <author>Stefaaan06</author>
    /// <version>1.0.0</version>
    public class ScenePhysicsTool : MonoBehaviour
    {
        private PhysicsScene _physicsScene;

        /// <summary>
        /// Simulates physics for a specified duration.
        /// </summary>
        /// <param name="time">The duration for which physics should be simulated in seconds.</param>
        /// <param name="runForChildren">Whether to also run physics for child objects.</param>
        public void simulatePhysics(float time, bool runForChildren)
        {
            //disables Physics for all Rigidbodies
            Rigidbody[] rbs = FindObjectsOfType<Rigidbody>();
            foreach (Rigidbody rigidbody in rbs)
            {
                rigidbody.isKinematic = true;
            }
            
            Rigidbody rb;
            
            this.TryGetComponent<Rigidbody>(out rb);    //tries to get Rigidbody of this Object
            if (rb != null)
            {
                //enable Physics & register Object state for unitys Undo system
                rb.isKinematic = false;
                Undo.RegisterCompleteObjectUndo(rb.gameObject, "Simulate Physics");
            }

            
            //if runForChildren is true, enables Physics for all child objects (if they have rigidbodies)
            if (runForChildren)
            {
                foreach (Transform child in transform)
                {
                    child.TryGetComponent<Rigidbody>(out rb);
                    if (rb != null)
                    {
                        rb.isKinematic = false;
                        Undo.RegisterCompleteObjectUndo(rb.gameObject, "Simulate Physics");     //remember Object state for unitys Undo system
                    }
                }
            }

            //simulates physics for the specified duration
            Physics.autoSimulation = false;
            for (; time > 0; time -= 0.01f)
            {
                Physics.Simulate(0.1f);
            }
            Physics.autoSimulation = true;

            
            //reenables Physics for all Rigidbodies
            foreach (Rigidbody rigidbody in rbs)
            {
                rigidbody.isKinematic = false;
            }
        }
    }

    /// <summary>
    /// Simple Editor Extension that allows you to simulate physics in the scene view.
    /// | Frontend Code to display Buttons in the Inspector.
    /// </summary>
    /// <author>Stefaaan06</author>
    /// <version>1.0.0</version>
    [CustomEditor(typeof(ScenePhysicsTool))]
    public class ScenePhysicsToolGUI : Editor
    {
        public float simulationTime = 1.0f;
        public bool runForChildren = false;

        /// <summary>
        /// Updates GUI elements
        /// </summary>
        public override void OnInspectorGUI()
        {
            GUILayout.Label("Scene Physics Tool");

            simulationTime = EditorGUILayout.FloatField("Simulation Time", simulationTime);
            runForChildren = EditorGUILayout.Toggle("Run for Children", runForChildren);

            if (GUILayout.Button("Run Physics"))
            {
                ScenePhysicsTool scenePhysicsTool = (ScenePhysicsTool)target;
                scenePhysicsTool.simulatePhysics(simulationTime, runForChildren);
            }
        }
    }
}

