﻿//using ArtifactStore.sdk.Model;
//using Identity.sdk.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Toolbox.Application;
//using Toolbox.Tools;

//namespace Identity.Application
//{
//    public record Option
//    {
//        public RunEnvironment RunEnvironment { get; init; }

//        public ArtifactStoreOption ArtifactStore { get; init; } = null!;

//        public string? HostUrl { get; init; }

//        public IdentityNamespaces Namespaces { get; init; } = null!;
//    }

//    public static class OptionExtensions
//    {
//        public static void Verify(this Option option)
//        {
//            option.VerifyNotNull(nameof(option));
//            option.ArtifactStore.Verify();
//            option.Namespaces.Verify();
//        }
//    }
//}
