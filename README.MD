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

//Our interface
public interface IMyAppRepo : IBaseDbRepository
{
    //we need to expose this access if we are injecting based on the interface
    UserRepositoryGroup UserGroup {get; set;}
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

Filters allow dynamically building where clauses based on which properties are set.  For now there are a few important rules to know about filters and how they work.

1. Properties in a filter MUST be nullable (example: int?, Guid?, DateTime?, string) as this is how we determine if a property is set or not.  This eliminates the problem of int defaulting to zero, but maybe you are actually trying to query where ID == 0.
2. Properties structure should be flat.  Adding nested user defined objects with their own properties is needlessly complex and will not resolve anything useful.
3. If you do not specify an OrderBy using the OrderByPropertyName or an extension on the filter object to SetOrderBy it will try to use a property with [Key] attribute, a property named "Id", or simply the first one that is set in the filter properties.

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
    }
```

Now how to apply it:

```C#
private IMyAppRepo _repository {get; set;}

public void FilterStuff()
{
    var filter = new OrderFilter() { NestedOrderTypeId = 1, UId = 1 };

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

    var ordersFromQuery = ordersQuery.Resolve();//now we have a FilteredResult
}
```


## Release History

* 1.0.1
    * Exposed the FilterIgnore attribute
* 1.0.0
    * Initial Release

