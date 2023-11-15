namespace WebApi.Dto
{
    public class ProductWithImageDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public byte[] Image { get; set; }
    }
}
