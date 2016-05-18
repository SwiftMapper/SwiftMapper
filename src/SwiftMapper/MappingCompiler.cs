using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace SwiftMapper
{
    internal class MappingCompiler
    {
        private const string StaticMethodName = "MapStatic";
        private const string InstanceMethodName = "Map";

        public MappingCompiler()
        {
            Name = $"SwiftMapper_{DateTime.Now.ToString("yyyyMMddHHmmss")}";

            AssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(Name), AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule(Name, $"{Name}.dll");
        }

        private string Name { get; }
        private AssemblyBuilder AssemblyBuilder { get; }
        private ModuleBuilder ModuleBuilder { get; }

        internal Expression<Func<TSource, TDestination>> GetExpression<TSource, TDestination>()
        {
            var srcProps = typeof(TSource).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead);
            var dstProps = typeof(TDestination).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite && srcProps.Any(s => s.Name == p.Name));

            var sourceParameter = Expression.Parameter(typeof(TSource), "source");
            var newExp = Expression.New(typeof(TDestination));
            var initExp = Expression.MemberInit(newExp,
                dstProps.Select(d => Expression.Bind(d, Expression.Property(sourceParameter, srcProps.Single(p => p.Name == d.Name))))
                );
            var expression = Expression.Lambda<Func<TSource, TDestination>>(initExp, sourceParameter);

            return expression;
        }

        private string GetMappingTypeName<TSource, TDestination>()
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            var sourceName = sourceType.FullName.Replace(".", "_");
            var destinationName = destinationType.FullName.Replace(".", "_");

            var typeName = $"{sourceName}To{destinationName}";
            return typeName;
        }

        internal Type GetType<TSource, TDestination>(Expression<Func<TSource, TDestination>> expression)
        {
            var typeName = GetMappingTypeName<TSource, TDestination>();
            var interfaceType = typeof(IMapping<TSource, TDestination>);

            var typeBuilder = ModuleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Sealed);
            typeBuilder.AddInterfaceImplementation(interfaceType);
            var methodBuilder = typeBuilder.DefineMethod(StaticMethodName, MethodAttributes.Public | MethodAttributes.Static);

            expression.CompileToMethod(methodBuilder);
            methodBuilder.SetImplementationFlags(MethodImplAttributes.AggressiveInlining);

            var instanceMethodBuilder = typeBuilder.DefineMethod(InstanceMethodName,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                typeof(TDestination), new[] {typeof(TSource)});
            instanceMethodBuilder.DefineParameter(1, ParameterAttributes.None, "entity");
            instanceMethodBuilder.SetImplementationFlags(MethodImplAttributes.AggressiveInlining);
            typeBuilder.DefineMethodOverride(instanceMethodBuilder, interfaceType.GetMethod(InstanceMethodName));

            var il = instanceMethodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Call, methodBuilder, new[] { typeof(TSource) });
            il.Emit(OpCodes.Ret);

            var type = typeBuilder.CreateType();

            return type;
        }

        internal Func<TSource, TDestination> GetMappingDelegate<TSource, TDestination>(Type type)
        {
            var method = (Func<TSource, TDestination>) Delegate.CreateDelegate(typeof(Func<TSource, TDestination>), type.GetMethod(StaticMethodName));

            return method;
        }

        internal IMapping<TSource, TDestination> GetMappingObject<TSource, TDestination>(Type type)
        {
            var newExp = Expression.New(type);
            var lambdaNew = Expression.Lambda(typeof(Func<IMapping<TSource, TDestination>>), newExp);

            return ((Func<IMapping<TSource, TDestination>>) lambdaNew.Compile())();
        }
    }
}