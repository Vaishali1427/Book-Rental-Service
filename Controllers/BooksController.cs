using Book_Rental_Service.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Data.SqlClient;



namespace Book_Rental_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    
    public class BooksController : ControllerBase
    {
        private readonly string connectionString;


        public BooksController(IConfiguration configuration) 
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        // GET: api/books
        //[Authorize]
        [HttpGet]
        public IActionResult GetBooks()
        {
            List<Book> books = new List<Book>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Books";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Book book = new Book
                    {
                        BookId = (int)reader["BookId"],
                        Title = reader["Title"].ToString(),
                        Author = reader["Author"].ToString(),
                        PublicationDate = (DateTime)reader["PublicationDate"],
                        Genre = reader["Genre"].ToString(),
                        Status = reader["Status"].ToString()
                    };

                    books.Add(book);
                }

                reader.Close();
            }

            return Ok(books);
        }

        // GET: api/books/{id}
        //   [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetBook(int id)
        {
            Book book = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Books WHERE BookId = @BookId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BookId", id);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    book = new Book
                    {
                        BookId = (int)reader["BookId"],
                        Title = reader["Title"].ToString(),
                        Author = reader["Author"].ToString(),
                        PublicationDate = (DateTime)reader["PublicationDate"],
                        Genre = reader["Genre"].ToString(),
                        Status = reader["Status"].ToString()
                    };
                }

                reader.Close();
            }

            if (book == null)
                return NotFound();

            return Ok(book);
        }

        // POST: api/books
        
        [HttpPost]
        public IActionResult AddBook([FromBody] Book book)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO Books (BookId,Title, Author, PublicationDate, Genre, Status) VALUES (@BookId,@Title, @Author, @PublicationDate, @Genre, @Status)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BookId", book.BookId);
                command.Parameters.AddWithValue("@Title", book.Title);
                command.Parameters.AddWithValue("@Author", book.Author);
                command.Parameters.AddWithValue("@PublicationDate", book.PublicationDate);
                command.Parameters.AddWithValue("@Genre", book.Genre);
                command.Parameters.AddWithValue("@Status", book.Status);
                command.ExecuteNonQuery();
            }

            return Ok();
        }

        // PUT: api/books/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateBook(int id, [FromBody] Book book)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "UPDATE Books SET Title = @Title, Author = @Author, PublicationDate = @PublicationDate, Genre = @Genre, Status = @Status WHERE BookId = @BookId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Title", book.Title);
                command.Parameters.AddWithValue("@Author", book.Author);
                command.Parameters.AddWithValue("@PublicationDate", book.PublicationDate);
                command.Parameters.AddWithValue("@Genre", book.Genre);
                command.Parameters.AddWithValue("@Status", book.Status);
                command.Parameters.AddWithValue("@BookId", id);
                command.ExecuteNonQuery();
            }

            return Ok();
        }

        // DELETE: api/books/{id}
        
        [HttpDelete("{id}")]
        public IActionResult DeleteBook(int id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var deleteRentalquery = "DELETE FROM RentalHistory WHERE BookId = @BookId";
                SqlCommand DeleteRentalcommand = new SqlCommand(deleteRentalquery, connection);
                DeleteRentalcommand.Parameters.AddWithValue("@BookId", id);
                DeleteRentalcommand.ExecuteNonQuery();

                var deleteBookquery = "DELETE FROM Books WHERE BookId = @BookId";
                SqlCommand DeleteBookcommand = new SqlCommand(deleteBookquery, connection);
                DeleteBookcommand.Parameters.AddWithValue("@BookId", id);
                DeleteBookcommand.ExecuteNonQuery();


            }

            return Ok();
        }

        
        [HttpPost("rent")]
        public IActionResult RentBook([FromBody] RentBookModel rentBookModel)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Check if the book is available for rent
                string checkQuery = "SELECT Status FROM Books WHERE BookId = @BookId";
                SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@BookId", rentBookModel.BookId);
                var status = checkCommand.ExecuteScalar();

                if (status == null || status.ToString() != "Available")
                {
                    return BadRequest("The requested book is not available for rent.");
                }

                // Update book status to "rented"
                string updateQuery = "UPDATE Books SET Status = 'rented' WHERE BookId = @BookId";
                SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                updateCommand.Parameters.AddWithValue("@BookId", rentBookModel.BookId);
                updateCommand.ExecuteNonQuery();

                // Add rental record to history
                string insertQuery = "INSERT INTO RentalHistory (BookId, BorrowerName, BorrowerEmail, BorrowerPhone, RentalDate) VALUES (@BookId, @BorrowerName, @BorrowerEmail, @BorrowerPhone, @RentalDate)";
                SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                
                insertCommand.Parameters.AddWithValue("@BookId", rentBookModel.BookId);
                insertCommand.Parameters.AddWithValue("@BorrowerName", rentBookModel.BorrowerName);
                insertCommand.Parameters.AddWithValue("@BorrowerEmail", rentBookModel.BorrowerEmail);
                insertCommand.Parameters.AddWithValue("@BorrowerPhone", rentBookModel.BorrowerPhone);
                insertCommand.Parameters.AddWithValue("@RentalDate", DateTime.UtcNow);
                insertCommand.ExecuteNonQuery();
            }

            return Ok();
        }

        // POST: api/books/return
        [HttpPost("return")]
        public IActionResult ReturnBook([FromBody] ReturnBookModel returnBookModel)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Check if the book is rented
                string checkQuery = "SELECT Status FROM Books WHERE BookId = @BookId";
                SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@BookId", returnBookModel.BookId);
                var status = checkCommand.ExecuteScalar();

                if (status == null || status.ToString() != "rented")
                {
                    return BadRequest("The book is not currently rented.");
                }

                // Update book status to "available"
                string updateQuery = "UPDATE Books SET Status = 'Available' WHERE BookId = @BookId";
                SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                updateCommand.Parameters.AddWithValue("@BookId", returnBookModel.BookId);
                updateCommand.ExecuteNonQuery();

                // Update return date in rental history
                string updateHistoryQuery = "UPDATE RentalHistory SET ReturnDate = @ReturnDate WHERE BookId = @BookId AND ReturnDate IS NULL";
                SqlCommand updateHistoryCommand = new SqlCommand(updateHistoryQuery, connection);
                updateHistoryCommand.Parameters.AddWithValue("@BookId", returnBookModel.BookId);
                updateHistoryCommand.Parameters.AddWithValue("@ReturnDate", DateTime.UtcNow);
                updateHistoryCommand.ExecuteNonQuery();
            }

            return Ok();
        }

    }
}

 