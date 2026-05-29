using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Albatross.Config {
	internal static class Factory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T> where T : ConfigBase {
		static readonly Func<IConfiguration, T> func;

		static Factory() {
			var constructor = typeof(T).GetConstructor([typeof(IConfiguration)]);
			if (constructor == null) {
				throw new InvalidOperationException($"Type {typeof(T).FullName} must have a constructor with a single parameter of type IConfiguration.");
			}

			var configurationParam = Expression.Parameter(typeof(IConfiguration), "configuration");
			var newExpr = Expression.New(constructor, configurationParam);
			var lambda = Expression.Lambda<Func<IConfiguration, T>>(newExpr, configurationParam);
			func = lambda.Compile();
		}
		public static T CreateAndValidate(IConfiguration configuration) {
			var t = func(configuration);
			t.Validate();
			return t;
		}
	}
}