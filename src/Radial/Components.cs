﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System.Reflection;
using Radial.Extensions;

namespace Radial
{
    /// <summary>
    /// Components
    /// </summary>
    public static class Components
    {
        static object SyncRoot = new object();
        static IUnityContainer InnerContainer;

        /// <summary>
        /// Gets the container.
        /// </summary>
        public static IUnityContainer Container
        {
            get
            {
                lock (SyncRoot)
                {
                    if (InnerContainer == null)
                    {
                        InnerContainer = new UnityContainer();

                        UnityConfigurationSection section = (UnityConfigurationSection)ConfigurationManager.GetSection(UnityConfigurationSection.SectionName);

                        if (section != null && section.Containers.Count > 0)
                            section.Configure(InnerContainer);
                    }

                    return InnerContainer;
                }
            }
        }


        /// <summary>
        /// Register all interface implementations, with RegisterInterface attribute.
        /// </summary>
        /// <param name="implAssembly">The implementation assembly.</param>
        /// <param name="symbol">The symbol</param>
        /// <param name="lifetimeManagerFunc">The lifetime manager function.</param>
        /// <param name="injectionMembers">The injection members.</param>
        public static void RegisterInterfaces(Assembly implAssembly, string symbol = null,
            Func<LifetimeManager> lifetimeManagerFunc = null, params InjectionMember[] injectionMembers)
        {
            var intyps = implAssembly.GetTypes().Where(o => o.CustomAttributes.Contains(x => x.AttributeType == typeof(RegisterInterfaceAttribute))).ToArray();
            RegisterInterfaces(intyps, symbol, lifetimeManagerFunc, injectionMembers);

        }

        /// <summary>
        /// Register all interface implementations, with RegisterInterface attribute.
        /// </summary>
        /// <param name="implTypes">The implementation types.</param>
        /// <param name="symbol">The symbol</param>
        /// <param name="lifetimeManagerFunc">The lifetime manager function.</param>
        /// <param name="injectionMembers">The injection members.</param>
        public static void RegisterInterfaces(Type[] implTypes, string symbol = null, 
            Func<LifetimeManager> lifetimeManagerFunc=null, params InjectionMember[] injectionMembers)
        {
            if (implTypes == null)
                return;

            foreach (var type in implTypes)
            {
                var attr = type.GetCustomAttributes(typeof(RegisterInterfaceAttribute), false)[0] as RegisterInterfaceAttribute;

                if (attr == null)
                    continue;

                LifetimeManager fm =null;
                if (lifetimeManagerFunc != null)
                    fm = lifetimeManagerFunc();

                if (!string.IsNullOrWhiteSpace(attr.Symbol))
                {
                    if (attr.Symbol == symbol)
                        Components.Container.RegisterType(attr.InterfaceType, type, fm, injectionMembers);
                }
                else
                    Components.Container.RegisterType(attr.InterfaceType, type, fm, injectionMembers);
            }
        }
    }
}
