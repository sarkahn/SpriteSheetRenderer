using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class DataState
{
    EntityQuery query_;
    ComponentType type_;

    public DataState(EntityQuery query, ComponentType type)
    {
        query_ = query;
        type_ = type;
    }
    
    public void SetFilter()
    {
        query_.SetFilterChanged(type_);
    }
}
