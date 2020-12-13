using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

namespace NDRCreates.ML.DeepLerning.NerualNetwork.Genetics
{
    public class GeneticsManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] AgentController controller;
        
        [Header("Controls")]
        //Starting population.
        [SerializeField] int initialPopulation = 85;
        [Range(0.0f, 1.0f)]
        // in what Rate, our Agents will Mutate?
        [SerializeField] float mutationRatio = 0.055f;

        [Header("Crossover Controls")]
        //How in what method, two parents selected, and what networks are combined.
        [SerializeField] int bestAgentSelection = 8;    // 
        [SerializeField] int worstAgentSelection = 3;   // 8 + 3 = 12 

        /// <summary>
        /// what number of genes we want to take from a breeding and pass it to next generation?
        /// a : 010100101 01110
        /// b : 110100011 11001
        /// ------Breeding (Combining?)-------
        /// a': 101110001 11001
        /// b': 000111001 01110
        /// 
        /// </summary>
        [SerializeField] int numberToCrossover;
        /// <summary>
        /// All the networks that have been selected.
        /// </summary>
        private List<int> genePool = new List<int>();
        
        private int naturallySelected;

        private NeuralNetwork[] population;

        [Header("Public View")]
        [SerializeField] int currentGeneration;
        [SerializeField] int currentGenome = 0;

        private void Start()
        {
            CreatePopulation();
        }

        void CreatePopulation()
        {
            population = new NeuralNetwork[initialPopulation];
            FillPopWithRandomValues(population, 0);
            ResetToCurrentGenome();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="population"></param>
        /// <param name="startingIndex"> we may only want to set a certian amount of this new pop to a random value</param>
        private void FillPopWithRandomValues(NeuralNetwork[] newPopulation, int startingIndex)
        {
            // until starting index was lower that starter pop number, keep doing this loop.
            while (startingIndex < initialPopulation)
            {
                // init NNET for each pop on loop.
                newPopulation[startingIndex] = new NeuralNetwork();
                // fill the net to pops in each iteration.
                newPopulation[startingIndex].Inititalization(controller.GetLayers, controller.GetNeurons);
                // go for next one.
                startingIndex++;
            }
        }

        private void ResetToCurrentGenome()
        {
            //current genome initialy is 0. it will reset genome in start and set first network in pop.after it finishes, 
            //it will genome ++ and we off to go to next genome.
            controller.ResetWithNetwork(population[currentGenome]);
            UIManager.instacne.UpdateUI(currentGeneration, currentGenome);
        }
        /// <summary>
        /// ones a agent dies, get calls and increament and do all the studd for ceremony.
        /// </summary>
        /// <param name="fitness"> save the fitness value for keeping track of our history. </param>
        /// <param name="neuralNetwork"></param>
        public void Death(float fitness, NeuralNetwork neuralNetwork)
        {
            if(currentGenome < population.Length - 1)
            {
                population[currentGenome].fitness = fitness;
                // Next agent in the current Generation.
                currentGenome++;
                // reset all of things and do all of it again from start but, with a diffrent outcome.
                ResetToCurrentGenome();
                UIManager.instacne.UpdateUI(currentGeneration, currentGenome);
            }
            // if it is not, then Repopulate
            else
            {
                Repopulate();
            }
        }

        /// <summary>
        /// Mating ...
        /// </summary>
        public void Repopulate()
        {
            genePool.Clear();
            currentGeneration++;
            naturallySelected = 0;
            //higher the fitness higher on the genepool list, then we should sort them.
            SortPopulation();

            // we dont mess with initial array, so we do it on temp arr
            NeuralNetwork[] newPop = PickBestPopulation();

            Corssover(newPop);
            Mutate(newPop);
            FillPopWithRandomValues(newPop, naturallySelected);

            // Reset them.
            population = newPop;
            currentGenome = 0;
            ResetToCurrentGenome();
            UIManager.instacne.UpdateUI(currentGeneration, currentGenome);
        }

        private void Corssover(NeuralNetwork[] newPop)
        {
            for (int i = 0; i < numberToCrossover; i += 2)
            {
                int firstParentIndex = i;
                int secondParentIndex = i + 1; // next one.

                // find couples
                if (genePool.Count >= 1)
                {
                    for (int j = 0; j < 100; j++)
                    {
                        firstParentIndex = genePool[Random.Range(0, genePool.Count)];
                        secondParentIndex = genePool[Random.Range(0, genePool.Count)];

                        // we found the couple?
                        if (firstParentIndex != secondParentIndex)
                            break;
                    }
                }

                NeuralNetwork firstChild = new NeuralNetwork();
                NeuralNetwork secondChild = new NeuralNetwork();
                
                GetBirth(firstChild);
                GetBirth(secondChild);

                // for weights ... 
                for (int w = 0; w < firstChild.weights.Count; w++)
                {
                    // 50% chance to go on this ...
                    // we are swaping out entire matrices not individual weight values inside of matrixes, because its sooooo complicated. 
                    if (Random.Range(0.0f, 1.0f) < 0.5f)
                    {
                        firstChild.weights[w] = population[firstParentIndex].weights[w];
                        secondChild.weights[w] = population[secondParentIndex].weights[w];
                    }
                    else
                    {
                        firstChild.weights[w] = population[secondParentIndex].weights[w];
                        secondChild.weights[w] = population[firstParentIndex].weights[w];
                    }
                }


                // for biasess ... accurate, we doing individuals not just entire matrixes.
                for (int w = 0; w < firstChild.biases.Count; w++)
                {
                    // 50% chance to go on this ...
                    if (Random.Range(0.0f, 1.0f) < 0.5f)
                    {
                        firstChild .biases[w] = population[firstParentIndex]  .biases[w];
                        secondChild.biases[w] = population[secondParentIndex].biases[w];
                    }
                    else
                    {
                        firstChild .biases[w] = population[secondParentIndex].biases[w];
                        secondChild.biases[w] = population[firstParentIndex] .biases[w];
                    }
                }

                // Darwin was right <:-) 
                newPop[naturallySelected] = firstChild;

                // we increase it, we dont want to override it, when we radomize it.
                naturallySelected++;

                // Hello Mr.Darwin :-) 
                newPop[naturallySelected] = secondChild;
                naturallySelected++;

            }
        }

        private void GetBirth(NeuralNetwork firstChild)
        {
            firstChild.Inititalization(controller.GetLayers, controller.GetNeurons);
            firstChild.fitness = 0;
        }

        /// <summary>
        /// just the ones who truly deserve, will mutate ... 
        /// </summary>
        /// <param name="newPop"> desired Population array </param>
        private void Mutate(NeuralNetwork[] newPop)
        {
            // we only looping on "naturallySelected" field, in the new pop, because the rest of the population haven't even initialized (born) yet.
            for (int i = 0; i < naturallySelected; i++)
            {
                for (int j = 0; j < newPop[i].weights.Count; j++)
                {
                    if(Random.Range(0.0f, 1.0f) < mutationRatio)
                    {
                        newPop[i].weights[j] = MutatedWeight(newPop[i].weights[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Take a matrix and Randomise it.
        /// </summary>
        /// <returns></returns>
        private Matrix<float> MutatedWeight(Matrix<float> matrix)
        {
            // not accurate i know but it came in manay exprimantaions.divid 7, its just the accurate value.
            int randomPoints = Random.Range(1, (matrix.RowCount * matrix.ColumnCount) / 7);

            Matrix<float> newMatrix = matrix;

            for (int i = 0; i < randomPoints; i++)
            {
                int randomisedColumn = Random.Range(0, newMatrix.ColumnCount);
                int randomisedRow = Random.Range(0, newMatrix.RowCount);

                // bumb it down or up a liiiiiitle bit.
                newMatrix[randomisedRow, randomisedColumn] = Mathf.Clamp(newMatrix[randomisedRow, randomisedColumn] + Random.Range(-1f, 1f), -1f, 1f);
            }


            return newMatrix;
        }

        /// <summary>
        /// verrrrry low efficent and simple sorting algorithm for education porpuses sake ... don't kill me Big O :))
        /// </summary>
        private void SortPopulation()
        {
            for (int i = 0; i < population.Length; i++)
            {
                for (int j = i; j < population.Length; j++)
                {
                    if (population[i].fitness < population[j].fitness)
                    {
                        NeuralNetwork tempNetwork = population[i];
                        population[i] = population[j];
                        population[j] = tempNetwork;
                    }
                }
            }
        }

        private NeuralNetwork[] PickBestPopulation()
        {
            NeuralNetwork[] newPop = new NeuralNetwork[initialPopulation];

            // loop through them for selecting the top, another loop for selecting the bottom.
            // select : add to genepool. genepool doesnt have a Fixed size, it's based on the fitness of the actual agent that adding.
            // top net is going to be aded more times to genepool, then
            // then we come to cross over and we have higher chance of picking the top network from geenpool rather than the fourth best network.
            // every thing is random, 99 % of this process is random.
            // no one knows what going on.

            // Im changign a refrence !!!! we are going to pick up a refrence to parents, but what if the parents change???
            // we shouuuuuuuuuuuuuuld copy every thing to keep track on all of things ... 
            //i didn't find better approach than this, to please, dont judge me on Big O Notation ... I will replace it ASAP.
            // 

            for (int i = 0; i < bestAgentSelection; i++)
            {
                // transfering the best network from current gen over to the next generation unharmed. we dont want to lose best networks.
                // best agent works amazing and when she crossed over, she may work worst, threfore why we loose it ?? 
                // save her and transfer her on gene pool.
                // this worked for me, dont judge me please. :\
                newPop[naturallySelected] = population[i].InitializeCopy(controller.GetLayers, controller.GetNeurons);
                // now we copied it, clean it ...
                newPop[naturallySelected].fitness = 0;
                // keep track of how much we acutaly taking from initial pop. 
                naturallySelected++;
                GenePoolModificationCalculation(i);

            }

            for (int i = 0; i < worstAgentSelection; i++)
            {
                int last = population.Length - 1;
                // go backward.
                last -= i;
                GenePoolModificationCalculation(last);
            }

            return newPop;
        }
        /// <summary>
        /// DRY dont repeat your self.
        /// </summary>
        /// <param name="i"></param>
        private void GenePoolModificationCalculation(int i)
        {
            int timesToAddCurrentnetInToGenePool = Mathf.RoundToInt(population[i].fitness + 10);

            for (int j = 0; j < timesToAddCurrentnetInToGenePool; j++)
            {
                genePool.Add(i);
            }
        }
    }
}