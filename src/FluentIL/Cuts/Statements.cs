using FluentIL.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace FluentIL
{
    public static class Statements
    {
        public static Cut Return(this Cut cut)
        {
            return cut.Write(OpCodes.Ret);
        }

        public static Cut Call(this Cut cut, MethodReference method, PointCut args = null)
        {
            if (args != null) cut = cut.Here(args);

            var methodRef = cut.Method.MakeCallReference(cut.Import(method));
            var methodDef = method.Resolve();

            var code = OpCodes.Call;
            if (methodDef.IsConstructor) code = OpCodes.Newobj;
            else if (methodDef.IsVirtual) code = OpCodes.Callvirt;

            return cut.Write(code, methodRef);
        }

        public static Cut IfEqual(this Cut pc, PointCut left, PointCut right, PointCut pos = null, PointCut neg = null)
        {
            if (pos == null && neg == null)
                return pc;

            if (pos != null && neg == null)
                return Compare(pc, left, right, OpCodes.Ceq, OpCodes.Brfalse, pos);

            if (pos == null && neg != null)
                return Compare(pc, left, right, OpCodes.Ceq, OpCodes.Brtrue, neg);

            return Compare(pc, left, right, OpCodes.Ceq, pos, neg);
        }

        private static Cut Compare(Cut pc, PointCut left, PointCut right, OpCode cmp, PointCut pos, PointCut neg)
        {
            pc = pc
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

        private static Cut Compare(Cut pc, PointCut left, PointCut right, OpCode cmp, OpCode brexit, PointCut action)
        {
            pc = pc
                .Here(left)
                .Here(right)
                .Write(cmp);

            var exit = pc.Write(OpCodes.Nop);

            pc.Write(brexit, exit).Here(action);

            return exit;
        }
    }
}
