using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;

namespace TSP
{
	public class KOptLocalSolver
	{
		City[] cities;
		ProblemAndSolver.TSPSolution bssf;
		string[] results;
		int timeLimit;
		int numUpdates;

		public KOptLocalSolver(City[] cities, ProblemAndSolver.TSPSolution bssf, string[] results, int timeLimit)
		{
			this.cities = cities;
			this.bssf = bssf;
			this.results = results;
			this.timeLimit = timeLimit;
		}

		public ProblemAndSolver.TSPSolution Solve()
		{
			numUpdates = 0;
			var timer = new Stopwatch();
			timer.Start();

			//--- Algorithm here ---

			var changed = true;
			while (changed)
			{
				changed = false;
				for (int i = cities.Length - 1; i >= 0; i--)
				{
					for (int j = cities.Length - 1; j >= 0; j--)
					{
						ArrayList swapped = Swap(bssf.Route, i, j);
						ProblemAndSolver.TSPSolution swappedSolution = new ProblemAndSolver.TSPSolution(swapped);
						if (swappedSolution.costOfRoute() < bssf.costOfRoute())
						{
							bssf = swappedSolution;
							changed = true;//15438
							System.Console.WriteLine(i + " " + j);
						}
					}
				}
			}

			//set a 'changed' flag to true
			//while changed
			//	for each pair (or 3-set, etc.) of edges
			//		swap the pair/set
			//		if the route is valid and its length is better, save it as the new bssf and set changed to true, and break out
			//Note: This algorithm is short-sighted. It grabs the first route that is better and uses it.
			//Instead, we could consider getting all of the new routes and setting the best one to the bssf, or keep the top three or four and try them all.

			//--- End algorithm ---

			timer.Stop();

			//When the timer goes off, update results and return bssf
			results[ProblemAndSolver.COST] = costOfBssf().ToString();
			results[ProblemAndSolver.TIME] = timer.Elapsed.ToString();
			results[ProblemAndSolver.COUNT] = numUpdates.ToString();
			return bssf;
		}

		/// <summary>
		/// Swap the destinations for the edges coming out of the given cities
		/// </summary>
		/// <param name="route">Route. The route in which to swap</param>
		/// <param name="city1">City1. The first city to swap</param>
		/// <param name="city2">City2. The second city to swap</param>
		/// <returns>The new route with the swapped destinations, or null if it is impossible</returns>
		private ArrayList Swap(ArrayList route, int city1, int city2)
		{
			City start1 = (City)route[city1];
			//City end1 = (City)route[city1 + 1 < route.Count ? city1 + 1 : 0];
			City start2 = (City)route[city2];
			//City end2 = (City)route[city2 + 1 < route.Count ? city2 + 1 : 0];
			//if (double.IsPositiveInfinity(start1.costToGetTo(end2)) || double.IsPositiveInfinity(start2.costToGetTo(end1)))
			//{
			//	return null;
			//}
			//swap is possible
			//make a new route with the swapped edges, and return it.
			ArrayList swapped = (ArrayList)route.Clone();
			swapped[city1] = start2;
			swapped[city2] = start1;
			return swapped;
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
}
