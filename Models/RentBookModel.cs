namespace Book_Rental_Service.Models
{
    public class RentBookModel
    {
        
            public int RentalId { get; set; }
            public int BookId { get; set; }
            public string BorrowerName { get; set; }
            public string BorrowerEmail { get; set; }
            public string BorrowerPhone { get; set; }
        
    }
}
