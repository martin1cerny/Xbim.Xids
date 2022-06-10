﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Xbim.InformationSpecifications.Helpers
{
	/// <summary>
	/// Provides static methods to get the collection of classes in the published schemas.
	/// </summary>
	public partial class SchemaInfo : IEnumerable<ClassInfo>
	{
        public SchemaInfo()
        {
			Classes = new Dictionary<string, ClassInfo>();
			AttributesToAllClasses = new Dictionary<string, string[]>();
			AttributesToTopClasses = new Dictionary<string, string[]>();
        }

		/// <summary>
		/// Ensures that the information up to date.
		/// </summary>
		bool linked = false;
		private Dictionary<string, ClassInfo> Classes;

		/// <summary>
		/// from the attribute name to the names of the classes that have the attribute.
		/// </summary>
		private Dictionary<string, string[]> AttributesToAllClasses { get; set; }

		/// <summary>
		/// from the attribute name to the names of the minimum set of classes that declare the attribute (no subclasses).
		/// </summary>
		private Dictionary<string, string[]> AttributesToTopClasses { get; set; }

		/// <summary>
		/// Get the classinfo by name string.
		/// </summary>
		public ClassInfo? this[string className]
		{
			get
			{
				if (Classes.TryGetValue(className, out var cl))
				{
					return cl;
				}
				return Classes.Values.FirstOrDefault(x => x.Name.Equals(className, StringComparison.InvariantCultureIgnoreCase));
			}
		}

		/// <summary>
		/// Add a new classInfo to the collection
		/// </summary>
		public void Add(ClassInfo classToAdd)
		{
			linked = false;
			if (Classes == null)
				Classes = new Dictionary<string, ClassInfo>();
			Classes.Add(classToAdd.Name, classToAdd);
		}

		/// <summary>
		/// Ensure relationships between classes are correct.
		/// </summary>
		private void LinkTree()
		{
			foreach (var currClass in Classes.Values)
			{
				var parent = currClass.ParentName;
				if (!string.IsNullOrWhiteSpace(parent) && Classes.TryGetValue(parent, out var gotten))
				{
					if (!gotten.SubClasses.Any(x => x.Name == currClass.Name))
					{
						gotten.SubClasses.Add(currClass);
					}
					currClass.Parent = gotten;
				}
			}		
			linked = true;
		}

		private static SchemaInfo? schemaIFC4;
		/// <summary>
		/// Static property for the Ifc4 schema
		/// </summary>
		public static SchemaInfo SchemaIfc4
		{
			get
			{
				if (schemaIFC4 == null)
				{
					var t = GetClassesIFC4();
					GetRelationTypesIFC4(t);
					GetAttributesIFC4(t);
					SetTypeObject(t, "IfcTypeObject");
					schemaIFC4 = t;
				}
				return schemaIFC4;
			}
		}

		public static IfcMeasureInfo? GetMeasure(string type)
        {
			return IfcMeasures.Values.FirstOrDefault(x => x.IfcMeasure == type);
		}

		public static IfcMeasureInfo GetMeasure(IfcMeasures meas)
		{
			return IfcMeasures[meas.ToString()];
		}

		private static void SetTypeObject(SchemaInfo t, string topTypeObjectClass)
        {
            foreach (var cls in t.Classes.Values)
            {
				if (cls.Is(topTypeObjectClass))
					cls.FunctionalType = FunctionalType.TypeOfElement;
            }
        }

		/// <summary>
		/// Returns names of the classes that have an attribute.
		/// /// See <seealso cref="GetAttributeRelations(string)"/> for similar function with different return type.
		/// </summary>
		/// <param name="attributeName">The attribute being sought</param>
		/// <param name="onlyTopClasses">reduces the return to the minimum set of top level classes that have the attribute (no subclasses)</param>
		/// <returns>enumeration of class names, possibly empty, if not found</returns>
		public string[] GetAttributeClasses(string attributeName, bool onlyTopClasses = false)
		{
			var toUse = onlyTopClasses
				? AttributesToTopClasses
				: AttributesToAllClasses;
			if (toUse.TryGetValue(attributeName, out var ret))
				return ret;
            return Array.Empty<string>();
		}

		private readonly Dictionary<string, ClassRelationInfo[]> relAttributes = new();

		/// <summary>
		/// Provides information of classes that have an attribute and the form of the relation to it.
		/// See <seealso cref="GetAttributeClasses(string, bool)"/> for similar function with different return type.
		/// </summary>
		/// <param name="attributeName">Name of the attribute in question</param>
		/// <returns></returns>
		public IEnumerable<ClassRelationInfo> GetAttributeRelations(string attributeName)
		{
			if (relAttributes.TryGetValue(attributeName, out var ret))
				return ret;
			var tmp = new List<ClassRelationInfo>();
            foreach (var className in GetAttributeClasses(attributeName, true))
            {
				var cls = this[className];
				if (cls == null)
					continue;
				var tp = cls.FunctionalType == FunctionalType.TypeOfElement
					? ClassAttributeMode.ViaRelationType
					: ClassAttributeMode.ViaElement;
				tmp.Add(new ClassRelationInfo()
					{
						ClassName = className,
						Connection = tp
					}
					);
            }
			var t = tmp.ToArray();
			relAttributes.Add(attributeName, t);
			return t;
		}

		
		public enum ClassAttributeMode
        {
			/// <summary>
			/// The attribute is directly defined in an IfcClass
			/// </summary>
			ViaElement = 1,
			/// <summary>
			/// The attribute is defined in the type that can be related to an IfcClass
			/// </summary>
			ViaRelationType = 2,
		}

		/// <summary>
		/// A structure contianing information about the ways in which an attribute is related to a class
		/// </summary>
		public struct ClassRelationInfo
        {
			public string ClassName { get; set; }
			public ClassAttributeMode Connection { get; set; }
        }

		private static SchemaInfo? schemaIFC2x3;
		/// <summary>
		/// Static property for the Ifc2x3 schema
		/// </summary>
		public static SchemaInfo SchemaIfc2x3
		{
			get
			{
				if (schemaIFC2x3 == null)
				{
					var t = GetClassesIFC2x3();
					GetRelationTypesIFC2x3(t);
					GetAttributesIFC2x3(t);
					SetTypeObject(t, "IfcTypeObject");
					schemaIFC2x3 = t;
				}
				return schemaIFC2x3;
			}
		}

		static partial void GetRelationTypesIFC2x3(SchemaInfo schema);
		static partial void GetRelationTypesIFC4(SchemaInfo schema);
        
		internal void SetRelationType(string objClass, IEnumerable<string> typeClasses)
        {
			var c = this[objClass];
			c?.SetTypeClasses(typeClasses);
            foreach (var typeClass in typeClasses)
            {
				var tpC = this[typeClass];
				if (tpC != null)
					tpC.FunctionalType = FunctionalType.TypeOfElement;
            }
        }


		private static partial SchemaInfo GetClassesIFC2x3();

		/// <summary>
		/// Returns all attribute names in the schema
		/// </summary>
		public IEnumerable<string> GetAttributeNames()
		{
			return AttributesToAllClasses.Keys;
		}

		private static partial SchemaInfo GetClassesIFC4();

		static partial void GetAttributesIFC2x3(SchemaInfo destinationSchema);
		static partial void GetAttributesIFC4(SchemaInfo destinationSchema);

		private void AddAttribute(string attributeName, string[] topClassNames, string[] allClassNames)
		{
			if (AttributesToAllClasses == null)
				AttributesToAllClasses = new Dictionary<string, string[]>();
			AttributesToAllClasses.Add(attributeName, allClassNames);

			if (AttributesToTopClasses == null)
				AttributesToTopClasses = new Dictionary<string, string[]>();
			AttributesToTopClasses.Add(attributeName, topClassNames);
		}

		/// <summary>
		/// The default enumerator for the schema returns the classes defined within
		/// </summary>
		public IEnumerator<ClassInfo> GetEnumerator()
		{
			if (!linked)
				LinkTree();
			return Classes.Values.GetEnumerator();
		}

		/// <summary>
		/// The default enumerator for the schema returns the classes defined within
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			if (!linked)
				LinkTree();
			return Classes.Values.GetEnumerator();
		}

		private static Dictionary<string, object>? _dicUnits;

		public static bool TryGetUnit(string unit, [NotNullWhen(true)] out object? found)
		{
			if (_dicUnits == null)
			{
				_dicUnits = new Dictionary<string, object>();
				foreach (var item in IfcMeasures.Values)
				{
					if (!_dicUnits.ContainsKey(item.UnitSymbol))
					{
						_dicUnits.Add(item.UnitSymbol, item);
					}
				}
			}
			if (_dicUnits.ContainsKey(unit))
			{
				found = _dicUnits[unit];
				return true;
			}
			found = null;
			return false;
		}
	}
}
