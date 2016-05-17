using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace SwiftMapper
{
    internal class MappingCompiler
    {
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

        internal Type GetType<TSource, TDestination>(Expression<Func<TSource, TDestination>> expression)
        {
            var sourceType = typeof(TSource);
            var destinationType = typeof(TDestination);

            var sourceName = sourceType.FullName.Replace(".", "_");
            var destinationName = destinationType.FullName.Replace(".", "_");

            var typeName = $"{sourceName}To{destinationName}";

            var typeBuilder = ModuleBuilder.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Sealed);
            var methodBuilder = typeBuilder.DefineMethod(typeName, MethodAttributes.Public | MethodAttributes.Static);

            expression.CompileToMethod(methodBuilder);
            methodBuilder.SetImplementationFlags(MethodImplAttributes.AggressiveInlining);

            var type = typeBuilder.CreateType();

            return type;
        }

        internal Func<TSource, TDestination> GetDelegate<TSource, TDestination>(Type type)
        {
            var method = (Func<TSource, TDestination>) Delegate.CreateDelegate(typeof(Func<TSource, TDestination>), type.GetMethod(type.Name));

            return method;
        }
    }
}