using System.ComponentModel.DataAnnotations;  // logically same with import, data validation.


namespace Bank.CustomerService.Models.Entities;  // it helps us to make it easier to find class from other classes.

public class Customer   // we can reach this class from other classes because it is public. 
{
    public long CustomerId { get; set; }  // primary key. getter and setter methods are used to get and set the value of the property.

    public string? Name { get; set; }    // it can be null. getter and setter methods are used to get and set the value of the property.


    public string? Surname { get; set; } // it can be null. getter and setter methods are used to get and set the value of the property.


    public string? Tc { get; set; }  // it can be null. getter and setter methods are used to get and set the value of the property.


    public string Status { get; set; } = null!;  // it cannot be null. getter and setter methods are used to get and set the value of the property. Active or passive
}  