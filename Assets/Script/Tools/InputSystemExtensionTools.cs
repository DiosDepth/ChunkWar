using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class InputSystemExtensionTools
{
    public static string PathToKeyName(this string path)
    {
        string[] str = path.Split('/');
        return str[2];
    }

    public static string PathToKeyBinding(this string path)
    {
        string[] str = path.Split('/');
        return "<" + str[1] + ">/" + str[2];
    }
}
