using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyUtils
{
    //why is this not default in Unity it should be
    public static int MyMod(int num, int mod) => (((num % mod) + mod) % mod);
}
