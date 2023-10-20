namespace notesApp.API.Models.DataTransferObject
{
    public class AddNoteREquest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DateCreated { get; set; }
    }
}
