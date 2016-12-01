//using Mono.Cecil;

//namespace AspectInjector.Core.Fluent
//{
//    public class FluentMemberConstructor
//    {
//        private MethodAttributes _methodAttributes;

//        private FieldAttributes _fieldAttributes;

//        private readonly FluentType _type;
//        private readonly EditorContext _ctx;

//        internal FluentMemberConstructor(EditorContext ctx, FluentType type)
//        {
//            _type = type;
//            _ctx = ctx;
//        }

//        public FluentMemberConstructor Public
//        {
//            get
//            {
//                return new FluentMemberConstructor(_ctx, _type)
//                {
//                    _methodAttributes = _methodAttributes | MethodAttributes.Public,
//                    _fieldAttributes = _fieldAttributes | FieldAttributes.Public
//                };
//            }
//        }

//        public FluentMemberConstructor Private
//        {
//            get
//            {
//                return new FluentMemberConstructor(_ctx, _type)
//                {
//                    _methodAttributes = _methodAttributes | MethodAttributes.Private,
//                    _fieldAttributes = _fieldAttributes | FieldAttributes.Private
//                };
//            }
//        }

//        public FluentMemberConstructor Virtual
//        {
//            get
//            {
//                return new FluentMemberConstructor(_ctx, _type) { _methodAttributes = _methodAttributes | MethodAttributes.Virtual };
//            }
//        }

//        public FluentMemberConstructor Abstract
//        {
//            get
//            {
//                return new FluentMemberConstructor(_ctx, _type) { _methodAttributes = _methodAttributes | MethodAttributes.Abstract };
//            }
//        }

//        public FluentMemberConstructor Sealed
//        {
//            get
//            {
//                return new FluentMemberConstructor(_ctx, _type) { _methodAttributes = _methodAttributes | MethodAttributes.Final };
//            }
//        }

//        public FluentMemberConstructor Internal
//        {
//            get
//            {
//                return new FluentMemberConstructor(_ctx, _type)
//                {
//                    _methodAttributes = _methodAttributes.HasFlag(MethodAttributes.Family) ?
//                    _methodAttributes | MethodAttributes.FamORAssem :
//                    _methodAttributes | MethodAttributes.Assembly
//                };
//            }
//        }

//        public FluentMemberConstructor Protected
//        {
//            get
//            {
//                return new FluentMemberConstructor(_ctx, _type)
//                {
//                    _methodAttributes = _methodAttributes.HasFlag(MethodAttributes.Assembly) ?
//                    _methodAttributes | MethodAttributes.FamORAssem :
//                    _methodAttributes | MethodAttributes.Family
//                };
//            }
//        }

//        public FluentMemberConstructor Static
//        {
//            get
//            {
//                return new FluentMemberConstructor(_ctx, _type) { _methodAttributes = _methodAttributes | MethodAttributes.Static };
//            }
//        }

//        public FluentMemberConstructor New
//        {
//            get
//            {
//                return new FluentMemberConstructor(_ctx, _type) { _methodAttributes = _methodAttributes | MethodAttributes.NewSlot };
//            }
//        }

//        public FluentMemberConstructor Hidden
//        {
//            get
//            {
//                return new FluentMemberConstructor(_ctx, _type) { _methodAttributes = _methodAttributes | MethodAttributes.HideBySig };
//            }
//        }

//        public MethodEditor Method(string name)
//        {
//            var method = new MethodDefinition(name, _methodAttributes, _ctx.TypeSystem.Void);
//            _type.Definition.Methods.Add(method);
//            return _ctx.GetFluentMember(method);
//        }
//    }
//}