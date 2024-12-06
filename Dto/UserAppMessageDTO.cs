namespace outofoffice.Dto
{
    public class UserAppMessageDTO
    {
        public Guid Company_ID { get; set; }
        public string Group_ID { get; set; }
        public string User_ID { get; set; }
        public string Message_Type { get; set; }
        public DateTime OOO_From_Dt { get; set; }
        public DateTime OOO_To_Dt { get; set; }
        public string Message_Txt { get; set; }
        public string Apps_To_Publish { get; set; }
        public string Publish_Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid UAID { get; set; }
    }
}
