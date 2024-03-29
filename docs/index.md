FakeItEasy provides Roslyn analyzer packages to detect incorrect usages of the library
that cannot be prevented by the API or the compiler but will typically result in
bugs at runtime.

## Installation

The packages work in Visual Studio 2017 version 15.9 or later. Install one of these
NuGet packages in each project that needs it:

- For C# projects: [FakeItEasy.Analyzer.CSharp](https://www.nuget.org/packages/FakeItEasy.Analyzer.CSharp)
- For VB.NET projects: [FakeItEasy.Analyzer.VisualBasic](https://www.nuget.org/packages/FakeItEasy.Analyzer.VisualBasic)

## Diagnostics

Each analyzer package provides the following diagnostics:

| Id             | Summary                                  | Code Fix? | Description                                                                                                                                           |
|----------------|------------------------------------------|-----------|-------------------------------------------------------------------------------------------------------------------------------------------------------|
| FakeItEasy0001 | Unused call specification                | no        | Triggered when you specify a call but don't configure or assert it, making it a no-op.                                                                |
| FakeItEasy0002 | Non-virtual member configuration         | no        | Triggered when you try to configure a non-virtual member, which cannot be faked.                                                                      |
| FakeItEasy0003 | Argument constraint outside call spec    | no        | Triggered when you try to use an [argument constraint](https://fakeiteasy.github.io/docs/stable/argument-constraints/) outside of a [call specification](https://fakeiteasy.github.io/docs/stable/specifying-a-call-to-configure/). |
| FakeItEasy0004 | Argument constraint nullability mismatch | yes       | Triggered when you use a non-nullable [argument constraint](https://fakeiteasy.github.io/docs/stable/argument-constraints/) for a nullable parameter. Calls where the argument is null won't be matched. If this is intentional, consider using `A<T?>.That.IsNotNull()` instead. If it's not, make the argument constraint nullable (`A<T?>`). |
| FakeItEasy0005 | Argument constraint type mismatch        | yes       | Triggered when you use an [argument constraint](https://fakeiteasy.github.io/docs/stable/argument-constraints/) whose type doesn't match the parameter. No such calls can be matched.        |
| ~~FakeItEasy0006~~ | ~~Assertion uses legacy Repeated class~~ | ~~yes~~ | ~~Triggered when you make an assertion using the obsolete `MustHaveHappened(Repeated)` overload.~~ **Retired since version 6.0.0**                  |
