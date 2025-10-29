using System;
using System.Collections.Generic;
using System.Text;

namespace PT.Domain.Model
{
    public class ResponseHepper
    {
        public static ResponseModel Success(string message, bool isClosePopup) =>
          new ResponseModel
          {
              Output = 1,
              Message = message,
              Type = ResponseTypeMessage.Success,
              IsClosePopup = isClosePopup
          };

        public static ResponseModel Warning(string message) =>
            new ResponseModel
            {
                Output = 0,
                Message = message,
                Type = ResponseTypeMessage.Warning
            };

        public static ResponseModel Error() =>
            new ResponseModel
            {
                Output = -1,
                Message = "Đã xảy ra lỗi, vui lòng F5 trình duyệt và thử lại.",
                Type = ResponseTypeMessage.Danger,
                Status = false
            };
    }

    public class ResponseModel
    {
        public bool Status { get; set; } = true;
        public string Message { get; set; }
        public int Output { get; set; }
        public string Data { get; set; }
        public string Type { get; set; }
        public bool IsUse { get; set; } = false;
        public bool IsClosePopup { get; set; } = false;
        public bool IsRedirect { get; set; } = false;
        public string RedirectUrl { get; set; }
    }
    public class ResponseModel<T> where T : class
    {
        public bool Status { get; set; } = true;
        public string Message { get; set; }
        public int Output { get; set; }
        public T Data { get; set; }
        public string Type { get; set; }
        public bool IsClosePopup { get; set; } = true;
    }
    public class ResponseTypeMessage
    {
        public static string Success { get { return "success"; } }
        public static string Danger { get { return "danger"; } }
        public static string Warning { get { return "warning"; } }
        public static string Error { get { return "error"; } }
    }
}
