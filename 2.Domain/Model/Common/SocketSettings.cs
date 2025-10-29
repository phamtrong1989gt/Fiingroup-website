using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PT.Domain.Model.Common;

namespace PT.Domain.Model
{
    public class SocketSettings
    {
        public string ServerIP { get; set; }
        public int ServerPort { get; set; }
        public string HealthCheck { get; set; }
        public int ClientMillisecondTimeout { get; set; }
        public int ClientHealthCheckInterval { get; set; }
    }
}
