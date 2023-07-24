﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;
using Arke.IntegrationApi.CallObjects;
using Arke.IVR;
using Arke.IVR.CallObjects;
using Arke.SipEngine;
using Arke.SipEngine.CallObjects;
using Arke.SipEngine.FSM;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using SimpleInjector.Packaging;

namespace Arke.SampleProject
{
    public class SamplePackageRegistration : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.Register<SampleCallState>();
            container.Register(typeof(ICallFlowService<ICallInfo>), typeof(ArkeCallFlowService<SampleCallState>), Lifestyle.Singleton);
            container.Register(typeof(ICall<ICallInfo>), typeof(ArkeCall<SampleCallState>), Lifestyle.Transient);
            container.Options.AllowOverridingRegistrations = true;
            container.Register<IStateMachineConfiguration, CustomStateMachineConfig>();
            container.Options.AllowOverridingRegistrations = false;

        }
    }
}
