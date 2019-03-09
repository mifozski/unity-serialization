﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Serialization.Json
{
    public enum JsonNodeType
    {
        None,
        Object,
        Array,
        String,
        Number,
        Boolean,
        Null,
        EndObject,
        EndArray
    }
}
