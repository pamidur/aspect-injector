using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AspectInjector.Tests.Runtime.Issues
{

    public class Issue_0123
    {
        [Fact]
        public void Fixed()
        {
            Checker.Passed = false;
            new HomeController().ActionOnlyAdminsCanDo().Wait();
            Assert.True(Checker.Passed);
        }

        public enum UserRole
        {
            Guest,
            Normal,
            Admin,
        }

        public enum UserRole2 : byte
        {
            Guest,
            Normal,
            Admin,
        }

        public enum UserRole3 : long
        {
            Guest = long.MaxValue,
            Normal = 1,
            Admin = 2,
        }
       
        public class HomeController
        {
            [CheckPrivileges(new UserRole[] { UserRole.Admin }, new UserRole2[] { UserRole2.Admin }, new UserRole3[] { UserRole3.Guest })]
            public async Task ActionOnlyAdminsCanDo()
            {
                await Task.Delay(100);
            }
        }

        [Injection(typeof(CheckPrivilegesAspect))]
        [AttributeUsage(AttributeTargets.Method)]
        public sealed class CheckPrivileges : Attribute
        {
            public UserRole[] Roles { get; }

            public CheckPrivileges(UserRole[] roles, UserRole2[] roles2, UserRole3[] role3s)
            {
                Roles = roles;
            }
        }

        [Aspect(Scope.PerInstance)]
        public class CheckPrivilegesAspect
        {
            [Advice(Kind.Before)]
            public void Before([Argument(Source.Triggers)] Attribute[] attributes)
            {
                if (attributes[0] is CheckPrivileges cp && cp.Roles.Contains(UserRole.Admin))
                    Checker.Passed = true;
            }
        }
    }
}
