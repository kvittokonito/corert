﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

using Internal.JitInterface;
using Internal.Text;
using Internal.TypeSystem;

namespace ILCompiler.DependencyAnalysis.ReadyToRun
{
    public class MethodFixupSignature : Signature
    {
        private readonly ReadyToRunFixupKind _fixupKind;

        private readonly MethodWithToken _method;

        private readonly SignatureContext _signatureContext;

        private readonly bool _isUnboxingStub;

        private readonly bool _isInstantiatingStub;

        public MethodFixupSignature(
            ReadyToRunFixupKind fixupKind, 
            MethodWithToken method, 
            SignatureContext signatureContext,
            bool isUnboxingStub,
            bool isInstantiatingStub)
        {
            _fixupKind = fixupKind;
            _method = method;
            _signatureContext = signatureContext;
            _isUnboxingStub = isUnboxingStub;
            _isInstantiatingStub = isInstantiatingStub;
        }

        public MethodDesc Method => _method.Method;

        public override int ClassCode => 150063499;

        public override ObjectData GetData(NodeFactory factory, bool relocsOnly = false)
        {
            if (relocsOnly)
            {
                // Method fixup signature doesn't contain any direct relocs
                return new ObjectData(data: Array.Empty<byte>(), relocs: null, alignment: 0, definedSymbols: null);
            }

            ReadyToRunCodegenNodeFactory r2rFactory = (ReadyToRunCodegenNodeFactory)factory;
            ObjectDataSignatureBuilder dataBuilder = new ObjectDataSignatureBuilder();
            dataBuilder.AddSymbol(this);
            SignatureContext innerContext = dataBuilder.EmitFixup(r2rFactory, _fixupKind, _method.Token.Module, _signatureContext);
            dataBuilder.EmitMethodSignature(_method, enforceDefEncoding: false, innerContext, _isUnboxingStub, _isInstantiatingStub);

            return dataBuilder.ToObjectData();
        }

        public override void AppendMangledName(NameMangler nameMangler, Utf8StringBuilder sb)
        {
            sb.Append(nameMangler.CompilationUnitPrefix);
            sb.Append($@"MethodFixupSignature(");
            sb.Append(_fixupKind.ToString());
            if (_isUnboxingStub)
            {
                sb.Append(" [UNBOX]");
            }
            if (_isInstantiatingStub)
            {
                sb.Append(" [INST]");
            }
            sb.Append(": ");
            _method.AppendMangledName(nameMangler, sb);
        }

        public override int CompareToImpl(ISortableNode other, CompilerComparer comparer)
        {
            throw new NotImplementedException();
        }
    }
}
