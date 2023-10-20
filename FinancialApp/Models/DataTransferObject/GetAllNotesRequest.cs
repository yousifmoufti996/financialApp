namespace notesApp.API.Models.DataTransferObject
{
    public class GetAllNotesRequest
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DateCreated { get; set; }
    }
}
