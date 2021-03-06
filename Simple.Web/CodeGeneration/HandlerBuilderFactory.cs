namespace Simple.Web.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class HandlerBuilderFactory
    {
        private readonly IConfiguration _configuration;

        public HandlerBuilderFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Func<IDictionary<string, string>, object> BuildHandlerBuilder(Type type)
        {
            var container = Expression.Constant(_configuration.Container);
            var getMethod = Expression.Call(container, _configuration.Container.GetType().GetMethod("Get").MakeGenericMethod(type));
            var instance = Expression.Variable(type);
            var construct = Expression.Assign(instance, getMethod);
            var variables = Expression.Parameter(typeof(IDictionary<string, string>));

            var block = MakePropertySetterBlock(type, variables, instance, construct);

            return Expression.Lambda<Func<IDictionary<string, string>, object>>(block, variables).Compile();
        }

        private static BlockExpression MakePropertySetterBlock(Type type, ParameterExpression variables,
                                                               ParameterExpression instance, BinaryExpression construct)
        {
            var lines = new List<Expression> { construct };

            var setters = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .Where(PropertySetterBuilder.PropertyIsPrimitive)
                .Select(p => new PropertySetterBuilder(variables, instance, p))
                .Select(ps => ps.CreatePropertySetter());

            lines.AddRange(setters);
            lines.Add(instance);

            var block = Expression.Block(type, new[] { instance }, lines);
            return block;
        }
    }
}