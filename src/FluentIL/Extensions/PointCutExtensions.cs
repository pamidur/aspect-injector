using Mono.Cecil.Cil;

namespace FluentIL.Extensions
{
    public static class PointCutExtensions
    {
        public static Cut Write(this Cut pc, OpCode opCode, object operand) => pc.Write(pc.Emit(opCode, operand));
        public static Cut Write(this Cut pc, OpCode opCode) => pc.Write(pc.Emit(opCode));

        public static Cut Replace(this Cut pc, OpCode opCode, object operand) => pc.Replace(pc.Emit(opCode, operand));
        public static Cut Replace(this Cut pc, OpCode opCode) => pc.Replace(pc.Emit(opCode));
    }
}
