using FluentIL.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;

namespace FluentIL
{
    public static class Statements
    {
        public static Cut Return(this in Cut cut)
        {
            return cut.Write(OpCodes.Ret);
        }

        public static Cut Call(this in Cut cut, MethodReference method, PointCut args = null)
        {
            var cur_cut = cut;

            if (!method.IsCallCompatible())
                throw new ArgumentException($"Uninitialized generic call reference: {method}");

            if (args != null) cur_cut = cur_cut.Here(args);

            var methodDef = method.Resolve();

            var code = OpCodes.Call;
            if (methodDef.IsConstructor) code = OpCodes.Newobj;
            else if (methodDef.IsVirtual) code = OpCodes.Callvirt;

            return cur_cut.Write(code, method);
        }

        public static Cut IfEqual(this in Cut pc, PointCut left, PointCut right, PointCut pos = null, PointCut neg = null)
        {
            if (pos != null && neg != null)
                return Compare(pc, left, right, OpCodes.Ceq, pos, neg);

            if (pos != null)
                return Compare(pc, left, right, OpCodes.Ceq, OpCodes.Brfalse, pos);

            if (neg != null)
                return Compare(pc, left, right, OpCodes.Ceq, OpCodes.Brtrue, neg);

            return pc;
        }

        private static Cut Compare(in Cut cut, PointCut left, PointCut right, OpCode cmp, PointCut pos, PointCut neg)
        {
            var pc = cut
                .Here(left)
                .Here(right)
                .Write(cmp);

            var pe = pc.Here(pos);
            var ne = pe.Here(neg);

            var exit = ne.Write(OpCodes.Nop);

            pc.Write(OpCodes.Brfalse, pe.Next());
            pe.Write(OpCodes.Br, exit);

            return exit;
        }

        private static Cut Compare(in Cut cut, PointCut left, PointCut right, OpCode cmp, OpCode brexit, PointCut action)
        {
            var pc = cut
                .Here(left)
                .Here(right)
                .Write(cmp);

            var exit = pc.Write(OpCodes.Nop);

            pc.Write(brexit, exit).Here(action);

            return exit;
        }
    }
}
