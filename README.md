# Phlat

Phlat is a simple helper built in C# that turns an object gra(PH) into a f(LAT) structure.  
It is meant to help manage [aggregates](https://martinfowler.com/bliki/DDD_Aggregate.html).
There are two core methods: **Flatten** and **Merge**.  

While Phlat is a helpful library for repositories, it is not partial to any database implementation.  
It can be used with EF as it transforms targets, or assist with Dapper CRUD statements for complex object types.
Phlat is intended to be simple and transparent in how it navigates an aggregate and reveals the underlying properties
and mutations.

[![.NET](https://github.com/BlackjacketMack/Phlat/actions/workflows/dotnet.yml/badge.svg)](https://github.com/BlackjacketMack/Phlat/actions/workflows/dotnet.yml)

**Sample Class Structure and Data**


The examples on this page use a simple Person/Address structure with some prepopulated values.  Specifically, a person named 'Stanley' has multiple addresses. 
[Full sample data used on this page can be viewed here.](docs/README-Data.md)


## Flatten

Flatten exists as a descriptive function that can be used for diagnostic purposes.  
(Note that it is the basis for our merge method outlined below.)

![Simple object](./docs/Phlat1.png)

Becomes...

| Path | Model | Values | 
| ------ | ----- | ------ |
| [ROOT] | Person 1 | [Name, Stanley],[Age, 29],[Id, 1] |
| [ROOT].Addresses | Address 1 | [Street, Lombard Street],[City, San Francisco],[State, California],[IsShipping, False],[Id, 1] |
| [ROOT].Addresses | Address 2 | [Street, Hollywood Boulevard],[City, Hollywood],[State, California],[IsShipping, True],[Id, 2] |



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

| IsRoot | Model | Values | 
| ------ | ----- | ------ |
| True | Person 1 | [Name, Stanley],[Age, 29],[Id, 1] |
| False | Address 1 | [Street, Lombard Street],[City, San Francisco],[State, California],[IsShipping, False],[Id, 1] |
| False | Address 2 | [Street, Hollywood Boulevard],[City, Hollywood],[State, California],[IsShipping, True],[Id, 2] |


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

config.Configure<Address>();

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

var results = phlat.Merge(sourcePerson,targetPerson);
```

*The above call yields the following results::*

| Path | Model | State | Values | Updates |
| ------ | ----- | ----- | ------ | ------- |
| [ROOT] | Person:1 | Updated | `[Name, FLAT Stanley],[Age, 30],[Id, 1]` | `[Name, { OldValue = Stanley, NewValue = FLAT Stanley }],[Age, { OldValue = 29, NewValue = 30 }]` |
| [ROOT].Addresses | Address:1 | Unchanged | `[Street, Lombard Street],[City, San Francisco],[State, California],[IsShipping, False],[Id, 1]` |  |
| [ROOT].Addresses | Address:2 | Unchanged | `[Street, Hollywood Boulevard],[City, Hollywood],[State, California],[IsShipping, True],[Id, 2]` |  |


## Additional Topics
* How items are deleted when merging
* How items are compared for equality when merging
* How deep nested properties can be setup
* How Phlat can be used with EF
* How Phlat can be used with Dapper
* How simple properties (non IList properties) can be setup