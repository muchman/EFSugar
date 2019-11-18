# EFCore Sugar
> EFCore Sugar setups boilerplate for a repository pattern and contains powerful filter objects to aid in creating predicate filters on the fly.


EFCore Sugar is an opinionated library to assist in setting up boilerplate repositories with basic CRUD operations and expose a way to create filters for querying data from Entity Framework Core based on a simple POCO style object.  This filter capability is especially useful for implementing things like search, ordering, and paging.  
The filters dynamically build predicates to inject into linq Where() clauses based on the filter properties and the entity properties. This keeps you from having to maintain code that looks like:

```C#
query.Where(order => OrderFilter.Id != 0 ? order.Id == OrderFilter.Id : true && 
    OrderFilter.UserName != null ? Order.User.Name == OrderFilter.userName : true ); //... and on and on
```

EFCore Sugar is opinionated in that it does not favor a pattern of repository for every entity type.  This often leads to injecting several different “repositories” just to access all the data you need in a service.  Instead this library introduces the idea of a repository grouping which is a fancy way of saying boxing functionality in the repository.  This idea allows us to create separation of functionality inside the repository while still only having to inject a single repository into our services.

## Installation


From Package Manager Console:
```sh
Install-Package EFCoreSugar
```

From Cli:
```sh
dotnet add package EFCoreSugar
```


## Usage example

### **BaseDbRepository**

Repositories should all inherit from BaseDbRepository\<Your DbContext> and it's interface, if you chose to have one, should inherit from IBaseDbRepository to ensure functionality works with injection patterns.  A special note is that all repo groups will share the same DbContext as the main repo they are contained in.  This is Intentional to leverage caching in the DbContext across all the groups.  It is called DBContext inside the BaseDbRepository and its DbSets can be accessed with Set\<T>() in the RepositoryGroup.

Rules:
1. If you want the service extension methods to work with interface types because you want to interface your repository and groups then the interface name *MUST* match the implementation name preceded with an "I".  Example - MyAppRepo : IMyAppRepo

An example repository with its groupings may look like this:

```C#
//Our RepoDefinition
public class MyAppRepo : BaseDbRepository<MyAppDbContext>, IMyAppRepo
{
    //Notice we never new up this thing it's handled by the BaseRepo from the IServiceProvider
    public UserRepositoryGroup UserGroup {get; set;}

    public MyAppRepo(TestDbContext context, IServiceProvider provider) : base(context, provider) { }
}

//Our Group definition
public class UserRepositoryGroup : RepositoryGroup<User>
{
    public User GetUsersBySpecialMagic(string magicStuff)
    {
        //execute special queries in leveraging GetQueryable exposed by both the repo group
        //and the Repository
        var query = GetQueryable<User>();
        query.Where(u => u.Magic == magicStuff).Tolist().First();
    }
}
```

If you really want to follow more Rigid SOLID principals and interface all the things and keep interface and implementation seperate for a repository then you have some more setup to do.
See rule 1.

```C#
//Our interface
public interface IMyAppRepo : IBaseDbRepository
{
    //we need to expose this access if we are injecting based on the interface
    IUserRepositoryGroup UserGroup {get; set;}
}

public interface IUserRepositoryGroup : IRepositoryGroup<User>
{
    User GetUsersBySpecialMagic(string magicStuff)
}
```

Now usage looks like this:

```c#
private IMyAppRepo _repository {get; set;}

public void DoWork()
{
    //Get all the users
    var users = _repository.GetAll<User>();

    //Get a single entity. Pass in whatever type the PK is.
    var singleUser = _repository.GetSingle<User>(1);
    
    //Or
    singleUser = _repository.UserGroup.GetSingle(1);

    //Delete
    _repository.Delete(singleUser);

    //Add
    var newUser = _repository.Create(new User(){Name = "Sugar"});

    //Update
    newUser.Name = "EFCoreSugar";
    _repository.Update(newUser);

    //As of 1.2.4 there are also async version of these CRUD calls with optional cancellationToken
    newUser = _repository.CreateAsync(new User(){Name = "Sugar"}, cancellationToken);

    //additionally we can use a deferred status to do bulk operations.  This prevents a call
    //to SaveChanges() so we are not calling out to the DB on every operation which can be very slow
    //in this kind of situation
    _repository.SetDeffered(true);
    foreach(var user in users)
    {
        _repository.Delete(user);
    }
    _repository.SaveChanges();
    _repository.SetDeffered(false);
    
    //Finally we can call special functions from the groups as expected
    _repository.UserGroup.GetUsersBySpecialMagic("Magic");
}
```

Right now this pattern relies heavily on having dependency injection through a IServiceProvider and using that for injecting the repo into wherever they are used.  To make this a little easier there are extension methods that help achieve this without having to manually add all the Repositories and RepositoryGroups.

```C#
//In ConfigureServices in the Startup.Cs or however you are creating your DI container
public void ConfigureServices(IServiceCollection services)
{
    services.RegisterBaseRepositories();
    services.RegisterRepositoryGroups();

    //or 

    services.RegisterBaseRepositories().RegisterRepositoryGroups();
}
```

### **Filters**

Filters allow dynamically building where clauses based on which properties are set.  Filters do not know about what entity they are filtering ahead of time.  This means API's do not need to import entities directly to build a structure that would like like Filter\<User>.  Instead filters can be re-used for other entities if the properties are the same.  For example if all your entities have a property that is int Id, you can have create an IdFilter that can be applied to all DbSets.  For now there are a few important rules to know about filters and how they work.

1. Properties in a filter MUST be nullable (example: int?, Guid?, DateTime?, string) as this is how we determine if a property is set or not.  This eliminates the problem of int defaulting to zero, but maybe you are actually trying to query where ID == 0.
2. Properties structure should be flat.  Adding nested user defined objects with their own properties is needlessly complex and will not resolve anything useful.
3. If you do not specify an OrderBy using the OrderByPropertyName or an extension on the filter object to SetOrderBy it will try to use a property with [Key] attribute, a property named "Id", or simply the first property in the entity.
   * Composed Filters by means of filtering a FilteredQuery will try this default OrderBy behavior on the *FIRST* filter and it will ignore other orderbys *UNLESS* you specify the OrderByPropertyName *AND* it is not already in a previous OrderBy.
4. Composed filters will use the paging of the first filter and will ignore all other paging from other filters so set it there.
5. The ordering and paging are applied when a FilteredQuery is .Resolved().  If you access the query from the FilteredQuery and resolve it manually with a ToList() it will not be paged or ordered.  *In the future this behavior may change so avoid accessing the query directly unless you have to or append to the query and resolve its FilteredQuery parent.*
6. As of 1.2.4 Enumerable collections are allowed in the filter having them map a single property in the entity.  This resolves with a .Contains().
7. As of 1.2.4 Enumerable navigation properties on the entity are supported when they are a part of the property path in the [FilterProperty()] attribute.  This will resolve with .Any() and can be nested.

Setup:

```C#
//In ConfigureServices in the Startup.Cs or wherever you would put startup logic
public void ConfigureServices(IServiceCollection services)
{
    //v1.1.0+ has a optimization loader
    EFCoreSugarGlobal.BuildFilters();
}
```

Usage:
```C#
    //our filter inherits from Filter
    public class OrderFilter : Filter
    {
        public int? Id { get; set; }

        //we can specify the name of the entity property we want to target
        [FilterProperty("UserId")]
        public int? UId { get; set; }

        [FilterProperty("ProductName")]
        public string PName { get; set; }

        public int? Value { get; set; }

        public int? OrderTypeId { get; set; }

        //We can have nested property resolution. 
        //In this an Order has a OrderType navigation property and that has an Id
        [FilterProperty("OrderType.Id")]
        public int? NestedOrderTypeId { get; set; }

        //In this case we dont want use equals we just always want everything 
        //less than or equal to CreatedDate we could also specify the property name here
        [FilterProperty(FilterTest.LessThanEqualTo)]
        public DateTime? CreatedDate {get; set;}

        //If we want to have additional properties here that are not part of the
        //query we just use the FilterIgnore attribute
        [FilterIgnore]
        public string SpecialThing {get; set;}
        //As of 1.2.4 you can have a collection on the filter side that maps to a 
        //single property in the entity.  This will resolve with a .Contains().  
        //This example would look like (entity => StatusList.Contains(entity.Status))
        [FilterProperty("Status")]
        public List<OrderStatus> StatusList {get; set;}
    }
```

Now how to apply it:

```C#
private IMyAppRepo _repository {get; set;}

public void FilterStuff()
{
    var filter = new OrderFilter() { NestedOrderTypeId = 1, UId = 1 };
    
    //Let's set the orderby
    filter.SetOrderBy(of => of.UId, OrderByDirection.Descending);
    
    //Or like this
    filter.OrderByDirection = OrderByDirection.Descending;
    filter.OrderByPropertyName = "UId";

    //Paging, if you dont set these you get back all the results
    filter.PageNumber = 1;
    filter.PageSize = 100;

    //we will get back a FilteredResult which has a Value that in this case is IEnumerable of
    //Orders that have UserId == 1 and OrderType.Id == 1.  It will also have a 
    //RecordCount which is useful for paging
    var orders = _repository.Filter<Order>(filter);

    //We can also do this with the DBContext
    var ordersQuery = _repository.DbContext.Filter<Order>(filter);

    //or this with an IQueryable
    ordersQuery = _repository.DBContext.Set<Order>().AsQueryable().Filter(filter);

    //ordersQuery is a FilteredQuery, it has a Query property that contains the 
    //constructored query and a RecordCount of that result.  
    //You could modify the query further if you wish or you can Resolve it.

    //now we have a FilteredResult
    var ordersFromQuery = ordersQuery.Resolve();

    //The pre-paged count
    ordersFromQuery.RecordCount;
    //The actual IEnumerable of things
    orderFromQuery.Value;
}
```

### **Filter Composition**
Starting in v1.1.0 FilteredQuery objects can be composed together with other filters.  This is useful for operations where the UI will pass you an initial filter and you want to construct an additional filter in an API layer.  For example the UI will construct a filter for OrderId == 1 and the api will make another filter where UserId == 2.  You can compose them as follows:

```C#
        public List<Orders> GetOrders(OrderFilter filter)
        {
            var userFilter = new UserFilter() { UserId = 2};

            //Repository is a BaseDbRepository
            var orders = Repository.GetQueryable<Order>();

            //this is still a FilteredQuery so it can still be modified
            var filteredQuery = orders.Filter(filter).Filter(userFilter);

            //or
            filteredQuery = Repository.DBContext.Filter<Order>(filter).Filter(userFilter);

            //this will apply the paging and orderby
            var filteredResult = filteredQuery.Resolve();
        }
```

### **Filter Operation**
Starting in v1.2.1 Filters can have a FilterOperation Attribute.  This specifies how the predicte is built with either all "and" (default) or all "or".

```C#
    //This would translate to Id == <value> || FirstName == <value> || LastName == <value> ...etc.
    [FilterOperation(FilterOperation.Or)]
    public class UserFilter : Filter
    {
        public int? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DOB { get; set; }
        public int? Age { get; set; }
    }
```

### **FuzzyMatchTerm**
Starting in v1.2.1 Filters all have a FuzzyMatchTerm property.  It's behavior can be set by the FilterFuzzyMatch Attribute to: 
* Contains (default)
* StartsWith
* EndsWith
* None

**NOTE: Prior to 1.2.5 these values were set with the FilterOperation attribute.  That is obsolete and will be removed in a future version.**

If you specify a value for a FuzzyMatchTerm it will try to apply a match based on the FuzzyMatchMode to all string properties that do not have a value set.  This allows doing things like dynamic partial searches where the user types in something like "Mon" and it finds "Montana", "Monday", "Jack the Monkey"

```C#
    [FilterFuzzyMatch(FuzzyMatchMode.Contains)]
    public class ItemFilter : Filter
    {
        public int? Id { get; set; }
		//You can override the top level fuzzy match type and hide the property from fuzzymatching
		[FilterFuzzyMatch(FuzzyMatchMode.None)]
        public string ItemName { get; set; }
    }
```


## Release History
* 1.2.7
	* Upgraded to EF Core 3.0
	* Removed the Query<T>() function from BaseRepository as this is obsolete with Ef Core 3.0
* 1.2.6
	* Fixed RegisterBaseRepositories to correctly load the repository even if the code before it has not referenced the repo and loaded the dll into the working memory.
	* Added an optional ServiceLifetime to RegisterBaseRepositories, by default it is Transient
	* Added a RecycleDbContext function to BaseDbRepositories that will dispose of the current context and try to acquire a new one.  This is useful in large ETL processes where you want better memory management control while still relying on the magic of DI.
	    * **NOTE:** This will only work if the context is registered in your DI container as Lifetime.Transient!
	* Added the Set<T> and Query<T> functions to the BaseDbRepository to keep it consistent with how those are accessed in the RepositoryGroups
	* Added a ResolveAsync function to FilteredQueries
	* Added Resolve and ResolveAsync functions to IQueryable<T> so instead of query.Filter(filter).Resolve() you can just write query.Resolve(filter)
* 1.2.5
	* Added the FuzzyMatchAttribute and added obsolete warning to FilterOperation for setting the FuzzyMatchMode.  You can now set FuzzyMatchMode on a property via the FuzzyMatchAttribute.  This will override any FuzzyMatchMode on the filter object that contains that property.
	* Added FuzzyMatchMode.None to exclude a property from fuzzy match searching.
	* Filters now look for both Public and NonPublic properties.  This is so you can hide properties in query strings but still use them for things like fuzzy search or orderby. 
* 1.2.4
    * Add the ability to resolve collection navigation properties in the entity. It will do a look up on the collection with .Any(\<predicate>) for the final condition.  It is nested so you can end up with expressions that look like query.Where(x => x.Users.Any(u => u.Items.Any(i => i.Name == "Hat")))
    * Add the ability to resolve a collection on the filter side.  This will resolve with a .Contains().  This way you can have something like a List\<someEnum>() against an entity with an Enum property.  This ends up looking like query.Where(x => ( Enum1, Enum2, Enum3 ).Contains( x.Status ))
    * Added several async calls to the repository for things like SaveChanges,Create,Update,Delete.
    * Added Query\<T>() to the repositorygroups to get DbQuery types from the context.
    * Calling GetQueryable\<T>() from the Repository or RepositoryGroup will first try to get a DbSet and then try to get a DbQuery.
* 1.2.3
	* Fixed the issue where a nullable entity type would fail with an error similar to no comparison GreaterThanOrEqualTo between some nullable thing and not nullable thing.
* 1.2.2
    * Grouped FuzzyMatchs together and attach them to the end of the predicate with an "AND".  The reason for this is to avoid disrupting the rest of the predicate with force "OR" fuzzy matches so we dont have "Id OR FuzzyMatch1 AND FirstName OR FuzzyMatch2" when we really want "Id AND FirstName And FuzzyMatch1 OR FuzzyMatch2"
    * Made FilterOperations into a "look behind" so we add the next section of the predicate with the previous properties filteroperation.  This is not particularly jarring right now since operations are filter wide but it's a potential solution when you have something like Id that you want joined with an "AND" and Name that you want joined with an "OR".  Before this change you would get Id OR Name ? (other stuff).  Now you would get Id AND Name OR (other stuff).
* 1.2.1
    * Fixed filters without any properties set exceptioning while filtering.  You can now use a filter to strictly page or order by.
    * Added FuzzyMatchTerm
    * Added FilterOperationAttribute. NOTE: There are plans to extend this capability at the property level to build mixed predicates.
* 1.1.0
    * Changed namespace to be EFCoreSugar since that makes more sense given the package name and altered the folder structure
    * Made FilteredQuery not have a RecordCount so you can compose filters/queries together.  RecordCount now only appears after you .Resolve() a FilteredQuery.
    * Added lots of caching for properties so we only need to reflect once per type
    * Added a GlobalEFCoreSugar class to aid in startup type of behavior (caching)
    * Revamped the behavior for loading RepositoryGroups and BaseDbRepositories to support abstracted interface types.
    * Made the direction in SetOrderBy() be optional, ascending is the default
* 1.0.1
    * Exposed the FilterIgnore attribute
* 1.0.0
    * Initial Release

