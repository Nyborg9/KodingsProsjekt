namespace WebApplication2.Models
{
    public class OverviewModel
    {
        public List<AreaChange> AreaChanges { get; set; }
        public List<UserData> UserDatas { get; set; }
        public string SelectedMapVariant { get; set; }
    }
}
