using System;
using System.Linq;
using System.Reflection;

namespace ModDemo.Scripting
{
    internal static class MethodInfoExtensions
    {
        public static Type CreateDelegateType(this MethodInfo method)
        {
            var parameters = method.GetParameters();
            var delegateParameters = parameters.Select(p => p.ParameterType).ToArray();
            
            if (method.ReturnType == typeof(void))
            {
                switch (delegateParameters.Length)
                {
                    case 0:
                        return typeof(Action);
                    case 1:
                        return typeof(Action<>).MakeGenericType(delegateParameters);
                    case 2:
                        return typeof(Action<,>).MakeGenericType(delegateParameters);
                    case 3:
                        return typeof(Action<,,>).MakeGenericType(delegateParameters);
                    case 4:
                        return typeof(Action<,,,>).MakeGenericType(delegateParameters);
                    default:
                        throw new NotSupportedException($"Methods with {delegateParameters.Length} parameters are not supported");
                }
            }
            else
            {
                switch (delegateParameters.Length)
                {
                    case 0:
                        return typeof(Func<>).MakeGenericType(method.ReturnType);
                    case 1:
                        return typeof(Func<,>).MakeGenericType(delegateParameters.Concat(new[] { method.ReturnType }).ToArray());
                    case 2:
                        return typeof(Func<,,>).MakeGenericType(delegateParameters.Concat(new[] { method.ReturnType }).ToArray());
                    case 3:
                        return typeof(Func<,,,>).MakeGenericType(delegateParameters.Concat(new[] { method.ReturnType }).ToArray());
                    case 4:
                        return typeof(Func<,,,,>).MakeGenericType(delegateParameters.Concat(new[] { method.ReturnType }).ToArray());
                    default:
                        throw new NotSupportedException($"Methods with {delegateParameters.Length} parameters are not supported");
                }
            }
        }
    }
}