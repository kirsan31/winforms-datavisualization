// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//  Purpose:	Keep track of all registered formula module types.
//

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace System.Windows.Forms.DataVisualization.Charting.Formulas
{
    /// <summary>
    /// Keep track of all registered formula modules types.
    /// </summary>
    internal sealed class FormulaRegistry : IServiceProvider
    {
        #region Fields
        // Storage for all registered formula modules
        private readonly Dictionary<string, Type> _registeredModules = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IFormula> _createdModules = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _modulesNames = [];
        #endregion Fields

        #region Methods

        /// <summary>
        /// Formula Registry public constructor
        /// </summary>
        public FormulaRegistry()
        {
        }

        /// <summary>
        /// Adds modules into the registry.
        /// </summary>
        /// <param name="name">Module name.</param>
        /// <param name="moduleType">Module class type.</param>
        public void Register(string name, Type moduleType)
        {
            // First check if module with specified name already registered
            if (_registeredModules.TryGetValue(name, out var curT))
            {
                // If same type provided - ignore
                if (curT == moduleType)
                    return;

                // Error - throw exception
                throw new ArgumentException(SR.ExceptionFormulaModuleNameIsNotUnique(name));
            }

            // Add Module Name
            _modulesNames.Add(name);

            // Make sure that specified class support IFormula interface
            bool found = false;
            Type[] interfaces = moduleType.GetInterfaces();
            foreach (Type type in interfaces)
            {
                if (type == typeof(IFormula))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                throw new ArgumentException(SR.ExceptionFormulaModuleHasNoInterface);
            }

            // Add formula module to the hash table
            _registeredModules[name] = moduleType;
        }

        /// <summary>
        /// Returns formula module registry service object.
        /// </summary>
        /// <param name="serviceType">Service AxisName.</param>
        /// <returns>Service object.</returns>
        [EditorBrowsableAttribute(EditorBrowsableState.Never)]
        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(FormulaRegistry))
                return this;

            throw new ArgumentException(SR.ExceptionFormulaModuleRegistryUnsupportedType(serviceType.ToString()));
        }

        /// <summary>
        /// Returns formula module object by name.
        /// </summary>
        /// <param name="name">Formula Module name.</param>
        /// <returns>Formula module object derived from IFormula.</returns>
        public IFormula GetFormulaModule(string name)
        {
            // Check if the formula module object is already created
            if (_createdModules.TryGetValue(name, out var curT))
                return curT;

            // Check if formula module with specified name registered
            if (!_registeredModules.TryGetValue(name, out var regT))
                throw new ArgumentException(SR.ExceptionFormulaModuleNameUnknown(name));

            // Create formula module type object
            var res = (IFormula)regT.Assembly.CreateInstance(regT.ToString());
            _createdModules[name] = res;
            return res;
        }

        /// <summary>
        /// Returns the name of the module.
        /// </summary>
        /// <param name="index">Module index.</param>
        /// <returns>Module Name.</returns>
        public string GetModuleName(int index)
        {
            return _modulesNames[index];
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Return the number of registered modules.
        /// </summary>
        public int Count
        {
            get
            {
                return _modulesNames.Count;
            }
        }

        #endregion Properties
    }

    /// <summary>
    /// Interface which defines the set of standard methods and
    /// properties for each formula module
    /// </summary>
	internal interface IFormula
    {
        #region IFormula Properties and Methods

        /// <summary>
        /// Formula Module name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The first method in the module, which converts a formula 
        /// name to the corresponding private method.
        /// </summary>
        /// <param name="formulaName">String which represent a formula name</param>
        /// <param name="inputValues">Arrays of doubles - Input values</param>
        /// <param name="outputValues">Arrays of doubles - Output values</param>
        /// <param name="parameterList">Array of strings - Formula parameters</param>
        /// <param name="extraParameterList">Array of strings - Extra Formula parameters from DataManipulator object</param>
        /// <param name="outLabels">Array of strings - Used for Labels. Description for output results.</param>
		void Formula(string formulaName, double[][] inputValues, out double[][] outputValues, string[] parameterList, string[] extraParameterList, out string[][] outLabels);

        #endregion IFormula Properties and Methods
    }
}
