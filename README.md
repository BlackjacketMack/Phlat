# Phlat
Phlat is a simple helper for your data tier, as it turns an object gra(PH) into a f(LAT) structure.  
It helps with managing objects as aggregate roots, which is commonly seen using the repository pattern.

While it can help track the state of objects, and indicate with underlying properties have changed, 
it's not intended to be an ORM.  Rather, it can assist EF/Dapper (for example) in determining how to 
merge complex types.
