using System;
using System.ComponentModel.DataAnnotations;
using PT.Domain.Seedwork;

namespace PT.Domain.Model
{
    public enum LogType
    {
        [Display(Name = "Không xác định")]
        None = -1,
        [Display(Name = "Tạo mới dữ liệu")]
        Create,
        [Display(Name = "Cập nhật dữ liệu")]
        Edit,
        [Display(Name = "Xóa dữ liệu")]
        Delete,
        [Display(Name = "Điều khiển")]
        Control
    }
  
    public class Log : IAggregateRoot
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int ObjectId { get; set; }
        [MaxLength(50)]
        public string Object { get; set; }
        [MaxLength(50)]
        public string ObjectType { get; set; }
        public DateTime ActionTime { get; set; }
        public string AcctionUser { get; set; }
        public LogType Type { get; set; }
    }
   
    public class WriteLogModel
    {
        //string name,int objectId,object valueBefore, object valueAfter, string table=null
        public string Name { get; set; }
        public int ObjectId { get; set; }
    }
    public class LogModel
    {
        public string Name { get; set; }
        public int ObjectId { get; set; }
        public DateTime ActionTime { get; set; }
        public LogType Type { get; set; }
    }
    public class LogUserModel
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
