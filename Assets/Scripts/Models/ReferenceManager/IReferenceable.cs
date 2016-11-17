using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IReferenceable
{
    //this should be IReferenceManager but it is 
    // better to hide the implementation of the 
    // folowing methods from the consumer:
    // HasLinkInstance
    // AddLinkInstance
    // RemoveLinkInstance
    // FirstLinkInstance
    ReferenceManager ReferenceManager { get; }
}

