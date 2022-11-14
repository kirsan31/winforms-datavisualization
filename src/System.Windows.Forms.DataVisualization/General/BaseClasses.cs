﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace System.Windows.Forms.DataVisualization.Charting
{
    /// <summary>
    /// ChartElement is the most basic element of the chart element hierarchy.
    /// </summary>
    public abstract class ChartElement : IChartElement
    {
        #region Member variables

        private IChartElement _parent;
        private CommonElements _common;
        private object _tag;

        #endregion Member variables

        #region Properties

        /// <summary>
        /// Gets or sets an object associated with this chart element.
        /// </summary>
        /// <value>
        /// An <see cref="Object"/> associated with this chart element.
        /// </value>
        /// <remarks>
        /// This property may be used to store additional data with this chart element.
        /// </remarks>
        [
        Browsable(false),
        DefaultValue(null),
        DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
        Utilities.SerializationVisibilityAttribute(Utilities.SerializationVisibility.Hidden)
        ]
        public object Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        /// <summary>
        /// Gets or sets the parent chart element or collection.
        /// </summary>
        /// <value>The parent chart element or collection.</value>
        internal virtual IChartElement Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        /// <summary>
        /// Gets a shortcut to Common instance providing access to the various chart related services.
        /// </summary>
        /// <value>The Common instance.</value>
        internal CommonElements Common
        {
            get
            {
                if (_common == null && _parent != null)
                {
                    _common = _parent.Common;
                }
                return _common;
            }
            set
            {
                _common = value;
            }
        }

        /// <summary>
        /// Gets the chart.
        /// </summary>
        /// <value>The chart.</value>
        internal Chart Chart
        {
            get
            {
                if (Common != null)
                    return Common.Chart;
                else
                    return null;
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartElement"/> class.
        /// </summary>
        protected ChartElement()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartElement"/> class.
        /// </summary>
        /// <param name="parent">The parent chart element or collection.</param>
        internal ChartElement(IChartElement parent)
        {
            _parent = parent;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Invalidates this chart element.
        /// </summary>
        internal virtual void Invalidate()
        {
            _parent?.Invalidate();
        }

        #endregion Methods

        #region IChartElement Members

        IChartElement IChartElement.Parent
        {
            get { return _parent; }
            set { this.Parent = value; }
        }

        void IChartElement.Invalidate()
        {
            this.Invalidate();
        }

        CommonElements IChartElement.Common
        {
            get { return this.Common; }
        }

        #endregion IChartElement Members

        #region Methods

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <remarks>For internal use.</remarks>
        internal virtual string ToStringInternal()
        {
            return GetType().Name;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return this.ToStringInternal();
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        /// <remarks>For internal use.</remarks>
        internal virtual bool EqualsInternal(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
        public override bool Equals(object obj)
        {
            return this.EqualsInternal(obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion Methods
    }

    /// <summary>
    /// ChartNamedElement is a base class for most chart elements. Series, ChartAreas, Legends and other chart elements have a Name and reuse the unique name generation and validation logic provided by the ChartNamedElementCollection.
    /// </summary>
    public abstract class ChartNamedElement : ChartElement
    {
        #region Member variables

        private string _name = String.Empty;

        #endregion Member variables

        #region Properties

        /// <summary>
        /// Gets or sets the name of the chart element.
        /// </summary>
        /// <value>The name.</value>
        [DefaultValue("")]
        public virtual string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    if (Parent is INameController nameController)
                    {
                        if (!nameController.IsUniqueName(value))
                            throw new ArgumentException(SR.ExceptionNameAlreadyExistsInCollection(value, nameController.GetType().Name));

                        // Fire the name change events in case when the old name is not empty
                        NameReferenceChangedEventArgs args = new NameReferenceChangedEventArgs(this, _name, value);
                        nameController.OnNameReferenceChanging(args);
                        nameController.ChangeName(_name, value);
                        _name = value;
                        nameController.OnNameReferenceChanged(args);
                    }
                    else
                    {
                        _name = value;
                    }
                    Invalidate();
                }
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartNamedElement"/> class.
        /// </summary>
        protected ChartNamedElement()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartNamedElement"/> class.
        /// </summary>
        /// <param name="name">The name of the new chart element.</param>
        protected ChartNamedElement(string name)
            : base()
        {
            _name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartNamedElement"/> class.
        /// </summary>
        /// <param name="parent">The parent chart element.</param>
        /// <param name="name">The name of the new chart element.</param>
        internal ChartNamedElement(IChartElement parent, string name) : base(parent)
        {
            _name = name;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        internal override string ToStringInternal()
        {
            string typeName = GetType().Name;
            return string.IsNullOrEmpty(_name) ? typeName : typeName + '-' + _name;
        }

        #endregion Methods
    }

    /// <summary>
    /// NameReferenceChanged events help chart maintain referential integrity.
    /// </summary>
    internal class NameReferenceChangedEventArgs : EventArgs
    {
        #region Member variables

        private readonly ChartNamedElement _oldElement;
        readonly string _oldName;
        readonly string _newName;

        #endregion MemberValiables

        #region Properties

        public ChartNamedElement OldElement
        {
            get { return _oldElement; }
        }

        public string OldName
        {
            get { return _oldName; }
        }

        public string NewName
        {
            get { return _newName; }
        }

        #endregion Properties

        #region Constructor

        public NameReferenceChangedEventArgs(ChartNamedElement oldElement, ChartNamedElement newElement)
        {
            _oldElement = oldElement;
            _oldName = oldElement != null ? oldElement.Name : string.Empty;
            _newName = newElement != null ? newElement.Name : string.Empty;
        }

        public NameReferenceChangedEventArgs(ChartNamedElement oldElement, string oldName, string newName)
        {
            _oldElement = oldElement;
            _oldName = oldName;
            _newName = newName;
        }

        #endregion Constructor
    }
}
