
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace FluentIL.Extensions
{
    public static class PointCutExtensions
    {
        //public static Cut Write(this in Cut pc, OpCode opCode, object operand) => pc.Write(pc.Emit(opCode, operand));

        //public static Cut Replace(this in Cut pc, OpCode opCode, object operand) => pc.Replace(pc.Emit(opCode, operand));
        public static Cut Replace(this in Cut pc, OpCode opCode) => pc.Replace(Instruction.Create(opCode));

        public static ITypeDefOrRef Import(this in Cut cut, ITypeDefOrRef tr) => (ITypeDefOrRef) cut.Method.Module.Import(tr);
        public static TypeRef Import(this in Cut cut, StandardType st) => cut.Method.Module.ImportStandardType(st);
        public static IMethod Import(this in Cut cut, IMethod mr) => cut.Method.Module.Import(mr);
        public static MemberRef Import(this in Cut cut, IField fr) => cut.Method.Module.Import(fr);


        public static Cut Write(this in Cut cut, OpCode opCode) => cut.Write(Instruction.Create(opCode));
        public static Cut Write(this in Cut cut, OpCode opCode, string str) => cut.Write(Instruction.Create(opCode, str));
        public static Cut Write(this in Cut cut, OpCode opCode, Parameter param) => cut.Write(Instruction.Create(opCode, param));
        public static Cut Write(this in Cut cut, OpCode opCode, Local local) => cut.Write(Instruction.Create(opCode, local));
        public static Cut Write(this in Cut cut, OpCode opCode, IField field) => cut.Write(Instruction.Create(opCode, cut.Import(field)));
        public static Cut Write(this in Cut cut, OpCode opCode, IMethod method) => cut.Write(Instruction.Create(opCode, cut.Import(method)));
        public static Cut Write(this in Cut cut, OpCode opCode, MemberRef method) => cut.Write(Instruction.Create(opCode, method));
        public static Cut Write(this in Cut cut, OpCode opCode, ITypeDefOrRef type) => cut.Write(Instruction.Create(opCode, cut.Import(type)));
        public static Cut Write(this in Cut cut, OpCode opCode, TypeSig type) => cut.Write(Instruction.Create(opCode, cut.Import(type.ToTypeDefOrRef())));
        public static Cut Write(this in Cut cut, OpCode opCode, Cut target) => cut.Write(Instruction.Create(opCode, target.ReferenceInstruction));
    }
}
