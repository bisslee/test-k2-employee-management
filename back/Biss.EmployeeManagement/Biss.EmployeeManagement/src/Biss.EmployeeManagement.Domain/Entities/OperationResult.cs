using System;
using System.Collections.Generic;
using System.Text;

namespace Biss.EmployeeManagement.Domain.Entities
{
    public class OperationResult<T>
    {
        public T Result { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; }
        public Exception Exception { get; set; }  // Use this property carefully.
        
    }
}
