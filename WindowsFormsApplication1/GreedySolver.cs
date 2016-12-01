using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace TSP
{
	public class GreedySolver
	{
		City[] cities;
		ProblemAndSolver.TSPSolution bssf;
		string[] results;

		public GreedySolver(City[] cities, ProblemAndSolver.TSPSolution bssf, string[] results)
		{
			this.cities = cities;
			this.bssf = bssf;
			this.results = results;
		}


		public ProblemAndSolver.TSPSolution Solve()
		{
			ArrayList route = new ArrayList();
			int numUpdates = -1;
			var timer = new Stopwatch();

			timer.Start();
			for (int startCity = 0; startCity < cities.Length; startCity++)
			{
				route.Clear();
				int currentCity = startCity;
				do
				{
					route.Add(cities[currentCity]);
					if (route.Count == cities.Length)
					{
						//go back to first city
						double pathCost = cities[currentCity].costToGetTo(cities[startCity]);
						if (double.IsPositiveInfinity(pathCost))
						{
							break;
						}
						ProblemAndSolver.TSPSolution solution = new ProblemAndSolver.TSPSolution(route);
						if (solution.costOfRoute() < costOfBssf())
						{
							bssf = solution;
							numUpdates++;
							break;
						}
					}

					double shortestRoute = double.PositiveInfinity;
					int nearestCity = -1;
					for (int i = 0; i < cities.Length; i++)
					{
						double pathCost = cities[currentCity].costToGetTo(cities[i]);
						if (pathCost < shortestRoute && !route.Contains(cities[i]))
						{
							shortestRoute = pathCost;
							nearestCity = i;
						}
					}
					if (nearestCity == -1)
					{
						//unable to find a path out of the current city to a new city
						break;
					}

					currentCity = nearestCity;

				} while (true);
			} 

			timer.Stop();

			results[ProblemAndSolver.COST] = costOfBssf().ToString();                          // load results array
			results[ProblemAndSolver.TIME] = timer.Elapsed.ToString();
			results[ProblemAndSolver.COUNT] = numUpdates.ToString();
			return bssf;
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
