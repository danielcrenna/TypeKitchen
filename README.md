# TypeKitchen

![logo](assets/logo.png?s=100)
TypeKitchen is a small library for fast meta-programming in .NET.

TypeKitchen was built on the premise that there are only so many actions that are useful to take with types at runtime.
However, rather than standardize on a single library that performs all of these actions well, there are myriad libraries available on NuGet, each taking a different approach with varying levels of performance, in terms of quality, speed, and memory use.

![xkcd](https://imgs.xkcd.com/comics/standards.png)

TypeKitchen replaces these libraries with a concise API to perform the following meta-programming tasks:

#### Common Tasks
- Field and Property Access: _getting and setting data members by name, including private members and anonymous types_
- Method Call Invocation: _calling methods on runtime types, when you know the arguments at runtime, and even when you don't_
- Object Activation: _creating new instances of types, typically because `Activator.CreateInstance` is too slow_
- Object Pooling: _when you want to avoid over-allocating memory that will be garbage collected later_
- Type Resolution: _when you want to describe how object instances should be created, and manage their lifetime, in a deferred fashion (i.e. inversion of control / dependency injection)_
- Wire Serialization: _when you want a fast and non-allocating wire format for serializing/deserializing runtime types_

#### Less Common Tasks
- Templating: _when you want to create string templates based on data in C# objects, and may want to limit what code can be executed in those templates to a strict DSL or set of types_
- Snippets: _when you want to write adhoc C# code, and have it compile and execute at runtime without requiring application downtime or reflection overhead_
- Weaving: _when you want to inject custom code before or after methods, even if they live outside your own code_
- Duck Casting: _when you have a type or method, and you want to call it _as if it were_ an implementation of a interface, when it isn't_

#### Rare / Advanced Tasks
- Composition: _when you want to build up a type at runtime to implement various members declared in other types, or provided inline_
- Flyweight Factory: _when you want to represent one or more views of a piece of data, but do not want to materialize those views_
- MSIL Helpers: _when you want to see the IL of a compiled Expression or DynamicMethod, or want to code-generate calls to `ILGenerator` that would produce a given method body_
- Coverage: _when you need to walk compiled bytecode to determine paths through other code, such as when building code coverage or visualization tools_

##### Logo

Logo is from Font Awesome, and is under a [CC BY 4.0 License](https://creativecommons.org/licenses/by/4.0/). 