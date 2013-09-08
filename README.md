pathlib
=======

Path manipulation library for .Net

We're on [NuGet](https://www.nuget.org/packages/PathLib/)!

Inspired by Python's [pathlib](https://pathlib.readthedocs.org/en/latest/)
module.

See Unit Tests for more thorough examples (and intellisense for description
of each method and property).

    var path = new PureNtPath(@"a");
    var expected = new PureNtPath(@"a\b");

    var joined = path.Join("b");

    Assert.AreEqual(expected, joined);