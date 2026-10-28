namespace Bikuida.Models
{
    public class Offsets
    {
        public int m_hPlayerPawn { get; set; } = 2316;
        public int m_iszPlayerName { get; set; } = 1780;
        public int dwEntityList { get; set; } = 38692240;
        public int m_iHealth { get; set; } = 844;
        public int m_iTeamNum { get; set; } = 1003;
        public int m_vOldOrigin { get; set; } = 5008;
        public int dwLocalPlayerPawn { get; set; } = 36963992;
        public int dwViewMatrix { get; set; } = 36985648;
        public int m_pGameSceneNode { get; set; } = 816;
        public int m_skeletonInstance { get; set; } = 128;
        public int m_modelState { get; set; } = 336;
        public int m_lifeState { get; set; } = 852;
    }

    public class Metadata
    {
        public string Id { get; set; }
        public bool Private { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Name { get; set; }
    }

    public class JsonBinResponse
    {
        public Offsets Record { get; set; }
        public Metadata Metadata { get; set; }
    }
}
