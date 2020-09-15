using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Concurrent;

namespace S031.MetaStack.Common
{
#if NO_COMMON
	internal enum NumericTypesScope
	{
		None,
		Integral,
		FloatingPoint,
		All,
	}
	internal static class TypeExtensions
	{
		public static void NullTest(this object value, string valueName) =>
			NullTest(value, valueName, (v, n) => { throw new ArgumentNullException(n); });

		public static void NullTest(this object value, string valueName, Action<object, string> action)
		{
			if (value == null) action(value, valueName);
		}
#else
	public static class TypeExtensions
	{
#endif
		static readonly object _obj4Lock = new object();
		static readonly MapTable<string, Func<object[], object>> _ctorCache =
			new MapTable<string, Func<object[], object>>();
		static readonly MapTable<Type, object> _instancesList = new MapTable<Type, object>();
		static readonly MapTable<string, ConstructorInfo> _ctorCache2 = new MapTable<string, ConstructorInfo>();

		private static readonly MapTable<string, Type> _typeCache = new MapTable<string, Type>();

		public static Type GetTypeByAssemblyQualifiedName(string name)
		{
			if (!_typeCache.TryGetValue(name, out Type t))
			{
				t = Type.GetType(name);
				_typeCache.Add(name, t);
			}
			return t;
		}

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
				_instancesList.AddOrUpdate(type, instance);
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
		public static IEnumerable<Type> GetImplements(this Type type, Assembly assembly = null)
        {
            IEnumerable<Assembly> l = assembly == null ? AppDomain.CurrentDomain.GetAssemblies() : new Assembly[] { assembly };
            foreach (var a in l)
#if NETCOREAPP //&& DEBUG
				if (!a.FullName.StartsWith("Microsoft.VisualStudio.TraceDataCollector", StringComparison.Ordinal))
#endif
                    foreach (Type t in a.GetTypes().Where(t => type.IsAssignableFrom(t) && !type.Equals(t)))
                        yield return t;
        }
		public static string GetWorkName(this Assembly assembly)
		{
			assembly.NullTest(nameof(assembly));
			string fullName = assembly.FullName;
			return fullName.Substring(0, fullName.IndexOf(','));
		}
		
		public static object GetDefaultValue(this Type type)
		{
			TypeCode code = Type.GetTypeCode(type);
			if (code == TypeCode.Object)
			{
				if (type == typeof(Guid))
					return Guid.Empty;
				if (type.IsValueType || type.IsPrimitive)
					return type.CreateInstance();
				return default;
			}
			return _defaults[(int)code].DefaultValue;
		}

		private struct __typeInfo
		{
			public readonly object DefaultValue;
			public readonly NumericTypesScope NumericType;

			public __typeInfo(object defaultValue, NumericTypesScope numericType)
			{
				DefaultValue = defaultValue;
				NumericType = numericType;
			}
		}

		private static readonly __typeInfo[] _defaults = new __typeInfo[]
		{
			new __typeInfo( null, NumericTypesScope.None ),
			new __typeInfo( null, NumericTypesScope.None ),
			new __typeInfo( DBNull.Value, NumericTypesScope.None ),
			new __typeInfo( false, NumericTypesScope.None ),
			new __typeInfo( '\0', NumericTypesScope.Integral ),
			new __typeInfo( (sbyte)0, NumericTypesScope.Integral ),
			new __typeInfo( (byte)0, NumericTypesScope.Integral ),
			new __typeInfo( (short)0, NumericTypesScope.Integral ),
			new __typeInfo( (ushort)0, NumericTypesScope.Integral ),
			new __typeInfo( 0, NumericTypesScope.Integral ),
			new __typeInfo( (uint)0, NumericTypesScope.Integral ),
			new __typeInfo( 0L, NumericTypesScope.Integral ),
			new __typeInfo( (ulong)0, NumericTypesScope.Integral ),
			new __typeInfo( 0f, NumericTypesScope.FloatingPoint ),
			new __typeInfo( 0d, NumericTypesScope.FloatingPoint ),
			new __typeInfo( 0m, NumericTypesScope.FloatingPoint ),
			new __typeInfo( default(DateTime), NumericTypesScope.None ),
			new __typeInfo( Guid.Empty, NumericTypesScope.None ),
			new __typeInfo( string.Empty, NumericTypesScope.None )
		};


		public static bool IsNumeric(this Type type, NumericTypesScope scope = NumericTypesScope.All)
		{
			if (type == null)
				return false;

			if (type.IsEnum &&
				(scope == NumericTypesScope.All || scope == NumericTypesScope.Integral))
				return true;

			TypeCode code = Type.GetTypeCode(type);
			__typeInfo ti = _defaults[(int)code];
			if (scope == NumericTypesScope.All)
				return ti.NumericType == NumericTypesScope.Integral
					|| ti.NumericType == NumericTypesScope.FloatingPoint;
			
			return ti.NumericType == scope;
		}
	}
}
