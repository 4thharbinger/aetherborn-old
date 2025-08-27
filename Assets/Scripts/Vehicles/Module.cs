using System;

using UnityEngine;

[Serializable]
public class Module : MonoBehaviour
{
    public Part part;

    public Vehicle vehicle => part.vehicle;
}
