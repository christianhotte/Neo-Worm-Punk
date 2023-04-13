using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyUtils
{
    public static int MyMod(int num, int mod) => (((num % mod) + mod) % mod);
}
