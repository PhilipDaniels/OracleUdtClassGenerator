/*
 * This project uses the generator as a NuGet package, not as a
 * project! This is the best way to test it.
 * 
 * During the development cycle, the NuGet package can be cleaned out
 * by using my script from BassUtils:
 * 
 *     git clean -xdf
 *     C:\repos\BassUtils\clear_package_cache.ps1 OracleUdtClassGenerator
 * 
 * Then just rebuild, the package will be pulled again from C:\NuGet.
 * 
 * The following lines ensure that code generation is working and
 * creating classes in the correct namespaces.
 */


var x = new People.PersonRecord();
var y = new OracleUdtClassGenerator.ConsoleTestHarness.ArticleMasterRecord();
var z = new MyNamespace.MyClass();
