using Mono.Cecil;
using Mono.Cecil.Cil;

namespace FluentIL.Extensions
{
    public static class PointCutExtensions
    {
        public static Cut Write(this in Cut pc, OpCode opCode, object operand) => pc.Write(pc.Emit(opCode, operand));
        public static Cut Write(this in Cut pc, OpCode opCode) => pc.Write(pc.Emit(opCode));

        public static Cut Replace(this in Cut pc, OpCode opCode, object operand) => pc.Replace(pc.Emit(opCode, operand));
        public static Cut Replace(this in Cut pc, OpCode opCode) => pc.Replace(pc.Emit(opCode));
        
        public static TypeReference Import(this in Cut cut, TypeReference tr) => cut.Method.Module.ImportReference(tr);
        public static TypeReference Import(this in Cut cut, StandardType st) => cut.Method.Module.ImportStandardType(st);
        public static MethodReference Import(this in Cut cut, MethodReference mr) => cut.Method.Module.ImportReference(mr);
        public static FieldReference Import(this in Cut cut, FieldReference fr) => cut.Method.Module.ImportReference(fr);
    }
}
