# Sample Data


**Define an aggregate**

```csharp
public class Base
{
	public int Id { get; set; }
	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
			return false;

		return this.Id == ((Base)obj).Id;
	}

	public override int GetHashCode() =>  Id.GetHashCode();

	public override string ToString() => $"{GetType().Name}:{Id}";
}

//Person will be our aggregate root
public class Person : Base
{
	public string Name { get; set; }
	public int Age { get; set; }
	public IList<Address> Addresses { get; set; } = new List<Address>();
}

//A person can have multiple addresses
public class Address : Base
{
	public string Street { get; set; }
	public string City { get; set; }
	public string State { get; set; }
	public bool IsShipping { get; set; }
}
```

**Create the aggregate with data**

```csharp
var person = new Person
{
	Id = 1,
	Name = "Stanley",
	Age = 29,
	Addresses = new List<Address>
	{
		new Address
		{
			Id = 1,
			Street = "Lombard Street",
			City = "San Francisco",
			State = "California",
			IsShipping = false
		},
		
		new Address
		{
			Id = 2,
			Street = "Hollywood Boulevard",
			City = "Hollywood",
			State = "California",
			IsShipping = true
		},
	}
}
```