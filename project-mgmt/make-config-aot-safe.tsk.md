# Make Albatross.Config AOT-safe

status: new
created: 2026-07-09T00:00:00-04:00
priority: low
tags: aot trimming config cli
----

## Objective

Make `Albatross.Config` genuinely trim- and NativeAOT-safe so it compiles clean under
`<IsAotCompatible>true</IsAotCompatible>` and runs correctly under `PublishAot` — without
suppressing warnings that can't be honestly stood behind. A prior attempt (using
`UnconditionalSuppressMessage`) was reverted because the suppressions masked real runtime
hazards rather than fixing them. This task tracks doing it properly **if it is ever deemed
worth the cost** — see the value assessment below, which currently argues against it.

## Reasoning

### Value assessment (read this first)

The original motivation was NativeAOT for **CLI applications**, to reduce startup time.
In practice that benefit has not materialized in a way that justifies the work — startup
gains were not meaningful for these tools, and nothing else in the stack needs AOT. So the
realistic disposition of this task is "low priority, likely won't-do." It is recorded here
so the analysis isn't lost and nobody re-scopes it cold. Only pick it up if a concrete AOT
requirement appears (e.g. a CLI where cold-start latency genuinely matters, or a container
size / self-contained-trim requirement).

### The three reflection hazards (all previously papered over)

1. **`ConfigBase` ctor — `section.Bind(this)`** (`ConfigBase.cs`). `ConfigurationBinder.Bind`
   is annotated **both** `[RequiresUnreferencedCode]` (IL2026) **and** `[RequiresDynamicCode]`
   (IL3050). The reverted attempt suppressed only IL2026 and justified it via
   `DynamicallyAccessedMembers(PublicProperties)` on `T` in `AddConfig<T>`. That justification
   is false: DAM only preserves `T`'s own members, not the nested/complex property types
   `Bind` recurses into, and only for types flowing through `AddConfig<T>` (a direct
   `new MyConfig(config)` gets nothing). This is the hard one.

2. **`ConfigBase.Validate()` — `Validator.ValidateObject`** (`ConfigBase.cs`).
   `[RequiresUnreferencedCode]` (IL2026) only — no dynamic-code requirement.

3. **`Factory<T>`** (`Factory.cs`). The AOT half is already fixed: `Expression.Compile()`
   (IL3050) → `Activator.CreateInstance(typeof(T), configuration)`, which is AOT-safe. But
   `Activator.CreateInstance` is *trim*-relevant, so this task must **re-add**
   `[DynamicallyAccessedMembers(PublicConstructors)]` on `Factory<T>`'s `T` (removed during
   the cleanup) to keep it trim-clean.

### The genuine fix (source-generator route)

- Enable `<IsAotCompatible>true</IsAotCompatible>` (turns on trim + AOT + single-file
  analyzers; net8.0+ only — see multi-targeting note).
- Replace `Bind(this)` with the **Configuration Binding Source Generator**
  (`<EnableConfigurationBindingGenerator>true</EnableConfigurationBindingGenerator>`). This
  is .NET's official AOT-safe answer for `IConfiguration` binding; it uses C# 12 interceptors
  to swap the reflection binder for generated code. **It does NOT require `IOptions<T>`** —
  it intercepts the plain `ConfigurationBinder` APIs directly (`configuration.Bind(obj)`,
  `Get<T>()`, `GetValue<T>()`), no DI/options involvement needed. (Confirmed against
  https://learn.microsoft.com/dotnet/core/extensions/configuration-generator.)

  **The blocking constraint — this is the whole problem:** the generator only intercepts a
  call site when the **concrete target type is statically visible there**. When it can't
  resolve a concrete type it emits **SYSLIB1104**, does NOT intercept, and *silently falls
  back to the reflection binder* (reintroducing IL2026/IL3050, broken under AOT). Both of the
  library's current bind sites hit exactly that fallback:
    1. `section.Bind(this)` inside the `ConfigBase` ctor — `this` is statically `ConfigBase`,
       not the derived type, so the generator sees the base type only → SYSLIB1104.
    2. Binding over the open generic `T` in `Factory<T>` / `AddConfig<T>` → SYSLIB1104.
  The generated binder hard-casts (`(ConcreteType)instance`), so it must know the closed type
  at compile time. Conclusion: **the base-class-binds-`this` pattern is fundamentally what the
  generator cannot intercept** — the reflection call itself isn't the obstacle, the indirection
  through the base class / generic is.

  **The fix that stays off `IOptions`:** move the bind to a site where the concrete type is a
  literal. Either (a) each config class binds itself in its own ctor —
  `configuration.GetSection(key).Bind(this)` inside `AtlasProxyConfig` sees `this` as
  `AtlasProxyConfig` and gets a generated binder — or (b) bind by concrete type at
  registration via `section.Get<AtlasProxyConfig>()`. Cost: every one of the ~82 config
  classes needs its own concrete bind call; the "base class binds everyone once" convenience
  is not recoverable under the generator.
- Replace `Validator.ValidateObject` (IL2026) with source-generated validation via
  `[OptionsValidator]` — note that one *is* options-flavored, unlike the binding generator.
- Add a throwaway consumer app with `<PublishAot>true</PublishAot>` that references the
  library and calls `AddConfig<SomeRealConfig>()` — analyzers alone miss warnings that only
  surface at the consumer's generic-instantiation boundary.

### Open questions

- The blocker is not `IOptions` (see the binding-generator note — the generator works on
  plain `IConfiguration.Bind`/`Get` without it). The blocker is that the source generator
  can't see through the base class or the generic factory, so engaging it means moving the
  bind into each concrete config class (~82 sites). That is itself a **breaking, invasive
  change** on top of the major-version bump already in flight. Given the low value, it is
  almost certainly not justified.
- **Multi-targeting:** `obj/` shows netstandard2.0/2.1, net9.0, net10.0 builds, but the
  csproj declares only `net8.0`. Confirm the real TFM set — the AOT/trim analyzers don't run
  on netstandard targets, so AOT-safety only applies to net8.0+.

### Current state

The committed state is the *reverted* version: AOT attributes removed, `Extension` class
name preserved (avoids an ABI break in the mixed-version dependency graph), `Factory` using
`Activator.CreateInstance`. The library is functionally unchanged and **not** AOT-safe.

## Conclusion

(pending)
