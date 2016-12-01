using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TSP
{
	public class BranchAndBoundSolver
	{
		public static double averageDistance = 0;

		City[] cities;
		ProblemAndSolver.TSPSolution bssf;
		string[] results;
		PriorityQueue queue;
		int prunedNodes;
		int nodesCreated;
		int timeLimit;
		int numUpdates;

		public BranchAndBoundSolver(City[] cities, ProblemAndSolver.TSPSolution bssf, string[] results, int timeLimit)
		{
			this.cities = cities;
			this.bssf = bssf;
			this.results = results;
			this.timeLimit = timeLimit;
		}

		public ProblemAndSolver.TSPSolution Solve()
		{
			//Tests show that heap is better for more cities, array is better for less
			queue = new HeapPriorityQueue();
			nodesCreated = 0;
			prunedNodes = 0;
			averageDistance = 0;
			numUpdates = 0;
			var timer = new Stopwatch();
			timer.Start();
			//Build the mother node state
			//Build a starting matrix. time O(n^2) space O(n^2)
			int count = 0;
			double[,] motherMatrix = new double[cities.Length,cities.Length];
			for (int row = 0; row < cities.Length; row++)
			{
				for (int col = 0; col < cities.Length; col++)
				{
					double cost = cities[row].costToGetTo(cities[col]);
					motherMatrix[row, col] = row == col ? double.PositiveInfinity : cost;
					if (!double.IsPositiveInfinity(cost))
					{
						averageDistance += cost;
						count++;
					}
				}
			}
			averageDistance = averageDistance / count;
			//Reduce the matrix & get the bound. time O(n^2)
			double bound = ReduceMatrix(motherMatrix);
			ArrayList route = new ArrayList();
			route.Add(cities[0]);
			//Put them in a new state
			State motherState = new State(motherMatrix, route, bound, 0);
			//Expand the mother node state. time O(n^3) space O(n^3)
			ExpandState(motherState);
			//While the priority queue is not empty. Worst case Time O(n^3*b^n+log(n!)), with b > 1
			while (!queue.IsEmpty() && timer.ElapsedMilliseconds < timeLimit)
			{
				//pop off the top state and expand it. time O(n^3) space O(n^3)
				//DeleteMin could potentially be as bad as time O(logn!)
				ExpandState(queue.DeleteMin());
			}
			timer.Stop();

			//When the timer goes off, update results and return bssf
			results[ProblemAndSolver.COST] = costOfBssf().ToString();
			results[ProblemAndSolver.TIME] = timer.Elapsed.ToString();
			results[ProblemAndSolver.COUNT] = numUpdates.ToString();
			Console.WriteLine("Max Stored States: " + queue.LargestSize());
			Console.WriteLine("States Created: " + nodesCreated);
			Console.WriteLine("States Pruned: " + prunedNodes);
			return bssf;
		}

		/// <summary>
		/// Makes states for all the nodes in the graph that aren't in state's route and stores them in a priority queue
		/// Time O(n^3) Space O(n^3)
		/// </summary>
		/// <param name="state">parentState. The state to expand</param>
		public void ExpandState(State parentState)
		{
			//if the state's bound >= bssf: increment prunedNodes & return
			if (parentState.bound > costOfBssf())
			{
				prunedNodes++;
				return;
			}
			//If all the nodes in the graph are in the route (compare sizes): wrap up the route by traveling to the first node and checking the result against bssf. Update bssf if needed.
			if (parentState.route.Count >= cities.Count())
			{
				//Time O(n)
				double costToReturn = TravelInMatrix(parentState.matrix, parentState.lastCity, 0);
				if (!double.IsPositiveInfinity(costToReturn))
				{
					parentState.bound += costToReturn;
					parentState.route.Add(cities[0]);
					if (parentState.bound < costOfBssf())
					{
						bssf = new ProblemAndSolver.TSPSolution(parentState.route);
						numUpdates++;
					}
				}
				return;
			}
			//Else:
			//For each node in the graph that isn't in the route: time O(n^3) space O(n^3)
			for (int i = 0; i < cities.Count(); i++)
			{
				City city = cities[i];
				if (double.IsPositiveInfinity(cities[parentState.lastCity].costToGetTo(city)) || parentState.route.Contains(i))
				{
					continue;
				}
				//Copy the parent node state
				//Time O(n^2) size O(n^2)
				State childState = parentState.Copy();
				nodesCreated++;
				childState.route.Add(cities[i]);
				childState.lastCity = i;
				//Travel in the matrix to the new node and set the appropriate cells to infinity (TravelInMatrix). time O(n)
				double travelCost = TravelInMatrix(childState.matrix, parentState.lastCity, i);
				if (double.IsPositiveInfinity(travelCost))
				{
					continue;
				}
				childState.bound += travelCost;
				//Reduce the matrix and update the bound. time O(n^2)
				childState.bound += ReduceMatrix(childState.matrix);
				//If the bound is lower than bssf's:
				if (childState.bound < costOfBssf())
				{
					//add the state to the priority queue. time O(logn)
					queue.Insert(childState);
				}
				else
				{
					prunedNodes++;
				}
			}

		}

		/// <summary>
		/// Reduces the given matrix. Time O(n^2) Space O(1)
		/// </summary>
		/// <returns>The additional cost associated with reducing the matrix</returns>
		/// <param name="matrix">Matrix to reduce.</param>
		public double ReduceMatrix(double[,] matrix)
		{
			double cost = 0;
			//reduce each row. time O(n^2)
			for (int row = 0; row < cities.Count(); row++)
			{
				//find the smallest cost in the row
				double smallestCost = double.PositiveInfinity;
				for (int col = 0; col < cities.Count(); col++)
				{
					if (matrix[row, col] < smallestCost)
					{
						smallestCost = matrix[row, col];
					}
				}
				if (smallestCost <= 0.01 || double.IsPositiveInfinity(smallestCost))
				{
					continue;
				}
				cost += smallestCost;
				//reduce the row
				for (int col = 0; col < cities.Count(); col++)
				{
					matrix[row, col] -= smallestCost;
				}
			}
			//reduce each column. time O(n^2)
			for (int col = 0; col < cities.Count(); col++)
			{
				//find the smallest cost in the column
				double smallestCost = double.PositiveInfinity;
				for (int row = 0; row < cities.Count(); row++)
				{
					if (matrix[row, col] < smallestCost)
					{
						smallestCost = matrix[row, col];
					}
				}
				if (smallestCost <= 0.01 || double.IsPositiveInfinity(smallestCost))
				{
					continue;
				}
				cost += smallestCost;
				//reduce the column
				for (int row = 0; row < cities.Count(); row++)
				{
					matrix[row, col] -= smallestCost;
				}
			}

			return cost;
		}

		/// <summary>
		/// Sets the fromNode row, toNode column, and cell (toNode, fromNode) values to infinity if the travel is possible
		/// Time O(n) Space O(1)
		/// </summary>
		/// <param name="matrix">Matrix to mutate after travelling.</param>
		/// <param name="fromNode">From node.</param>
		/// <param name="toNode">To node.</param>
		/// <returns>The cost required to travel from node to node.</returns>
		public double TravelInMatrix(double[,] matrix, int fromNode, int toNode)
		{
			double cost = matrix[fromNode, toNode];
			if (double.IsPositiveInfinity(cost))
			{
				return double.PositiveInfinity;
			}
			matrix[toNode, fromNode] = double.PositiveInfinity;
			for (int i = 0; i < cities.Count(); i++)
			{
				matrix[fromNode, i] = double.PositiveInfinity;
				matrix[i, toNode] = double.PositiveInfinity;
			}

			return cost;
		}

		/// <summary>
		///  return the cost of the best solution so far. 
		/// </summary>
		/// <returns></returns>
		public double costOfBssf()
		{
			if (bssf != null)
				return (bssf.costOfRoute());
			else
				return double.PositiveInfinity;
		}
	}

	public class State
	{
		//(the states have a route property, so they know where they are in the tree without needing pointers)
		public double[,] matrix;
		public ArrayList route;
		public double bound;
		public int lastCity;

		//Only needed in heap implementation of PQ
		public int queueIndex;

		public State(double[,] matrix, ArrayList route, double bound, int lastCity)
		{
			this.matrix = matrix;
			this.route = route;
			this.bound = bound;
			this.lastCity = lastCity;
		}

		public State Copy()
		{
			return new State((double[,])matrix.Clone(), (ArrayList)route.Clone(), bound, lastCity);
		}

		public bool PrioritizeOver(State state)
		{
			//Weight the difference between route lengths by the average distance of an edge.
			int sizeDiff = state.route.Count - route.Count;
			double padding = sizeDiff * BranchAndBoundSolver.averageDistance;
			return padding + bound <= state.bound;
		}
	}



	public interface PriorityQueue
	{
		void Insert(State node);
		State DeleteMin();
		//void DecreaseKey(State node);
		bool IsEmpty();
		int LargestSize();
	}

	public class ArrayPriorityQueue : PriorityQueue
	{
		List<State> nodes;
		int maxNumNodes;

		public ArrayPriorityQueue()
		{
			nodes = new List<State>();
			maxNumNodes = 0;
		}

		/// <summary>
		/// Finds and returns the node with the smallest distance value. time: O(|V|)
		/// </summary>
		/// <returns>The node with the smallest distance value </returns>
		public State DeleteMin()
		{
			// iterate over queue and find the point that is closest to the path so far. 
			State closestNode = nodes[0];
			int indexOfClosest = 0;
			//Go over every node in the list. time: O(|V|)
			for (int i = 1; i < nodes.Count; i++)
			{
				State node = nodes[i];
				if (node.PrioritizeOver(closestNode))
				{
					closestNode = node;
					indexOfClosest = i;
				}
			}
			nodes.RemoveAt(indexOfClosest);
			return closestNode;
		}

		/// <summary>
		/// Insert the specified node into the queue. time: O(1) space: 1 + number of outgoing edges
		/// </summary>
		/// <param name="node">Node to insert.</param>
		public void Insert(State node)
		{
			nodes.Add(node);
			if (nodes.Count > maxNumNodes)
			{
				maxNumNodes = nodes.Count;
			}
		}

		public bool IsEmpty()
		{
			return nodes.Count == 0;
		}

		public int LargestSize()
		{
			return maxNumNodes;
		}

	}

	public class HeapPriorityQueue : PriorityQueue
	{
		List<State> queue;
		int maxNumNodes;

		public HeapPriorityQueue()
		{
			queue = new List<State>();
			maxNumNodes = 0;
		}

		/// <summary>
		/// Finds and returns the node with the smallest distance value. time: O(log|V|)
		/// </summary>
		/// <returns>The node with the smallest distance value </returns>
		public State DeleteMin()
		{
			// return the root node and settle the heap 
			State root = queue.First();
			root.queueIndex = -1;
			State lastNode = queue.Last();
			lastNode.queueIndex = 0;
			queue[0] = lastNode;
			queue.RemoveAt(queue.Count - 1);
			if (queue.Count > 0)
			{
				//time: O(log|V|)
				SiftDown(lastNode);
			}
			return root;
		}

		/// <summary>
		/// Insert the specified node. time: O(log|V|)
		/// </summary>
		/// <param name="node">Node.</param>
		public void Insert(State node)
		{
			//Add the node to the bottom of the heap and bubble up
			node.queueIndex = queue.Count;
			queue.Add(node);
			BubbleUp(node);
			if (queue.Count > maxNumNodes)
			{
				maxNumNodes = queue.Count;
			}
		}

		public bool IsEmpty()
		{
			return queue.Count == 0;
		}

		/// <summary>
		/// Bubbles up the given node. O(log|V|)
		/// </summary>
		/// <param name="node">Node to bubble up.</param>
		private void BubbleUp(State node)
		{
			if (node.queueIndex == 0)
			{
				return;
			}
			State parent = queue[(int)Math.Ceiling(node.queueIndex / (decimal)2.0) - 1];
			int position = node.queueIndex;
			//Switch the node with its parent until its parent is smaller that it.
			//In the worst case, the node goes from the bottom to the top.
			//This is a binary heap, so there can be at most log|V| switches. time: O(log|V|)
			while (position != 0 && node.PrioritizeOver(parent))
			{
				queue[position] = parent;

				int childPosition = position;
				position = parent.queueIndex;
				node.queueIndex = position;
				parent.queueIndex = childPosition;

				if (position == 0)
				{
					break;
				}

				parent = queue[(int)Math.Ceiling(position / (decimal)2.0) - 1];
			}
			queue[position] = node;
		}

		/// <summary>
		/// Sifts down the given node. time: O(log|V|)
		/// </summary>
		/// <param name="node">Node to sift down.</param>
		private void SiftDown(State node)
		{
			State minChild = MinChild(node);
			int position = node.queueIndex;
			//Switch the node with its smallest child until its smallest child is bigger that it.
			//In the worst case, the node goes from the top to the bottom.
			//This is a binary heap, so there can be at most log|V| switches. time: O(log|V|)
			while (minChild != null && minChild.PrioritizeOver(node))
			{
				queue[position] = minChild;

				int parentPosition = position;
				position = minChild.queueIndex;
				node.queueIndex = position;
				minChild.queueIndex = parentPosition;

				minChild = MinChild(node);
			}
			queue[position] = node;
		}

		/// <summary>
		/// Returns the smaller of the two children of the given node, if it has any children. time: O(1)
		/// </summary>
		/// <returns>The parent node.</returns>
		/// <param name="node">Node.</param>
		private State MinChild(State node)
		{
			if (1 + node.queueIndex * 2 >= queue.Count)
			{
				//No children
				return null;
			}

			State child1 = queue[node.queueIndex * 2 + 1];
			if (node.queueIndex * 2 + 2 >= queue.Count)
			{
				return child1;
			}

			State child2 = queue[node.queueIndex * 2 + 2];

			if (child2.PrioritizeOver(child1))
			{
				return child2;
			}
			else
			{
				return child1;
			}
		}

		public int LargestSize()
		{
			return maxNumNodes;
		}
	}

}
