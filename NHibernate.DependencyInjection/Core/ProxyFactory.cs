using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using NHibernate.Proxy;
using NHibernate.Proxy.DynamicProxy;

namespace NHibernate.DependencyInjection.Core
{
	public sealed class ProxyFactory
	{
		//rippo: this was the original base constructor template defined in NH (notice it is typeof object)
		private static readonly ConstructorInfo BaseConstructor =
			typeof (object)
				.GetConstructor(new System.Type[0]);

		private static readonly MethodInfo GetTypeFromHandle =
			typeof (System.Type)
				.GetMethod("GetTypeFromHandle");

		private static readonly MethodInfo GetValue =
			typeof (SerializationInfo)
				.GetMethod("GetValue",
						   BindingFlags.Public | BindingFlags.Instance,
						   null,
						   new[] {typeof (string), typeof (System.Type)},
						   null);

		private static readonly MethodInfo SetType =
			typeof (SerializationInfo)
				.GetMethod("SetType",
						   BindingFlags.Public | BindingFlags.Instance,
						   null, new[] {typeof (System.Type)},
						   null);

		private static readonly MethodInfo AddValue =
			typeof (SerializationInfo)
				.GetMethod("AddValue",
						   BindingFlags.Public | BindingFlags.Instance,
						   null, new[] {typeof (string), typeof (object)},
						   null);

		public ProxyFactory() : this(new DefaultyProxyMethodBuilder()) {}

		public ProxyFactory(IProxyMethodBuilder proxyMethodBuilder)
		{
			if (proxyMethodBuilder == null)
			{
				throw new ArgumentNullException("proxyMethodBuilder");
			}
			ProxyMethodBuilder = proxyMethodBuilder;
			Cache = new ProxyCache();
		}

		public IProxyCache Cache { get; private set; }

		public IProxyMethodBuilder ProxyMethodBuilder { get; private set; }

		public object CreateProxy(System.Type instanceType, Proxy.DynamicProxy.IInterceptor interceptor, params System.Type[] baseInterfaces)
		{
			var proxyType = CreateProxyType(instanceType, baseInterfaces);
			var result = Activator.CreateInstance(proxyType);
			var proxy = (IProxy) result;
			proxy.Interceptor = interceptor;
			return result;
		}

		public System.Type CreateProxyType(System.Type baseType, params System.Type[] interfaces)
		{
			var baseInterfaces = ReferenceEquals(null, interfaces) 
				? new System.Type[0] 
				: interfaces.Where(t => t != null).ToArray();
			if (Cache.Contains(baseType, baseInterfaces))
			{
				return Cache.GetProxyType(baseType, baseInterfaces);
			}
			var result = CreateUncachedProxyType(baseType, baseInterfaces);
			if (result != null && Cache != null)
			{
				Cache.StoreProxyType(result, baseType, baseInterfaces);
			}
			return result;
		}

		private System.Type CreateUncachedProxyType(System.Type baseType, System.Type[] baseInterfaces)
		{
			var currentDomain = AppDomain.CurrentDomain;
			var typeName = string.Format("{0}Proxy", baseType.Name);
			var assemblyName = string.Format("{0}Assembly", typeName);
			var moduleName = string.Format("{0}Module", typeName);
			var name = new AssemblyName(assemblyName);

			//rippo: the commented lines allow the serialize of the assembly containing the proxy
			//you can then run PEVERIFY to check the IL for type safety

			//const AssemblyBuilderAccess access = AssemblyBuilderAccess.RunAndSave;
			const AssemblyBuilderAccess access = AssemblyBuilderAccess.Run;

			var assemblyBuilder = currentDomain.DefineDynamicAssembly(name, access);

			//var moduleBuilder = assemblyBuilder.DefineDynamicModule("generatedAssembly.dll", "generatedAssembly.dll", true);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);

			const TypeAttributes typeAttributes =
				TypeAttributes.AutoClass |
				TypeAttributes.Class |
				TypeAttributes.Public |
				TypeAttributes.BeforeFieldInit;
			var interfaces = new HashSet<System.Type>();
			interfaces.Merge(baseInterfaces);
			var parentType = baseType;
			if (baseType.IsInterface)
			{
				parentType = typeof (ProxyDummy);
				interfaces.Add(baseType);
			}
			var computedInterfaces = interfaces.ToArray();
			foreach (var interfaceType in computedInterfaces)
			{
				interfaces.Merge(GetInterfaces(interfaceType));
			}
			interfaces.Add(typeof (ISerializable));
			var typeBuilder = moduleBuilder.DefineType(typeName, typeAttributes, parentType, interfaces.ToArray());
			var defaultConstructor = DefineConstructor(typeBuilder, baseType);
			var implementor = new ProxyImplementor();
			implementor.ImplementProxy(typeBuilder);
			var interceptorField = implementor.InterceptorField;
			foreach (var method in GetProxiableMethods(baseType, interfaces).Where(method => method.DeclaringType != typeof(ISerializable)))
			{
				ProxyMethodBuilder.CreateProxiedMethod(interceptorField, method, typeBuilder);
			}
			AddSerializationSupport(baseType, baseInterfaces, typeBuilder, interceptorField, defaultConstructor);
			var proxyType = typeBuilder.CreateType();

			//assemblyBuilder.Save("generatedAssembly.dll");

			return proxyType;
		}

		private IEnumerable<System.Type> GetInterfaces(System.Type currentType)
		{
			return GetAllInterfaces(currentType);
		}

		private static IEnumerable<System.Type> GetAllInterfaces(System.Type currentType)
		{
			var interfaces = currentType.GetInterfaces();
			foreach (System.Type current in interfaces)
			{
				yield return current;
				foreach (var @interface in GetAllInterfaces(current))
				{
					yield return @interface;
				}
			}
		}

		private static IEnumerable<MethodInfo> GetProxiableMethods(System.Type type, IEnumerable<System.Type> interfaces)
		{
			const BindingFlags candidateMethodsBindingFlags =
				BindingFlags.Public |
				BindingFlags.NonPublic |
				BindingFlags.Instance;
			return 
				type.GetMethods(candidateMethodsBindingFlags)
					.Where(method=> method.IsProxiable())
					.Concat(interfaces.SelectMany(interfaceType => interfaceType.GetMethods()))
					.Distinct();
		}

		private static ConstructorBuilder DefineConstructor(TypeBuilder typeBuilder, System.Type baseType)
		{
			const MethodAttributes constructorAttributes = 
				MethodAttributes.Public |
				MethodAttributes.HideBySig | 
				MethodAttributes.SpecialName |
				MethodAttributes.RTSpecialName;
			var constructor =
				typeBuilder
				.DefineConstructor(constructorAttributes, CallingConventions.Standard, new System.Type[0]);
			var il = constructor.GetILGenerator();
			constructor.SetImplementationFlags(MethodImplAttributes.IL);
			il.Emit(OpCodes.Ldarg_0);
			//**bug**
			//IL.Emit(OpCodes.Call, baseConstructor);
			//**fix**
			//rippo: here we have replaced the object base type with the correct base type
			var typeConstructor = baseType.GetConstructor(System.Type.EmptyTypes);
			if (typeConstructor == null)
			{
				throw new InvalidOperationException(string.Format("Could not determine constructor for {0}", baseType));
			}
			il.Emit(OpCodes.Call, typeConstructor);
			//**end bug**
			il.Emit(OpCodes.Ret);
			return constructor;
		}

		private static void ImplementGetObjectData(System.Type baseType, System.Type[] baseInterfaces, TypeBuilder typeBuilder, FieldInfo interceptorField)
		{
			const MethodAttributes attributes =
				MethodAttributes.Public |
				MethodAttributes.HideBySig |
				MethodAttributes.Virtual;
			var parameterTypes = new[] {typeof (SerializationInfo), typeof (StreamingContext)};
			var methodBuilder =
				typeBuilder
					.DefineMethod("GetObjectData", attributes, typeof (void), parameterTypes);

			var il = methodBuilder.GetILGenerator();
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldtoken, typeof (ProxyObjectReference));
			il.Emit(OpCodes.Call, GetTypeFromHandle);
			il.Emit(OpCodes.Callvirt, SetType);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldstr, "__interceptor");
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, interceptorField);
			il.Emit(OpCodes.Callvirt, AddValue);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldstr, "__baseType");
			if (string.IsNullOrEmpty(baseType.AssemblyQualifiedName))
			{
				throw new InvalidOperationException(string.Format("Could not determine assembly qualified name for {0}", baseType));
			}
			il.Emit(OpCodes.Ldstr, baseType.AssemblyQualifiedName);
			il.Emit(OpCodes.Callvirt, AddValue);
			var baseInterfaceCount = baseInterfaces.Length;
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldstr, "__baseInterfaceCount");
			il.Emit(OpCodes.Ldc_I4, baseInterfaceCount);
			il.Emit(OpCodes.Box, typeof (Int32));
			il.Emit(OpCodes.Callvirt, AddValue);
			var index = 0;
			foreach (var baseInterface in baseInterfaces)
			{
				if (string.IsNullOrEmpty(baseInterface.AssemblyQualifiedName))
				{
					throw new InvalidOperationException(string.Format("Could not determine assembly qualified name for {0}", baseInterface));
				}
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Ldstr, string.Format("__baseInterface{0}", index++));
				il.Emit(OpCodes.Ldstr, baseInterface.AssemblyQualifiedName);
				il.Emit(OpCodes.Callvirt, AddValue);
			}
			il.Emit(OpCodes.Ret);
		}

		private static void DefineSerializationConstructor(TypeBuilder typeBuilder, FieldInfo interceptorField, ConstructorBuilder defaultConstructor)
		{
			const MethodAttributes constructorAttributes =
				MethodAttributes.Public |
				MethodAttributes.HideBySig |
				MethodAttributes.SpecialName |
				MethodAttributes.RTSpecialName;
			var parameterTypes = new[] {typeof (SerializationInfo), typeof (StreamingContext)};
			var constructor = typeBuilder
				.DefineConstructor(constructorAttributes,
								   CallingConventions.Standard,
								   parameterTypes);
			var il = constructor.GetILGenerator();
			var interceptorType = il.DeclareLocal(typeof(System.Type));
			constructor.SetImplementationFlags(MethodImplAttributes.IL);
			il.Emit(OpCodes.Ldtoken, typeof (Proxy.DynamicProxy.IInterceptor));
			il.Emit(OpCodes.Call, GetTypeFromHandle);
			il.Emit(OpCodes.Stloc, interceptorType);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, defaultConstructor);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Ldstr, "__interceptor");
			il.Emit(OpCodes.Ldloc, interceptorType);
			il.Emit(OpCodes.Callvirt, GetValue);
			il.Emit(OpCodes.Castclass, typeof (Proxy.DynamicProxy.IInterceptor));
			il.Emit(OpCodes.Stfld, interceptorField);
			il.Emit(OpCodes.Ret);
		}

		private static void AddSerializationSupport(System.Type baseType, System.Type[] baseInterfaces, TypeBuilder typeBuilder, FieldInfo interceptorField, ConstructorBuilder defaultConstructor)
		{
			var serializableConstructor = typeof(SerializableAttribute).GetConstructor(new System.Type[0]);
			if (serializableConstructor == null)
			{
				throw new InvalidOperationException("Could not determine constructor for SerializableAttribute");
			}
			var customAttributeBuilder = new CustomAttributeBuilder(serializableConstructor, new object[0]);
			typeBuilder.SetCustomAttribute(customAttributeBuilder);
			DefineSerializationConstructor(typeBuilder, interceptorField, defaultConstructor);
			ImplementGetObjectData(baseType, baseInterfaces, typeBuilder, interceptorField);
		}
	}
}