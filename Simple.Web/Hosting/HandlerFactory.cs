﻿namespace Simple.Web.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Web;
    using Simple.Web.CodeGeneration;

    /// <summary>
    /// Builds handlers. To be used by Hosting plug-ins.
    /// </summary>
    public sealed class HandlerFactory
    {
        private static HandlerFactory _instance;

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static HandlerFactory Instance
        {
            get { return _instance ?? (_instance = new HandlerFactory(SimpleWeb.Configuration)); }
        }

        private readonly HandlerBuilderFactory _handlerBuilderFactory;

        private readonly
            ConcurrentDictionary<string, ConcurrentDictionary<Type, Func<IDictionary<string, string>, object>>> _builders =
                new ConcurrentDictionary<string, ConcurrentDictionary<Type, Func<IDictionary<string, string>, object>>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerFactory"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <remarks>For testing only. In production, use the singleton <see cref="Instance"/>.</remarks>
        public HandlerFactory(IConfiguration configuration)
        {
            _handlerBuilderFactory = new HandlerBuilderFactory(configuration);
        }

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <param name="handlerInfo">The handler info.</param>
        /// <returns></returns>
        public object GetHandler(HandlerInfo handlerInfo)
        {
            var builderDictionary = _builders.GetOrAdd(handlerInfo.HttpMethod,
                                                       _ =>
                                                       new ConcurrentDictionary
                                                           <Type, Func<IDictionary<string, string>, object>>());

            var builder = builderDictionary.GetOrAdd(handlerInfo.HandlerType, _handlerBuilderFactory.BuildHandlerBuilder);
            return builder(handlerInfo.Variables);
        }
    }
}