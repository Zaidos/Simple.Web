﻿namespace Simple.Web.Razor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Dynamic;
    using System.IO;
    using System.Reflection;
    using ContentTypeHandling;

    [ContentTypes(ContentType.Html, ContentType.XHtml)]
    public class RazorHtmlContentTypeHandler : IContentTypeHandler
    {
        public object Read(Stream inputStream, Type inputType)
        {
            throw new NotImplementedException();
        }

        public void Write(IContent content, Stream outputStream)
        {
            var razorViews = new RazorViews();
            var handlerType = content.Handler != null ? content.Handler.GetType() : null;
            var modelType = content.Model != null ? content.Model.GetType() : null;
            var viewType = razorViews.GetViewType(handlerType, modelType);

            if (viewType == null)
            {
                throw new ViewNotFoundException();
            }

            using (var streamWriter = new StreamWriter(outputStream))
            {
                RenderView(content, streamWriter, viewType);
            }
        }

        internal static void RenderView(IContent content, TextWriter textWriter, Type viewType)
        {
            var instance = (SimpleTemplateBase) Activator.CreateInstance(viewType);
            instance.SetHandler(content.Handler);
            instance.SetModel(content.Model);

            var viewData = new ExpandoObject() as IDictionary<string, object>;

            foreach (var pair in content.Variables)
            {
                viewData.Add(pair);
            }

            instance.Var = viewData;
            instance.Writer = textWriter;
            instance.Execute();
        }
    }
}