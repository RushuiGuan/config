using Microsoft.Extensions.Configuration;
using System;

namespace Albatross.Config {
	internal static class Factory<T> where T : ConfigBase {
		public static T CreateAndValidate(IConfiguration configuration) {
			var t = (T)Activator.CreateInstance(typeof(T), configuration) 
				?? throw new InvalidOperationException($"Failed to create instance of {typeof(T).FullName}");
			t.Validate();
			return t;
		}
	}
}