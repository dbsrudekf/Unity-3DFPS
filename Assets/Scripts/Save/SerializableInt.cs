using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializableInt : MonoBehaviour
{
    int data;

    public SerializableInt(int i)
    {
        data = i;
    }

    public int ToInt()
    {
        return data;
    }
}
