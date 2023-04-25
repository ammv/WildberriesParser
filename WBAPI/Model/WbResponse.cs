namespace SimpleWbApi.Model
{
    public class WbResponse
    {
        public WbMetadata Metadata { get; set; }
        public int State { get; set; }
        public int Version { get; set; }
        public WbParams Params { get; set; }

        public WbData Data { get; set; }
    }
}