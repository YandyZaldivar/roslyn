﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.Cci;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols
{
    internal sealed class SynthesizedRecordPropertySymbol : SourceOrRecordPropertySymbol
    {
        private readonly ParameterSymbol _backingParameter;
        internal override SynthesizedBackingFieldSymbol BackingField { get; }
        public override MethodSymbol GetMethod { get; }
        public override NamedTypeSymbol ContainingType { get; }

        public SynthesizedRecordPropertySymbol(
            NamedTypeSymbol containingType,
            ParameterSymbol backingParameter)
            : base(backingParameter.Locations[0])
        {
            ContainingType = containingType;
            _backingParameter = backingParameter;
            string name = backingParameter.Name;
            BackingField = new SynthesizedBackingFieldSymbol(
                this,
                GeneratedNames.MakeBackingFieldName(name),
                isReadOnly: true,
                isStatic: false,
                hasInitializer: backingParameter.HasExplicitDefaultValue);
            GetMethod = new GetAccessorSymbol(this, name);
        }

        internal override bool IsAutoProperty => true;

        public override RefKind RefKind => RefKind.None;

        public override TypeWithAnnotations TypeWithAnnotations => _backingParameter.TypeWithAnnotations;

        public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

        public override ImmutableArray<ParameterSymbol> Parameters => ImmutableArray<ParameterSymbol>.Empty;

        public override bool IsIndexer => false;

        public override MethodSymbol? SetMethod => null;

        public override ImmutableArray<PropertySymbol> ExplicitInterfaceImplementations => ImmutableArray<PropertySymbol>.Empty;

        public override Symbol ContainingSymbol => ContainingType;

        public override ImmutableArray<Location> Locations => _backingParameter.Locations;

        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => _backingParameter.DeclaringSyntaxReferences;

        public override Accessibility DeclaredAccessibility => Accessibility.Public;

        public override bool IsStatic => false;

        public override bool IsVirtual => false;

        public override bool IsOverride => false;

        public override bool IsAbstract => false;

        public override bool IsSealed => false;

        public override bool IsExtern => false;

        internal override bool HasSpecialName => false;

        internal override CallingConvention CallingConvention => CallingConvention.HasThis;

        internal override bool MustCallMethodsDirectly => false;

        internal override ObsoleteAttributeData? ObsoleteAttributeData => null;

        public override string Name => _backingParameter.Name;

        protected override IAttributeTargetSymbol AttributesOwner => this;

        protected override AttributeLocation AllowedAttributeLocations => AttributeLocation.None;

        protected override AttributeLocation DefaultAttributeLocation => AttributeLocation.None;

        public override ImmutableArray<CSharpAttributeData> GetAttributes() => ImmutableArray<CSharpAttributeData>.Empty;

        internal override bool HasPointerType => Type.IsPointerType();

        public override SyntaxList<AttributeListSyntax> AttributeDeclarationSyntaxList => new SyntaxList<AttributeListSyntax>();

        private sealed class GetAccessorSymbol : SynthesizedInstanceMethodSymbol
        {
            private readonly SynthesizedRecordPropertySymbol _property;

            public override string Name { get; }

            public GetAccessorSymbol(SynthesizedRecordPropertySymbol property, string paramName)
            {
                _property = property;
                Name = SourcePropertyAccessorSymbol.GetAccessorName(
                    paramName,
                    getNotSet: true,
                    isWinMdOutput: false /* unused for getters */);
            }

            public override MethodKind MethodKind => MethodKind.PropertyGet;

            public override int Arity => 0;

            public override bool IsExtensionMethod => false;

            public override bool HidesBaseMethodsByName => false;

            public override bool IsVararg => false;

            public override bool ReturnsVoid => false;

            public override bool IsAsync => false;

            public override RefKind RefKind => RefKind.None;

            public override TypeWithAnnotations ReturnTypeWithAnnotations => _property.TypeWithAnnotations;

            public override FlowAnalysisAnnotations ReturnTypeFlowAnalysisAnnotations => FlowAnalysisAnnotations.None;

            public override ImmutableArray<TypeWithAnnotations> TypeArgumentsWithAnnotations => ImmutableArray<TypeWithAnnotations>.Empty;

            public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

            public override ImmutableArray<ParameterSymbol> Parameters => _property.Parameters;

            public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

            public override ImmutableArray<CustomModifier> RefCustomModifiers => _property.RefCustomModifiers;

            public override Symbol AssociatedSymbol => _property;

            public override Symbol ContainingSymbol => _property.ContainingSymbol;

            public override ImmutableArray<Location> Locations => _property.Locations;

            public override Accessibility DeclaredAccessibility => _property.DeclaredAccessibility;

            public override bool IsStatic => _property.IsStatic;

            public override bool IsVirtual => _property.IsVirtual;

            public override bool IsOverride => _property.IsOverride;

            public override bool IsAbstract => _property.IsAbstract;

            public override bool IsSealed => _property.IsSealed;

            public override bool IsExtern => _property.IsExtern;

            public override ImmutableHashSet<string> ReturnNotNullIfParameterNotNull => ImmutableHashSet<string>.Empty;

            internal override bool HasSpecialName => _property.HasSpecialName;

            internal override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.Managed;

            internal override bool HasDeclarativeSecurity => false;

            internal override MarshalPseudoCustomAttributeData? ReturnValueMarshallingInformation => null;

            internal override bool RequiresSecurityObject => false;

            internal override CallingConvention CallingConvention => CallingConvention.HasThis;

            internal override bool GenerateDebugInfo => false;

            public override DllImportData? GetDllImportData() => null;

            internal override ImmutableArray<string> GetAppliedConditionalSymbols()
                => ImmutableArray<string>.Empty;

            internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
                => Array.Empty<SecurityAttribute>();

            internal override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false) => false;

            internal override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false) => false;

            internal override bool SynthesizesLoweredBoundBody => true;

            internal override void GenerateMethodBody(TypeCompilationState compilationState, DiagnosticBag diagnostics)
            {
                // Method body:
                //
                // {
                //      return this.<>backingField;
                // }

                var F = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);

                F.CurrentFunction = this;
                F.CloseMethod(F.Block(F.Return(F.Field(F.This(), _property.BackingField))));
            }
        }
    }
}
