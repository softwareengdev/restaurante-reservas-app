using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurante.Modelo.Model.Common
{
    public class Auditable
    {
        DateTime CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
    }
}
