﻿using System;
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

			timer.Stop();

			//When the timer goes off, update results and return bssf
			results[ProblemAndSolver.COST] = costOfBssf().ToString();
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
