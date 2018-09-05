using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

namespace S031.MetaStack.Common
{
	public static class TypeExtensions
	{
		static readonly object objLock = new object();
		static readonly Dictionary<string, Func<object[], object>> _ctorCache =
			new Dictionary<string, Func<object[], object>>();
		static readonly Dictionary<Type, object> _instancesList = new Dictionary<Type, object>();

		static readonly Dictionary<string, ConstructorInfo> _ctorCache2 = new Dictionary<string, ConstructorInfo>();

		/// <summary>
		/// Create instance of Type with parameters, or returns Instance property if it exists
		/// </summary>
		/// <typeparam name="T">Returned type</typeparam>
		/// <param name="type">It's same</param>
		/// <param name="args">Constructor arguments</param>
		/// <returns></returns>
		public static T CreateInstance<T>(this Type type, params object[] args)
		{
			return (T)CreateInstance(type, args);
		}
		/// <summary>
		/// Create instance of Type with parameters, or returns Instance property if it exists
		/// </summary>
		/// <param name="type">Returned type</param>
		/// <param name="args">Constructor arguments</param>
		/// <returns></returns>
		public static object CreateInstance(this Type type, params object[] args)
		{
			type.NullTest(nameof(type));
			string key = (args.Length == 0) ? type.FullName : $"{type.FullName}_{ string.Join("_", args.Select(o => o.GetType().Name))}";
			object instance = null;
			lock (objLock)
			{
				if (_ctorCache.TryGetValue(key, out var ctor))
					return ctor(args);
				else if (_instancesList.TryGetValue(type, out instance))
					return instance;

				instance = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
				if (instance != null)
				{
					_instancesList[type] = instance;
					return instance;
				}

				ctor = getCtor(type.GetConstructor(args.Select(o => o.GetType()).ToArray()));
				_ctorCache[key] = ctor;
				return ctor(args);
			}
		}

		static Func<object[], object> getCtor(ConstructorInfo ctorInfo)
		{
			var argumentsExpression = Expression.Parameter(typeof(object[]), "arguments");
			var argumentExpressions = new List<Expression>();
			var parameterInfos = ctorInfo.GetParameters();
			for (var i = 0; i < parameterInfos.Length; ++i)
			{
				var parameterInfo = parameterInfos[i];
				argumentExpressions.Add(Expression.Convert(Expression.ArrayIndex(argumentsExpression, Expression.Constant(i)), parameterInfo.ParameterType));
			}
			var callExpression = Expression.New(ctorInfo, argumentExpressions);
			return Expression.Lambda<Func<object[], object>>(Expression.Convert(callExpression, typeof(object)), argumentsExpression).Compile();
		}
		/// <summary>
		/// For perfomance test
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static T CreateInstance2<T>(this Type type, params object[] args)
		{
			string key = (args.Length == 0) ? type.FullName : $"{type.FullName}_{ string.Join("_", args.Select(o => o.GetType().Name))}";
			if (_ctorCache2.TryGetValue(key, out var ctor))
				return (T)ctor.Invoke(args);
			ctor = type.GetConstructor(args.Select(o => o.GetType()).ToArray());
			lock (objLock) _ctorCache2[key] = ctor;
			return (T)ctor.Invoke(args);
		}
		public static bool IsNumeric(this Type type, NumericTypesScope scope = NumericTypesScope.All)
		{
			type.NullTest(nameof(type));
			if (scope == NumericTypesScope.Integral)
				return vbo.integralTypes.Contains(type);
			else if (scope == NumericTypesScope.Long)
				return type == typeof(long);
			else if (scope == NumericTypesScope.FloatingPoint)
				return vbo.floatingPointTypes.Contains(type);
			else
				return vbo.integralTypes.Contains(type) ||
					vbo.floatingPointTypes.Contains(type) || type == typeof(long);
		}

		public static object GetDefaultValue(this Type type)
		{
			if (type == typeof(string)) { return ""; }
			else if (type.IsNumeric()) { return 0; }
			else if (type == typeof(DateTime)) { return DateTime.MinValue; }
			else if (type == typeof(bool)) { return false; }
			else { return null; }

		}

		public static IEnumerable<Type> GetImplements(this Type type, Assembly assembly = null)
		{
			IEnumerable<Assembly> l = assembly == null ? AppDomain.CurrentDomain.GetAssemblies() : new Assembly[] { assembly };
			foreach (var a in l)
				foreach (Type t in a.GetTypes().Where(t => type.IsAssignableFrom(t) && !type.Equals(t)))
					yield return t;
		}
		public static string GetWorkName(this Assembly assembly)
		{
			assembly.NullTest(nameof(assembly));
			string fullName = assembly.FullName;
			return fullName.Left(fullName.IndexOf(','));
		}
	}
}
