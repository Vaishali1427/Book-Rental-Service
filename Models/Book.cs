namespace Book_Rental_Service.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Genre { get; set; }
        public string Status { get; set; }
    }
}
