## 9.0.0
* Referencing Dotnet 9 libraries
* Align verison with dotnet version
* Remove the `ConfigBase.Key` property.  The key is now passed directly in the constructor.
* Add `Extensions.AddConfig<TInterface, TConfig>(...)` method to support interface based config classes.
* Create Factory<T> class to support creating instances of T with expressions instead of reflection.
