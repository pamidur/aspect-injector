using AspectInjector.Core.Models;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Extensions
{
    public static class MethodBodyExtensions
    {
        public static void OnEntry(this MethodBody boby, Action<PointCut> action)
        {
        }

        public static void OnExit(this MethodBody boby, Action<PointCut> action)
        {
        }

        public static void OnException(this MethodBody boby, Action<PointCut> action)
        {
        }

        public static void OnInstruction(this MethodBody boby, Instruction intsruction, Action<PointCut> action)
        {
        }
    }
}