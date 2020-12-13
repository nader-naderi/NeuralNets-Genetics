using UnityEngine;
using System.Collections.Generic;
// Plugin.
using MathNet.Numerics.LinearAlgebra;
using System;

using Random = UnityEngine.Random;


namespace NDRCreates.ML.DeepLerning.NerualNetwork
{
    public class NeuralNetwork : MonoBehaviour
    {
        public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 3);

        public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();
        
        public Matrix<float> outputLayers = Matrix<float>.Build.Dense(1, 2);

        public List<Matrix<float>> weights = new List<Matrix<float>>();

        public List<float> biases = new List<float>();

        public float fitness;

        /// <summary>
        /// Inititalization of our Neural Network.
        /// </summary>
        /// <param name="hiddenLayerCount"></param>
        /// <param name="hiddenNeuronCounts"></param>
        public void Inititalization(int hiddenLayerCount, int hiddenNeuronCounts)
        {
            inputLayer.Clear();
            hiddenLayers.Clear();
            outputLayers.Clear();
            weights.Clear();
            biases.Clear();
          
            // Loop through all hidden layer and populate each hiddenLayer current Index. + 1, because array indexes starts from 0.
            for (int i = 0; i < hiddenLayerCount + 1; i++)
            {
                // place the current looping index on a temporary, and populate it with hiddenLayerCount argumant value.
                Matrix<float> temporaryHiddenLayersMatrix = Matrix<float>.Build.Dense(1, hiddenNeuronCounts);

                //Add the temporary matrix to hidden layer
                hiddenLayers.Add(temporaryHiddenLayersMatrix);

                //Creation of Biases.
                biases.Add(Random.Range(-1f, 1f));

                //Creation of Weights.

                // if i equals one, this hidden layer, is connected to inputlayer. we can design the weight matrix, 
                //to have desired amount of rows and columns to fet between the two layers. and they need to have
                //a certian amount of rows and columns to each input and each hiddenNeuron can have a individual weight between this.
                if(i == 0)
                {
                    // A matrix is Hidden layer.
                    // B matrix is Output layer.

                    // if we multipliy inputLayer and first hidden layer  weights, it will give us the matrix of size hidden layer one.
                    //  [I1] [H1]
                    //  [I2] [H2]
                    //  [I3]
                    //  ------------
                    //       I1  I2  I3
                    //  H1 [ X   X   Z ]
                    //  H2 [ Z   X   Y ]
                    Matrix<float> inputToH1 = Matrix<float>.Build.Dense(3, hiddenNeuronCounts);
                    weights.Add(inputToH1);
                }

                Matrix<float> hiddenToHidden = Matrix<float>.Build.Dense(hiddenNeuronCounts, hiddenNeuronCounts);

                weights.Add(hiddenToHidden);
            }

            Matrix<float> outputWeight = Matrix<float>.Build.Dense(hiddenNeuronCounts, 2);
            weights.Add(outputWeight);
            biases.Add(Random.Range(-1f, 1f));
            RandomiseWeights();
        }

        /// <summary>
        /// Randomise Weight with 3d nested loops.
        /// </summary>
        public void RandomiseWeights()
        {
            for (int i = 0; i < weights.Count; i++)
            {
                for (int j = 0; j < weights[i].RowCount; j++)
                {
                    for (int k = 0; k < weights[i].ColumnCount; k++)
                    {
                        weights[i][j, k] = Random.Range(-1, 1f);
                    }
                }
            }
        }

        /// <summary>
        /// do all the multiplication and processing the layers.
        /// </summary>
        /// <param name="up_Right">northEast sensor </param>
        /// <param name="up">north sensor </param>
        /// <param name="up_Left">northWest sensor </param>
        /// <returns></returns>
        public (float, float) RunNetwork(float up_Right, float up, float up_Left)
        {
            //Inside Method summary :
            //  Neural net by itself does not do all of teh lerning, you can think it like a data structure.
            //  somthing holds bunch of data. and the nwe have lerning models, genetic algorithm and etc.
            //  we use this methods for diffrent reasons, because our NNET is so modular.
            inputLayer[0, 0] = up_Right;
            inputLayer[0, 1] = up;
            inputLayer[0, 2] = up_Left;

            // activation function, the point is, to not make a linear function.we want modified for minimiseing our error.
            // Sigmoid func, give us 0, 1.
            // TanH func, give us -1, 1.
            // our values that we want at the end, have to be between -1 and 1.move and rotate is -1 and 1. we dont want 0.
            inputLayer = inputLayer.PointwiseTanh();

            //Create the value for the first hidden layer. (first layer, connection between the input layer and hidden layer) 
            //inputLayer * firstWeight + biases = firstHiddenLayer.
            //we done it manualy, because we have pre-defined matrises here.
            hiddenLayers[0] = ((inputLayer * weights[0]) + biases[0]).PointwiseTanh();

            // i = 1, we dont want to hit the hiddenlayer[0] we done it above manualy.
            for (int i = 1; i < hiddenLayers.Count; i++)
            {
                hiddenLayers[i] = ((hiddenLayers[i - 1] * weights[i]) + biases[i]).PointwiseTanh(); 
            }

            // assigne the values for Output layer.Activate it.
            outputLayers = ((hiddenLayers[hiddenLayers.Count - 1] * weights[weights.Count - 1]) + biases[biases.Count - 1]).PointwiseTanh();

            //First output is acceleration and second is steering.
            // Acceleration is between 0 and +1 (-1 to 0 is for reversing), then we need Sigmoid() 
            // Steering is between -1 (right) 0 (forward) +1 (right), then we need Tanh()
            return (SigmoudFunction(outputLayers[0, 0]), (float)Math.Tanh(outputLayers[0, 1]));
        }

        private float SigmoudFunction(float s)
        {
            return (1 / (1 + Mathf.Exp(-s)));
        }

        public NeuralNetwork InitializeCopy (int hiddenLayerCount, int hiddenNeuronCount)
        {
            NeuralNetwork neural = new NeuralNetwork();

            List<Matrix<float>> newWeights = new List<Matrix<float>>();

            for (int i = 0; i < this.weights.Count; i++)
            {
                Matrix<float> curWeight = Matrix<float>.Build.Dense(weights[i].RowCount, weights[i].ColumnCount);

                // again double 'for' loop, sorry Big O :(
                for (int x = 0; x < curWeight.RowCount; x++)
                {
                    for (int y = 0; y < curWeight.ColumnCount; y++)
                    {
                        curWeight[x, y] = weights[i][x, y];
                    }
                }
                newWeights.Add(curWeight);
            }

            List<float> newBiases = new List<float>();
            newBiases.AddRange(biases);

            neural.weights = newWeights;
            neural.biases = newBiases;

            neural.InitializeHiddenLayers(hiddenLayerCount, hiddenNeuronCount);
            return neural;
        }

        public void InitializeHiddenLayers(int hiddenLayerCount, int hiddenNeuronCount)
        {
            inputLayer.Clear();
            hiddenLayers.Clear();
            outputLayers.Clear();

            for (int i = 0; i < hiddenLayerCount; i++)
            {
                Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, hiddenNeuronCount);
                hiddenLayers.Add(newHiddenLayer);
            }
        }
    }
}