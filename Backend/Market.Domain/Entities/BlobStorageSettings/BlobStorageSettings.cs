using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Market.Domain.Entities.BlobStorageSettings
{
    public class BlobStorageSettings
    {
        public string ConnectionString { get; set; } = "";
        public string ContainerName { get; set; } = "";
    }

}
