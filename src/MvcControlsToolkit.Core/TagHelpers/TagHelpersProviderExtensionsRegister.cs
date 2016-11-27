﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MvcControlsToolkit.Core.Templates;
using System.Reflection;

namespace MvcControlsToolkit.Core.TagHelpers
{
    public static class TagHelpersProviderExtensionsRegister
    {
        private static IDictionary<Type,
            List<KeyValuePair<string,
            Func<TagHelperContext,
                TagHelperOutput,
                TagHelper,
                TagProcessorOptions,
                ContextualizedHelpers, Task>>>> allExtensions = 
            new Dictionary<Type,
            List<KeyValuePair<string,
            Func<TagHelperContext,
                TagHelperOutput,
                TagHelper,
                TagProcessorOptions,
                ContextualizedHelpers, Task>>>>();
        private static IDictionary<Type,
            List<KeyValuePair<string,
            DefaultTemplates>>> allTemplates =
                new Dictionary<Type,
                List<KeyValuePair<string,
                DefaultTemplates>>>();
        internal static void Prepare(IHostingEnvironment env)
        {
            var tenum = DefaultAssemblyPartDiscoveryProvider.DiscoverAssemblyParts(env.ApplicationName).
               Where(m => !m.Name.StartsWith("MvcControlsToolkit"))
               .SelectMany(m => (m as AssemblyPart).Types).Where(m => m.IsPublic
               && m.GetInterfaces().Contains(typeof(ITagHelperproviderExtension)))
               .Select(m => m.AsType());
            foreach(var x in tenum)
            {
                ITagHelperproviderExtension curr = Activator.CreateInstance(x) as ITagHelperproviderExtension;
                var res = GetProcessors(curr.For);
                if (res.Count == 0) allExtensions[curr.For] = res;
                res.AddRange(curr.TagProcessors);
                var res1 = GetTemplates(curr.For);
                if (res1.Count == 0) allTemplates[curr.For] = res1;
                res.AddRange(curr.TagProcessors);
            }
        }
        public static List<KeyValuePair<string,
            Func<TagHelperContext,
                TagHelperOutput,
                TagHelper,
                TagProcessorOptions,
                ContextualizedHelpers, Task>>> GetProcessors(Type t)
        {
            List<KeyValuePair<string,
            Func<TagHelperContext,
                TagHelperOutput,
                TagHelper,
                TagProcessorOptions,
                ContextualizedHelpers, Task>>> res = null;
            allExtensions.TryGetValue(t, out res);
            return res??new 
                List<KeyValuePair<string, Func<TagHelperContext, TagHelperOutput, TagHelper, TagProcessorOptions, ContextualizedHelpers, Task>>>();
        }
        public static List<KeyValuePair<string, DefaultTemplates>> GetTemplates(Type t)
        {
            List<KeyValuePair<string, DefaultTemplates>> res = null;
            allTemplates.TryGetValue(t, out res);
            return res ?? new
                List<KeyValuePair<string, DefaultTemplates>>();
        }
    }
}
