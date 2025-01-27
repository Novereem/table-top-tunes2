using Shared.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class Scene : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
