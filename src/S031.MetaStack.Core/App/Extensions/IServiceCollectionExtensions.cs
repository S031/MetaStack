using S031.MetaStack.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Microsoft.Extensions.DependencyInjection
{
	using Extensions;
	public static class IServiceCollectionExtensions
	{
		private static readonly HashSet<string> _ctorRefs = new HashSet<string>();
		public static IServiceCollection Add<T>(this IServiceCollection services, string typeName,
			string assemblyName, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
		{
			typeName.NullTest(nameof(typeName));
			assemblyName.NullTest(nameof(assemblyName));

			var a = LoadAssembly(assemblyName);
			var sd = new ServiceDescriptor(typeof(T), a.GetType(typeName), serviceLifetime);
			services.Add(sd);
			return services;
		}

		public static IServiceCollection AddFromAssembly<T>(this IServiceCollection services,
			ServiceLifetime serviceLifetime = ServiceLifetime.Transient,
			Func<IServiceProvider, Type, object> implementationFactory = null,
			Assembly assembly = null)
		{
			var typesList = typeof(T).GetImplements(assembly);
			foreach (Type type in typesList)
			{
				var sd1 = services.FirstOrDefault(s => s.ServiceType == typeof(T));
				if (implementationFactory == null)
				{
					if (services.Any(sd => sd.ServiceType == typeof(T) &&
						sd.ImplementationType == type && sd.Lifetime == serviceLifetime))
						continue;
					services.Add(new ServiceDescriptor(typeof(T), type, serviceLifetime));
				}
				else
				{
					object ctor(IServiceProvider serviceProvider) => implementationFactory(serviceProvider, type);
					//Func<IServiceProvider, object> ctor = (serviceProvider) => implementationFactory(serviceProvider, type);
					if (services.Any(sd => sd.ServiceType == typeof(T) && 
						sd.Lifetime == serviceLifetime && sd.ImplementationFactory != null &&
						sd.ImplementationFactory(services.BuildServiceProvider()).GetType() == type))
						continue;
					services.Add(new ServiceDescriptor(serviceType: typeof(T), factory: ctor, lifetime: serviceLifetime));
					//services.TryAddEnumerable(new ServiceDescriptor(serviceType: typeof(T), factory: ctor, lifetime: serviceLifetime));
				}
			}
			return services;
		}

		private static Assembly LoadAssembly(string assemblyID)
		{
			Assembly a = AssemblyLoadContext.Default.LoadFromAssemblyPath(
				System.IO.Path.Combine(System.AppContext.BaseDirectory, $"{assemblyID}.dll"));
			return a;
		}

	}
}
