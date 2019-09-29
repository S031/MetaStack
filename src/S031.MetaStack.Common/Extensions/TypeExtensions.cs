using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Concurrent;

namespace S031.MetaStack.Common
{
	public static class TypeExtensions
	{
		static readonly object _obj4Lock = new object();
		static readonly ConcurrentDictionary<string, Func<object[], object>> _ctorCache =
			new ConcurrentDictionary<string, Func<object[], object>>();
		static readonly ConcurrentDictionary<Type, object> _instancesList = new ConcurrentDictionary<Type, object>();
		static readonly ConcurrentDictionary<string, ConstructorInfo> _ctorCache2 = new ConcurrentDictionary<string, ConstructorInfo>();

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
			if (_ctorCache.TryGetValue(key, out var ctor))
				return ctor(args);
			else if (_instancesList.TryGetValue(type, out instance))
				return instance;

			instance = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
			if (instance != null)
			{
				_instancesList.TryAdd(type, instance);
				return instance;
			}

			ctor = GetCtor(type.GetConstructor(args.Select(o => o.GetType()).ToArray()));
			_ctorCache[key] = ctor;
			return ctor(args);
		}

		private static Func<object[], object> GetCtor(ConstructorInfo ctorInfo)
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
			lock (_obj4Lock) _ctorCache2[key] = ctor;
			return (T)ctor.Invoke(args);
		}
		public static bool IsNumeric(this Type type, NumericTypesScope scope = NumericTypesScope.All)
		{
			type.NullTest(nameof(type));
			if (scope == NumericTypesScope.All)
				return vbo.AllNumericTypes.Contains(type);
			else if (scope == NumericTypesScope.Integral)
				return vbo.IntegralTypes.Contains(type);
			else if (scope == NumericTypesScope.Long)
				return type == typeof(long);
			else if (scope == NumericTypesScope.FloatingPoint)
				return vbo.FloatingPointTypes.Contains(type);
			return vbo.AllNumericTypes.Contains(type);
		}

		public static object GetDefaultValue(this Type type)
		{
			type.NullTest(nameof(type));
			if (_defaults.TryGetValue(type, out object result))
				return result;
			else if (type.IsValueType || type.IsPrimitive)
				return CreateInstance(type);
			return default;
		}

		private static readonly ReadOnlyCache<Type, object> _defaults = new ReadOnlyCache<Type, object>(
			(typeof(string), string.Empty),
			(typeof(DateTime), DateTime.MinValue),
			(typeof(bool), false),
			(typeof(byte), 0),
			(typeof(char), '\0'),
			(typeof(decimal), 0m),
			(typeof(double), 0d),
			(typeof(float), 0f),
			(typeof(int), 0),
			(typeof(long), 0L),
			(typeof(sbyte), 0),
			(typeof(short), 0),
			(typeof(uint), 0),
			(typeof(ulong), 0),
			(typeof(ushort), 0),
			(typeof(Guid), Guid.Empty)
			);

        public static IEnumerable<Type> GetImplements(this Type type, Assembly assembly = null)
        {
            IEnumerable<Assembly> l = assembly == null ? AppDomain.CurrentDomain.GetAssemblies() : new Assembly[] { assembly };
            foreach (var a in l)
#if NETCOREAPP
                if (!a.FullName.StartsWith("Microsoft.VisualStudio.TraceDataCollector", StringComparison.Ordinal))
#endif
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
