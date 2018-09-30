using System;
using System.Collections.Generic;
using System.Text;

namespace RestTemplate
{
    public class RestResponseBody<T> : RestResponseContent
    {
        public T Body { get; set; }
    }
}
