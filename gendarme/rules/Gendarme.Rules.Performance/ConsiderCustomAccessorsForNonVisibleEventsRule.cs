//
// Gendarme.Rules.Performance.ConsiderCustomAccessorsForNonVisibleEventsRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	[Problem ("The compiler created add/remove event accessors are, by default, synchronized, i.e. the runtime will wrap them inside a Monitor.Enter/Exit.")]
	[Solution ("For non-visible events looks if your code could work without being synchronized by supplying your own accessor implementations.")]
	public class ConsiderCustomAccessorsForNonVisibleEventsRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to classes... 
			if (type.IsEnum || type.IsInterface || type.IsValueType)
				return RuleResult.DoesNotApply;
			// ... that defines events
			if (type.Events.Count == 0)
				return RuleResult.DoesNotApply;

			// type can be non-visible (private or internal) but still reachable
			// with an interface or with an attribute (internals)
			bool type_visible = type.IsVisible ();

			foreach (EventDefinition evnt in type.Events) {
				// we assume that Add|Remove have the same visibility
				if (evnt.AddMethod.IsVisible ())
					continue;

				// report if Add|Remove is synchronized
				if (evnt.AddMethod.IsSynchronized) {
					Confidence confidence = type_visible ? Confidence.Normal : Confidence.Low;
					Runner.Report (evnt, Severity.Medium, confidence);
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
