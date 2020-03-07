# JitPad
[![Releases](https://img.shields.io/github/release/YoshihiroIto/JitPad)](https://github.com/YoshihiroIto/JitPad/releases/latest) 
[![Chocolatey](https://img.shields.io/chocolatey/dt/jitpad)](https://chocolatey.org/packages/jitpad/) 
[![MIT License](http://img.shields.io/badge/license-MIT-blue)](LICENSE)

On the fly disassemble C# code based on JitDasm

![JitPad](JitPad.gif)

## Install (for end user)

- Download from [release page](https://github.com/YoshihiroIto/JitPad/releases/latest)
- Install from [Chocolatey](https://chocolatey.org/packages/jitpad)
    - ```> choco install jitpad```

## Build and Run
```
> git clone --recursive https://github.com/YoshihiroIto/JitPad.git
> cd JitPad
> dotnet build
> JitPad\bin\Debug\netcoreapp3.1\JitPad.exe
```

or Open JitPad.sln

## Credits
- Disassembler
    - [JitDasm](https://github.com/0xd4d/JitDasm) Disassemble jitted .NET methods

- C# compiler
    - [Roslyn](https://github.com/dotnet/roslyn) The Roslyn .NET compiler provides C# and Visual Basic languages with rich code analysis APIs.

- GUI
    - [Biaui](https://github.com/YoshihiroIto/Biaui) WPF dark theme and controls.
    - [AvalonEdit](https://github.com/icsharpcode/AvalonEdit) The WPF-based text editor component used in SharpDevelop
    - [XamlBehaviors for WPF](https://github.com/Microsoft/XamlBehaviorsWpf) XAML Behaviors is an easy-to-use means of adding common and reusable interactivity to your WPF applications with minimal code. 
    - [RoslynPad](https://github.com/aelij/RoslynPad) A cross-platform C# editor based on Roslyn and AvalonEdit 
    - [Material Design](https://github.com/Templarian/MaterialDesign) ✒4900+ Material Design Icons from the Community

- Test
    - [VSTest](https://github.com/microsoft/vstest/) Visual Studio Test Platform is the runner and engine that powers test explorer and vstest.console.
    - [xUnit.net](https://github.com/xunit/xunit) xUnit.net is a free, open source, community-focused unit testing tool for the .NET Framework.
    - [coverlet.collector](https://github.com/tonerdo/coverlet) Cross platform code coverage for .NET

- Misc
    - [IgnoresAccessChecksToGenerator](https://github.com/aelij/IgnoresAccessChecksToGenerator) Generates reference assemblies where all the internal types & members become public, and applies the IgnoresAccessChecksTo attribute
    - [ReactiveProperty](https://github.com/runceel/ReactiveProperty) ReactiveProperty provides MVVM and asynchronous support features under Reactive Extensions.
    - [Reactive Extensions](https://github.com/dotnet/reactive) The Reactive Extensions for .NET
    - [K4os.Compression.LZ4](https://github.com/MiloszKrajewski/K4os.Compression.LZ4) LZ4/LH4HC compression for .NET Standard 1.6/2.0 (formerly known as lz4net)
    - [Livet](https://github.com/runceel/Livet) WPF MVVM Infrastructure.


## References

- [Tiered Compilation Guide](https://github.com/dotnet/runtime/blob/master/docs/design/features/tiered-compilation-guide.md)
- [Run-time configuration options for compilation](https://docs.microsoft.com/en-us/dotnet/core/run-time-config/compilation)


## Useful Patterns

This solution is made on a very small scale. Therefore it is very readable. I hope you find it helpful.

- Basic WPF
    - Style
    - Behavior
- Non freeze GUI
- MVVM
- Dependency Injection
- C# compilation by Roslyn
- C# code completion by Roslyn
- C# editor by AvalonEdit
    - Popup code completion
    - Error indication


## Author
Yoshihiro Ito  
Twitter: [https://twitter.com/yoiyoi322](https://twitter.com/yoiyoi322)  
Email: yo.i.jewelry.bab@gmail.com  


## License
MIT

