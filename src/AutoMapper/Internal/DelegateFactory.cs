using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
#if NET4
using System.Reflection.Emit;
#endif
using System.Runtime.CompilerServices;
using System.Security;

[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityTransparent]
#if NET4
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
#endif

namespace AutoMapper
{
    using AutoMapper.Internal;

    public class DelegateFactory
    {
        private static readonly IDictionaryFactory DictionaryFactory = PlatformAdapter.Resolve<IDictionaryFactory>();

        private readonly IDictionary<Type, LateBoundCtor> _ctorCache = DictionaryFactory.CreateDictionary<Type, LateBoundCtor>();

        public LateBoundMethod CreateGet(MethodInfo method)
        {
#if NET4
            var dm = new DynamicMethod("", typeof(object), new[] { typeof(object), typeof(object[]) }, method.DeclaringType, true);
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, method.DeclaringType);
            il.Emit(OpCodes.Callvirt, method);
            if (!method.ReturnType.IsClass())
                il.Emit(OpCodes.Box, method.ReturnType);
            il.Emit(OpCodes.Ret);
            var func = (LateBoundMethod)method.CreateDelegate(typeof(LateBoundMethod));
            return func;
#else
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

            MethodCallExpression call;
            if (!method.IsDefined(typeof(ExtensionAttribute), false))
            {
                // instance member method
                call = Expression.Call(
                    Expression.Convert(instanceParameter, method.DeclaringType),
                    method,
                    CreateParameterExpressions(method, instanceParameter, argumentsParameter));
            }
            else
            {
                // static extension method
                call = Expression.Call(
                    method,
                    CreateParameterExpressions(method, instanceParameter, argumentsParameter));
            }

            Expression<LateBoundMethod> lambda = Expression.Lambda<LateBoundMethod>(
                Expression.Convert(call, typeof(object)),
                instanceParameter,
                argumentsParameter);

            return lambda.Compile();
#endif
            //return (LateBoundMethod) method.CreateDelegate(typeof(LateBoundMethod), null);
        }

        public LateBoundPropertyGet CreateGet(PropertyInfo property)
        {
#if NET4
            var method = new DynamicMethod("", typeof(object), new[] { typeof(object) }, property.DeclaringType, true);
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, property.DeclaringType);
            il.Emit(OpCodes.Callvirt, property.GetMethod);
            if (!property.PropertyType.IsClass())
                il.Emit(OpCodes.Box, property.PropertyType);
            il.Emit(OpCodes.Ret);
            var func = (LateBoundPropertyGet)method.CreateDelegate(typeof(LateBoundPropertyGet));
            return func;
#else
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");

            MemberExpression member = Expression.Property(Expression.Convert(instanceParameter, property.DeclaringType), property);

            Expression<LateBoundPropertyGet> lambda = Expression.Lambda<LateBoundPropertyGet>(
                Expression.Convert(member, typeof(object)),
                instanceParameter
                );

            return lambda.Compile();
#endif
        }

        public LateBoundFieldGet CreateGet(FieldInfo field)
        {
#if NET4
            var method = new DynamicMethod("", typeof(object), new[] { typeof(object) }, field.DeclaringType, true);
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, field.DeclaringType);
            il.Emit(OpCodes.Ldfld, field);
            if (!field.FieldType.IsClass())
                il.Emit(OpCodes.Box, field.FieldType);
            il.Emit(OpCodes.Ret);
            var func = (LateBoundFieldGet)method.CreateDelegate(typeof(LateBoundFieldGet));
            return func;

#else
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");

            MemberExpression member = Expression.Field(Expression.Convert(instanceParameter, field.DeclaringType), field);

            Expression<LateBoundFieldGet> lambda = Expression.Lambda<LateBoundFieldGet>(
                Expression.Convert(member, typeof(object)),
                instanceParameter
                );

            return lambda.Compile();
#endif
        }

        public LateBoundFieldSet CreateSet(FieldInfo field)
        {
#if NET4
            DynamicMethod method2 = new DynamicMethod("", typeof(void), new[] { typeof(object), typeof(object) }, true);
            var ilgen = method2.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Castclass, field.DeclaringType);
            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(field.FieldType.IsClass() ? OpCodes.Castclass : OpCodes.Unbox_Any, field.FieldType);

            ilgen.Emit(OpCodes.Stfld, field);
            ilgen.Emit(OpCodes.Ret);
            return (LateBoundFieldSet)method2.CreateDelegate(typeof(LateBoundFieldSet));
#else
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
            ParameterExpression valueParameter = Expression.Parameter(typeof(object), "value");

            MemberExpression member = Expression.Field(Expression.Convert(instanceParameter, field.DeclaringType), field);
            BinaryExpression assignExpression = Expression.Assign(member, Expression.Convert(valueParameter, field.FieldType));

            Expression<LateBoundFieldSet> lambda = Expression.Lambda<LateBoundFieldSet>(
                assignExpression,
                instanceParameter,
                valueParameter
                );

            return lambda.Compile();
#endif
        }

        public LateBoundPropertySet CreateSet(PropertyInfo property)
        {
#if NET4
            DynamicMethod method2 = new DynamicMethod("", typeof(void), new[] { typeof(object), typeof(object) }, true);
            var ilgen = method2.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Castclass, property.DeclaringType);
            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(property.PropertyType.IsClass() ? OpCodes.Castclass : OpCodes.Unbox_Any, property.PropertyType);

            ilgen.Emit(OpCodes.Callvirt, property.SetMethod);
            ilgen.Emit(OpCodes.Ret);
            return (LateBoundPropertySet)method2.CreateDelegate(typeof(LateBoundPropertySet));

            //return (LateBoundPropertySet)property.SetMethod.CreateDelegate(typeof(LateBoundPropertySet), null);
#else
            ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
            ParameterExpression valueParameter = Expression.Parameter(typeof(object), "value");

            MemberExpression member = Expression.Property(Expression.Convert(instanceParameter, property.DeclaringType), property);
            BinaryExpression assignExpression = Expression.Assign(member, Expression.Convert(valueParameter, property.PropertyType));

            Expression<LateBoundPropertySet> lambda = Expression.Lambda<LateBoundPropertySet>(
                assignExpression,
                instanceParameter,
                valueParameter
                );


            return lambda.Compile();
#endif
        }

        public LateBoundCtor CreateCtor(Type type)
        {
            LateBoundCtor ctor = _ctorCache.GetOrAdd(type, t =>
            {
#if NET4
                DynamicMethod dm = new DynamicMethod("", typeof(object), new[] { typeof(object[]) }, true);
                var ilgen = dm.GetILGenerator();

                var ctorInfo = type.GetDeclaredConstructors()
                    .Where(ci => !ci.IsStatic())
                    .First(c => c.GetParameters().All(p => p.IsOptional));

                int i = 0;
                foreach (var pi in ctorInfo.GetParameters())
                {
                    ilgen.Emit(OpCodes.Ldarg_0);
                    ilgen.Emit(OpCodes.Ldc_I4, i);
                    ilgen.Emit(OpCodes.Ldelem_Ref);
                    ilgen.Emit(pi.ParameterType.IsClass() ? OpCodes.Castclass : OpCodes.Unbox_Any, pi.ParameterType);
                    i++;
                }

                ilgen.Emit(OpCodes.Newobj, ctorInfo);
                ilgen.Emit(OpCodes.Ret);

                return (LateBoundCtor)dm.CreateDelegate(typeof(LateBoundCtor));
#else
    //handle valuetypes
                if (!type.IsClass())
                {
                    var ctorExpression = Expression.Lambda<LateBoundCtor>(Expression.Convert(Expression.New(type), typeof(object)));
                    return ctorExpression.Compile();
                }
                else 
                {
                    var constructors = type
                        .GetDeclaredConstructors()
                        .Where(ci => !ci.IsStatic);
                    
                    //find a ctor with only optional args
                    var ctorWithOptionalArgs = constructors.FirstOrDefault(c => c.GetParameters().All(p => p.IsOptional));
                    if (ctorWithOptionalArgs == null)
                        throw new ArgumentException("Type needs to have a constructor with 0 args or only optional args", "type");

                    //get all optional default values
                    var args = ctorWithOptionalArgs
                        .GetParameters()
                        .Select(p => Expression.Constant(p.DefaultValue,p.ParameterType)).ToArray();

                    //create the ctor expression
                    var ctorExpression = Expression.Lambda<LateBoundCtor>(Expression.Convert(Expression.New(ctorWithOptionalArgs,args), typeof(object)));
                    return ctorExpression.Compile();
                }
#endif
            });

            return ctor;
        }

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression instanceParameter, Expression argumentsParameter)
        {
            var expressions = new List<UnaryExpression>();
            var realMethodParameters = method.GetParameters();
            if (method.IsDefined(typeof(ExtensionAttribute), false))
            {
                Type extendedType = method.GetParameters()[0].ParameterType;
                expressions.Add(Expression.Convert(instanceParameter, extendedType));
                realMethodParameters = realMethodParameters.Skip(1).ToArray();
            }

            expressions.AddRange(realMethodParameters.Select((parameter, index) =>
                Expression.Convert(
                    Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)),
                    parameter.ParameterType)));

            return expressions.ToArray();
        }

        public LateBoundParamsCtor CreateCtor(ConstructorInfo constructorInfo, IEnumerable<ConstructorParameterMap> ctorParams)
        {
            ParameterExpression paramsExpr = Expression.Parameter(typeof(object[]), "parameters");

            var convertExprs = ctorParams
                .Select((ctorParam, i) => Expression.Convert(
                    Expression.ArrayIndex(paramsExpr, Expression.Constant(i)),
                    ctorParam.Parameter.ParameterType))
                .ToArray();

            NewExpression newExpression = Expression.New(constructorInfo, convertExprs);

            var lambda = Expression.Lambda<LateBoundParamsCtor>(newExpression, paramsExpr);

            return lambda.Compile();
        }
    }
}
