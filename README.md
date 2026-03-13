# Template-WebApp

Note implementative e suggerimenti

- Il sorting dinamico usa EF.Property<object>(e, propertyName). Valida che i nomi delle proprietà passate in sortBy siano corretti per evitare runtime errors; puoi aggiungere una whitelist o reflection-based validation se vuoi sicurezza.
- Il global query filter considera qualsiasi entità che implementa ISoftDelete. Le entità non soft-deletable restano inalterate.
- Per "undelete" (restore) basta recuperare l'entità con IgnoreQueryFilters(), impostare IsDeleted = false e salvare.
- Se preferisci usare System.Linq.Dynamic.Core per sorting dinamico con stringhe (supporta "Name desc"), posso sostituire l'implementazione con quella libreria.

Testing

- Lista delle entità configurate:
GET http://localhost:5000/api/sortables

- Lista delle proprietà ordinabili per Product:
GET http://localhost:5000/api/sortables?entity=Product

- Possibilità d'usare anche il full type name:
GET http://localhost:5000/api/sortables?entity=Repository.Api.Entities