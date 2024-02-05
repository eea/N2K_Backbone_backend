namespace N2K_BackboneBackEnd.Models
{
    public class FMEJobEventArgs : EventArgs
    {
        public EnvelopesToProcess Envelope { get; set; }
        public bool FirstInCountry { get; set; }
    }
}