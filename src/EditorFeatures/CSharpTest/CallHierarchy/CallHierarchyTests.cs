﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor.UnitTests.CallHierarchy;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.CallHierarchy
{
    public class CallHierarchyTests
    {
        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task InvokeOnMethod()
        {
            var text = @"
namespace N
{
    class C
    {
        void F$$oo()
        {
        }
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "N.C.Foo()");
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task InvokeOnProperty()
        {
            var text = @"
namespace N
{
    class C
    {
        public int F$$oo { get; set;}
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "N.C.Foo");
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task InvokeOnEvent()
        {
            var text = @"
using System;
namespace N
{
    class C
    {
        public event EventHandler Fo$$o;
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "N.C.Foo");
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task Method_FindCalls()
        {
            var text = @"
namespace N
{
    class C
    {
        void F$$oo()
        {
        }
    }

    class G
    {
        void Main()
        {
            var c = new C();
            c.Foo();
        }

        void Main2()
        {
            var c = new C();
            c.Foo();
        }
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "N.C.Foo()", new[] { string.Format(EditorFeaturesResources.CallsTo, "Foo") });
            testState.VerifyResult(root, string.Format(EditorFeaturesResources.CallsTo, "Foo"), new[] { "N.G.Main()", "N.G.Main2()" });
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task Method_InterfaceImplementation()
        {
            var text = @"
namespace N
{
    interface I
    {
        void Foo();
    }

    class C : I
    {
        public async Task F$$oo()
        {
        }
    }

    class G
    {
        void Main()
        {
            I c = new C();
            c.Foo();
        }

        void Main2()
        {
            var c = new C();
            c.Foo();
        }
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "N.C.Foo()", new[] { string.Format(EditorFeaturesResources.CallsTo, "Foo"), string.Format(EditorFeaturesResources.CallsToInterfaceImplementation, "N.I.Foo()") });
            testState.VerifyResult(root, string.Format(EditorFeaturesResources.CallsTo, "Foo"), new[] { "N.G.Main2()" });
            testState.VerifyResult(root, string.Format(EditorFeaturesResources.CallsToInterfaceImplementation, "N.I.Foo()"), new[] { "N.G.Main()" });
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task Method_CallToOverride()
        {
            var text = @"
namespace N
{
    class C
    {
        protected virtual void F$$oo() { }
    }

    class D : C
    {
        protected override void Foo() { }

        void Bar()
        {
            C c; 
            c.Foo()
        }

        void Baz()
        {
            D d;
            d.Foo();
        }
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "N.C.Foo()", new[] { string.Format(EditorFeaturesResources.CallsTo, "Foo"), EditorFeaturesResources.CallsToOverrides });
            testState.VerifyResult(root, string.Format(EditorFeaturesResources.CallsTo, "Foo"), new[] { "N.D.Bar()" });
            testState.VerifyResult(root, EditorFeaturesResources.CallsToOverrides, new[] { "N.D.Baz()" });
        }

        [WpfFact, WorkItem(829705), Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task Method_CallToBase()
        {
            var text = @"
namespace N
{
    class C
    {
        protected virtual void Foo() { }
    }

    class D : C
    {
        protected override void Foo() { }

        void Bar()
        {
            C c; 
            c.Foo()
        }

        void Baz()
        {
            D d;
            d.Fo$$o();
        }
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "N.D.Foo()", new[] { string.Format(EditorFeaturesResources.CallsTo, "Foo"), string.Format(EditorFeaturesResources.CallsToBaseMember, "N.C.Foo()") });
            testState.VerifyResult(root, string.Format(EditorFeaturesResources.CallsTo, "Foo"), new[] { "N.D.Baz()" });
            testState.VerifyResult(root, string.Format(EditorFeaturesResources.CallsToBaseMember, "N.C.Foo()"), new[] { "N.D.Bar()" });
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task FieldInitializers()
        {
            var text = @"
namespace N
{
    class C
    {
        public int foo = Foo();

        protected int Foo$$() { return 0; }
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "N.C.Foo()", new[] { string.Format(EditorFeaturesResources.CallsTo, "Foo") });
            testState.VerifyResultName(root, string.Format(EditorFeaturesResources.CallsTo, "Foo"), new[] { EditorFeaturesResources.Initializers });
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task FieldReferences()
        {
            var text = @"
namespace N
{
    class C
    {
        public int f$$oo = Foo();

        protected int Foo() { foo = 3; }
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "N.C.foo", new[] { string.Format(EditorFeaturesResources.ReferencesToField, "foo") });
            testState.VerifyResult(root, string.Format(EditorFeaturesResources.ReferencesToField, "foo"), new[] { "N.C.Foo()" });
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task PropertyGet()
        {
            var text = @"
namespace N
{
    class C
    {
        public int val
        {
            g$$et
            {
                return 0;
            }
        }

        public int foo()
        {
            var x = this.val;
        }
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "N.C.val.get", new[] { string.Format(EditorFeaturesResources.CallsTo, "get_val") });
            testState.VerifyResult(root, string.Format(EditorFeaturesResources.CallsTo, "get_val"), new[] { "N.C.foo()" });
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task Generic()
        {
            var text = @"
namespace N
{
    class C
    {
        public int gen$$eric<T>(this string generic, ref T stuff)
        {
            return 0;
        }

        public int foo()
        {
            int i;
            generic("", ref i);
        }
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "N.C.generic<T>(this string, ref T)", new[] { string.Format(EditorFeaturesResources.CallsTo, "generic") });
            testState.VerifyResult(root, string.Format(EditorFeaturesResources.CallsTo, "generic"), new[] { "N.C.foo()" });
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task ExtensionMethods()
        {
            var text = @"
namespace ConsoleApplication10
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = ""string"";
            x.BarStr$$ing();
        }
    }
    
    public static class Extensions
    {
        public static string BarString(this string s)
        {
            return s;
        }
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "ConsoleApplication10.Extensions.BarString(this string)", new[] { string.Format(EditorFeaturesResources.CallsTo, "BarString") });
            testState.VerifyResult(root, string.Format(EditorFeaturesResources.CallsTo, "BarString"), new[] { "ConsoleApplication10.Program.Main(string[])" });
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task GenericExtensionMethods()
        {
            var text = @"
using System.Collections.Generic;
using System.Linq;
namespace N
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int> x = new List<int>();
            var z = x.Si$$ngle();
        }
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "System.Linq.Enumerable.Single<TSource>(this System.Collections.Generic.IEnumerable<TSource>)", new[] { string.Format(EditorFeaturesResources.CallsTo, "Single") });
            testState.VerifyResult(root, string.Format(EditorFeaturesResources.CallsTo, "Single"), new[] { "N.Program.Main(string[])" });
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task InterfaceImplementors()
        {
            var text = @"
namespace N
{
    interface I
    {
        void Fo$$o();
    }

    class C : I
    {
        public async Task Foo()
        {
        }
    }

    class G
    {
        void Main()
        {
            I c = new C();
            c.Foo();
        }

        void Main2()
        {
            var c = new C();
            c.Foo();
        }
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "N.I.Foo()", new[] { string.Format(EditorFeaturesResources.CallsTo, "Foo"), string.Format(EditorFeaturesResources.ImplementsArg, "Foo") });
            testState.VerifyResult(root, string.Format(EditorFeaturesResources.CallsTo, "Foo"), new[] { "N.G.Main()" });
            testState.VerifyResult(root, string.Format(EditorFeaturesResources.ImplementsArg, "Foo"), new[] { "N.C.Foo()" });
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task NoFindOverridesOnSealedMethod()
        {
            var text = @"
namespace N
{
    class C
    {
        void F$$oo()
        {
        }
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            Assert.DoesNotContain("Overrides", root.SupportedSearchCategories.Select(s => s.DisplayName));
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task FindOverrides()
        {
            var text = @"
namespace N
{
    class C
    {
        public virtual void F$$oo()
        {
        }
    }

    class G : C
    {
        public override void Foo()
        {
        }
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "N.C.Foo()", new[] { string.Format(EditorFeaturesResources.CallsTo, "Foo"), EditorFeaturesResources.Overrides });
            testState.VerifyResult(root, EditorFeaturesResources.Overrides, new[] { "N.G.Foo()" });
        }

        [WorkItem(844613)]
        [WpfFact, Trait(Traits.Feature, Traits.Features.CallHierarchy)]
        public async Task AbstractMethodInclusionToOverrides()
        {
            var text = @"
using System;

abstract class Base
{
    public abstract void $$M();
}
 
class Derived : Base
{
    public override void M()
    {
        throw new NotImplementedException();
    }
}";
            var testState = await CallHierarchyTestState.CreateAsync(text);
            var root = testState.GetRoot();
            testState.VerifyRoot(root, "Base.M()", new[] { string.Format(EditorFeaturesResources.CallsTo, "M"), EditorFeaturesResources.Overrides, EditorFeaturesResources.CallsToOverrides });
            testState.VerifyResult(root, EditorFeaturesResources.Overrides, new[] { "Derived.M()" });
        }
    }
}
