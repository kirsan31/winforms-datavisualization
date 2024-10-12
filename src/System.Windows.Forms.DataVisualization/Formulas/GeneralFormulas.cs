// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	This class calculates Running total and average.
//				Could be used for Pareto chart.
//

namespace System.Windows.Forms.DataVisualization.Charting.Formulas
{
    /// <summary>
    /// This class calculates Running total and average.
    /// Could be used for Pareto chart
    /// </summary>
    internal sealed class GeneralFormulas : PriceIndicators
    {
        #region Properties

        /// <summary>
        /// Formula Module name
        /// </summary>
        public override string Name
        { get { return SR.FormulaNameGeneralFormulas; } }

        #endregion Properties

        #region Formulas

        /// <summary>
        /// Formula which calculates cumulative total.
        /// ---------------------------------------------------------
        /// Input: 
        /// 	- Y values.
        /// Output: 
        /// 	- Running Total.
        /// </summary>
        /// <param name="inputValues">Arrays of doubles: 1. row - X values, 2. row - Y values</param>
        /// <param name="outputValues">Arrays of doubles: 1. row - X values, 2. row - Moving average</param>
        private static void RuningTotal(double[][] inputValues, out double[][] outputValues)
        {
            // There is not enough series
            if (inputValues.Length != 2)
            {
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);
            }

            // Different number of x and y values
            CheckNumOfValues(inputValues, 1);

            outputValues = new double[2][];

            outputValues[0] = new double[inputValues[0].Length];
            outputValues[1] = new double[inputValues[1].Length];

            // Cumulative total
            for (int index = 0; index < inputValues[0].Length; index++)
            {
                outputValues[0][index] = inputValues[0][index];

                if (index > 0)
                {
                    outputValues[1][index] = inputValues[1][index] + outputValues[1][index - 1];
                }
                else
                {
                    outputValues[1][index] = inputValues[1][index];
                }
            }
        }

        /// <summary>
        /// Running Average Formula
        /// ---------------------------------------------------------
        /// Input: 
        /// 	- Y values.
        /// Output: 
        /// 	- Running Average.
        /// </summary>
        /// <param name="inputValues">Arrays of doubles: 1. row - X values, 2. row - Y values</param>
        /// <param name="outputValues">Arrays of doubles: 1. row - X values, 2. row - Moving average</param>
		private static void RunningAverage(double[][] inputValues, out double[][] outputValues)
        {
            // There is no enough series
            if (inputValues.Length != 2)
                throw new ArgumentException(SR.ExceptionPriceIndicatorsFormulaRequiresOneArray);

            // Different number of x and y values
            CheckNumOfValues(inputValues, 1);

            outputValues = new double[2][];

            outputValues[0] = new double[inputValues[0].Length];
            outputValues[1] = new double[inputValues[1].Length];

            // Total
            double total = 0;
            for (int index = 0; index < inputValues[0].Length; index++)
            {
                total += inputValues[1][index];
            }

            // Runing Average
            for (int index = 0; index < inputValues[0].Length; index++)
            {
                outputValues[0][index] = inputValues[0][index];

                if (index > 0)
                    outputValues[1][index] = inputValues[1][index] / total * 100 + outputValues[1][index - 1];
                else
                    outputValues[1][index] = inputValues[1][index] / total * 100;
            }
        }

        #endregion Formulas

        #region Methods

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GeneralFormulas()
        {
        }

        /// <summary>
        /// The first method in the module, which converts a formula 
        /// name to the corresponding private method.
        /// </summary>
        /// <param name="formulaName">String which represent a formula name.</param>
        /// <param name="inputValues">Arrays of doubles - Input values.</param>
        /// <param name="outputValues">Arrays of doubles - Output values.</param>
        /// <param name="parameterList">Array of strings - Formula parameters.</param>
        /// <param name="extraParameterList">Array of strings - Extra Formula parameters from DataManipulator object.</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		public override void Formula(string formulaName, double[][] inputValues, out double[][] outputValues, string[] parameterList, string[] extraParameterList, out string[][] outLabels)
        {
            outputValues = null;
            // Not used for these formulas.
            outLabels = null;

            try
            {
                if (string.Equals(formulaName, "RUNINGTOTAL", StringComparison.OrdinalIgnoreCase))
                {
                    RuningTotal(inputValues, out outputValues);
                }
                else if (string.Equals(formulaName, "RUNINGAVERAGE", StringComparison.OrdinalIgnoreCase))
                {
                    RunningAverage(inputValues, out outputValues);
                }
                else
                {
                    outputValues = null;
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException(SR.ExceptionFormulaInvalidPeriod(formulaName));
            }
            catch (OverflowException)
            {
                throw new InvalidOperationException(SR.ExceptionFormulaNotEnoughDataPoints(formulaName));
            }
        }

        #endregion Methods
    }
}
