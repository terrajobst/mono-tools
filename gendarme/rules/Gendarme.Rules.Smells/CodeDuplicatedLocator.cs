//
// Gendarme.Rules.Smells.CodeDuplicatedLocator class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007-2008 Néstor Salceda
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
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;

namespace Gendarme.Rules.Smells {

	internal sealed class CodeDuplicatedLocator {
		private HashSet<string> methods = new HashSet<string> ();
		private HashSet<string> types = new HashSet<string> ();

		internal ICollection<string> CheckedMethods {
			get {
				return methods;
			}
		}

		internal ICollection<string> CheckedTypes {
			get {
				return types;
			}
		}

		public void Clear ()
		{
			methods.Clear ();
			types.Clear ();
		}

		private static bool ExistsExpressionsReplied (ICollection currentExpressions, ICollection targetExpressions)
		{
			IEnumerator currentEnumerator = currentExpressions.GetEnumerator ();
			IEnumerator targetEnumerator = targetExpressions.GetEnumerator ();
			bool equality = false;

			while (currentEnumerator.MoveNext () & targetEnumerator.MoveNext ()) {
				ExpressionCollection currentExpression = (ExpressionCollection) currentEnumerator.Current;
				ExpressionCollection targetExpression = (ExpressionCollection) targetEnumerator.Current;

				if (equality && currentExpression.Equals (targetExpression))
					return true;
				else {
					equality = currentExpression.Equals (targetExpression);
				}
			}
			return false;
		}

		private static ICollection GetExpressionsFrom (MethodBody methodBody)
		{
			ExpressionFillerVisitor expressionFillerVisitor = new ExpressionFillerVisitor ();
			methodBody.Accept (expressionFillerVisitor);
			return expressionFillerVisitor.Expressions;
		}

		private bool CanCompareMethods (MethodDefinition currentMethod, MethodDefinition targetMethod)
		{
			return currentMethod.HasBody && targetMethod.HasBody &&
				!CheckedMethods.Contains (targetMethod.Name) &&
				currentMethod != targetMethod;
		}

		private bool ContainsDuplicatedCode (MethodDefinition currentMethod, MethodDefinition targetMethod)
		{
			if (CanCompareMethods (currentMethod, targetMethod)) {
				ICollection currentExpressions = GetExpressionsFrom (currentMethod.Body);
				ICollection targetExpressions = GetExpressionsFrom (targetMethod.Body);

				return ExistsExpressionsReplied (currentExpressions, targetExpressions);
			}
			return false;
		}

		internal bool CompareMethodAgainstTypeMethods (IRule rule, MethodDefinition currentMethod, TypeDefinition targetTypeDefinition)
		{
			bool containsDuplicated = false;
			if (!CheckedTypes.Contains (targetTypeDefinition.Name) && targetTypeDefinition.HasMethods) {
				foreach (MethodDefinition targetMethod in targetTypeDefinition.Methods) {
					if (ContainsDuplicatedCode (currentMethod, targetMethod)) {
						rule.Runner.Report (currentMethod, Severity.High, Confidence.Normal, String.Format ("Duplicate code with {0}", targetMethod));
						containsDuplicated = true;
					}
				}
			}
			return containsDuplicated; 
		}
	}
}
