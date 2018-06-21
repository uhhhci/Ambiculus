using UnityEngine;
using System.Collections;

public static class ExtensionMethods {

	public static void Foo()
	{
		Debug.Log("foo");
	}
	public static void Foo(this MonoBehaviour mono)
	{
		Debug.Log("Foo");
	}
}
