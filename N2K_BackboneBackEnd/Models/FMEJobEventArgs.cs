using Microsoft.Extensions.Caching.Memory;
using N2K_BackboneBackEnd.Data;
using N2K_BackboneBackEnd.Models.backbone_db;

namespace N2K_BackboneBackEnd.Models
{
    public class FMEJobEventArgs : EventArgs
    {
        public EnvelopesToProcess Envelope { get; set; }

        public bool FirstInCountry { get; set; }
    }
}
