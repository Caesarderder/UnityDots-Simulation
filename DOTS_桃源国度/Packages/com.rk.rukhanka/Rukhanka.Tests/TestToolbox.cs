using NUnit.Framework;
using System.Reflection;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Tests
{ 
public static class TestToolbox
{
    public static MethodInfo GetStaticPrivateMethod<T>(string fnName)
    {
        if (string.IsNullOrWhiteSpace(fnName))
			Assert.Fail("Function name cannot be empty");

		var classType = typeof(T);
		MethodInfo rv = classType.GetMethod(fnName, BindingFlags.Static | BindingFlags.NonPublic);

		if (rv == null)
			Assert.Fail(string.Format("{0} method not found", fnName));

		return rv;
    }
}
}
