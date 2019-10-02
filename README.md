# SimpleNARC is a super-simple library to read NARC files

There is only one class:

* It holds an array of each file as a MemoryStream
* There's a method to save each file to a drive

As I said, it's super-simple.

It should work with the majority of NARC files out there, but I don't have odd cases to test, so create an issue if you experience problems.

This library is .NET Standard 2.0.

----
# SimpleNARC uses:
* [My EndianBinaryIO library](https://github.com/Kermalis/EndianBinaryIO)