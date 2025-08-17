using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeNetworkException : Exception 
{ 
    public FakeNetworkException(string m) : base(m) { } 
}


public class FakeTimeoutException : Exception 
{ 
    public FakeTimeoutException(string m) : base(m) { } 
}
