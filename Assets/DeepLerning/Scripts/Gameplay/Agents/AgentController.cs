/////////////////////////////////////////////////////////////////////////////////////////////////
///                             NADER NADERI Creator of this Masterpiece .... :)))
///                             Check @NDRCreates on telegram for further courses.
///
///
////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NDRCreates.ML.DeepLerning.NerualNetwork
{
    [RequireComponent(typeof(NeuralNetwork))]
    public class AgentController : MonoBehaviour
    {
        //Every time the Agent dies, it will reset back to this loaction.
        private Vector3 startPosition;
        private Vector3 startRotation;

        /// <summary>
        /// How the agent will be accelerated on the time scale ?
        /// </summary>
        [Range(-1f, 1f)]
        [SerializeField] float accelerationValue;

        /// <summary>
        /// how fast the agent will rotate?
        /// </summary>
        [Range(-1f, 1f)]
        [SerializeField] float turningValue;

        /// <summary>
        /// it will keep track for, how long it been since the agent has started ?
        /// reason : check if the car is Idle? therfore is usless and we reset the network for new generation.
        /// </summary>
        [SerializeField] float lifeTime = 0f;

        //Score of the progression of car.
        /// <summary>
        /// Weight Contribution.
        /// </summary>
        [Header("Fitness")]
        [SerializeField] float overallFitness;
        /// <summary>
        /// How important the distance is to the fitness Method, "How far it goes ?".
        /// </summary>
        [SerializeField] float distanceMultiplier = 1.4f;

        /// <summary>
        /// How important the speed is to the overall fitness Method, "How fast it goes ?".
        /// </summary>
        [SerializeField] float averageSpeedMultiplier = 0.2f;
        /// <summary>
        /// how important it is to stay in middle of the track.allow the car to not crash to wall.
        /// </summary>
        [SerializeField] float sensorMultiplier = 0.1f;

        /// <summary>
        /// [Fitness Calculation Variable]
        /// what was the last position of this agent?
        /// </summary>
        private Vector3 lastPosition;

        /// <summary>
        /// [Fitness Calculation Variable]
        /// how far thsi agent walked-drived?
        /// </summary>
        private float totalDistanceTravelled;

        /// <summary>
        /// [Fitness Calculation Variable]
        /// what was the average of the agent's speed?
        /// </summary>
        private float averageSpeed;

        /// <summary>
        /// Distance values, Inputs of our Neural Net.
        /// on the other hand, it is a ray origin for casting and recieving to 
        /// multiple directions for detecting the aheading.
        /// </summary>
        private float northEastSensor, northSensor, northWestSensor;

        /// <summary>
        /// player Input.
        /// </summary>
        private Vector3 inputDirection;

        private NeuralNetwork network;

        [Header("Netword Options")]
        [SerializeField]
        int LAYERS = 1;
        [SerializeField]
        int NEURONS = 10;

        public int GetLayers { get => LAYERS;}
        public int GetNeurons { get => NEURONS;}

        [SerializeField]
        Mesh[] carPhenotypes;
        [SerializeField]
        MeshFilter currentMesh;


        private void Awake()
        {
            //positon initialization on Application Awake.
            startPosition = transform.position;
            //rotaion initialization on Application Awake.
            startRotation = transform.eulerAngles;

            network = GetComponent<NeuralNetwork>();

        }

        /// <summary>
        /// Every time the agent dies, we push this method and 
        /// all of the variables will be reset to default values.
        /// </summary>
        public void Reset()
        {
            
            lifeTime = 0f;
            totalDistanceTravelled = 0f;
            averageSpeed = 0f;
            //pass the current Start position to last position.
            lastPosition = startRotation;
            overallFitness = 0f;
            transform.position = startPosition;
            transform.eulerAngles = startRotation;
            currentMesh.mesh = carPhenotypes[Random.Range(0, carPhenotypes.Length)];

           // Camera.main.GetComponent<CameraController>().ResetEverything(this.transform);
        }

        /// <summary>
        /// will be called when this agent hits anything in the game world.
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionEnter(Collision collision)
        {
            //Reset every thing when collided to anything.
            //Reset();
            //Kill it.
            Death();
        }

        public void MoveTheAgent(float verticalValue, float horizontalValue)
        {
            // Lerping between zero coord Space to this movement speed. (Hard Coded, Sorry :)
            inputDirection = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, verticalValue * 11.4f), 0.02f);

            // Converting input direction to the Agent Desired Direction.
            inputDirection = transform.TransformDirection(inputDirection);

            //Add the direction input every frame to the current position of the agent. (make the car move !!!!)
            transform.position += inputDirection;

            // Calculate rotation direction of the agent, and then implement it on current agent rotation transformation.
            transform.eulerAngles += new Vector3(0, (horizontalValue * 90) * 0.02f, 0);
        }

        private void InputSensors()
        {
            // X  Y  Z         X  Y  Z               X  Y  Z

            //(1, 0, 1)                  =     (0, 0, 1)      +      (1, 0, 0)..=> NorthEast
            Vector3 northEastDirection   = (transform.forward + transform.right);
            
            //(1, 0, 1)            =     (0, 0, 1) .............................=> North
            Vector3 northDirection = transform.forward;
            
            //          (1, 0, 1)      =     (0, 0, 1)      +      (- 1, 0, 0)..=> NorthEast
            Vector3 northWestDirection = (transform.forward - transform.right);

            // shoot a ray from this position origin in north East direction.
            Ray ray = new Ray(transform.position, northEastDirection);
            
            // save collision information.
            RaycastHit hit;

            // raycast logic.
            if(Physics.Raycast(ray, out hit))
            {
                // we normalize it with 20f.(we don't want a biiiiig value)
                northEastSensor = hit.distance / 20f;

                // print it on the Unity log (for Debugging sake).
               // Debug.Log("North East Sensor : " + northEastSensor);
                Debug.DrawLine(ray.origin, hit.point, Color.red);
            }

            //replace our ray for north sensor ray direction.
            ray.direction = northDirection;

            
            if (Physics.Raycast(ray, out hit))
            {
                // we normalize it with 20f.(we don't want a biiiiig value)
                northSensor = hit.distance / 20f;

                // print it on the Unity log (for Debugging sake).
                //Debug.Log("North Sensor : " + northSensor);
                Debug.DrawLine(ray.origin, hit.point, Color.green);
            }

            //replace our ray for north West sensor ray direction.
            ray.direction = northWestDirection;


            if (Physics.Raycast(ray, out hit))
            {
                // we normalize it with 20f.(we don't want a biiiiig value)
                northWestSensor = hit.distance / 20f;

                // print it on the Unity log (for Debugging sake).
                //Debug.Log("North West Sensor : " + northWestSensor);
                Debug.DrawLine(ray.origin, hit.point, Color.blue);
            }
        }

        private void CalculateFitness()
        {
            // Eveey frame add distance from agent position to lastPosition, to total Dist travelled.
            totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);

            // average speed, i think this sentence is self descripted pretty well. :)
            averageSpeed = totalDistanceTravelled / lifeTime;

            // Calculate overall fitness.multiplier give us a insane amount of controls to fitness.
            overallFitness = (totalDistanceTravelled * distanceMultiplier) + (averageSpeed * averageSpeedMultiplier) +
                ((northEastSensor + northSensor + northWestSensor / 3) * sensorMultiplier);
        
            //if it's usless and doing horrible, not worth of living, reset everything or kill it.
            if(lifeTime > 20 && overallFitness < 40)
            {
                //Reset();
                Death();
            }

            //if Achieve atleast three times ... Reset
            if(overallFitness >= 1000)
            {
                // TODO: Saves the network to JSON and deserialize it on a differnt course.
                //Reset();
                Death();
            }
        }

        //Constant scale of time.
        private void FixedUpdate()
        {
            InputSensors();

            lastPosition = transform.position;

            //TODO: Put neuro net code here.
            //network.Inititalization(LAYERS, NEURONS);

            (accelerationValue, turningValue) = network.RunNetwork(northEastSensor, northSensor, northWestSensor);

            MoveTheAgent(accelerationValue, turningValue);

            lifeTime += Time.deltaTime;

            CalculateFitness();

            //accelerationValue = 0;
            //turningValue = 0;


        }

        public void ResetWithNetwork(NeuralNetwork neuralNetwork)
        {
            this.network = neuralNetwork;
            Reset();
        }


        /// <summary>
        /// Killing the car in game scene.
        /// </summary>
        private void Death()
        {
            GameObject.FindObjectOfType<Genetics.GeneticsManager>().Death(overallFitness, network);
        }

    }
}