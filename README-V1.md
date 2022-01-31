# Phlat
Phlat is a simple helper built in C# that turns an object gra(PH) into a f(LAT) structure.
It has two core methods: **Flatten** and **Merge**.  It can be used in repositories to help wrangle aggregate roots and account for changes in the aggregate.


## Sample Class Structure and Data



The examples on this page use a simple Person/Address structure with some prepopulated values.  Specifically, a person named 'Stanley' has multiple addresses. 
[Full sample data used on this page can be viewed here.](docs/README-Data.md)


## Flatten

**Create a configuration**

Configurations can be defined for each aggregate root, or they can be defined once per application.

```csharp
var config = new PhlatConfiguration();

config.Configure<Person>()
		//indicate that there are many addresses
		.HasMany(m => m.Addresses);

config.Configure<Address>();

var phlat = new Phlat(config);
```

**Flatten a Person object**
```csharp
var results = phlat.Flatten(person);
```

*The above call yields the following results:*

| IsRoot | Model | State | Values | 
| ------ | ----- | ----- | ------ |
| True | Person 1 | Unchanged | [Name, Stanley],[Age, 29],[Id, 1] |
| False | Address 1 | Unchanged | [Street, Lombard Street],[City, San Francisco],[State, California],[IsShipping, False],[Id, 1] |
| False | Address 2 | Unchanged | [Street, Hollywood Boulevard],[City, Hollywood],[State, California],[IsShipping, True],[Id, 2] |


The results above show how our aggregate root has been flattened into several results.

## Merge
Merge flattens two objects and combines them.  The 'source' object gets merged onto the *target* object.  In practice, the *source* object is typically a graph received from an api, or possibly from form fields.  The *target* on the other hand would be the stored object in the database.  When merging you want to take the object from the interface and mutate the database object.  

Finally, it should be noted that we are actually mutating the target model.  If an non-root gets added to a list, the target model will have that item added to the list.  

**Create a configuration**

Our configuration is a little more involved this time because we need to instruct Phlat about how to mutate the target.
```csharp
var config = new PhlatConfiguration();

//configure *Person* but this time indicate what fields you want updated during a merge process
config.Configure<Person>((s,t)=>{
			t.Name = s.Name;		
			t.Age = s.Age;
		})
		//indicate that there are many addresses
		.HasMany(m => m.Addresses);

//configure *Address* and configure the same fields
config.Configure<Address>((s,t)=>{
			t.Street = s.Street
            t.City = s.City
            t.State = s.State
            t.IsShipping = s.IsShipping
		}));

var phlat = new Phlat(config);
```

**Merge a Person object**
```csharp
var sourcePerson = new Person{
Id = 1,
Name = "FLAT Stanley",
Age = 30
};



var targetPerson = //person from sample data...would be in db

var results = phlat.Flatten(sourcePerson,targetPerson);
```

*The above call yields the following results::*

| IsRoot | Model | State | Values | Changes |
| ------ | ----- | ----- | ------ | ------- |
| True | Person:1 | Updated | [Name, Stanley],[Age, 29],[Id, 1] | [Name, FLAT Stanley],[Age, 30] |
| False | Address:1 | Unchanged | [Street, Lombard Street],[City, San Francisco],[State, California],[IsShipping, False],[Id, 1] |  |
    | False | Address:2 | Unchanged | [Street, Hollywood Boulevard],[City, Hollywood],[State, California],[IsShipping, True],[Id, 2] |  |

