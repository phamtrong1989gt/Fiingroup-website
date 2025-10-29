using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PT.Domain.Seedwork
{
    public class ApiResponse<T> where T : class
    {
        public bool Status { get; set; } = false;
        public string Message { get; set; }
        public string Error { get; set; }
        public T Data { get; set; }
        public long? TotalRecord { get; set; }        
    }

    public class ResponseApi<T> : ApiResponse<T> where T : class
    {
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse WithData(IEnumerable<object> list, string message = null)
        {
            var lst = list.ToList();
            return new ApiResponse
            {
                Data = lst,
                TotalRecord = lst.Count,
                Message = message,
                Status = true
            };
        }

        public static ApiResponse WithData(object data, int size = 1, string message = null)
        {
            return new ApiResponse
            {
                Data = data,
                TotalRecord = data==null ? 0 : size,
                Message = message,
                Status = true
            };
        }

        public static ApiResponse WithStatus(bool status, string error = null, string message = null)
        {
            return new ApiResponse
            {
                Status = status,
                Error = error,
                Message = message
            };
        }
    }
}
