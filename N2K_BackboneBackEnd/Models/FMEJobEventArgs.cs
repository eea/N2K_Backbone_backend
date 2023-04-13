using N2K_BackboneBackEnd.Models.backbone_db;

namespace N2K_BackboneBackEnd.Models
{
    public class FMEJobEventArgs : EventArgs
    {
            public bool AllFinished { get; set; }
            public EnvelopesToProcess Envelope { get; set; }

    }
}
