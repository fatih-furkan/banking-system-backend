using System.ComponentModel.DataAnnotations; 


namespace Bank.CustomerService.Models.Entities; 

public class Customer   
{
    public long CustomerId { get; set; }  

    public string? Name { get; set; }  

    public string? Surname { get; set; } 


    public string? Tc { get; set; }  


    public string Status { get; set; } = null!;  
}  